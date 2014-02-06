using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;

namespace MinecraftServerStatusConsole
{
    class MinecraftServerStatus
    {
        public Boolean Online = false;
        public int PlayersOnline = -1;
        public int PlayersMax = -1;
        public int ProtocolVersion = -1;
        public string ServerVersion;
        public string MOTD;

        public MinecraftServerStatus(string ServerLocation, int ServerPort = 25565)
        {
            var tcpClient = new TcpClient();
            Byte[] recv = new Byte[2048];

            try
            {
                tcpClient.Connect(ServerLocation, ServerPort);

                if (!tcpClient.Connected)
                {
                    // This port isn't even open
                    return;
                }

                // some servers (bukkit?) seem to respond to this protocol
                List<byte> requestBytes = new List<byte>();
                requestBytes.Add(0xFE);
                requestBytes.Add(0x01);
                requestBytes.Add(0xFA);
                requestBytes.AddRange(Encoding.Unicode.GetBytes("MC|PingHost").ToArray());
                requestBytes.Add((byte)((ServerLocation.Length * 2) + 7));
                requestBytes.Add((byte)78);
                requestBytes.AddRange(Encoding.Unicode.GetBytes(ServerLocation).ToArray());
                requestBytes.AddRange(BitConverter.GetBytes(ServerPort).Reverse());

                using (var networkStream = tcpClient.GetStream())
                {
                    networkStream.Write(requestBytes.ToArray(), 0, requestBytes.ToArray().Length);
                    networkStream.Read(recv, 0, recv.Length);
                }

                if (recv[4] == 0xA7 && recv[6] == 0x31 && recv[8] == 0x00)
                {
                    var responsePieces = Encoding.Unicode.GetString(recv).Split('\0');
                    Online = true;
                    ProtocolVersion = int.Parse(responsePieces[1]);
                    ServerVersion = responsePieces[2];
                    MOTD = responsePieces[3];
                    PlayersOnline = int.Parse(responsePieces[4]);
                    PlayersMax = int.Parse(responsePieces[5]);

                    tcpClient.Close();
                    return;
                }


                // Try other protocol

                // my vanilla server responds to this protocol
                tcpClient = new TcpClient();
                requestBytes = new List<byte>();
                requestBytes.Add(0xFE);
                requestBytes.Add(0x01);

                if (!tcpClient.Connected)
                {
                    tcpClient.Connect(ServerLocation, ServerPort);
                }

                using (var networkStream = tcpClient.GetStream())
                {
                    networkStream.Write(requestBytes.ToArray(), 0, requestBytes.ToArray().Length);
                    networkStream.Read(recv, 0, recv.Length);
                }

                if (recv[4] == 0xA7 && recv[6] == 0x31 && recv[8] == 0x00)
                {
                    var sresponsePieces = Encoding.Unicode.GetString(recv).Split('\0');
                    Online = true;
                    ProtocolVersion = int.Parse(sresponsePieces[1]);
                    ServerVersion = sresponsePieces[2];
                    MOTD = sresponsePieces[3];
                    PlayersOnline = int.Parse(sresponsePieces[4]);
                    PlayersMax = int.Parse(sresponsePieces[5]);
                    tcpClient.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error retrieving server status!");
                Debug.WriteLine(e.Message);
                throw (e);
            }
        }
    }
}
