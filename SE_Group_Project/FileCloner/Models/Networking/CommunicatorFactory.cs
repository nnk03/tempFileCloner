using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.Models.Networking
{
    public class CommunicatorFactory
    {
        public static ICommunicator CreateCommunicator()
        {
            // Please note that this can throw if the port is already in use.
            int port = 8080;
            Debug.WriteLine($"Starting communicator in port {port}");
            return new TCPCommunicator(port);
        }
    }
}
