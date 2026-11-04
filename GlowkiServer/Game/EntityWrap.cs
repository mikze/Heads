using Box2D.NetStandard.Dynamics.Bodies;

namespace GlowkiServer.Game
{
    /// <summary>Rodzaj obiektu, po którym klient wybiera grafikę.</summary>
    public enum EntityKind
    {
        Box = 1,
        Ball = 2,
        Player = 3,
        Bonus = 4
    }

    /// <summary>Opis obiektu wysyłany klientowi na starcie meczu (pozycje i rozmiary w pikselach).</summary>
    public class Entity
    {
        public int Id { get; set; }
        public int Kind { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public string Params { get; set; }
    }

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
