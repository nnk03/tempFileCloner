using FileCloner.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;

namespace FileCloner.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private LocalServer server;
        private Thread serverThread;

        public ObservableCollection<string> MessagesSent { get; set; } = new();
        public ObservableCollection<string> MessagesReceived { get; set; } = new();
        private bool isServerRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand StartServerCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand SummarizeCommand { get; }
        public ICommand SendSummaryCommand { get; }

        public MainPageViewModel()
        {
            server = new LocalServer();
            serverThread = new Thread(() =>
            {
                server.GetAllIPAddresses(); 
            });
            StartServerCommand = new RelayCommand(StartServer, () => !isServerRunning);
            SendRequestCommand = new RelayCommand(SendRequest, () => isServerRunning);
            SummarizeCommand = new RelayCommand(SummarizeResponses, () => isServerRunning);
            SendSummaryCommand = new RelayCommand(SendSummary, () => isServerRunning);
        }

        private void StartServer()
        {
            serverThread.IsBackground = true;
            serverThread.Start();
            isServerRunning = true;
            NotifyPropertyChanged(nameof(isServerRunning));
        }

        private void SendRequest()
        {
            server.SendRequest();
            AddMessageToLog(MessagesSent, "localhost", "<Request>");
        }

        private void SummarizeResponses()
        {
            server.SummarizeResponses();
        }

        private void SendSummary()
        {
            server.SendSummary();
            AddMessageToLog(MessagesSent, "localhost", "<Summary of JSONs>");
        }

        private void AddMessageToLog(ObservableCollection<string> log, string clientIp, string message)
        {
            log.Add($"{clientIp}: {message}");
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // RelayCommand to implement ICommand
    public class RelayCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
