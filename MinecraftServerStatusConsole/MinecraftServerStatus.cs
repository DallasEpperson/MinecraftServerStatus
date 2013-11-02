using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace MinecraftServerStatusConsole
{
    class MinecraftServerStatus
    {
        public Boolean Online = false;
        public int PlayersOnline;
        public int PlayersMax;
        public string MOTD;

        private byte[] RequestByteArray = new byte[] { 0xFE };
        private const byte InitialByte = (byte)0xFF;
        private const byte SplitByte = (byte)0xA7;

        public MinecraftServerStatus(string ServerLocation, int ServerPort = 25565)
        {
            var tcpClient = new TcpClient();
            Byte[] recv = new Byte[2048];

            try
            {
                IAsyncResult result = tcpClient.BeginConnect(ServerLocation, ServerPort, null, null);

                result.AsyncWaitHandle.WaitOne(3000, true);

                if (!tcpClient.Connected)
                {
                    return;
                }

                using (var networkStream = tcpClient.GetStream())
                {
                    networkStream.Write(RequestByteArray, 0, RequestByteArray.Length);
                    networkStream.Read(recv, 0, recv.Length);
                }

                //we expect this to start with 0xFF and contain two 0xA7
                var validResponse = (recv[0] == InitialByte) && (recv.Count(b => b == SplitByte) == 2);
                if (!validResponse)
                {
                    return;
                }

                Online = true;

                int positionOfFirstSplitByte = Array.IndexOf(recv, SplitByte);
                int positionOfSecondSplitByte = Array.IndexOf(recv, SplitByte, positionOfFirstSplitByte + 1);

                var MOTDArray = new byte[((recv.Length - 4) - (recv.Length - positionOfFirstSplitByte))];
                var PlayersOnlineArray = new byte[(positionOfSecondSplitByte - positionOfFirstSplitByte) - 2];
                var PlayersMaxArray = new byte[(recv.Length - positionOfSecondSplitByte) - 2];

                Buffer.BlockCopy(recv, 4, MOTDArray, 0, (recv.Length - 4) - (recv.Length - positionOfFirstSplitByte));
                Buffer.BlockCopy(recv, positionOfFirstSplitByte + 2, PlayersOnlineArray, 0, PlayersOnlineArray.Length);
                Buffer.BlockCopy(recv, positionOfSecondSplitByte + 2, PlayersMaxArray, 0, PlayersMaxArray.Length);

                MOTD = Encoding.UTF8.GetString(MOTDArray).Replace("\0", string.Empty);
                PlayersOnline = int.Parse(Encoding.UTF8.GetString(PlayersOnlineArray).Replace("\0", string.Empty));
                PlayersMax = int.Parse(Encoding.UTF8.GetString(PlayersMaxArray).Replace("\0", string.Empty));
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
