using GlowkiServer.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlowkiServer.Game
{
    public static class Game
    {
        public static CancellationToken cancellationToken;
        public static StateHandler stateHandler;
        public static bool IsLive;
        public static void StartGameLoop()
        {
            stateHandler = new StateHandler(new LobbyState());
            cancellationToken = new CancellationToken();
            Task.Run(() => GameLoop(), cancellationToken);
        }

        private static void GameLoop()
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                stateHandler.HandleState();
            }
        }

        public static List<EntityWrap> Entities { get; set; }
    }
}
