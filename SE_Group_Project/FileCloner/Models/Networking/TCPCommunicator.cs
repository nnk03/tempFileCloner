using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileCloner.Models.Networking
{
    public class TCPCommunicator : ICommunicator
    {
        private readonly TcpListener _listener;     // TCP listener to accept connections.
        private readonly Thread _listenThread;      // Thread that listens for messages on the TCP port.
        private readonly Dictionary<string, IMessageListener> _subscribers; // List of subscribers.
        private string myIP = "localhost";

        /// <inheritdoc />
        public int ListenPort { get; private set; }

        public TCPCommunicator(int listenPort)
        {
            _subscribers = new Dictionary<string, IMessageListener>();

            // Create and start the thread that listens for messages.
            ListenPort = listenPort;
            _listener = new TcpListener(IPAddress.Any, listenPort);
            _listener.Start();  // Start listening for TCP connections.
            _listenThread = new(new ThreadStart(ListenerThreadProc))
            {
                IsBackground = true // Stop the thread when the main thread stops.
            };
            _listenThread.Start();
        }

        /// <inheritdoc />
        public void AddSubscriber(string id, IMessageListener subscriber)
        {
            Debug.Assert(!string.IsNullOrEmpty(id));
            Debug.Assert(subscriber != null);

            lock (this)
            {
                if (_subscribers.ContainsKey(id))
                {
                    _subscribers[id] = subscriber;
                }
                else
                {
                    _subscribers.Add(id, subscriber);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveSubscriber(string id)
        {
            Debug.Assert(!string.IsNullOrEmpty(id));

            lock (this)
            {
                if (_subscribers.ContainsKey(id))
                {
                    _subscribers.Remove(id);
                }
            }
        }

        /// <inheritdoc/>
        public void SendMessage(string ipAddress, int port, string senderId, string message)
        {
            try
            {
                using TcpClient client = new TcpClient(ipAddress, port);  // Establish a TCP connection.
                byte[] sendBuffer = Encoding.ASCII.GetBytes($"{senderId}: {myIP}${message}");

                // Get the network stream and send the message.
                NetworkStream stream = client.GetStream();
                stream.Write(sendBuffer, 0, sendBuffer.Length);
                Debug.WriteLine($"Sent message: {senderId}: {myIP}${message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error sending message: {e.Message}");
            }
        }

        /// <summary>
        /// Listens for incoming TCP connections and messages.
        /// </summary>
        private void ListenerThreadProc()
        {
            Debug.WriteLine($"Listener Thread Id = {Environment.CurrentManagedThreadId}.");

            while (true)
            {
                try
                {
                    // Accept a TCP client connection.
                    using TcpClient client = _listener.AcceptTcpClient();

                    // for finding the ip address of the accepted connectoin
                    IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    string clientIPAddress = clientEndPoint.Address.ToString();
                    int clientPort = clientEndPoint.Port;
                    Debug.WriteLine($"Received Connection from {clientIPAddress}:{clientPort}");

                    NetworkStream stream = client.GetStream();

                    // Read the incoming message.
                    byte[] buffer = new byte[1024]; // Adjust buffer size as needed.
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string payload = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Debug.WriteLine($"Received payload: {payload}");

                    // The received payload is expected to be in the format <Identity>:<Message>
                    string[] tokens = payload.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 2)
                    {
                        string id = tokens[0];
                        string message = tokens[1];
                        Debug.WriteLine($"Message Received is : {message}");
                        lock (this)
                        {
                            if (_subscribers.ContainsKey(id))
                            {
                                if (message.Contains("<Request>"))
                                {
                                    Thread acceptThread = new(AcceptRequest);
                                    acceptThread.Start();
                                }
                                else if (message.Contains("<Summary>"))
                                {
                                    Thread receiveThread = new(ReceiveSummary);
                                    receiveThread.Start();
                                }
                                _subscribers[id].OnMessageReceived(message);
                            }
                            else
                            {
                                Debug.WriteLine($"Received message for unknown subscriber: {id}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error in listener thread: {e.Message}");
                }
            }
        }

        // Retrieves the IP addresses of all connected clients
        public List<string> GetAllActiveClientIPAddresses()
        {
            List<string> activeClients = new List<string>();
            // activeClients.Add(myIP);
            activeClients.Add("10.32.16.142");
            return activeClients;
        }

        public void AcceptRequest()
        {
            string message = "<Acceptance & a JSON file>";
            SendMessage("10.32.16.142", ListenPort, "ChatMessenger", message);
        }

        public void ReceiveSummary()
        {
            string message = "<Files for cloning>";
            SendMessage("localhost", ListenPort, "ChatMessenger", message);
        }
    }
}
