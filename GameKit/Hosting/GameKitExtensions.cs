using GameKit.Rooms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameKit.Hosting
{
    public static class GameKitExtensions
    {
        /// <summary>Pokoje, lobby i hub SignalR. Gry dodaje się osobno przez AddGame.</summary>
        public static IServiceCollection AddGameKit(this IServiceCollection services)
        {
            services.AddSignalR();
            services.TryAddSingleton<RoomManager>();
            return services;
        }

        /// <summary>Rejestruje grę. Pierwsza dodana jest domyślna.</summary>
        public static IServiceCollection AddGame<T>(this IServiceCollection services)
            where T : class, IGameDefinition
        {
            services.AddSingleton<IGameDefinition, T>();
            return services;
        }

        public static HubEndpointConventionBuilder MapGameKit(this IEndpointRouteBuilder endpoints, string path = "/game") =>
            endpoints.MapHub<GameHub>(path);
    }
}
