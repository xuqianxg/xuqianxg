using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using System.IO;
using System;

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

    public class MemoryStreamEx:MemoryStream
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
    public delegate void dNetWorkStateCallBack (EClientNetWorkState a_eState,string ip,ushort port);
    public class CClientNetWorkCtrl
    {
        private IAsyncResult m_ar_Recv = null;
        private IAsyncResult m_ar_Send = null;
        private IAsyncResult m_ar_Connect = null;
        private Socket m_ClientSocket = null;
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
        private bool mSendFlag = false;
        private readonly int CONECT_TIME_OUT = 10;
        private float connect_timeout = 0;
        private bool isReceived = false;
        
        public bool IsConnect()
        {
            return m_ClientSocket != null ? m_ClientSocket.Connected : false;
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
        public bool Connct(string a_strRoteIP,ushort a_uPort)
        {
            if(m_ClientSocket == null)
            {
                try 
                {
                    m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    return false;
                }
                m_strRomoteIP = a_strRoteIP;
                m_uRemotePort = a_uPort;
                m_eNetWorkState = EClientNetWorkState.E_CNWS_NORMAL;
                connect_timeout = 0;
                IPAddress ip = IPAddress.Parse(a_strRoteIP);
                mAsyncArgs = new SocketAsyncEventArgs();
                mAsyncArgs.RemoteEndPoint = new IPEndPoint(ip, a_uPort);
                mAsyncArgs.UserToken = m_ClientSocket;
                mAsyncArgs.Completed += SocketEventArg_Completed;
                m_ClientSocket.ConnectAsync(mAsyncArgs);
                isReceived = false;
                return true;
            }
            return false;
        }

        void SocketEventArg_Completed(object sender,SocketAsyncEventArgs e)
        {
            switch(e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    break;
                case SocketAsyncOperation.Send:
                    break;
                case SocketAsyncOperation.Receive:
                    break;
                case SocketAsyncOperation.Disconnect:
                    break;
            }
        }
        void ProcessConnect(SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_NORMAL;
                mReceiveA = new SocketAsyncEventArgs();
                mReceiveA.SetBuffer(new byte[4096], 0, 4096);
                mReceiveA.Completed += SocketEventArg_Completed;
                isReceived = true;
                connect_timeout = 0;
                Reveive();
            }
            else
            {
                DiDisconnect();
            }
        }

        void ProcessSend(SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                lock(m_SendQueue)
                {
                    if(m_SendQueue.Count ==0)
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
                DiDisconnect();
            }
        }

        void PricessReceive(SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                if(e.BytesTransferred > 0)
                {
                    lock(m_CommunicateionMem)
                    {
                        m_CommunicateionMem.Write(e.Buffer, 0, e.BytesTransferred);
                    }
                    Reveive();
                }
            }
            else
            {
                DiDisconnect();
            }
        }
        public bool ReConnect()
        {
            if(m_strRomoteIP !=null)
            {
                ReleaseSocket();
                return Connct(m_strRomoteIP, m_uRemotePort);
            }
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

        public bool SendMessage(int msgID,int dataType,byte[] data)
        {
            if(m_Writer !=null)
            {
                byte[] newData = m_Writer.MakeDataStream(dataType, data);
                MemoryStream mStream = new MemoryStream();
                mStream.Write(newData, 0, newData.Length);
                byte[] stream = m_Writer.MakeStream(msgID, mStream);
                lock(m_SendQueue)
                {
                    if(mSendFlag == false)
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

        public bool SendUnsafeMessga(int msgID,byte[] data)
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

        public void ReleseSocket()
        {
            if(m_ClientSocket != null)
            {
                if(m_ClientSocket.Connected == true)
                {
                    try 
                    {
                        m_ClientSocket.Shutdown(SocketShutdown.Both);

                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                    finally
                    {
                        m_ClientSocket.Close();
                        m_ClientSocket = null;
                    }
                }
            }
            m_eNetWorkState = EClientNetWorkState.E_CNWS_NDT_UNABLE;
            if (mReceiveA != null) mReceiveA.Completed -= SocketEventArg_Completed;
            if (mAsyncArgs != null) mAsyncArgs.Completed -= SocketEventArg_Completed;
            if (m_Reader != null) m_Reader.Reset();
            if (m_Writer != null) m_Writer.Reset();
        }

        public void Update()
        {
            lock(m_CommunicateionMem)
            {
                if(m_CommunicateionMem.Length>0)
                {
                    if(m_Reader!=null)
                    {
                        m_Reader.DidReadData(m_CommunicateionMem.GetBuffer(), (int)m_CommunicateionMem.Length);
                    }
                    m_CommunicateionMem.SetLength(0);
                }
            }
            lock(m_eNetWorkState)
            {
                EClientNetWorkState eState = (EClientNetWorkState)m_eNetWorkState;
                if(eState > EClientNetWorkState.E_CNWS_NORMAL)
                {
                    if(m_ClientSocket !=null)
                    {
                        ReleaseSocket();
                        CallBackNetState(eState);
                        eState = EClientNetWorkState.E_CNWS_NDT_UNABLE;
                    }
                }
                else if(isReceived == false)
                {
                    connect_timeout += Time.deltaTime;
                    if(connect_timeout > CONECT_TIME_OUT)
                    {
                        connect_timeout = 0;
                        DiDisconnect();
                    }
                }
            }
        }

        void CallBackNetState(EClientNetWorkState state)
        { 
            if(m_SateCallBack !=null)
            {
                m_SateCallBack(state, m_strRomoteIP, m_uRemotePort);
            }
        
        }
        

        void Send(byte[] bytes) { }

        void Reveive() { }
        public bool DiDisconnect() 
        {
            ReleaseSocket();
            return true;
        }
       public void ReleaseSocket()
        {

        }
    }
}

