using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GEM_NET_LIB;

    class Client : Signleton<Client>
    {
        private CNetWorkGlobal m_NetWorkGlobal = new CNetWorkGlobal();
        private CClientHandleMessage m_ClientHandle = new CClientHandleMessage();


        public void Init()
        {
            m_NetWorkGlobal.RegisterHandleMessageClient(m_ClientHandle);
            m_NetWorkGlobal.RegisteNetWorkStateLister(ClientListenNetWork);
        }
        private void ClientListenNetWork(GEM_NET_LIB.EClientNetWorkState state,string ip, ushort port)
        {
             Console.WriteLine(string.Format("net error {0} {1}:{2:d}", state, ip, port));
            if(state == EClientNetWorkState.E_CNWS_ON_DISCONNECTED  || state == EClientNetWorkState.E_CNWS_ON_CONNECTED_FAILED)
            {

            }
        }
        public static  CNetWorkGlobal NetWork
        {
            get { return Client.instance.m_NetWorkGlobal; }
        }

    }
