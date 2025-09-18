using ChatServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        private PacketReader packetReader { get; set; }

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            packetReader = new PacketReader(ClientSocket.GetStream());
            var opcode = packetReader.ReadByte();
            Username = packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client {Username} has connected.");
            Task.Run(() => Process());
        }

        private void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message Received! {msg}");
                            Program.BroadcastMessage($"[{DateTime.Now}][{Username}]: " + msg);
                            break;

                        default:
                            throw new Exception();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID.ToString()}]: Dissconected!");
                    ClientSocket.Close();
                    Program.BroadcastDisconnect(UID.ToString());
                    break;
                }
            }
        }
    }
}
