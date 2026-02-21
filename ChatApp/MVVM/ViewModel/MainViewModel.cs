using ChatClient.MVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<ChatMessageModel> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }

        private readonly Server server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<ChatMessageModel>();

            server = new Server();
            server.ConnectedEvent += UserConnected;
            server.MsgReceivedEvent += MessageReceived;
            server.UserDisconnectedEvent += UserDisconnected;

            ConnectToServerCommand = new RelayCommand(o => server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o => server.SendMessageToServer(Message), o => !string.IsNullOrEmpty(Message));
        }

        public void UserConnected()
        {
            var user = new UserModel { UserName = server.PacketReader.ReadMessage(), UID = server.PacketReader.ReadMessage() };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        public void MessageReceived()
        {
            var msg = server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new ChatMessageModel
                {
                    Content = msg,
                    IsOwnMessage = IsMessageFromCurrentUser(msg)
                });
            });
        }

        public void UserDisconnected()
        {
            var uid = server.PacketReader.ReadMessage();
            var user = Users.Where(user => user.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private bool IsMessageFromCurrentUser(string message)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                return false;
            }

            var match = Regex.Match(message, @"\[[^\]]+\]\[(?<username>[^\]]+)\]:");
            return match.Success && string.Equals(match.Groups["username"].Value, Username, StringComparison.Ordinal);
        }
    }
}