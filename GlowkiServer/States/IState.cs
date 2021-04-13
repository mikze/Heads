using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.States
{
    public interface IState
    {
        public void LoadContent();
        public void HandleState();
        public void SetToGameState();
        public void SetToLobbyState();
    }
}
