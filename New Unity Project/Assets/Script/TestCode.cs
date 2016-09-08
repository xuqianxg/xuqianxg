using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using net_protocol;

public class TestCode : MonoBehaviour
{
    string strMsg = "";
     void Awake()
    {
        Client.Instance.Init();
        Client.NetWork.Connect("127.0.0.1", 9999);
       
            

           

        //Client.Instance.Test += new Client.OnTest(ttest);
    }

    void Start()
     {

         Client.NetWork.SendNetEmptyMessage(2);
        // InvokeRepeating("RepeatTest", 0, 3);
     }


    void RepeatTest()
    {
        PBString pb = new PBString();
        pb.str_value = "hello world";
        Client.NetWork.SendNetMessage<PBString>(1, pb);
    }

     void Update()
    {
        Client.NetWork.Update();
    }


     void Destroy()
     {
         Client.NetWork.DisConnect();
     }



    void ttest(string str)
    {
        strMsg = str;
    }

    void OnGUI()
    {
        GUI.TextField(new Rect(100, 100, 150, 20), strMsg);
    }

}