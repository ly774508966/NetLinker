using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meow.NetLinker
{
    public struct ReflectionCallback
    {
        public object Instance;
        public MethodInfo InvokeMethod;
    }
    public class MessageDispatcher
    {
        public OutPutFunc OutPut;
        
        private readonly Dictionary<int, Dictionary<int, ReflectionCallback>> _callBackDict = new Dictionary<int, Dictionary<int, ReflectionCallback>>();

        private readonly int _versionNum;

        public MessageDispatcher(int versionNum)
        {
            _versionNum = versionNum;
        }
        
        public void Send<T>(int msgNum, T msgObj)
        {
            var sendedData = DataPackPool.GetDataPack();
            sendedData.TryAddDataToDict("VersionNum", -1, -1, _versionNum, typeof(int));
            sendedData.TryAddDataToDict("MsgBody", -1, -1, msgObj, typeof(T));
            sendedData.TryAddDataToDict("MsgNum", -1, -1, msgNum, typeof(int));
            OutPut(sendedData);
        }

        public void AddListener<T>(int msgNum, Action<T> callback)
        {
            if (!_callBackDict.ContainsKey(msgNum))
            {
                _callBackDict.Add(msgNum, new Dictionary<int, ReflectionCallback>());
            }
            var callbackList = _callBackDict[msgNum];
            var invokeMethod = callback.GetType().GetMethod("Invoke", new[] {typeof(T)});
            callbackList.Add(callback.GetHashCode(), new ReflectionCallback{ Instance = callback, InvokeMethod = invokeMethod});
        }

        public void RemoveListener<T>(int msgNum, Action<T> callback)
        {
            if (_callBackDict.ContainsKey(msgNum))
            {
                var callbackList = _callBackDict[msgNum];
                callbackList.Remove(callback.GetHashCode());
            }
        }

        public void Input(DataPack dataPack)
        {
            var msgNum = (int)dataPack.DataDict["MsgNum"].Data;
            var msgBody = dataPack.DataDict["MsgBody"].Data;
            if (_callBackDict.ContainsKey(msgNum))
            {
                foreach (var callbackReflection in _callBackDict[msgNum].Values)
                {
                    var callbackInstance = callbackReflection.Instance;
                    var callbackMethod = callbackReflection.InvokeMethod;
                    callbackMethod.Invoke(callbackInstance, new[] { msgBody });
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