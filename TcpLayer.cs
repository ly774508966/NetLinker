using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Meow.NetLinker
{
    public class TcpLayer : Layer
    {
        private readonly SwitchQueue<byte[]> _recvQueue = new SwitchQueue<byte[]>(128);

        // ReSharper disable once InconsistentNaming
        private const int MAX_READ = 8192;
        private readonly byte[] _recvBufferTemp = new byte[MAX_READ];
         
        private readonly TcpClient _tcpClient;
        private IPEndPoint _localEndPoint;
        private NetworkStream _stream;
        private Thread _threadRecv;

        private bool _isRunning;


        public TcpLayer()
        {
            _tcpClient = new TcpClient();
        }
        
        public void Connect(string remoteIp, int remotePort, Action<bool> callback)
        {
            _localEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
            _tcpClient.BeginConnect(_localEndPoint.Address, _localEndPoint.Port, ar =>
            {
                callback.Invoke(_tcpClient.Connected);

                if (_tcpClient.Connected)
                {
                    _threadRecv = new Thread(ThreadRecv) {IsBackground = true};
                    _threadRecv.Start();
                }
            }, _tcpClient);
            _isRunning = true;
            
        }
        
        public override void Send(DataPack dataPack)
        {
            var bytes = dataPack.ReadAllBytes();
            DataPackPool.Recycle(dataPack);
            if (_stream == null)
            {
                _stream = _tcpClient.GetStream();
            }
            _stream.Write(bytes, 0, bytes.Length);
            
        }
        
        public override void Update()
        {
            HandleRecvQueue();
        }

        public override void Input(DataPack dataPack)
        {
            throw new NotImplementedException("TcpLayer do not receive input from another layer");
        }

        public override void Dispose()
        {
            _isRunning = false;

            if (_threadRecv != null)
            {
                _threadRecv.Abort();
                _threadRecv = null;
            }

            _tcpClient.Close();
        }
        
        private void ThreadRecv()
        {
            while (_isRunning)
            {
                try
                {
                    
                    if (_tcpClient.Available <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    int bytesRead = _tcpClient.GetStream().Read(_recvBufferTemp, 0, MAX_READ);
                    if (bytesRead > 0)
                    {
                        byte[] dst = new byte[bytesRead];
                        Buffer.BlockCopy(_recvBufferTemp, 0, dst, 0, bytesRead);
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