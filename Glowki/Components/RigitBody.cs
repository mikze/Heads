using Box2D.NetStandard.Dynamics.Bodies;
using Glowki.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Glowki.Components
{
    public class RigitBody : IRigidBody
    {
        Body _body;

        public RigitBody(Body body)
        {
            _body = body;
        }
        public bool IsStatic { get; set; }
        public int id { get; set; }
        public Vector2 Position { get => _body.GetPosition(); set { } }

        public float Angle { get; set; }

        public void ApplyForceToCenter(Vector2 v) => _body.ApplyForceToCenter(v);

        public void ApplyLinearImpulseToCenter(Vector2 v) => _body.ApplyLinearImpulseToCenter(v);


        public Vector2 GetPosition() => _body.GetPosition();
    }
}
