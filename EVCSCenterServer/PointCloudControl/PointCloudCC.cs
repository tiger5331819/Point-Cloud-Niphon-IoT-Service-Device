using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static System.Console;

namespace EVCS
{
    class PointCloudCC
    {
        Special cloud;
        public PointCloudDeviceC[] DeviceC = new PointCloudDeviceC[200];
        PointCloudUserC[] UserC = new PointCloudUserC[200];
        
        PointCloudCC cc;
        string article;

        public PointCloudCC(ref Special c)
        {
            cloud = c;
            for (int i = 0; i < 200; i++)
            {
                DeviceC[i] = null;
                UserC[i] = null;
            }
            cc = this;
            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true;
            check.Start();
            
            shell();

        }
        void CreateThreadToCheckData()
        {
            Boolean []DeviceList=new Boolean [200];
            Boolean []UserList = new Boolean[200];
            for (int i = 0; i < 200; i++)
            {
                DeviceList[i] = false;
                UserList[i] = false;
            }

            while(true)
            {
                for(int i=0;i<200;i++)
                {
                    IPList ip = cloud.Data.iplist[i];
                    if (ip.ID != null)
                    {
                        if (!DeviceList[i])
                        {
                            DeviceList[i] = true;

                            DeviceC[i] = new PointCloudDeviceC(ref cloud.Data.Devicedata[i], ref cloud,i);
                        }
                    }
                    else if (DeviceList[i]) { DeviceList[i] = false;Console.WriteLine(cloud.Data.Devicedata[i].ID); }

                    ip = cloud.Data.UserList[i];
                    if (ip.ID != null)
                    {
                        if (!UserList[i])
                        {
                            UserList[i] = true;
                            UserC[i] = new PointCloudUserC(ref cloud.Data.Userdata[i], ref cloud,ref cc,i);
                        }
                    }
                    else if (UserList[i]) UserList[i] = false;
                }
                Thread.Sleep(100);
            }

        }

        public void shell()
        {
            var Devicelist = from r in cloud.Data.iplist where r.ID != null orderby r.ID descending select r;
            var Userlist = from r in cloud.Data.UserList where r.ID != null orderby r.ID descending select r;
            while (true)
            {
                try
                {
                    article = Console.ReadLine();
                    switch (article)
                    {
                        case "DeviceList":foreach (IPList r in Devicelist) WriteLine(r.ID + " " + r.IP); break;
                        case "UserList": foreach (IPList r in Userlist) WriteLine(r.ID + " " + r.IP); break;
                        case "Select": Select(); break;
                        case "Data": Console.WriteLine(cloud.Data.Devicedata[0].ID); break;
                        default:break;
                    }
                    article = null;
                }
                catch (Exception) { }

            }
        }

        void Select()
        {
            
            article = null;
            article = ReadLine();

            int flag = Convert.ToInt32(article);
            PointCloudDeviceC d = DeviceC[flag];
            WriteLine(d.ID());

            while(true)
            {
                article = null;
                article = ReadLine();
                switch(article)
                {
                    case "back":return;
                    case "play":TODO(d,Codemode.play); break;
                    case "monitor":TODO(d, Codemode.monitor);break;
                    case "sendvolume": TODO(d, Codemode.sendvolume); break;
                    case "stopsendvolume": TODO(d, Codemode.stopsendvolume); break;
                    case "stop": TODO(d, Codemode.stop); break;
                }
            }

        }
        void TODO(PointCloudDeviceC Device,Codemode codemode)
        {
            Device.SendCode(codemode);
        }

    }
}
