using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Collections;

namespace GEM_NET_LIB
{
    public class NetStreamWriter : INetMessageWriter
    {
        private MemoryStream m_Buffer = new MemoryStream();
        private byte[] m_NotUseByte = new byte[4] { 0, 0, 0, 0 };
        private byte mCounter = 0;
        
        byte[] INetMessageWriter.MakeStream(int msgID,MemoryStream data)
        {
            m_NotUseByte[0] = mCounter;
            m_Buffer.SetLength(0);
            int net_msgID=IPAddress.HostToNetworkOrder(msgID);
            byte[] net_MsgID_byte = BitConverter.GetBytes(net_msgID);
            int net_data_Size = IPAddress.HostToNetworkOrder(net_MsgID_byte.Length + m_NotUseByte.Length + (data != null ? (int)data.Length : 0));
            byte[] net_Data_Size_byte = BitConverter.GetBytes(net_data_Size);
           // m_Buffer.Write(net_Data_Size_byte, 0, net_Data_Size_byte.Length);
            m_Buffer.Write(net_MsgID_byte, 0, net_MsgID_byte.Length);
/*            m_Buffer.Write(m_NotUseByte, 0, m_NotUseByte.Length);*/
            mCounter++;
            if (data != null)
                m_Buffer.Write(data.GetBuffer(), 0, (int)data.Length);
            return m_Buffer.ToArray();
        }
        byte[] INetMessageWriter.MakeDataStream(int msgID,byte[] data)
        {
            m_Buffer.SetLength(0);
            byte[] net_MsgID_byte = BitConverter.GetBytes(msgID);
            m_Buffer.Write(net_MsgID_byte, 0, net_MsgID_byte.Length);
            if (data != null)
                m_Buffer.Write(data, 0, data.Length);
            return m_Buffer.ToArray();
        }

        void INetMessageWriter.Reset()
        {
            m_Buffer.SetLength(0);
            mCounter = 0;
        }
    }
}
