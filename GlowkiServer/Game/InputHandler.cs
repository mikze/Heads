using GlowkiServer.UDP;
using System;
using System.Threading.Tasks;

namespace GlowkiServer.Game
{
    [Flags]
    public enum Input
    {
        none = 1,
        up = 2,
        right = 4,
        left = 8,
        kick = 16,
        kickUp = 32,
        enemy = 64
    }
    public class InputHandler
    {
        UDPServer<byte> uDPServer;
        public Action<int, int> ReciveHandler;
        public InputHandler()
        {
            uDPServer = new UDPServer<byte>(1338);
            uDPServer.host.RecieveHandler += InputHandle;
            new TaskFactory().StartNew(() => uDPServer.host.Listen());
        }

        private void InputHandle(byte input, int clientId) =>
            ReciveHandler?.Invoke(input, clientId);
    }
}
