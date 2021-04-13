using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.States
{
    public abstract class State : IState
    {
        protected StateHandler _stateHandler;
        public abstract void HandleState();
        public abstract void LoadContent();
        public abstract void SetToGameState();
        public abstract void SetToLobbyState();
        public void SetStateHandler(StateHandler stateHandler) => _stateHandler = stateHandler;
    }
}
