using System.Collections.Generic;

namespace Meow.NetLinker
{
    public delegate void RawBytesCallback(int msgNum, byte[] bytes);
    
    public class RawByteDispatcher
    {
        private readonly int _initPostion;
        private readonly int _versionNum;

        private readonly Dictionary<int, List<RawBytesCallback>> _msgNumListenerCount = new Dictionary<int, List<RawBytesCallback>>();

        public OutPutFunc OutPut;
        
        public RawByteDispatcher(int versionNum, int initPosition)
        {
            _versionNum = versionNum;
            _initPostion = initPosition;
        }

        public void Send(int msgNum, byte[] bytes)
        {
            var pack = DataPackPool.GetDataPack(_initPostion, bytes);
            pack.TryAddDataToDict("VersionNum", -1, -1, _versionNum, typeof(int));
            pack.TryAddDataToDict("MsgNum", -1, -1, msgNum, typeof(int));
            pack.TryAddDataToDict("MsgBody", _initPostion, bytes.Length, null, null);
            OutPut(pack);
        }


        public void AddListener(int msgNum, RawBytesCallback callback)
        {
            if (!_msgNumListenerCount.ContainsKey(msgNum))
            {
                _msgNumListenerCount.Add(msgNum, new List<RawBytesCallback>());
            }
            _msgNumListenerCount[msgNum].Add(callback);
        }

        public void RemoveListener(int msgNum,  RawBytesCallback callback)
        {
            if (_msgNumListenerCount.ContainsKey(msgNum))
            {
                if (_msgNumListenerCount[msgNum].Contains(callback))
                {
                    _msgNumListenerCount[msgNum].Remove(callback);
                }
            }
        }

        public void Input(DataPack dataPack)
        {
            var msgNum = (int) dataPack.DataDict["MsgNum"].Data;
            var length = dataPack.DataDict["MsgBody"].Length;
            var bytes = dataPack.Reader.ReadBytes(length);
            if (_msgNumListenerCount.ContainsKey(msgNum))
            {
                foreach (var callback in _msgNumListenerCount[msgNum])
                {
                    callback.Invoke(msgNum, bytes);
                }
            }
        }

        public void Link(Layer lowerLayer)
        {
            OutPut += lowerLayer.Send;
            lowerLayer.Recv += Input;
        }
    }
}