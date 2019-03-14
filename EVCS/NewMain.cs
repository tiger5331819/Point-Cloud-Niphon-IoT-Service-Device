using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EVCS
{
    class NewMain
    {
        public NewMain()
        {
            Nform = this;
            cloud = new Special();
        }

        public Special cloud;
        public static NewMain Nform;

        public void NewMain_Load()
        {
            cloud.cloudnet.userconnect();//向服务器发送连接请求
            //Console.ReadLine();
        }

    }
}
