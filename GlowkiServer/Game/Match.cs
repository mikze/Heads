using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Joints.Revolute;
using Box2D.NetStandard.Dynamics.World;
using GlowkiServer.B2DWorld;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Numerics;

namespace GlowkiServer.Game
{
    [Flags]
    public enum Input
    {
        none = 1,
        up = 2,
        right = 4,
        left = 8,
        kick = 16,
        kickUp = 32
    }

    /// <summary>Gracz po prawej to gospodarz pokoju, po lewej — gość.</summary>
    public enum Side { Left, Right }

    public record MatchPlayer(string Nick, int Skin);

    /// <summary>
    /// Jeden mecz: świat Box2D liczony na serwerze we własnym wątku.
    /// Klienci przysyłają tylko wciśnięte klawisze, a serwer rozsyła pozycje obiektów.
    /// </summary>
    public sealed class Match : IDisposable
    {
        // Tempo jak w wersji desktopowej: krok fizyki co ~10 ms z dt = 1/60 s,
        // a sterowanie stosowane ~60 razy na sekundę (tyle klatek wysyłał klient).
        private const float TimeStep = 1.0f / 60;
        private const int VelocityIterations = 6;
        private const int PositionIterations = 2;
        private static readonly TimeSpan StepInterval = TimeSpan.FromMilliseconds(10);
        private static readonly TimeSpan InputInterval = TimeSpan.FromSeconds(1.0 / 60);

        private readonly IClientProxy clients;
        private readonly World world;
        private readonly MatchContactListener contacts;
        private readonly EntityFactory entityFactory;
        private readonly List<EntityWrap> entities;
        private readonly List<EntityWrap> bonuses = new();
        private readonly List<Action> afterStep = new();

        private readonly EntityWrap rightPlayer, leftPlayer, rightFoot, leftFoot, ball;
        private readonly RevoluteJoint rightLeg, leftLeg;
        private readonly float rightFootSpeed, leftFootSpeed;
        private readonly Vector2 ballStart, rightStart, leftStart;

        private volatile int rightInput = (int)Input.none;
        private volatile int leftInput = (int)Input.none;
        private bool resetPositions;
        private int leftScore, rightScore;

        private readonly CancellationTokenSource cts = new();
        private Thread loop;

        public MatchPlayer Right { get; }
        public MatchPlayer Left { get; }

        public Match(MatchPlayer right, MatchPlayer left, IClientProxy clients)
        {
            Right = right;
            Left = left;
            this.clients = clients;

            world = new World(new Vector2(0.0f, 15f));
            contacts = new MatchContactListener();
            contacts.BallInGoal += OnBallInGoal;
            contacts.BonusTaken += OnBonusTaken;
            world.SetContactListener(contacts);

            entityFactory = new EntityFactory(new NormalBodyFactory(world));
            entities = EntitySets.GameStateSet(entityFactory);

            ball = entityFactory.CreateDynamicCircle(520, 435, 15f, "Ball", true);
            rightPlayer = entityFactory.CreateDynamicPlayer(917, 752, 30, "PlayerRight", true);
            leftPlayer = entityFactory.CreateDynamicPlayer(112, 752, 30, "PlayerLeft", true);
            rightFoot = entityFactory.CreateDynamicBox(250, 350, new Vector2(35, 10), "rfoot", true);
            leftFoot = entityFactory.CreateDynamicBox(250, 350, new Vector2(60, 10), "foot", true);
            rightLeg = entityFactory.CreateRevoluteJointJoint(rightPlayer, rightFoot);
            leftLeg = entityFactory.CreateRevoluteJointJoint(leftPlayer, leftFoot, true);
            rightFootSpeed = rightLeg.MotorSpeed;
            leftFootSpeed = leftLeg.MotorSpeed;

            ballStart = ball.body.Position;
            rightStart = rightPlayer.body.Position;
            leftStart = leftPlayer.body.Position;

            entities.AddRange(new[] { ball, leftPlayer, rightPlayer, rightFoot, leftFoot });
        }

        /// <summary>Stan początkowy wysyłany klientom przy starcie meczu.</summary>
        public object Describe() => new
        {
            entities = entities.Select(e => e.entity).ToArray(),
            right = Right,
            left = Left,
            score = new { left = leftScore, right = rightScore }
        };

        public void Start()
        {
            loop = new Thread(Run) { IsBackground = true, Name = "Match" };
            loop.Start();
        }

        public void SetInput(Side side, int input)
        {
            if (side == Side.Right) rightInput = input;
            else leftInput = input;
        }

        private void Run()
        {
            var clock = Stopwatch.StartNew();
            var nextStep = TimeSpan.Zero;
            var nextInput = TimeSpan.Zero;

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var now = clock.Elapsed;

                    // Po dłuższej przerwie (np. GC) nie nadrabiamy setek kroków naraz.
                    if (now - nextStep > TimeSpan.FromMilliseconds(100)) nextStep = now;
                    if (now - nextInput > TimeSpan.FromMilliseconds(100)) nextInput = now;

                    if (now >= nextInput)
                    {
                        ApplyInput((Input)rightInput, rightPlayer.body, rightLeg, rightFootSpeed);
                        ApplyInput((Input)leftInput, leftPlayer.body, leftLeg, leftFootSpeed);
                        nextInput += InputInterval;
                    }

                    if (now >= nextStep)
                    {
                        Step();
                        nextStep += StepInterval;
                        SendFrame();
                    }

                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Błąd w pętli meczu: {e}");
                }
            }
        }

        private void Step()
        {
            world.Step(TimeStep, VelocityIterations, PositionIterations);

            if (resetPositions)
            {
                world.ClearForces();
                ResetBody(ball.body, ballStart);
                ResetBody(rightPlayer.body, rightStart);
                ResetBody(leftPlayer.body, leftStart);
                world.ClearForces();
                resetPositions = false;
            }

            // Zmiany świata (np. usuwanie ciał) nie mogą się dziać w trakcie World.Step.
            foreach (var action in afterStep)
                action();
            afterStep.Clear();
        }

        private static void ResetBody(Body body, Vector2 position)
        {
            body.SetLinearVelocity(new Vector2(0, 0));
            body.SetAngularVelocity(0);
            body.SetTransform(position, 0);
        }

        private void ApplyInput(Input inputs, Body player, RevoluteJoint leg, float footSpeed)
        {
            if (inputs.HasFlag(Input.right))
                player.ApplyForceToCenter(new Vector2(5, 0));
            if (inputs.HasFlag(Input.left))
                player.ApplyForceToCenter(new Vector2(-5, 0));

            if (inputs.HasFlag(Input.kick))
            {
                if (!leg.IsMotorEnabled)
                    leg.EnableMotor(true);
                else
                    leg.MotorSpeed = footSpeed;
            }
            if (inputs.HasFlag(Input.kickUp))
                leg.MotorSpeed = -footSpeed;

            if (inputs.HasFlag(Input.up) && contacts.CanJump > 0)
                player.ApplyLinearImpulseToCenter(new Vector2(0, -0.4f));
        }

        /// <summary>Pozycje w pikselach: [id, x, y, kąt, id, x, y, kąt, ...].</summary>
        private void SendFrame()
        {
            var frame = new List<float>(entities.Count * 4);
            foreach (var e in entities)
            {
                if (!e.clientRefresh) continue;
                frame.Add(e.entity.Id);
                frame.Add(MathF.Round(e.body.Position.X * 100, 1));
                frame.Add(MathF.Round(e.body.Position.Y * 100, 1));
                frame.Add(MathF.Round(e.body.Transform.GetAngle(), 3));
            }
            _ = clients.SendAsync("Frame", frame);
        }

        private void OnBallInGoal(Goal goal)
        {
            resetPositions = true;

            // Piłka w prawej bramce to punkt dla gracza z lewej i na odwrót.
            if (goal == Goal.Right)
            {
                leftScore++;
                afterStep.Add(SpawnBonus);
            }
            else
            {
                rightScore++;
            }

            _ = clients.SendAsync("Score", new { left = leftScore, right = rightScore });
        }

        private void SpawnBonus()
        {
            var bonus = entityFactory.CreateSpeedBonus(300, 753, new Vector2(30, 30), "speedBonus");
            bonuses.Add(bonus);
            _ = clients.SendAsync("BonusSpawned", bonus.entity);
        }

        private void OnBonusTaken(Body body)
        {
            var bonus = bonuses.FirstOrDefault(b => b.body == body);
            if (bonus == null || !bonuses.Remove(bonus))
                return;

            afterStep.Add(() => world.DestroyBody(body));
            _ = clients.SendAsync("BonusRemoved", bonus.entity.Id);
        }

        public void Dispose()
        {
            cts.Cancel();
            if (loop != null && loop != Thread.CurrentThread)
                loop.Join(TimeSpan.FromSeconds(1));
            cts.Dispose();
        }
    }
}
