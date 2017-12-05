using System;
using System.Collections;
using System.Collections.Generic;
using Example;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
    public class MessageDispatcherTest
    {
        private MessageDispatcher _dispatcher;

        private int SendedMsgNum;
        private object SendedObj;

        private Person _receivedObj;
        
        [UnityTest]
        public IEnumerator MessageDispatcherSendTest()
        {
            _dispatcher = new MessageDispatcher(1);
            
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

            _dispatcher.OutPut += pack =>
            {
                SendedMsgNum = (int) pack.DataDict["MsgNum"].Data;
                SendedObj = pack.DataDict["MsgBody"].Data;
            };

            try
            {
                _dispatcher.Send(0, msgObj);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

            yield return null;
            
            Assert.AreEqual(SendedMsgNum, 0);
            Assert.AreEqual(SendedObj, msgObj);
        }

        [UnityTest]
        public IEnumerator MessageDispatcherAddRemoveListenerTest()
        {
            _dispatcher = new MessageDispatcher(1);

            _dispatcher.AddListener<Person>(0, OnPersonReceived);
            
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
            
            try
            {
                var dataPack = new DataPack();
                dataPack.TryAddDataToDict("MsgBody", -1, -1, msgObj, typeof(Person));
                dataPack.TryAddDataToDict("MsgNum", -1, -1, 0, typeof(int));
                
                _dispatcher.Input(dataPack);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            yield return null;
            
            Assert.AreEqual("snatic0@126.com", _receivedObj.Email);
            Assert.AreEqual(1, _receivedObj.Id);
            Assert.AreEqual("Meow", _receivedObj.Name);
            Assert.AreEqual("13512347890", _receivedObj.Phone[0].Number);
            Assert.AreEqual(Person.PhoneType.HOME, _receivedObj.Phone[0].Type);
            Assert.AreEqual("13823455678", _receivedObj.Phone[1].Number);
            Assert.AreEqual(Person.PhoneType.MOBILE, _receivedObj.Phone[1].Type);
            Assert.AreEqual("15867893456", _receivedObj.Phone[2].Number);
            Assert.AreEqual(Person.PhoneType.WORK, _receivedObj.Phone[2].Type);
            
            _dispatcher.RemoveListener<Person>(0, OnPersonReceived);
            
            msgObj = new Person
            {
                Email = "snatic@163.com",
                Id = 2,
                Name = "MeowMeow",
                Phone = new List<Person.PhoneNumber>
                {
                    new Person.PhoneNumber {Number = "23512347891", Type = Person.PhoneType.HOME},
                    new Person.PhoneNumber {Number = "23823455679", Type = Person.PhoneType.MOBILE},
                    new Person.PhoneNumber {Number = "25867893457", Type = Person.PhoneType.WORK}
                }
            };
            
            try
            {
                var dataPack = new DataPack();
                dataPack.TryAddDataToDict("MsgBody", -1, -1, msgObj, typeof(Person));
                dataPack.TryAddDataToDict("MsgNum", -1, -1, 0, typeof(int));
                
                _dispatcher.Input(dataPack);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            Assert.AreEqual("snatic0@126.com", _receivedObj.Email);
            Assert.AreEqual(1, _receivedObj.Id);
            Assert.AreEqual("Meow", _receivedObj.Name);
            Assert.AreEqual("13512347890", _receivedObj.Phone[0].Number);
            Assert.AreEqual(Person.PhoneType.HOME, _receivedObj.Phone[0].Type);
            Assert.AreEqual("13823455678", _receivedObj.Phone[1].Number);
            Assert.AreEqual(Person.PhoneType.MOBILE, _receivedObj.Phone[1].Type);
            Assert.AreEqual("15867893456", _receivedObj.Phone[2].Number);
            Assert.AreEqual(Person.PhoneType.WORK, _receivedObj.Phone[2].Type);
        }

        private void OnPersonReceived(Person p)
        {
            _receivedObj = p;
        }
    }
}