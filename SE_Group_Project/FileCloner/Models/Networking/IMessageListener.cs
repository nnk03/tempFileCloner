using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.Models.Networking
{
    public interface IMessageListener
    {
        /// <summary>
        /// Handles reception of a message.
        /// </summary>
        /// <param name="message">Message that is received</param>
        void OnMessageReceived(string message);
    }
}
