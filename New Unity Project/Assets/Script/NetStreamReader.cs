using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Collections;
namespace GEM_NET_LIB
{
    public interface IReaderHandleMessage
    {
        void HandleMessage(int msgID, int data_type, MemoryStream data);
    }
    internal class CNetStreamBuffer
    {
        private ArrayList m_streanList = new ArrayList();
        private int m_nActiveStreamPosition = 0;
        private const int m_nMaxStreamCount = 2;
        public CNetStreamBuffer()
        {
            for (int i=0;i<m_nMaxStreamCount;i++)
            {
                m_streanList.Add(new MemoryStream());
            }
        }
        public MemoryStream GetActivedStream()
        {
            return (MemoryStream)m_streanList[m_nActiveStreamPosition];
        }

        public MemoryStream MoveStream(int index)
        {
            MemoryStream oldStream = (MemoryStream)m_streanList[m_nActiveStreamPosition];
            if(index>0)
            {
                m_nActiveStreamPosition = (m_nActiveStreamPosition+1)%m_nMaxStreamCount;
                MemoryStream newStream = (MemoryStream)m_streanList[m_nActiveStreamPosition];
                newStream.SetLength(0);
                newStream.Write(oldStream.GetBuffer(),(int)(index),(int)(oldStream.Length-index));
                oldStream.SetLength(0);
                return newStream;
            }
            else 
            {
                oldStream.SetLength(0);
            }
            return oldStream;
        }
        public void Reset()
        {
            for(int i=0;i<m_nMaxStreamCount;i++)
            {
                ((MemoryStream)m_streanList[i]).SetLength(0);
            }
        }


    }

  
   public class NetStreamReader: INetMessageReader
    {
        private IReaderHandleMessage m_HandleMessage;
        private int m_nProgress=0;
        private int m_nStreamSize=0;
        private int m_nMsgID=0;
        private int m_nDataType = 0;
        private static readonly int m_nMaxDataSize = 200*1024;
        private CNetStreamBuffer m_NetBuffer = new CNetStreamBuffer();
        private MemoryStream m_MsgDataBody = new MemoryStream();
        public IReaderHandleMessage HandleMessage
        {
            set{m_HandleMessage = value;}
        }
        void INetMessageReader.DidReadData(byte[] data,int size)
        {
            MemoryStream activedStream = m_NetBuffer.GetActivedStream();
            activedStream.Write(data, 0, size);
            byte[] nowData = activedStream.GetBuffer();
            long nowStreamLength = activedStream.Length;
            while(true)
            {
                try
                {
                    if(m_nProgress != 4)
                    {
                        while(m_nProgress<3)
                        {
                            int tmpValue = 0;
                            if(nowStreamLength <sizeof(int))
                            {
                                if(m_nProgress!=4)
                                {
                                    m_nProgress = 0;
                                }
                                return;
                            }
                            tmpValue = BitConverter.ToInt32(nowData,m_nProgress*sizeof(int));
                            tmpValue = IPAddress.NetworkToHostOrder(tmpValue);
                            switch(m_nProgress)
                            {
                                case 0:
                                    m_nStreamSize = tmpValue;
                                    break;
                                case 1:
                                    m_nMsgID = tmpValue;
                                    break;
                                case 2:
                                    m_nDataType = tmpValue;
                                    break;
                            }
                            m_nProgress++;
                            nowStreamLength -= sizeof(int);
                        }
                        if (CheckHead())
                        {
                            m_nProgress = 4;
                        }
                        else
                        {
                            activedStream.SetLength(0);
                            return;
                        }
                    }
                    int nDataSize = m_nStreamSize - 8;
                    bool bReturn = false;
                    m_MsgDataBody.SetLength(0);
                    if(nDataSize >0 )
                    {
                        if(nowStreamLength >= nDataSize)
                        {
                            m_MsgDataBody.Write(nowData, (int)(activedStream.Length - nowStreamLength), nDataSize);
                            nowStreamLength -= nDataSize;
                        }
                        else
                        {
                            bReturn = true;
                        }
                    }
                    activedStream = m_NetBuffer.MoveStream((int)(activedStream.Length - nowStreamLength));
                    if (bReturn) return;
                    else
                    {
                        try
                        {
                            m_HandleMessage.HandleMessage(m_nMsgID, m_nDataType, m_MsgDataBody);
                        }
                        catch(Exception e)
                        {
                            NGUIDebug.Log(e);
                        }
                    }
                               nowData = activedStream.GetBuffer();
                nowStreamLength = activedStream.Length;
                m_nProgress = 0;
                }
                catch
                {
                    if (m_nProgress != 4)
                        m_nProgress = 0;
                    break;
                }

            }
            
        }

        void INetMessageReader.Reset()
        {
            m_nProgress = 0;
            m_nStreamSize = 0;
            m_nMsgID = 0;
            m_nDataType = 0;
            m_NetBuffer.Reset();
            m_MsgDataBody.SetLength(0);
        }
        bool CheckHead()
        {
            if (m_nStreamSize < 8 || m_nStreamSize > NetStreamReader.m_nMaxDataSize)
                return false;
            return true;
        }
    }
}
