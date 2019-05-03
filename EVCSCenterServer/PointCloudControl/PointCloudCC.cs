using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Console;

namespace EVCS
{
    class PointCloudCC
    {
        Special cloud;
        public List<PointCloudDeviceC>DeviceC = new List<PointCloudDeviceC>(50);
        List<PointCloudUserC>UserC = new List<PointCloudUserC>(50);
        
        PointCloudCC cc;
        string article;

        public PointCloudCC(ref Special c)
        {
            cloud = c;
            cc = this;
            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true;
            check.Start();
            
            shell();

        }
        void CreateThreadToCheckData()
        {
            EventManager m = new EventManager();
            while(true)
            {
                Socket socket=null;
                cloud.cloudnet.socketslist.TryDequeue(out socket);
                if(socket!=null)
                {
                    byte[] buf = new byte[1024 * 1024];

                    socket.Receive(buf);
                    Package package = IoT_Net.BytesToPackage(buf);

                    if (package.message == Messagetype.codeus)
                    {
                        PointCloudUserC userC = new PointCloudUserC(ref socket, ref package, ref cloud, ref cc,m);
                        UserC.Add(userC);

                        IPList iP = new IPList();
                        iP.ID = userC.ID;
                        iP.IP = socket.RemoteEndPoint.ToString();
                        cloud.Data.UserList.Add(iP);
                    }
                    else
                    {
                        if (package.message == Messagetype.ID)
                        {
                            PointCloudDeviceC deviceC= new PointCloudDeviceC(ref socket,ref package,ref cloud);

                            //if (DeviceC.Exists(x => x.ID == deviceC.ID))
                            //{
                            //    int i = DeviceC.FindIndex(x => x.ID == deviceC.ID);
                            //    DeviceC[i].giveLive = true;
                            //}
                            //else

                            DeviceC.Add(deviceC);

                            IPList iP = new IPList();
                            iP.ID = deviceC.ID;
                            iP.IP = socket.RemoteEndPoint.ToString();
                            cloud.Data.iplist.Add(iP);
                        }
                    }
                    m.SimulateNewEvent(cloud.Data.iplist);
                }

                int i = -1,j=-1;
                 i=DeviceC.FindIndex(x => x.giveLive == false);
                j = UserC.FindIndex(x => x.data.Live == false);
                if(i!=-1)DeviceC.RemoveAt(i);
                if (j != -1) {UserC[j].Unregister(m); UserC.RemoveAt(j); }
                Thread.Sleep(100);
            }

        }

        public void shell()
        {
            while (true)
            {
                try
                {
                    article = Console.ReadLine();
                    switch (article)
                    {
                        case "DeviceList":foreach (IPList r in cloud.Data.iplist) WriteLine(r.ID + " " + r.IP); break;
                        case "UserList": foreach (IPList r in cloud.Data.UserList) WriteLine(r.ID + " " + r.IP); break;
                        case "Select": Select(); break;
                        default:break;
                    }
                    article = null;
                }
                catch (Exception ex)
                {
                    ErrorMessage.GetError(ex);
                }

            }
        }

        void Select()
        {
            
            article = null;
            article = ReadLine();

            PointCloudDeviceC d = DeviceC.Find(x=>x.ID==article);
            WriteLine(d.ID);

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
