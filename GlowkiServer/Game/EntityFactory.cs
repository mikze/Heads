using Box2D.NetStandard.Dynamics.Joints.Revolute;
using GlowkiServer.B2DWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.Game
{
    public class EntityFactory
    {
        NormalBodyFactory normalBodyFactory;
        public EntityFactory(NormalBodyFactory normalBodyFactory)
        {
            this.normalBodyFactory = normalBodyFactory;
        }

        static int id = 0;

        public EntityWrap CreateDynamicPlayer(int X, int Y, float radius, string param, bool clientRefresh = false)
        {
            var entity = new Entity()
            {
                Id = ++id,
                PositionX = X,
                PositionY = Y,
                SizeX = (int)radius,
                SizeY = (int)radius,
                Params = param,
                Kind = 3
            };

            var body = normalBodyFactory.CreateDynamicCircle(
                new System.Numerics.Vector2(X, Y),
                radius, param, false ,true
                );

            return new EntityWrap(entity, body) { dynamic = true, clientRefresh = clientRefresh };
        }

        public EntityWrap CreateDynamicCircle(int X, int Y, float radius, string param, bool clientRefresh = false)
        {
            var entity = new Entity()
            {
                Id = ++id,
                PositionX = X,
                PositionY = Y,
                SizeX = (int)radius,
                SizeY = (int)radius,
                Params = param,
                Kind = 2
            };

            var body = normalBodyFactory.CreateDynamicCircle(
                new System.Numerics.Vector2(X, Y),
                radius, param
                );

            return new EntityWrap(entity, body) { dynamic = true, clientRefresh = clientRefresh };
        }

        public EntityWrap CreateStaticBox(int X, int Y, System.Numerics.Vector2 size, string param, bool clientRefresh = false)
        {
            var entity = new Entity()
            {
                Id = ++id,
                PositionX = X,
                PositionY = Y,
                SizeX = (int)size.X,
                SizeY = (int)size.Y,
                Params = param,
                Kind = 1
            };

            var body = normalBodyFactory.CreateStaticBox(
                new System.Numerics.Vector2(X, Y),
                size, param
                );

            return new EntityWrap(entity, body) { clientRefresh = clientRefresh };
        }

        public EntityWrap CreateDynamicBox(int X, int Y, System.Numerics.Vector2 size, string param, bool clientRefresh = false)
        {
            var entity = new Entity()
            {
                Id = ++id,
                PositionX = X,
                PositionY = Y,
                SizeX = (int)size.X,
                SizeY = (int)size.Y,
                Params = param,
                Kind = 1
            };

            var body = normalBodyFactory.CreateDynamicBox(
                new System.Numerics.Vector2(X, Y),
                size,false, param
                );

            return new EntityWrap(entity, body) { dynamic = true, clientRefresh = clientRefresh };
        }

        public EntityWrap CreateStaticBoxSensor(int X, int Y, System.Numerics.Vector2 size, string param, bool clientRefresh = false)
        {
            var entity = new Entity()
            {
                Id = ++id,
                PositionX = X,
                PositionY = Y,
                SizeX = (int)size.X,
                SizeY = (int)size.Y,
                Params = param,
                Kind = 1
            };

            var body = normalBodyFactory.CreateStaticBoxSensor(
                new System.Numerics.Vector2(X, Y),
                size, param
                );
            
            return new EntityWrap(entity, body) { clientRefresh = clientRefresh };
        }

        public RevoluteJoint CreateRevoluteJointJoint(EntityWrap entityA, EntityWrap entityB) => normalBodyFactory.CreateRevoluteJointJoint(entityA.body, entityB.body);
        public void CreateDistanceJointJoint(EntityWrap entityA, EntityWrap entityB) => normalBodyFactory.CreateDistanceJoint(entityA.body, entityB.body);
    }
}
