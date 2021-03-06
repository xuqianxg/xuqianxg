﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GEM_NET_LIB;
using System.IO;
using ProtoBuf.Meta;
using net_protocol;
using System.Net.Sockets;
public class CNetRecvMsg
{
    public int m_nMsgID = 0;
    public MemoryStream m_DataMsg;
    public Socket client;
    public T DeSerializeProtocol<T>()
    {
        m_DataMsg.Position = 0;
        return ProtoBuf.Serializer.Deserialize<T>(m_DataMsg);
    }
}
public delegate void OnHandleOneMessga(CNetRecvMsg msg);

public interface ILogicHandleMessage
{
    void ClientHnadleMessage(CNetRecvMsg msg);
}

public class CNetRecvMsgBuilder : IReaderHandleMessage
{
    private CNetRecvMsg clientNetMsg = new CNetRecvMsg();
    private ILogicHandleMessage m_Logic;
    public ILogicHandleMessage LogicHandleMessage
    {
        set {m_Logic = value;}
    }

    void IReaderHandleMessage.HandleMessage(Socket client, int msgID, MemoryStream data)
    {
        clientNetMsg.m_nMsgID = msgID;
        clientNetMsg.m_DataMsg = data;
        clientNetMsg.client = client;
         Console.WriteLine(msgID + "   " + data);
        if(m_Logic !=null)
        {
            string msgName;
            if(NetOpcodes_S2CString.Instance.GetString(msgID,out msgName))
            {
                m_Logic.ClientHnadleMessage(clientNetMsg);
            }
        }
        clientNetMsg.m_DataMsg = null;
        clientNetMsg.m_nMsgID = 0;
    }
}

public class CNetSendMsgBuilder
{
    private MemoryStream m_streamBuff = new MemoryStream();
    public MemoryStream Serialize<T> (T instance)
    {
        m_streamBuff.SetLength(0);
        ProtoBuf.Serializer.Serialize<T>(m_streamBuff,instance);
        return m_streamBuff;
    }
}
class CNetWorkGlobal
{
    private CNetRecvMsgBuilder m_RecvBuilder;
    private CNetSendMsgBuilder m_SendBuilder;
    private CClientNetWorkCtrl m_Ctrl;
    private NetStreamReader m_Reader;
    private NetStreamWriter m_Writer;
    public CNetWorkGlobal()
    {
        m_RecvBuilder = new CNetRecvMsgBuilder();
        m_Reader = new NetStreamReader();
        m_Reader.HandleMessage = m_RecvBuilder;
        m_Ctrl = new CClientNetWorkCtrl(500);
        m_Ctrl.Reader = m_Reader;
        m_Writer = new NetStreamWriter();
        m_Ctrl.Writer = m_Writer;
        m_SendBuilder = new CNetSendMsgBuilder();
    }

    public void RegisterHandleMessageClient(ILogicHandleMessage client)
    {
        m_RecvBuilder.LogicHandleMessage = client;
    }
    public void RegisteNetWorkStateLister(dNetWorkStateCallBack lister)
    {
        m_Ctrl.RegisterNetWorkStateLister(lister);
    }

    public void UnRegisteNetWorkStateLister(dNetWorkStateCallBack lister)
    {
        m_Ctrl.UnRegisterNetWorkStateLister(lister);
    }
    public bool IsConnected()
    {
        return m_Ctrl.IsConnect();
    }

    public bool Connect(string ip,ushort port)
    {
        return m_Ctrl.Connct(ip, port);
    }
    public bool DisConnect()
    {
        return m_Ctrl.Disconnect();
    }

    public void Close()
    {
        m_Ctrl.ReleaseSocket();
    }
    public bool SendNetMessage<T>(Socket client, int msgID,T data)
    {
        if(IsConnected() == true)
        {
            return m_Ctrl.SendMessage(client, msgID, m_SendBuilder.Serialize<T>(data));
        }
        return false;
    }
    public bool SendNetMessage(int msgID,int dataTypeId,byte[] bytes)
    {
        if(IsConnected() == true)
        {
            return m_Ctrl.SendMessage(msgID, dataTypeId, bytes);
        }
        return false;
    }

    public bool SendUnSafeNetMessage(int msgID,byte[] bytes)
    {
        if(IsConnected() == true)
        {
            return m_Ctrl.SendUnsafeMessga(msgID, bytes);
        }
        return false;
    }

    public bool SendNetEmptyMessage(int msgID)
    {
        if (IsConnected())
        {
            return m_Ctrl.SendMessage(msgID, null);
        }
        return false;
    }
    public bool SendNetEmptyMessage(Socket client,int msgID)
    {
        if (IsConnected())
        {
            return m_Ctrl.SendMessage(client ,msgID, null);
        }
        return false;
    }
    public void  Update()
    {
        //m_Ctrl.Update();
    }
    public void SetSocketSendNoDelay(bool nodelay)
    {
        m_Ctrl.SetSocketSendNoDeley(nodelay);
    }
}


public class Signleton<T> where T:new()
{
    protected Signleton(){}
    protected static T instance = new T();
    public static T Instance
    {
        get {return instance ;}
    }
}

public class NetOpcodes_C2SEnmu
{
    public static readonly int C2S_TEST = 1;
    public static readonly int C2S_CRTATE = 2;
    public static readonly int C2S_READY = 3;
    public static readonly int MAX = 64;
}

public class NetOpcodes_S2CEnmu
{
    public static readonly int S2C_CRTATE = 2;
    public static readonly int S2C_READY = 3;
    public static readonly int MAX = 64;
}


public class NetOpcodes_S2CString :Signleton<NetOpcodes_S2CString>
{
    private Dictionary<int ,string> m_StringMap = new Dictionary<int,string>();
    public bool GetString(int msgID,out string strOut)
    {
        return m_StringMap.TryGetValue(msgID, out strOut);
    }
   public  NetOpcodes_S2CString()
    {
        m_StringMap.Add(1, "C2S_TEST");
        m_StringMap.Add(2, "C2S_CRTATE");
        m_StringMap.Add(3, "C2S_READY");
    }
}


public class CClientHandleMessage :ILogicHandleMessage
{
    private OnHandleOneMessga[] m_HandleMap;
    void ILogicHandleMessage.ClientHnadleMessage(CNetRecvMsg msg)
    {
        if(msg.m_nMsgID>=0 && msg.m_nMsgID<m_HandleMap.Length)
        {
            OnHandleOneMessga handle = m_HandleMap[msg.m_nMsgID];
            if(handle != null)
            {
                handle(msg);
            }
            else
            {
                 Console.WriteLine("error  " + msg.m_nMsgID);
            }
        }
    }

    public CClientHandleMessage()
    {
        m_HandleMap = new OnHandleOneMessga[NetOpcodes_C2SEnmu.MAX];
        m_HandleMap[NetOpcodes_C2SEnmu.C2S_CRTATE] = new OnHandleOneMessga(HandleOnCreate);
        m_HandleMap[NetOpcodes_C2SEnmu.C2S_READY] = new OnHandleOneMessga(HandleOnReadStart);
    }

    void HandleTest(CNetRecvMsg msg)
    {
;
    }

    void HandleOnCreate(CNetRecvMsg msg)
    {
        Console.WriteLine("HandleOnCreate");
        Player player = new Player(msg.client,Game.Instance.playerList.Count+1);
        Game.Instance.playerList.Add(player);
        Server.NetWork.SendNetEmptyMessage(msg.client, 2);
        // Server.NetWork.SendNetEmptyMessage(NetOpcodes_S2CEnmu.S2C_CRTATE);
    }

    void  HandleOnReadStart(CNetRecvMsg msg)
    {
        Console.WriteLine("HandleOnReadStart");
        
    }

}