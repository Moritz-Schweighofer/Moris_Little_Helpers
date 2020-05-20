using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Schweigm_NETCore_Helpers
{
    public sealed class AsyncSocket
    {
        #region Constructur and Singelton

        private static readonly Lazy<AsyncSocket> Lazy = new Lazy<AsyncSocket>(() => new AsyncSocket());

        public static AsyncSocket Instance =>
            Lazy.Value;

        private AsyncSocket()
        {
        }

        #endregion Constructur and Singelton

        #region Public-Members

        /// <summary>
        /// Gets a value indicating whether the underlying System.Net.Sockets.Socket for
        /// a System.Net.Sockets.TcpClient is connected to a remote host.
        /// </summary>
        public bool IsConnected => _tcpClient.Connected;

        /// <summary>
        /// Delegate for the OnMessage event
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate Task OnMessageDelegate(string data);

        /// <summary>
        /// Event fired on a new Message from the Socket
        /// </summary>
        public event OnMessageDelegate OnMessage;

        #endregion Public-Members

        #region Private-Members

        /// <summary>
        /// Buffer Size which the Socket can Read
        /// </summary>
        private const int ReadBufferSize = 1024;

        /// <summary>
        /// TPC Client for the Connection
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// Byte Array for the Read Values
        /// </summary>
        private readonly byte[] _readBuffer = new byte[ReadBufferSize];

        #endregion Private-Members

        #region Public-Functions

        public void Connect(IPAddress iPAddress, int port)
        {
            // Create a TCP/IP  socket and connect to it
            _tcpClient = new TcpClient();

            var connectAttempt = _tcpClient.BeginConnect(iPAddress, port, null, null);

            var connectionSuccessful = connectAttempt.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            if (!connectionSuccessful) throw new Exception("Could not connect");

            _tcpClient.GetStream().BeginRead(_readBuffer, 0, ReadBufferSize, DoRead, null);
        }

        public void Disconnect()
        {
            try
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Disconnect the AsyncClient: " + ex.Message);
                throw;
            }
        }

        public void Send(byte[] data)
        {
            var myTcpStream = _tcpClient.GetStream();
            myTcpStream.Write(data, 0, data.Length);
            myTcpStream.Flush();
        }

        #endregion Public-Functions

        #region Private-Functions

        /// <summary>
        /// This Function reads the Socket and sends the Data via a Delegate to the BleGatewayController
        /// </summary>
        /// <param name="ar"></param>
        private void DoRead(IAsyncResult ar)
        {
            try
            {
                var bytesRead = _tcpClient.GetStream().EndRead(ar);

                if (bytesRead < 1)
                {
                    OnMessage?.Invoke("You have been disconnected.\r\n");
                    return;
                }

                var data = Encoding.ASCII.GetString(_readBuffer, 0, bytesRead);
                OnMessage?.Invoke(data);

                _tcpClient.GetStream().BeginRead(_readBuffer, 0, ReadBufferSize, DoRead, null);
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke("ConnectionClosed\r\n");
                _tcpClient.Close();
                _tcpClient.Dispose();
                Trace.WriteLine(ex);
            }
        }

        #endregion Private-Functions
    }
}