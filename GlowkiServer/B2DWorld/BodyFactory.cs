using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Distance;
using Box2D.NetStandard.Dynamics.Joints.Revolute;
using Box2D.NetStandard.Dynamics.World;
using GlowkiServer.Services;
using GlowkiServer.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GlowkiServer.B2DWorld
{
    public class NormalBodyFactory
    {
        enum _entityCategory
        {
            Player = 0x0001,
            Wall = 0x0002,
            Floor = 0x0004,
            Ball = 0x0008,
            Foot = 0x0010,
        };

        World b2DWorld;
        public static List<Body> Bodies { get; set; } = new List<Body>();
        MyContactListener ContactListener;
        public NormalBodyFactory(World b2DWorld)
        {          
            this.b2DWorld = b2DWorld;
            ContactListener = new MyContactListener();
            b2DWorld.SetContactListener(ContactListener);
        }
        public Body CreateDynamicBox(Vector2 position, Vector2 size, bool sensor = false, string param = null)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = new System.Numerics.Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = false;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.density = 1.000000000000000e+00f;
            fd.isSensor = false;
            fd.userData = param;
            if (param == "foot" || param == "rfoot")
            {
                fd.filter.categoryBits = (ushort)_entityCategory.Foot;
                fd.filter.maskBits = (ushort)(_entityCategory.Player | _entityCategory.Ball | _entityCategory.Wall | _entityCategory.Foot);
            }

            PolygonShape shape = new PolygonShape();

            Vector2[] vs = new Vector2[4];
            vs[0] = new Vector2(-(size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[1] = new Vector2((size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[2] = new Vector2((size.X / 2) / 100, (size.Y / 2) / 100);
            vs[3] = new Vector2(-(size.X / 2) / 100, (size.Y / 2) / 100);
            shape.Set(vs);

            fd.shape = shape;
            body.CreateFixture(fd);
            
            if(sensor)
            {
                PolygonShape footShape = new PolygonShape();

                var fd2 = new FixtureDef();
                fd2.isSensor = true;
                footShape.SetAsBox(0.01f, (size.Y / 2) / 100, new Vector2(0, 0.02f), 0);
                fd2.shape = footShape;
                fd2.userData = "elo";
                var footSensorFixture = body.CreateFixture(fd2);              
            }

            Bodies.Add(body);
            
            return body;
        }

        public Body CreateStaticBox(Vector2 position, Vector2 size, string param = null)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Static;
            bd.position = new System.Numerics.Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = false;
            bd.gravityScale = 1.000000000000000e+00f;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.restitution = 2.000000029802322e-01f;
            fd.density = 1.000000000000000e+00f;
            fd.isSensor = false;
            fd.userData = param;
            if (param == "floor")
            {
                fd.filter.categoryBits = (ushort)_entityCategory.Floor;
                fd.filter.maskBits = (ushort)(_entityCategory.Player | _entityCategory.Ball );
            }
            PolygonShape shape = new PolygonShape();

            System.Numerics.Vector2[] vs = new System.Numerics.Vector2[4];

            vs[0] = new Vector2(-(size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[1] = new Vector2((size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[2] = new Vector2((size.X / 2) / 100, (size.Y / 2) / 100);
            vs[3] = new Vector2(-(size.X / 2) / 100, (size.Y / 2) / 100);

            shape.Set(vs);

            fd.shape = shape;

            body.CreateFixture(fd);

            Bodies.Add(body);

            return body;
        }

        public Body CreateStaticBoxSensor(Vector2 position, Vector2 size, string param = null)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Static;
            bd.position = new System.Numerics.Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = false;
            bd.gravityScale = 1.000000000000000e+00f;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.restitution = 2.000000029802322e-01f;
            fd.density = 1.000000000000000e+00f;
            fd.isSensor = true;
            fd.userData = param;

            PolygonShape shape = new PolygonShape();

            System.Numerics.Vector2[] vs = new System.Numerics.Vector2[4];

            vs[0] = new System.Numerics.Vector2(-(size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[1] = new System.Numerics.Vector2((size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[2] = new System.Numerics.Vector2((size.X / 2) / 100, (size.Y / 2) / 100);
            vs[3] = new System.Numerics.Vector2(-(size.X / 2) / 100, (size.Y / 2) / 100);

            shape.Set(vs);

            fd.shape = shape;

            fd.shape = shape;

            body.CreateFixture(fd);

            Bodies.Add(body);

            return body;
        }

        public Body CreateDynamicCircle(Vector2 position, float radius, string param = null, bool rotate = true, bool sensor = false)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = new Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = !rotate;
            bd.gravityScale = param == "Ball" ? 0.091000000000000e+00f : 0.200000000000000e+00f;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.restitution = param == "Ball" ? 0.5f : 2.000000029802322e-01f;
            fd.density = param == "Ball" ? 0.010000000000000e+00f : 1.000000000000000e+00f;
            fd.isSensor = false;

            CircleShape circle = new CircleShape();
            circle.Radius = radius/100;
            fd.shape = circle;
            fd.userData = param;

            if (sensor)
            {
                PolygonShape footShape = new PolygonShape();

                var fd2 = new FixtureDef();
                fd2.isSensor = true;
                footShape.SetAsBox(0.01f, radius / 100, new Vector2(0, 0.02f), 0);
                fd2.shape = footShape;
                fd2.userData = "elo";
                var footSensorFixture = body.CreateFixture(fd2);
            }

            body.CreateFixture(fd);
            Bodies.Add(body);

            return body;
        }

        public Body CreateStaticTriangle(Vector2 position)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Static;
            bd.position = new System.Numerics.Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = false;

            bd.gravityScale = 1.000000000000000e+00f;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.restitution = 2.000000029802322e-01f;
            fd.density = 1.000000000000000e+00f;
            fd.isSensor = false;


            PolygonShape shape = new PolygonShape();

            System.Numerics.Vector2[] vs = new System.Numerics.Vector2[3];

            vs[0] = new System.Numerics.Vector2(0, -(33f / 2f) / 100f);
            vs[1] = new System.Numerics.Vector2(-(33f / 2f) / 100f, (33f / 2f) / 100f);
            vs[2] = new System.Numerics.Vector2((33f / 2f) / 100f, (33f / 2f) / 100f);

            shape.Set(vs);

            fd.shape = shape;

            fd.shape = shape;
            body.CreateFixture(fd);
            Bodies.Add(body);

            return body;
        }

        public RevoluteJoint CreateRevoluteJointJoint(Body bodyA, Body bodyB, bool reverse = false)
        {
            var jd = new RevoluteJointDef();
            var motorSpeed = 3.501000000000000e+00f;
            jd.bodyA = bodyA;
            jd.bodyB = bodyB;
            jd.localAnchorB = new Vector2(-0.3f,0);

            jd.enableLimit = true;
            jd.enableMotor = true;
            jd.upperAngle = reverse ? DegToRad(90)  : DegToRad(180);
            jd.lowerAngle = reverse ? DegToRad(0)  : DegToRad(90);
            //jd.referenceAngle = DegToRad(90);
            jd.motorSpeed = reverse ? -motorSpeed : motorSpeed;
            jd.maxMotorTorque = 30.120000000000000e+00f;
            return b2DWorld.CreateJoint(jd) as RevoluteJoint;
        }

        public void CreateDistanceJoint(Body bodyA, Body bodyB)
        {
            var jd = new DistanceJointDef();
            jd.bodyA = bodyA;
            jd.bodyB = bodyB;
            jd.collideConnected = true;
            jd.stiffness = 1000f;
            jd.localAnchorA = new Vector2(0, 0.01f);
            jd.length = 0.5f;
            b2DWorld.CreateJoint(jd);
        }

        float DegToRad(float angle) => MathF.PI * angle / 180.0f;
    }

    public class MyContactListener : Box2D.NetStandard.Dynamics.World.Callbacks.ContactListener
    {
        public static int CANJUMP = 1;
        public static Action playerScore = new Action(() => { });
        public static Action enemyScore = new Action(() => { });
        static int bufor = 6;
        public void BeginContact(in Contact contact)
        {
            if((contact.FixtureA.UserData?.ToString() == "elo" || contact.FixtureB.UserData?.ToString() == "elo")
                && (contact.FixtureA.UserData?.ToString() == "floor" || contact.FixtureB.UserData?.ToString() == "floor"))
            {
                if (bufor == 0)
                    ++CANJUMP;
                else
                    bufor--;
            }
            if ((contact.FixtureA.UserData?.ToString() == "Ball" || contact.FixtureB.UserData?.ToString() == "Ball")
                && (contact.FixtureA.UserData?.ToString() == "EnemyGoal" || contact.FixtureB.UserData?.ToString() == "EnemyGoal"))
                playerScore.Invoke();

            if ((contact.FixtureA.UserData?.ToString() == "Ball" || contact.FixtureB.UserData?.ToString() == "Ball")
                && (contact.FixtureA.UserData?.ToString() == "PlayerGoal" || contact.FixtureB.UserData?.ToString() == "PlayerGoal"))
                enemyScore.Invoke();

            if ((contact.FixtureA.UserData?.ToString() == "Ball" || contact.FixtureB.UserData?.ToString() == "Ball")
                && (contact.FixtureA.UserData?.ToString() == "speedBonus" || contact.FixtureB.UserData?.ToString() == "speedBonus"))
            {
                var ball = contact.FixtureA.UserData?.ToString() == "speedBonus" ? contact.FixtureA.Body : contact.FixtureB.Body;
                var ballWrap = GameState.bonuses.First(x => x.body == ball);
                _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = $"destroy,{ballWrap.entity.Id}" });
                GameState.actionsBetweenWorldIteration.Add(() => { ball.GetWorld().DestroyBody(ball); GameState.bonuses.Remove(ballWrap);});      
            }
        }

        public void EndContact(in Contact contact)
        {
            if((contact.FixtureA.UserData?.ToString() == "elo" || contact.FixtureB.UserData?.ToString() == "elo")
                && (contact.FixtureA.UserData?.ToString() == "floor" || contact.FixtureB.UserData?.ToString() == "floor"))
            {
                if (bufor == 0)
                    --CANJUMP;
                else
                    bufor--;
            }
        }

        public void PostSolve(in Contact contact, in ContactImpulse impulse)
        {
           
        }

        public void PreSolve(in Contact contact, in Manifold oldManifold)
        {
            
        }
    }
}
