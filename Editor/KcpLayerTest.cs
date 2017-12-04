using System.Collections;
using System.Text;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
	public class KcpLayerTest
	{
		private KcpLayer _kcp1;
		private KcpLayer _kcp2;
		
		private UdpLayer _udp1;
		private UdpLayer _udp2;

		private string _kcp1Receive;
		private string _kcp2Receive;

		[UnityTest]
		public IEnumerator KcpLayerSendBytesTest()
		{
			try
			{
				_kcp1 = new KcpLayer(0);
				_kcp2 = new KcpLayer(0);

				_kcp1.Recv += pack =>
				{
					_kcp1Receive = Encoding.ASCII.GetString(pack.ReadAllBytes());
				};

				_kcp2.Recv += pack =>
				{
					_kcp2Receive = Encoding.ASCII.GetString(pack.ReadAllBytes());
				};

				_kcp1.OutPut += _kcp2.Input;
				_kcp2.OutPut += _kcp1.Input;
				
				yield return null;
				_kcp1.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("i am kcp 1")));
				yield return null;
				_kcp2.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("i am kcp 2")));
			
				for (var i = 0; i < 50; i++)
				{
					yield return null;
					_kcp1.Update();
					_kcp2.Update();
				}
			
			}
			finally
			{
				_kcp1.Dispose();
				_kcp2.Dispose();
			}
			
			Assert.AreEqual(_kcp1Receive, "i am kcp 2");
			Assert.AreEqual(_kcp2Receive, "i am kcp 1");
		}
		
		
		[UnityTest]
		public IEnumerator KcpLinkUdpLayerSendBytesTest()
		{
			try
			{
				_kcp1 = new KcpLayer(0);
				_kcp2 = new KcpLayer(0);

				_kcp1.Recv += pack =>
				{
					_kcp1Receive = Encoding.ASCII.GetString(pack.ReadAllBytes());
				};

				_kcp2.Recv += pack =>
				{
					_kcp2Receive = Encoding.ASCII.GetString(pack.ReadAllBytes());
				};
				
				_udp1 = new UdpLayer();
				_udp2 = new UdpLayer();
		
				_kcp1.Link(_udp1);
				_kcp2.Link(_udp2);

				_udp1.Connect("127.0.0.1", 10001, 10000);
				yield return null;
				_udp2.Connect("127.0.0.1", 10000, 10001);

				yield return null;
				_kcp1.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("i am kcp 1")));
				yield return null;
				_kcp2.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("i am kcp 2")));
			
				for (var i = 0; i < 50; i++)
				{
					yield return null;
					_kcp1.Update();
					_kcp2.Update();
					_udp1.Update();
					_udp2.Update();
				}
			
			}
			finally
			{
				_kcp1.Dispose();
				_kcp2.Dispose();
				_udp1.Dispose();
				_udp2.Dispose();
			}
			
			Assert.AreEqual(_kcp1Receive, "i am kcp 2");
			Assert.AreEqual(_kcp2Receive, "i am kcp 1");
		}
		
	}
}
