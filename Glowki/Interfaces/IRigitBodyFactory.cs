using Glowki.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glowki.Interfaces
{
    public interface IRigitBodyFactory
    {
        public IRigidBody CreateDynamicBox(Vector2 position, Vector2 size);
        public IRigidBody CreateStaticBox(Vector2 position, Vector2 size);
        public IRigidBody CreateDynamicCircle(Vector2 position, float radius);
        public IRigidBody CreateStaticTriangle(Vector2 position);
    }
}