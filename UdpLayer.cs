using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Meow.NetLinker
{
    public class UdpLayer : Layer
    {
        private bool _isRunning;
        private Socket _udpSocket;
        private IPEndPoint _remoteEndPoint;
        private IPEndPoint _localEndPoint;
        private Thread _threadRecv;
        private readonly byte[] _recvBufferTemp = new byte[4096];

        private readonly SwitchQueue<byte[]> _recvQueue = new SwitchQueue<byte[]>(128);

        public UdpLayer()
        {
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Connect(string ip, int remotePort, int localBindPort)
        {
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), remotePort);
            _localEndPoint = new IPEndPoint(IPAddress.Any, localBindPort);
            _udpSocket.Bind(_localEndPoint);

            _isRunning = true;
            _threadRecv = new Thread(Thread_Recv) {IsBackground = true};
            _threadRecv.Start();
        }


        public override void Dispose()
        {
            _isRunning = false;

            if (_threadRecv != null)
            {
                _threadRecv.Interrupt();
                _threadRecv = null;
            }

            if (_udpSocket != null)
            {
                _udpSocket.Close();
                _udpSocket = null;
            }
        }

        public override void Send(DataPack dataPack)
        {
            var bytes = dataPack.ReadAllBytes();
            DataPackPool.Recycle(dataPack);
            _udpSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, _remoteEndPoint);
        }

        public override void Input(DataPack dataPack)
        {
            throw new NotImplementedException("UdpLayer do not receive input from another layer");
        }

        public override void Update()
        {
            if (_isRunning)
            {
                HandleRecvQueue();
            }
        }

        private void Thread_Recv()
        {

            while (_isRunning)
            {
                try
                {
                    if (_udpSocket.Available <= 0)
                    {
                        continue;
                    }

                    EndPoint remotePoint = _remoteEndPoint;
                    int cnt = _udpSocket.ReceiveFrom(_recvBufferTemp, _recvBufferTemp.Length,
                        SocketFlags.None, ref remotePoint);

                    if (cnt > 0)
                    {
                        byte[] dst = new byte[cnt];
                        Buffer.BlockCopy(_recvBufferTemp, 0, dst, 0, cnt);
                        _recvQueue.Push(dst);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

        }

        private void HandleRecvQueue()
        {
            _recvQueue.Switch();
            while (!_recvQueue.Empty())
            {
                byte[] recvBufferRaw = _recvQueue.Pop();

                var dataPack = DataPackPool.GetDataPack(0, recvBufferRaw);
                dataPack.Position = 0;

                Recv(dataPack);
            }
        }
    }
}