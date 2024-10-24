using FileCloner.Models.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.Models.ChatMessaging
{
    /// <summary>
    /// Handler for chat messages.
    /// </summary>
    /// <param name="message">received message</param>
    public delegate void ChatMessageReceived(string message);

    public class ChatMessenger: IMessageListener
    {
        private readonly ICommunicator _communicator;

        /// <summary>
        /// Creates an instance of the chat messenger.
        /// </summary>
        /// <param name="communicator">The communicator instance to use</param>
        public ChatMessenger(ICommunicator communicator)
        {
            _communicator = communicator;
            communicator.AddSubscriber("ChatMessenger", this);
        }

        /// <summary>
        /// Sends the given message to the given ip and port.
        /// </summary>
        /// <param name="ipAddress">IP address of the destination</param>
        /// <param name="port">Port of the destination</param>
        /// <param name="message">Message to be sent</param>
        public void SendMessage(string ipAddress, int port, string message)
        {
            _communicator.SendMessage(ipAddress, port, "ChatMessenger", message);
        }

        /// <summary>
        /// Event for handling received chat messages.
        /// </summary>
        public event ChatMessageReceived? OnChatMessageReceived;

        /// <inheritdoc />
        public void OnMessageReceived(string message)
        {
            OnChatMessageReceived?.Invoke(message);
        }
    }
}
