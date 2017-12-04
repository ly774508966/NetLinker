using System.Collections;
using System.Text;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
	public class UdpLayerTest
	{
		private UdpLayer _udp1;
		private UdpLayer _udp2;

		private string _udp1Receive;
		private string _udp2Receive;

		[UnityTest]
		public IEnumerator UdpLayerSendBytesTest()
		{
			try
			{
				_udp1 = new UdpLayer();
				_udp2 = new UdpLayer();

				_udp1.Recv += pack =>
				{
					_udp1Receive = Encoding.ASCII.GetString(pack.ReadAllBytes());
				};

				_udp2.Recv += pack =>
				{
					_udp2Receive = Encoding.ASCII.GetString(pack.ReadAllBytes());
				};

				_udp1.Connect("127.0.0.1", 10001, 10000);
				yield return null;
				_udp2.Connect("127.0.0.1", 10000, 10001);

				yield return null;
				_udp1.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("i am udp 1")));
				yield return null;
				_udp2.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("i am udp 2")));
			
				for (var i = 0; i < 10; i++)
				{
					yield return null;
					Update();
				}
			
			}
			finally
			{
				_udp1.Dispose();
				_udp2.Dispose();
			}
			
			Assert.AreEqual(_udp1Receive, "i am udp 2");
			Assert.AreEqual(_udp2Receive, "i am udp 1");
		}

		private void Update()
		{
			_udp1.Update();
			_udp2.Update();
		}
	}
}
