
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
        void DidReadData(Socket clientSocket,byte[] data, int size);
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
        class ReceiveData
        {
            Socket clientSocket = null;
            MemoryStreamEx m_CommunicateionMem = new MemoryStreamEx();
        }
        private MemoryStreamEx m_CommunicateionMem = new MemoryStreamEx();
        private object m_eNetWorkState = EClientNetWorkState.E_CNWS_NDT_UNABLE;
        private Queue<byte[]> m_SendQueue = new Queue<byte[]>();
        private SocketAsyncEventArgs mAsyncArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs mReceiveA = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
        private bool mSendFlag = false;
        private int m_numConnections=256; //最大支持连接个数
        private int m_receiveBufferSize=10; //每个连接接收缓存大小
        private Semaphore m_maxNumberAcceptedClients; //限制访问接收连接的线程数，用来控制最大并发数
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        private int m_socketTimeOutMS; //Socket最大超时时间，单位为MS
        public int SocketTimeOutMS { get { return m_socketTimeOutMS; } set { m_socketTimeOutMS = value; } }
        SocketAsyncEventArgsPool m_readWritePool;
       // AutoResetEvent m_maxNumberAcceptedClients = new AutoResetEvent(false);

        public CClientNetWorkCtrl(int num)
        {
            m_numConnections = num;
            m_maxNumberAcceptedClients = new Semaphore(num, num);
            m_readWritePool = new SocketAsyncEventArgsPool(num);
            Init();
        }

        public void Init()
        {
            SocketAsyncEventArgs readWriteEventArg;
            for(int i=0;i<m_numConnections;i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
                byte[] asyncReceiveBuffer = new byte[4096];
                readWriteEventArg.SetBuffer(asyncReceiveBuffer, 0, asyncReceiveBuffer.Length);
                m_readWritePool.Push(readWriteEventArg);
            }
        }

        public bool IsConnect()
        {
            //return true;
            return listenSocket != null ;
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
            mAsyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed); 
            StartAccept(null);
            Console.ReadKey();
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
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接  
            }

            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }


        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            {
                Console.WriteLine("one");
                ProcessAccept(acceptEventArgs);
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
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            Console.WriteLine(string.Format("Client connection accepted. Local Address: {0}, Remote Address: {1}",
                acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint));
            SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();
            readEventArgs.UserToken = acceptEventArgs.AcceptSocket;
            readEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
           
            try
            {
                bool willRaiseEvent = acceptEventArgs.AcceptSocket.ReceiveAsync(readEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
           StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接  
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
            Console.WriteLine("receive");
            Console.WriteLine(e.AcceptSocket.RemoteEndPoint);
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    lock (m_CommunicateionMem)
                    {
                        m_CommunicateionMem.Write(e.Buffer, 0, e.BytesTransferred);
                        if (m_Reader != null)
                        {
                             m_Reader.DidReadData(e.AcceptSocket, m_CommunicateionMem.GetBuffer(), (int)m_CommunicateionMem.Length);
                        }
                         m_CommunicateionMem.SetLength(0);
                         bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(e);
                         if (!willRaiseEvent)
                         {
                             ProcessReceive(e);
                         }
                    }
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


        public bool SendMessage(Socket clientSocket,int msgID,MemoryStream data)
        {
            if (m_Writer != null)
            {
                byte[] stream = m_Writer.MakeStream(msgID, data);
                lock (m_SendQueue)
                {
                    if (mSendFlag == false)
                    {
                        mSendFlag = true;
                        try
                        {
                            mAsyncArgs.SetBuffer(stream, 0, stream.Length);
                            if (clientSocket.SendAsync(mAsyncArgs) == false)
                            {
                                Console.WriteLine("send error ");
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                            DidDisconnect(clientSocket);
                        }
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
                        try
                        {
                            mAsyncArgs.SetBuffer(stream, 0, stream.Length);
                            if (ConnectSocket.SendAsync(mAsyncArgs) == false)
                            {

                            }
                            return true;
                        }
                        catch (Exception e)
                        {
                            DidDisconnect();
                        }
                        return false;
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

        private void DidDisconnect(Socket client)
        {
            if(client == null) return;
            client.Shutdown(SocketShutdown.Both);
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
            Console.WriteLine("Disconnect");
            ReleaseSocket();
            return true;
        }
    }


    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> m_pool;

        // Initializes the object pool to the specified size
        //
        // The "capacity" parameter is the maximum number of 
        // SocketAsyncEventArgs objects the pool can hold
        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        // Add a SocketAsyncEventArg instance to the pool
        //
        //The "item" parameter is the SocketAsyncEventArgs instance 
        // to add to the pool
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        // Removes a SocketAsyncEventArgs instance from the pool
        // and returns the object removed from the pool
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        // The number of SocketAsyncEventArgs instances in the pool
        public int Count
        {
            get { return m_pool.Count; }
        }

    }
}

