using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Meow.NetLinker.Test
{
    public class TcpLayerTest
    {
        private Thread _serverThread;
        private TcpLayer _tcpLayer;

        private bool _connectState;
        private string _tcpLayerRecv;
        
        [UnityTest]
        public IEnumerator TcpConnectTest()
        {
            try
            {
                _serverThread = new Thread(MyTcpListener.Start);
                _serverThread.Start();

                _tcpLayer = new TcpLayer();
                _tcpLayer.Connect("127.0.0.1", 13000, b =>
                {
                    _connectState = b;
                });

                for (int i = 0; i < 20; i++)
                {
                    _tcpLayer.Update();
                    yield return null;
                }

                yield return null;
               
            }
            finally
            {
                _tcpLayer.Dispose();
                _serverThread.Interrupt();
                _serverThread.Abort();
            }
            
            Assert.AreEqual(true, _connectState);
            
        }
        
        [UnityTest]
        public IEnumerator TcpSendAndRecvTest()
        {
            try
            {
                _serverThread = new Thread(MyTcpListener.Start);
                _serverThread.Start();

                _tcpLayer = new TcpLayer();
                _tcpLayer.Connect("127.0.0.1", 13000, ok =>
                {
                    if (ok)
                    {
                        _tcpLayer.Send(DataPackPool.GetDataPack(0, Encoding.ASCII.GetBytes("abcdefg")));
                    }
                });

                _tcpLayer.Recv += pack =>
                {
                    _tcpLayerRecv = Encoding.ASCII.GetString(pack.ReadAllBytes());
                };



                for (var i = 0; i < 50; i++)
                {
                    _tcpLayer.Update();
                    yield return null;
                }
                
            }
            finally
            {
                _tcpLayer.Dispose();
                _serverThread.Interrupt();
                _serverThread.Abort();
            }
            
            Assert.AreEqual("ABCDEFG", _tcpLayerRecv);
        }
    }
    
    /// <summary>
    /// It will change all character to upper
    /// </summary>
    internal static class MyTcpListener
    {
        public static void Start()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                int port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];

                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        var data = Encoding.ASCII.GetString(bytes, 0, i);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            finally
            {
                // Stop listening for new clients.
                if (server != null) server.Stop();
            }
        }
    }
}