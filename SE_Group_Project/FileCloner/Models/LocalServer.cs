using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace FileCloner.Models
{
    public class LocalServer
    {
        private TcpListener listener;
        private const int port = 12345;
        private string summary = string.Empty;
        private List<string> activeClients = new();
        private List<TcpClient> responders = new();

        // Future: List<Files>
        private List<string> responderJSONs = new();
        private List<string> filesForCloning = new();

        public LocalServer()
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine($"Server started on port {port}...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();

            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int byteCount = stream.Read(buffer, 0, buffer.Length);

                    if (byteCount == 0)
                    {
                        break;
                    }

                    string message = Encoding.ASCII.GetString(buffer, 0, byteCount);

                    switch (message)
                    {
                        case "<Request>":
                            AcceptRequest(client);
                            break;
                        case "<Acceptance & a JSON file>":
                            HandleAcceptance(message);
                            break;
                        case "<Summary of JSONs>":
                            ReceiveSummary(client);
                            break;
                        case "<Files for cloning>":
                            HandleCloning(message);
                            break;
                        default:
                            Console.WriteLine($"Unknown message from client: {message}");
                            break;
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading from client: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected: " + clientEndPoint);
            }
        }

        // Retrieves the IP addresses of all connected clients
        public void GetAllIPAddresses()
        {
            activeClients.Add("localhost");
            Console.WriteLine("Retrieved client IP addresses from remote server.");
        }

        public void SendRequest()
        {
            string message = "<Request>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            foreach (string clientIp in activeClients)
            {
                try
                {
                    TcpClient client = new TcpClient(clientIp, port);
                    NetworkStream stream = client.GetStream();
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine($"Sent clone request: {message} to client: {clientIp}");
                    stream.Close();
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to send clone request to {clientIp}: {e.Message}");
                }
            }
        }

        public void AcceptRequest(TcpClient client)
        {
            string message = "<Acceptance & a JSON file>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            try
            {
                NetworkStream stream = client.GetStream();
                stream.Write(messageBytes, 0, messageBytes.Length);
                Console.WriteLine($"Sent acceptance : {message} to requester: {((IPEndPoint)client.Client.RemoteEndPoint)?.Address}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to accept clone request : " + e.Message);
            }
        }

        public void HandleAcceptance(string message)
        {
            responderJSONs.Add(message);
        }

        public void SummarizeResponses()
        {
            summary = "<Summary of JSONs>";
        }

        public void SendSummary()
        {
            byte[] summaryBytes = Encoding.UTF8.GetBytes(summary);

            foreach (var client in responders)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(summaryBytes, 0, summaryBytes.Length);
                Console.WriteLine($"Sent summary to client: {((IPEndPoint)client.Client.RemoteEndPoint)?.Address}");
            }
        }

        public void ReceiveSummary(TcpClient client)
        {
            byte[] summaryBytes = Encoding.UTF8.GetBytes("<Files for cloning>");
            NetworkStream stream = client.GetStream();
            stream.Write(summaryBytes, 0, summaryBytes.Length);
            Console.WriteLine($"Sent summary to requester: {((IPEndPoint)client.Client.RemoteEndPoint)?.Address}");
        }

        public void HandleCloning(string message)
        {
            filesForCloning.Add(message);
        }
    }
}