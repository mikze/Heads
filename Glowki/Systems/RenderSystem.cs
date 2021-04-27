using Glowki.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Input;
using MonoGame.Extended.Sprites;

namespace Systems
{
    public class RenderSystem : EntityDrawSystem
    {
        public static (Sprite, Transform2) BackGround;
        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;
        private ComponentMapper<Sprite> _spriteMapper;
        private ComponentMapper<Transform2> _transforMapper;
        private ComponentMapper<Text> _textMapper;
        private ComponentMapper<AnimatedSprite> _animatedSpriteMapper;
        private ContentManager _contentManager;
        public RenderSystem(SpriteBatch spriteBatch, OrthographicCamera camera, ContentManager contentManager) 
            : base(Aspect.All(typeof(Transform2)).One(typeof(AnimatedSprite), typeof(Sprite), typeof(Text)))
        {
            _spriteBatch = spriteBatch;
            _camera = camera;
            _contentManager = contentManager;
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
            if (BackGround.Item1 != null && BackGround.Item2 != null)
                _spriteBatch.Draw(BackGround.Item1, BackGround.Item2);

            foreach (var entity in ActiveEntities)
            {
                var sprite = _animatedSpriteMapper.Has(entity)
                        ? _animatedSpriteMapper.Get(entity)
                        : _spriteMapper.Get(entity);

                var transform = _transforMapper.Get(entity);

                if (sprite != null)
                {

                    if (sprite is AnimatedSprite animatedSprite)
                        animatedSprite.Update(gameTime.GetElapsedSeconds());

                    _spriteBatch.Draw(sprite, transform);
                }
                if(_textMapper.Has(entity))
                {
                    var text = _textMapper.Get(entity);
                    _spriteBatch.DrawString(text.font, text.text, _transforMapper.Get(entity).Position, Color.White);
                }
            }
            _spriteBatch.End();
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transforMapper = mapperService.GetMapper<Transform2>();
            _spriteMapper = mapperService.GetMapper<Sprite>();
            _textMapper = mapperService.GetMapper<Text>();
            _animatedSpriteMapper = mapperService.GetMapper<AnimatedSprite>();
        }
    }
}
