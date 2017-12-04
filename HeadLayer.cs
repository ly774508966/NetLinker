using System;
using System.Collections.Generic;

namespace Meow.NetLinker
{
    public class HeadLayer : Layer
    {
        private Dictionary<int, Type> _msgNumToTypeDict = new Dictionary<int, Type>();
        
        public override void Send(DataPack dataPack)
        {
            var bodyLength = dataPack.DataDict["MsgBody"].Length;
            dataPack.PrepareWriteBefore(typeof(int));
            dataPack.Writer.Write(bodyLength);
            
            var msgNum = (int)dataPack.DataDict["MsgNum"].Data;
            dataPack.PrepareWriteBefore(typeof(int));
            dataPack.Writer.Write(msgNum);

            var versionNum = (int)dataPack.DataDict["VersionNum"].Data;
            dataPack.PrepareWriteBefore(typeof(int));
            dataPack.Writer.Write(versionNum);
            
            OutPut(dataPack);
        }

        public override void Input(DataPack dataPack)
        {
            var versionNum = dataPack.Reader.ReadInt32();
            var msgNum = dataPack.Reader.ReadInt32();
            var bodyLength = dataPack.Reader.ReadInt32();
            var msgType = TryGetTypeByMsgNum(msgNum);
            dataPack.TryAddDataToDict("VersionNum",-1,-1,versionNum, versionNum.GetType());
            dataPack.TryAddDataToDict("MsgNum",-1,-1,msgNum, msgNum.GetType());
            dataPack.TryAddDataToDict("MsgBody", -1, bodyLength, null, msgType);

            Recv(dataPack);
        }

        public void BindMsgNumToType(int msgNum, Type type)
        {
            if (!_msgNumToTypeDict.ContainsKey(msgNum))
            {
                _msgNumToTypeDict.Add(msgNum, type);
            }
        }

        private Type TryGetTypeByMsgNum(int msgNum)
        {
            return _msgNumToTypeDict.ContainsKey(msgNum) ? _msgNumToTypeDict[msgNum] : null;
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }
    }
}