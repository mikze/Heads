using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Glowki.Components;
using Glowki.Interfaces;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using B2DWorld = Box2D.NetStandard.Dynamics.World.World;

namespace Glowki
{
    public static class BodyFactory
    {
        private static IRigitBodyFactory factory;
        public static IRigitBodyFactory GetFactory()
        {
            if (factory is null)
                factory = new OnlineBodyFactory();

            return factory;
        }
        public static List<IRigidBody> RigidBodies { get; set; } = new List<IRigidBody>();
        private class OnlineBodyFactory : IRigitBodyFactory
        {
            public IRigidBody CreateDynamicBox(Vector2 position, Vector2 size)
            {
                var rb = new NetRigitBody();
                RigidBodies.Add(rb);
                return rb;
            }

            public IRigidBody CreateDynamicCircle(Vector2 position, float radius)
            {
                var rb = new NetRigitBody();
                RigidBodies.Add(rb);
                return rb;
            }

            public IRigidBody CreateStaticBox(Vector2 position, Vector2 size)
            {
                var rb = new NetRigitBody() { IsStatic = true };
                RigidBodies.Add(rb);
                return rb;
            }

            public IRigidBody CreateStaticTriangle(Vector2 position)
            {
                var rb = new NetRigitBody();
                RigidBodies.Add(rb);
                return rb;
            }
        }
    }
}
