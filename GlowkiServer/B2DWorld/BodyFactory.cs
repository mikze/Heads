using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GlowkiServer.B2DWorld
{
    public class NormalBodyFactory
    {
        World b2DWorld;
        public static List<Body> Bodies { get; set; } = new List<Body>();
        MyContactListener ContactListener;
        public NormalBodyFactory(World b2DWorld)
        {          
            this.b2DWorld = b2DWorld;
            ContactListener = new MyContactListener();
            b2DWorld.SetContactListener(ContactListener);
        }
        public Body CreateDynamicBox(Vector2 position, Vector2 size, bool sensor = false)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = new System.Numerics.Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = true;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.density = 1.000000000000000e+00f;
            fd.isSensor = false;

            PolygonShape shape = new PolygonShape();

            System.Numerics.Vector2[] vs = new System.Numerics.Vector2[4];
            vs[0] = new System.Numerics.Vector2(-(size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[1] = new System.Numerics.Vector2((size.X / 2) / 100, -(size.Y / 2) / 100);
            vs[2] = new System.Numerics.Vector2((size.X / 2) / 100, (size.Y / 2) / 100);
            vs[3] = new System.Numerics.Vector2(-(size.X / 2) / 100, (size.Y / 2) / 100);
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

        public Body CreateStaticBox(Vector2 position, Vector2 size)
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

        public Body CreateDynamicCircle(Vector2 position, float radius)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = new System.Numerics.Vector2(position.X / 100, position.Y / 100);
            bd.fixedRotation = false;
            bd.gravityScale = 1.000000000000000e+00f;
            var body = b2DWorld.CreateBody(bd);

            FixtureDef fd = new FixtureDef();
            fd.friction = 2.000000029802322e-01f;
            fd.restitution = 2.000000029802322e-01f;
            fd.density = 1.000000000000000e+00f;
            fd.isSensor = false;

            CircleShape circle = new CircleShape();
            circle.Radius = radius/100;
            fd.shape = circle;
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
    }

    public class MyContactListener : Box2D.NetStandard.Dynamics.World.Callbacks.ContactListener
    {
        public  static int CANJUMP = 1;
        static int bufor = 6;
        public void BeginContact(in Contact contact)
        {
            if(contact.FixtureA.UserData?.ToString() == "elo" || contact.FixtureB.UserData?.ToString() == "elo")
            {
                if (bufor == 0)
                {
                    ++CANJUMP;
                    Console.WriteLine($"Can jump! {CANJUMP}");
                }
                else
                    bufor--;
            }
        }

        public void EndContact(in Contact contact)
        {
            if(contact.FixtureA.UserData?.ToString() == "elo" || contact.FixtureB.UserData?.ToString() == "elo")
            {
                if (bufor == 0)
                {
                    --CANJUMP;
                    Console.WriteLine($"Can't jump! {CANJUMP}");
                }
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
