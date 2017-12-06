using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
    public class RawByteDispatcherTest
    {
        private RawByteDispatcher _dispatcher;

        private int _sendedMsgNum;

        private byte[] _sendedBytes;

        private int _receivedMsgNum;

        private byte[] _receivedBytes;
        
        [UnityTest]
        public IEnumerator RawByteDispatcherSendTest()
        {
            _dispatcher = new RawByteDispatcher(1, 12);

            _dispatcher.OutPut += pack =>
            {
                _sendedMsgNum = (int) pack.DataDict["MsgNum"].Data;
                _sendedBytes = pack.ReadAllBytes();
            };

            try
            {
                _dispatcher.Send(0, new byte[] {1, 2, 3, 4, 5, 6, 0, 0, 0, 0, 0, 1});
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

            yield return null;
            
            Assert.AreEqual(_sendedMsgNum, 0);
            Assert.AreEqual(_sendedBytes, new byte[] {1, 2, 3, 4, 5, 6, 0, 0, 0, 0, 0, 1});
        }

        [UnityTest]
        public IEnumerator RawByteDispatcherAddRemoveListenerTest()
        {
            _dispatcher = new RawByteDispatcher(1, 0);
            
            _dispatcher.AddListener(0, Callback);

            var dataPack = DataPackPool.GetDataPack();
            var msgBytes = new byte[] {1, 2, 3, 4, 5, 6, 0, 0, 0, 0, 0, 1};
            dataPack.TryAddDataToDict("VersionNum", -1, -1, 1, typeof(int));
            dataPack.TryAddDataToDict("MsgNum", -1, -1, 0, typeof(int));
            dataPack.TryAddDataToDict("MsgBody", 0, msgBytes.Length, null, null);
            dataPack.PrepareWriteAfter(msgBytes.Length);
            dataPack.Writer.Write(msgBytes);
            dataPack.Position = 0;

            try
            {
                _dispatcher.Input(dataPack);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            yield return null;
            
            Assert.AreEqual(_receivedMsgNum, 0);
            Assert.AreEqual(_receivedBytes, new byte[] {1, 2, 3, 4, 5, 6, 0, 0, 0, 0, 0, 1});
            
            dataPack = DataPackPool.GetDataPack();
            msgBytes = new byte[] {1, 2, 3, 4, 5, 6, 0, 0, 0, 0, 0, 0};
            dataPack.TryAddDataToDict("VersionNum", -1, -1, 1, typeof(int));
            dataPack.TryAddDataToDict("MsgNum", -1, -1, 0, typeof(int));
            dataPack.TryAddDataToDict("MsgBody", 0, msgBytes.Length, null, null);
            dataPack.PrepareWriteAfter(msgBytes.Length);
            dataPack.Writer.Write(msgBytes);
            dataPack.Position = 0;
            
            _dispatcher.RemoveListener(0, Callback);
            
            try
            {
                
                _dispatcher.Input(dataPack);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            Assert.AreEqual(_receivedMsgNum, 0);
            Assert.AreEqual(_receivedBytes, new byte[] {1, 2, 3, 4, 5, 6, 0, 0, 0, 0, 0, 1});
            
            yield return null;
        }

        private void Callback(int msgNum, byte[] bytes)
        {
            _receivedMsgNum = msgNum;
            _receivedBytes = bytes;
        }
    }
}