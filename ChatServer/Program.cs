
using ChatServer.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        private static List<Client> users = new List<Client>();
        private static TcpListener listener;

        public static void Main(string[] args)
        {
            users = new List<Client>(); 
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            listener.Start();

            while (true)
            {
                var client = new Client(listener.AcceptTcpClient());
                users.Add(client);

                Console.WriteLine("Client has connected.");
                BroadcastConnection();
            }
        }

        private static void BroadcastConnection()
        {
            foreach(var user in users)
            {
                foreach (var usr in users)
                {
                    var broadCastPacket = new PacketBuilder();
                    broadCastPacket.WriteOpCode(1);
                    broadCastPacket.WriteMessage(usr.Username);
                    broadCastPacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadCastPacket.GetPacketBytes());
                }
            }
        }

        public static void BroadcastMessage(string message)
        {

            foreach (var user in users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }
        public static void BroadcastDisconnect(string uid)
        {
            var dissconectedUser = users.Where(user => user.UID.Equals(uid)).FirstOrDefault();
            users.Remove(dissconectedUser);

            foreach (var user in users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }

            BroadcastMessage($"[{dissconectedUser.Username}] Disconnected!");
        }
    }
}