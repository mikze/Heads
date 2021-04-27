using Box2D.NetStandard.Dynamics.World;
using GlowkiServer.B2DWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GlowkiServer.Game
{
    public static class EntitySets
    {
        public static List<EntityWrap> GameStateSet(EntityFactory entityFactory)
        {
            return new List<EntityWrap>() {
            entityFactory.CreateStaticBox(512, 50, new Vector2(1024, 30), ""),
            entityFactory.CreateStaticBox(512, 800, new Vector2(1024, 30), "floor"),
            entityFactory.CreateStaticBox(1, 400, new Vector2(10, 800), "wall1"),
            entityFactory.CreateStaticBox(1023, 400, new Vector2(10, 800), "wall2"),
            entityFactory.CreateStaticBox(56, 593, new Vector2(100, 10), "PlayerGoalTop"),
            entityFactory.CreateStaticBox(968, 593, new Vector2(100, 10), "EnemyGoalTop"),
            entityFactory.CreateStaticBoxSensor(937, 799, new Vector2(30, 410), "EnemyGoal"),
            entityFactory.CreateStaticBoxSensor(87, 799, new Vector2(30, 410), "PlayerGoal")
            };
        }
    }
}
