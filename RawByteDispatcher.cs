using System.Collections.Generic;

namespace Meow.NetLinker
{
    public delegate void RawBytesCallback(int msgNum, byte[] bytes);
    
    public class RawByteDispatcher
    {
        private readonly RawBytesCallback _callback;
        private readonly int _initPostion;
        private readonly int _versionNum;
        
        private readonly Dictionary<int, int> _msgNumListenerCount = new Dictionary<int, int>();

        public OutPutFunc OutPut;
        
        public RawByteDispatcher(int versionNum, int initPosition, RawBytesCallback callback)
        {
            _versionNum = versionNum;
            _initPostion = initPosition;
            _callback = callback;
        }

        public void Send(int msgNum, byte[] bytes)
        {
            var pack = DataPackPool.GetDataPack(_initPostion, bytes);
            pack.TryAddDataToDict("VersionNum", -1, -1, _versionNum, typeof(int));
            pack.TryAddDataToDict("MsgNum", -1, -1, msgNum, typeof(int));
            pack.TryAddDataToDict("MsgBody", _initPostion, bytes.Length, null, null);
            OutPut(pack);
        }


        public void AddListener(int msgNum)
        {
            if (!_msgNumListenerCount.ContainsKey(msgNum))
            {
                _msgNumListenerCount.Add(msgNum, 0);
            }
            _msgNumListenerCount[msgNum]++;
        }

        public void RemoveListener(int msgNum)
        {
            if (_msgNumListenerCount.ContainsKey(msgNum))
            {
                _msgNumListenerCount[msgNum]--;
                if (_msgNumListenerCount[msgNum] == 0)
                {
                    _msgNumListenerCount.Remove(msgNum);
                }
            }
        }

        public void Input(DataPack dataPack)
        {
            var msgNum = (int) dataPack.DataDict["MsgNum"].Data;
            if (_msgNumListenerCount.ContainsKey(msgNum) && _msgNumListenerCount[msgNum] > 0)
            {
                _callback.Invoke(msgNum, dataPack.ReadAllBytes());
            }
        }

        public void Link(Layer lowerLayer)
        {
            OutPut += lowerLayer.Send;
            lowerLayer.Recv += Input;
        }
    }
}