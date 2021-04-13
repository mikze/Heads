using SimpleUDP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.UDP
{
    public class UDPServer<t>
    {
        public Host<t> host;
        public UDPServer(int port)
        {
            host = new Host<t>(port);

            host.OnClientAdded = a =>
            {
            };

            host.RecieveHandler = (frame, id) =>
            {
            };

            new TaskFactory().StartNew(() => host.Listen());          
        }

        public void BroadCast(t frame) => host.BroadCast(frame);
    }
}
