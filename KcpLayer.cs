using UnityEngine;

namespace Meow.NetLinker
{
    public class KcpLayer : Layer
    {
        private readonly KCP _kcp;
        
        public KcpLayer(uint key)
        {
            _kcp = new KCP(key, (bytes, i) =>
            {
                var dataPack = DataPackPool.GetDataPack(0, bytes, i);
                dataPack.Position = 0;
                OutPut(dataPack);
            });
            //_kcp.NoDelay(1, 10, 2, 1);
            //_kcp.WndSize(128, 128);
        }
        
        public override void Send(DataPack dataPack)
        {
            var bytes = dataPack.ReadAllBytes();
            dataPack.Recycle();
            _kcp.Send(bytes, bytes.Length);
        }

        public override void Input(DataPack dataPack)
        {
            int ret = _kcp.Input(dataPack.ReadAllBytes());
            if (ret < 0)
            {
                Debug.LogError("Wrong KCP package");
                Recv(dataPack);
            }
            else
            {
                for (int size = _kcp.PeekSize(); size > 0; size = _kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (_kcp.Recv(recvBuffer) > 0)
                    {
                        var newPack = DataPackPool.GetDataPack(0, recvBuffer);
                        newPack.Position = 0;
                        Recv(newPack);
                    }
                }
            }
            dataPack.Recycle();
        }

        public override void Update()
        {
            _kcp.Update(Utils.GetClockMs());
        }

        public override void Dispose()
        {
            _kcp.Dispose();
        }
    }
}