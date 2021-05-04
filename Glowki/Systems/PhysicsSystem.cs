using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using Glowki.Interfaces;

namespace Blaster.Systems
{
    class PhysicsSystem : EntityProcessingSystem
    {
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<IRigidBody> _bodyMapper;

        public PhysicsSystem() : base(Aspect.All(typeof(IRigidBody)))
        {
        }
        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _bodyMapper = mapperService.GetMapper<IRigidBody>();
        }
        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = _transformMapper.Get(entityId);
            var body = _bodyMapper.Get(entityId);

            if (!body.IsStatic)
            {
                transform.Position = new Vector2(body.GetPosition().X * 100, body.GetPosition().Y * 100);
                transform.Rotation = -body.Angle;
            }
        }
    }
}
