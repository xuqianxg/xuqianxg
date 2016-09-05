using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEM_NET_LIB;
using AsyncSocketServer;
using net_protocol;
using System.Threading;
namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoResetEvent autoReset = new AutoResetEvent(false);
            Client.NetWork.Connect("", 9999);
            PBString pb = new PBString();
            pb.str_value="hello world";
            Client.NetWork.SendNetMessage<PBString>(1, pb);
            autoReset.WaitOne();
        }
    }
}
