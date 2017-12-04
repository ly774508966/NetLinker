using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
    public class HeadLayerTest
    {
        private HeadLayer _head1;
        private HeadLayer _head2;

        private UdpLayer _udp1;
        private UdpLayer _udp2;

        private int _head1VersionNum;
        private int _head1MsgNum;
        private int _head1MsgBodyLength;
        
        private int _head2VersionNum;
        private int _head2MsgNum;
        private int _head2MsgBodyLength;

        [UnityTest]
        public IEnumerator UdpLayerSendBytesTest()
        {
            try
            {
                _head1 = new HeadLayer();
                _head1.Recv += pack =>
                {
                    _head1VersionNum = (int) pack.DataDict["VersionNum"].Data;
                    _head1MsgNum = (int) pack.DataDict["MsgNum"].Data;
                    _head1MsgBodyLength = pack.DataDict["MsgBody"].Length;
                };
                _head2 = new HeadLayer();
                _head2.Recv += pack =>
                {
                    _head2VersionNum = (int) pack.DataDict["VersionNum"].Data;
                    _head2MsgNum = (int) pack.DataDict["MsgNum"].Data;
                    _head2MsgBodyLength = pack.DataDict["MsgBody"].Length;
                };
            
                _udp1 = new UdpLayer();
                _udp2 = new UdpLayer();
            
                _head1.Link(_udp1);
                _head2.Link(_udp2);
            
                _udp1.Connect("127.0.0.1", 10001, 10000);
                yield return null;
                _udp2.Connect("127.0.0.1", 10000, 10001);
            
                DataPack dataPack1 = new DataPack();
                dataPack1.TryAddDataToDict("VersionNum", -1, -1, 0, typeof(int));
                dataPack1.TryAddDataToDict("MsgNum", -1, -1, 10, typeof(int));
                dataPack1.TryAddDataToDict("MsgBody", -1, 30, null, null);
            
                DataPack dataPack2 = new DataPack();
                dataPack2.TryAddDataToDict("VersionNum", -1, -1, 1, typeof(int));
                dataPack2.TryAddDataToDict("MsgNum", -1, -1, 11, typeof(int));
                dataPack2.TryAddDataToDict("MsgBody", -1, 31, null, null);
            
                yield return null;
                _head1.Send(dataPack1);
                yield return null;
                _head2.Send(dataPack2);
                yield return null;
                
                for (var i = 0; i < 50; i++)
                {
                    yield return null;
                    _udp1.Update();
                    _udp2.Update();
                }
			
            }
            finally
            {
                _udp1.Dispose();
                _udp2.Dispose();
            }
			
            Assert.AreEqual(0, _head2VersionNum);
            Assert.AreEqual(10, _head2MsgNum);
            Assert.AreEqual(30, _head2MsgBodyLength);
            Assert.AreEqual(1, _head1VersionNum);
            Assert.AreEqual(11, _head1MsgNum);
            Assert.AreEqual(31, _head1MsgBodyLength);
        }
    }
}
