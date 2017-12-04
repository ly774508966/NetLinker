using System.IO;
using System.Reflection;

namespace Meow.NetLinker
{
    public class ProtobufLayer : Layer
    {
        private readonly int _initPosition;
        
        public ProtobufLayer(int initPosition)
        {
            _initPosition = initPosition;
        }
        
        public override void Send(DataPack dataPack)
        {
            var obj = dataPack.DataDict["MsgBody"].Data;
            var msgType = dataPack.DataDict["MsgBody"].DataType;

            using (MemoryStream tempStream = new MemoryStream())
            {
                MethodInfo serializeMethod = msgType.GetMethod("Serialize", new[] { typeof(Stream), msgType });
                serializeMethod.Invoke(msgType, new[] { tempStream, obj });
                byte[] msgbody = tempStream.ToArray();
                
                dataPack.WriteBytes(_initPosition, msgbody);
                dataPack.TryAddDataToDict("MsgBody", _initPosition, msgbody.Length, null, null);
            }
            OutPut(dataPack);
        }

        public override void Input(DataPack dataPack)
        {
            var msgType = dataPack.DataDict["MsgBody"].DataType;
            if (msgType == null) return;

            int index = (int)dataPack.Position;
            int length = dataPack.DataDict["MsgBody"].Length;
            
            var msgBody = dataPack.Reader.ReadBytes(length);
            MethodInfo deserializeMethod = msgType.GetMethod("Deserialize", new[] {typeof(byte[])});
            var obj = deserializeMethod.Invoke(null, new object[] {msgBody});
            dataPack.TryAddDataToDict("MsgBody", index, -1, obj, null);
            Recv(dataPack);
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }
    }
}