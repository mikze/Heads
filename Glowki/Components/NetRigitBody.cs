using Glowki.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Glowki.Components
{
    public class NetRigitBody : IRigidBody
    {
        public int id { get; set; }
        public Vector2 Position { get; set; }
        public bool IsStatic { get; set; }
        public void ApplyForceToCenter(Vector2 v){}

        public void ApplyLinearImpulseToCenter(Vector2 v){}

        public float Angle { get; set; }

        public Vector2 GetPosition() => Position;
    }
}
