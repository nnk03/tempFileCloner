using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using FileCloner.Models.Networking;
using FileCloner.Models.ChatMessaging;
using System.Windows;

namespace FileCloner.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly ICommunicator _communicator;    // Communicator used to send and receive messages.
        private readonly ChatMessenger _chatMessenger;   // Messenger used to send and receive chat messages.
        private string _summary = String.Empty;

        public ObservableCollection<string> MessagesSent { get; set; } = new();
        public ObservableCollection<string> MessagesReceived { get; set; } = new();

        public ICommand SendRequestCommand { get; }
        public ICommand SummarizeCommand { get; }
        public ICommand SendSummaryCommand { get; }
        public ICommand StartCloningCommand { get; }

        /// <summary>
        /// Creates an instance of the main page viewmodel.
        /// </summary>
        /// <param name="communicator">Optional instance of communicator</param>
        public MainPageViewModel(ICommunicator? communicator = null)
        {
            _communicator = communicator ?? CommunicatorFactory.CreateCommunicator();

            // Update the port that the communicator is listening on.
            ReceivePort = _communicator.ListenPort.ToString();
            OnPropertyChanged(nameof(ReceivePort));

            // Create an instance of the chat messenger and signup for callback.
            _chatMessenger = new(_communicator);
            _chatMessenger.OnChatMessageReceived += delegate (string message)
            {
                // UI element update needs to happen on the UI thread, and this callback is
                // likely run on a worker thread. However we do not need to explicitly
                // dispatch to the UI thread here because OnPropertyChanged event is
                // automatically marshaled to the UI thread.
                Dispatcher.Invoke(() =>
                {
                    MessagesReceived.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                    OnPropertyChanged(nameof(MessagesReceived));
                });
            };

            SendRequestCommand = new RelayCommand(SendRequest);
            SummarizeCommand = new RelayCommand(SummarizeResponses);
            SendSummaryCommand = new RelayCommand(SendSummary);
            StartCloningCommand = new RelayCommand(StartCloning);
        }

        private void SendRequest()
        {
            string requestMessage = "<Request>";
            List<string> activeClientIPAddresses = _communicator.GetAllActiveClientIPAddresses();
            foreach (string clientIP in activeClientIPAddresses)
            {
                _chatMessenger.SendMessage(clientIP, int.Parse(ReceivePort), requestMessage);
                MessagesSent.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {clientIP} $ {requestMessage}");
                OnPropertyChanged(nameof(MessagesReceived));
            }
        }

        private void SummarizeResponses()
        {
            // Evans work comes here
            _summary = "<Summary>";
            Dispatcher.Invoke(() => {
                MessageBox.Show("Summary is generated");
            });
        }

        private void SendSummary()
        {
            // For now this is fine, but summary should only be sent to responders
            List<string> activeClientIPAddresses = _communicator.GetAllActiveClientIPAddresses();
            foreach (string clientIP in activeClientIPAddresses)
            {
                _chatMessenger.SendMessage(clientIP, int.Parse(ReceivePort), _summary);
                MessagesSent.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {clientIP} $ {_summary}");
                OnPropertyChanged(nameof(MessagesReceived));
            }
        }

        private void StartCloning()
        {
            Dispatcher.Invoke(() => {
                MessageBox.Show("File Cloning Started");
            });
        }

        /// <summary>
        /// Gets the port for receiving messages.
        /// </summary>
        public string? ReceivePort { get; private set; }
    }
}
