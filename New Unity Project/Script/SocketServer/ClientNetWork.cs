
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using System.IO;
using System;
using AsyncSocketServer;
namespace GEM_NET_LIB
{


    internal class StateObjectForRecvData
    {
        public Socket workSocket = null;
        public const int BufferSize = 4096;
        public byte[] buffer = new byte[BufferSize];
    }
    public interface INetMessageReader
    {
        void DidReadData(byte[] data, int size);
        void Reset();
    }
    public interface INetMessageWriter
    {
        byte[] MakeStream(int msgID, MemoryStream data);
        byte[] MakeDataStream(int msgId, byte[] data);
        void Reset();
    }

    public class MemoryStreamEx : MemoryStream
    {
        public void Clear()
        {
            SetLength(0);
        }
    }
    public enum EClientNetWorkState
    {
        E_CNWS_NDT_UNABLE,
        E_CNWS_NORMAL,
        E_CNWS_ON_CONNECTED_FAILED,
        E_CNWS_ON_DISCONNECTED,
    }
    public delegate void dNetWorkStateCallBack(EClientNetWorkState a_eState, string ip, ushort port);
    public class CClientNetWorkCtrl
    {
        private Socket listenSocket = null;
        private Socket ConnectSocket = null;
        private string m_strRomoteIP = "127.0.0.1";
        private ushort m_uRemotePort = 0;
        private INetMessageReader m_Reader = null;
        private INetMessageWriter m_Writer = null;
        private dNetWorkStateCallBack m_SateCallBack = null;
        private MemoryStreamEx m_CommunicateionMem = new MemoryStreamEx();
        private object m_eNetWorkState = EClientNetWorkState.E_CNWS_NDT_UNABLE;
        private Queue<byte[]> m_SendQueue = new Queue<byte[]>();
        private SocketAsyncEventArgs mAsyncArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs mReceiveA = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
        private bool mSendFlag = false;
        private readonly int CONECT_TIME_OUT = 10;
        private float connect_timeout = 0;
        private bool isReceived = false;

        private int m_numConnections=256; //最大支持连接个数
        private int m_receiveBufferSize=10; //每个连接接收缓存大小
        private Semaphore m_maxNumberAcceptedClients; //限制访问接收连接的线程数，用来控制最大并发数
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        private int m_socketTimeOutMS; //Socket最大超时时间，单位为MS
        public int SocketTimeOutMS { get { return m_socketTimeOutMS; } set { m_socketTimeOutMS = value; } }

        private AsyncSocketUserTokenPool m_asyncSocketUserTokenPool;
        private AsyncSocketUserTokenList m_asyncSocketUserTokenList;

       // AutoResetEvent m_maxNumberAcceptedClients = new AutoResetEvent(false);



        public bool IsConnect()
        {
            return true;
           // return m_ClientSocket != null ? m_ClientSocket.Connected : false;
        }
        public INetMessageReader Reader
        {
            set { m_Reader = value; }
        }
        public INetMessageWriter Writer
        {
            set { m_Writer = value; }
        }
        public void RegisterNetWorkStateLister(dNetWorkStateCallBack callBack)
        {
            if (m_SateCallBack == null)
            {
                m_SateCallBack = new dNetWorkStateCallBack(callBack);
            }
            else
                m_SateCallBack += callBack;
        }
        public void UnRegisterNetWorkStateLister(dNetWorkStateCallBack callBack)
        {
            if (m_SateCallBack != null)
                m_SateCallBack -= callBack;
        }
        public bool Connct(string a_strRoteIP, ushort a_uPort)
        {
           // m_maxNumberAcceptedClients = new Semaphore(8000, 8000);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),9999);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(256);
            StartAccept(null);
            //m_daemonThread = new DaemonThread(this);
            return true;
        }
        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                mAsyncArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接  
            }

//             try
//             {
//                 m_maxNumberAcceptedClients.WaitOne(); //获取信号量 
//             }
//             catch(Exception e)
//             {
//                 Console.Write(e.Message);
//             }

            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);
            autoEvent.WaitOne();
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }


        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception E)
            {
                Console.WriteLine(string.Format("Accept client {0} error, message: {1}", acceptEventArgs.AcceptSocket, E.Message));
            }
        }

        void SocketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine(e.LastOperation.ToString());
            switch (e.LastOperation)
            {
                 
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
//                 case SocketAsyncOperation.Accept:
//                     ProcessAccept(e);
//                     break;
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            Console.WriteLine(string.Format("Client connection accepted. Local Address: {0}, Remote Address: {1}",
                acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint));
            mAsyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            ConnectSocket = acceptEventArgs.AcceptSocket;
            //autoEvent.Reset();
            autoEvent.Set();
//             AsyncSocketUserToken userToken = m_asyncSocketUserTokenPool.Pop();
//             m_asyncSocketUserTokenList.Add(userToken); //添加到正在连接列表  
//             userToken.ConnectSocket = acceptEventArgs.AcceptSocket;
//             userToken.ConnectDateTime = DateTime.Now;

//             try
//             {
//                 bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求  
//                 if (!willRaiseEvent)
//                 {
//                     lock (userToken)
//                     {
//                         ProcessSend(acceptEventArgs);
//                         //ProcessReceive(userToken.ReceiveEventArgs);
//                     }
//                 }
//             }
//             catch (Exception E)
//             {
//                 Console.WriteLine(string.Format("Accept client {0} error, message: {1}", userToken.ConnectSocket, E.Message));
//             }

           // StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接  
        }
        void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_NORMAL;
                mReceiveA = new SocketAsyncEventArgs();
                mReceiveA.SetBuffer(new byte[4096], 0, 4096);
                mReceiveA.Completed += SocketEventArg_Completed;
                isReceived = true;
                connect_timeout = 0;
                Receive();
            }
            else
            {
                Disconnect();
            }
        }

        void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                lock (m_SendQueue)
                {
                    if (m_SendQueue.Count == 0)
                    {
                        mSendFlag = false;
                    }
                    else
                    {
                        Send(m_SendQueue.Dequeue());
                    }
                }
            }
            else
            {
                Disconnect();
            }
        }

        void ProcessReceive(SocketAsyncEventArgs e)
        {
             Console.WriteLine("success  ");
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    lock (m_CommunicateionMem)
                    {
                        m_CommunicateionMem.Write(e.Buffer, 0, e.BytesTransferred);
                    }
                    Receive();
                }
            }
            else
            {
                DidDisconnect();
            }
        }
        public bool ReConnect()
        {
            if (m_strRomoteIP != null)
            {
                ReleaseSocket();
                return Connct(m_strRomoteIP, m_uRemotePort);
            }
            return false;
        }

        public bool SendMessage(int msgID, MemoryStream data)
        {
            if (m_Writer != null)
            {
                byte[] stream = m_Writer.MakeStream(msgID, data);
                lock (m_SendQueue)
                {
                    if (mSendFlag == false)
                    {
                        mSendFlag = true;
                        Send(stream);
                    }
                    else
                    {
                        m_SendQueue.Enqueue(stream);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool SendMessage(int msgID, int dataType, byte[] data)
        {
            if (m_Writer != null)
            {
                byte[] newData = m_Writer.MakeDataStream(dataType, data);
                MemoryStream mStream = new MemoryStream();
                mStream.Write(newData, 0, newData.Length);
                byte[] stream = m_Writer.MakeStream(msgID, mStream);
                lock (m_SendQueue)
                {
                    if (mSendFlag == false)
                    {
                        mSendFlag = true;
                        Send(stream);
                    }
                    else
                    {
                        m_SendQueue.Enqueue(stream);
                    }
                }
                return true;
            }
            return false;
        }

        public bool SendUnsafeMessga(int msgID, byte[] data)
        {

            if (m_Writer != null)
            {
                MemoryStream mStream = new MemoryStream();
                mStream.Write(data, 0, data.Length);
                byte[] stream = m_Writer.MakeStream(msgID, mStream);
                lock (m_SendQueue)
                {
                    if (mSendFlag == false)
                    {
                        mSendFlag = true;
                        Send(stream);
                    }
                    else
                    {
                        m_SendQueue.Enqueue(stream);
                    }
                }
                return true;
            }
            return false;
        }

        public void ReleaseSocket()
        {
            if (listenSocket != null)
            {
                if (listenSocket.Connected == true)
                {
                    try
                    {
                        listenSocket.Shutdown(SocketShutdown.Both);

                    }
                    catch (Exception e)
                    {
                         Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        listenSocket.Close();
                        listenSocket = null;
                    }
                }
            }
            m_eNetWorkState = EClientNetWorkState.E_CNWS_NDT_UNABLE;
            if (mReceiveA != null) mReceiveA.Completed -= SocketEventArg_Completed;
            if (mAsyncArgs != null) mAsyncArgs.Completed -= SocketEventArg_Completed;
            if (m_Reader != null) m_Reader.Reset();
            if (m_Writer != null) m_Writer.Reset();
        }

//         public void Update()
//         {
//             lock (m_CommunicateionMem)
//             {
//                 if (m_CommunicateionMem.Length > 0)
//                 {
//                     if (m_Reader != null)
//                     {
//                         m_Reader.DidReadData(m_CommunicateionMem.GetBuffer(), (int)m_CommunicateionMem.Length);
//                     }
//                     m_CommunicateionMem.SetLength(0);
//                 }
//             }
//             lock (m_eNetWorkState)
//             {
//                 EClientNetWorkState eState = (EClientNetWorkState)m_eNetWorkState;
//                 if (eState > EClientNetWorkState.E_CNWS_NORMAL)
//                 {
//                     if (listenSocket != null)
//                     {
//                         ReleaseSocket();
//                         CallBackNetState(eState);
//                         eState = EClientNetWorkState.E_CNWS_NDT_UNABLE;
//                     }
//                 }
//                 else if (isReceived == false)
//                 {
//                     connect_timeout += Time.deltaTime;
//                     if (connect_timeout > CONECT_TIME_OUT)
//                     {
//                         connect_timeout = 0;
//                         Disconnect();
//                     }
//                 }
//             }
//        }

        void CallBackNetState(EClientNetWorkState state)
        {
            if (m_SateCallBack != null)
            {
                m_SateCallBack(state, m_strRomoteIP, m_uRemotePort);
            }

        }

        public void SetSocketSendNoDeley(bool nodely)
        {
            if (listenSocket != null)
            {
                listenSocket.NoDelay = nodely;
            }
        }

        private void DidConnectError(Exception e)
        {
            lock(m_eNetWorkState)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_ON_CONNECTED_FAILED;
            }
        }
        private void DidDisconnect()
        {
            lock(m_eNetWorkState)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_ON_DISCONNECTED;
            }
        }
        private void Receive()
        {
            try
            {
                if (listenSocket.ReceiveAsync(mReceiveA) == false)
                {

                }
            }
            catch (Exception e)
            {
                 Console.WriteLine(e.Message);
                DidDisconnect();
            }
        }
        bool Send(byte[] bytes) 
        {
            try
            {
                mAsyncArgs.SetBuffer(bytes, 0, bytes.Length);
                if (ConnectSocket.SendAsync(mAsyncArgs) == false)
                {

                }
                return true;
            }
            catch(Exception e)
            {
                DidDisconnect();
            }
            return false;
        }


        public static IPAddress GetLocalIP()
        {
            string hostNmae = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostNmae);
            return ips.Length > 0 ? ips[0] : null;
        }
        public string GetLocalIPString()
        {
            IPAddress ip = GetLocalIP();
            return ip != null ? ip.ToString() : null;
        }

        public bool Disconnect()
        {
            ReleaseSocket();
            return true;
        }
    }
}

