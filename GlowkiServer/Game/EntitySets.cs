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
            entityFactory.CreateDynamicCircle(397, 435, 10f, "Ball", true),
            entityFactory.CreateStaticBox(300, 50, new Vector2(1000, 30), ""),
            entityFactory.CreateStaticBox(300, 450, new Vector2(1000, 30), "floor"),
            entityFactory.CreateStaticBox(10, 350, new Vector2(10, 600), ""),
            entityFactory.CreateStaticBox(790, 350, new Vector2(10, 600), ""),
            entityFactory.CreateStaticBox(45, 325, new Vector2(100, 10), ""),
            entityFactory.CreateStaticBox(745, 325, new Vector2(100, 10), ""),
            entityFactory.CreateStaticBoxSensor(730, 330, new Vector2(30, 160), "EnemyGoal"),
            entityFactory.CreateStaticBoxSensor(55, 330, new Vector2(30, 160), "PlayerGoal")
            };
        }
    }
}
