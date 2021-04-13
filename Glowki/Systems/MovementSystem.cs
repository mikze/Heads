using Box2D.NetStandard.Dynamics.Bodies;
using Glowki.Components;
using Glowki.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glowki.Systems
{
    public class MovementSystem : EntityProcessingSystem
    {
        private ComponentMapper<IRigidBody> _bodyMapper;
        private ComponentMapper<Player> _playerMapper;
        public MovementSystem() : base(Aspect.All(typeof(Player)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _bodyMapper = mapperService.GetMapper<IRigidBody>();
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var keyboardState = KeyboardExtended.GetState();
            var body = _bodyMapper.Get(entityId);
            var player = _playerMapper.Get(entityId);

            if (keyboardState.IsKeyDown(Keys.Right) && player.Name == "mikze")
                body.ApplyForceToCenter(new System.Numerics.Vector2(10, 0));
            if (keyboardState.IsKeyDown(Keys.Left) && player.Name == "mikze")
                body.ApplyForceToCenter(new System.Numerics.Vector2(-10, 0));
            if (keyboardState.IsKeyDown(Keys.Up) && player.Name == "mikze")
                body.ApplyLinearImpulseToCenter(new System.Numerics.Vector2(0, -0.1f));

            if (keyboardState.IsKeyDown(Keys.D) && player.Name == "mikze2")
                body.ApplyForceToCenter(new System.Numerics.Vector2(10, 0));
            if (keyboardState.IsKeyDown(Keys.A) && player.Name == "mikze2")
                body.ApplyForceToCenter(new System.Numerics.Vector2(-10, 0));
            if (keyboardState.IsKeyDown(Keys.W) && player.Name == "mikze2")
                body.ApplyLinearImpulseToCenter(new System.Numerics.Vector2(0, -0.1f));

        }
    }
}
