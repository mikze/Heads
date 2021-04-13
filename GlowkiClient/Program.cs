using GlowkiServer;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace GlowkiClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var input = new EntitiesRequest();
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new GlowkiService.GlowkiServiceClient(channel);

            var t = client.GetEntities(input);

            await foreach(var e in t.ResponseStream.ReadAllAsync())
                Console.WriteLine($"Kind:{e.Kind}  Params:{e.Params}");
        }
    }
}
