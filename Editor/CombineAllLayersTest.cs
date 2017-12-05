using System.Collections;
using System.Collections.Generic;
using Example;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
    public class CombineAllLayersTest
    {
        private MessageDispatcher _dispatcher;
        private ProtobufLayer _protobuf;
        private HeadLayer _head;
        private KcpLayer _kcp;
        private UdpLayer _udp;

        private UdpLayer _udp2;

        private Person _receivedObj;
        
        [UnityTest]
        public IEnumerator CombineTest()
        {
            try
            {
                _dispatcher = new MessageDispatcher(1);
                _protobuf = new ProtobufLayer(12);
                _head = new HeadLayer();
                _kcp = new KcpLayer(0);
                _udp = new UdpLayer();

                _dispatcher.Link(_protobuf);
                _protobuf.Link(_head);
                _head.Link(_kcp);
                _kcp.Link(_udp);

                _dispatcher.AddListener<Person>(0, person =>
                {
                    _receivedObj = person;
                });
                _head.BindMsgNumToType(0, typeof(Person));

                _udp2 = new UdpLayer();
                _udp2.Recv += pack =>
                {
                    var bytes = pack.ReadAllBytes();
                    var newPack = DataPackPool.GetDataPack(0, bytes);
                    _udp2.Send(newPack);
                };


                _udp.Connect("127.0.0.1", 10001, 10000);
                yield return null;
                _udp2.Connect("127.0.0.1", 10000, 10001);

                yield return null;
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
                _dispatcher.Send(0, msgObj);

                yield return null;

                for (var i = 0; i < 50; i++)
                {
                    _kcp.Update();
                    _udp.Update();
                    _udp2.Update();
                    yield return null;
                }

            }
            finally
            {
                _kcp.Dispose();
                _udp.Dispose();
                _udp2.Dispose();
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
    }
}