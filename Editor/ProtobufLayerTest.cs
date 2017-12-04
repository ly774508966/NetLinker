using System;
using System.Collections;
using System.Collections.Generic;
using Example;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
    public class ProtobufLayerTest
    {
        private ProtobufLayer _protobuf;

        private byte[] _serializedBytes;
        private int _serializedLength;
        private int _serializedIndex;

        private Person _deserializedObj;

        [UnityTest]
        public IEnumerator ProtobufLayerSendTest()
        {
            var sendPack = new DataPack();
            var msgObj = new Person
            {
                Email = "snatic0@126.com",
                Id = 1,
                Name = "Meow",
                Phone = new List<Person.PhoneNumber>
                {
                    new Person.PhoneNumber {Number = "13512347890", Type = Person.PhoneType.HOME},
                    new Person.PhoneNumber {Number = "13823455678", Type = Person.PhoneType.MOBILE},
                    new Person.PhoneNumber {Number = "15867893456", Type = Person.PhoneType.WORK}
                }
            };
            sendPack.TryAddDataToDict("MsgBody", -1, -1, msgObj, msgObj.GetType());
            
            try
            {
                _protobuf = new ProtobufLayer(8)
                {
                    OutPut = pack =>
                    {
                        _serializedBytes = pack.ReadAllBytes();
                        _serializedLength = pack.DataDict["MsgBody"].Length;
                        _serializedIndex = pack.DataDict["MsgBody"].Index;
                    }
                };

                _protobuf.Send(sendPack);
			
            }
            catch(Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            yield return null;
            
            var exceptedBytes = Person.SerializeToBytes(msgObj);
            
            Assert.AreEqual(exceptedBytes, _serializedBytes);
            Assert.AreEqual(exceptedBytes.Length, _serializedLength);
            Assert.AreEqual(8, _serializedIndex);
        }

        [UnityTest]
        public IEnumerator ProtobufLayerRecvTest()
        {
            var recvPack = new DataPack();
            var msgObj = new Person
            {
                Email = "snatic0@126.com",
                Id = 1,
                Name = "Meow",
                Phone = new List<Person.PhoneNumber>
                {
                    new Person.PhoneNumber {Number = "13512347890", Type = Person.PhoneType.HOME},
                    new Person.PhoneNumber {Number = "13823455678", Type = Person.PhoneType.MOBILE},
                    new Person.PhoneNumber {Number = "15867893456", Type = Person.PhoneType.WORK}
                }
            };
            var bytes = Person.SerializeToBytes(msgObj);
            recvPack.Position = 8;
            recvPack.Writer.Write(bytes);
            recvPack.TryAddDataToDict("MsgBody", -1, bytes.Length, null, typeof(Person));
            recvPack.Position = 8;

            try
            {
                _protobuf = new ProtobufLayer(8)
                {
                    Recv = pack =>
                    {
                        _deserializedObj = pack.DataDict["MsgBody"].Data as Person;
                    }
                };
                
                _protobuf.Input(recvPack);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            yield return null;
            
            Assert.AreEqual("snatic0@126.com", _deserializedObj.Email);
            Assert.AreEqual(1, _deserializedObj.Id);
            Assert.AreEqual("Meow", _deserializedObj.Name);
            Assert.AreEqual("13512347890", _deserializedObj.Phone[0].Number);
            Assert.AreEqual(Person.PhoneType.HOME, _deserializedObj.Phone[0].Type);
            Assert.AreEqual("13823455678", _deserializedObj.Phone[1].Number);
            Assert.AreEqual(Person.PhoneType.MOBILE, _deserializedObj.Phone[1].Type);
            Assert.AreEqual("15867893456", _deserializedObj.Phone[2].Number);
            Assert.AreEqual(Person.PhoneType.WORK, _deserializedObj.Phone[2].Type);
        }
    }
}
