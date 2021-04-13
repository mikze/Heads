using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GlowkiServer
{
    public class GreeterService : GlowkiService.GlowkiServiceBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task GetEntities(EntitiesRequest request, IServerStreamWriter<Entity> responseStream, ServerCallContext context)
        {                  
            foreach(var e in Game.Game.Entities)
                await responseStream.WriteAsync(e.entity);
        }
        public override Task<State> SetState(State request, ServerCallContext context)
        {
            var s = (States.States)request.State_;
            if (s == States.States.GameState)
                Game.Game.stateHandler.SetToGameState();
            if (s == States.States.LobbyState)
                Game.Game.stateHandler.SetToLobbyState();

            return Task.FromResult(new State() { State_ = (int)Game.Game.stateHandler.state});
        }
    }
}
