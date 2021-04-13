using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.States
{
    public enum States
    {
        LobbyState,
        GameState
    }
    public class StateHandler : IState
    {
        private State _state = null;

        public States state;
        public StateHandler(State state)
        {
            ChangeScene(state);
        }

        public void ChangeScene(State state)
        {
            _state = state;
            _state.SetStateHandler(this);
            LoadContent();
        }
        public void LoadContent() => _state.LoadContent();
        public void HandleState() => _state.HandleState();
        public void SetToGameState() => _state.SetToGameState();
        public void SetToLobbyState() => _state.SetToLobbyState();
    }
}
