using Box2D.NetStandard.Dynamics.Bodies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.Game
{
    public class EntityWrap
    {
        public bool dynamic = false;
        public bool clientRefresh = false;
        public Entity entity { get; private set; }
        public Body body { get; private set; }

        public EntityWrap(Entity entity, Body body)
        {
            this.entity = entity;
            this.body = body;
        }
    }
}
