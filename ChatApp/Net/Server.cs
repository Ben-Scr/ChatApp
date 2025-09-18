using System.Net.Sockets;
using ChatClient.Net.IO;

namespace ChatClient.Net
{
    class Server
    {
        public PacketReader PacketReader;
        public event Action ConnectedEvent;
        public event Action MsgReceivedEvent;
        public event Action UserDisconnectedEvent;

        private TcpClient client;

        public Server()
        {
            client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (client.Connected) return;

            client.Connect("127.0.0.1", 7891);
            PacketReader = new PacketReader(client.GetStream());

            if (!string.IsNullOrEmpty(username))
            {
                var connectPacket = new PacketBuilder();
                connectPacket.WriteOpCode(0);
                connectPacket.WriteMessage(username);
                client.Client.Send(connectPacket.GetPacketBytes());
            }
            ReadPackets();
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();

                    switch (opcode)
                    {
                        case 1:
                            ConnectedEvent?.Invoke();
                            break;

                        case 5:
                            MsgReceivedEvent?.Invoke();
                            break;

                        case 10:
                            UserDisconnectedEvent?.Invoke();
                            break;
                        default:
                            throw new Exception();
                    }
                }
            });
        }

        public void SendMessageToServer(string msg)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(msg);
            client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
