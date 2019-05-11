using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Console;

namespace EVCS
{
    /// <summary>
    /// 服务器控制器
    /// </summary>
    class PointCloudCC
    {
        Special cloud;//总控制器映射
        List<PointCloudDeviceC> DeviceC = new List<PointCloudDeviceC>(50);//设备控制器表
        List<PointCloudUserC> UserC = new List<PointCloudUserC>(50);//用户控制器表
        PointCloudCC cc;
        EventManager m = new EventManager();
        public PointCloudCC(Special c)
        {
            cloud = c;
            cc = this;

            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true;
            check.Start();
        }
        /// <summary>
        /// 当设备或客户链接时，分配控制器
        /// </summary>
        void CreateThreadToCheckData()
        {
            m.DataLiveEvent += new EventManager.DataLiveHandler(DisposeControl);
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
                        PointCloudUserC userC = new PointCloudUserC(socket,package,cc,m);
                        UserC.Add(userC);

                        IPList iP = new IPList();
                        iP.ID = userC.ID;
                        iP.IP = socket.RemoteEndPoint.ToString();
                        cloud.IPmanager("Add","UserList", iP);
                    }
                    else
                    {
                        if (package.message == Messagetype.ID)
                        {
                            PointCloudDeviceC deviceC= new PointCloudDeviceC(socket,package,m);
                            DeviceC.Add(deviceC);

                            IPList iP = new IPList();
                            iP.ID = deviceC.ID;
                            iP.IP = socket.RemoteEndPoint.ToString();
                            cloud.IPmanager("Add","DeviceList", iP);   
                        }
                    }
                    m.SimulateNewIPLEvent(cloud.DeviceList);                  
                }
                Thread.Sleep(100);
            }
        }
        public PointCloudDeviceC FindDeviceC(string DeviceID)
        {
            return DeviceC.Find(x=>x.ID==cloud.FindIp("DeviceList",DeviceID).ID);
        }
        void DisposeControl(object o,DataLive dataLive)
        {
            int i = -1;
            if(dataLive.ControlType=="Device")
            {
                i = DeviceC.FindIndex(x => x.ID==dataLive.ControlName);
                DeviceC[i].userRelease();
                cloud.IPmanager("Remove", "DeviceList", cloud.FindIp("DeviceList", DeviceC[i].ID));
                if (i != -1) DeviceC.RemoveAt(i);
                Console.WriteLine("RemoveDevice");
                m.SimulateNewIPLEvent(cloud.DeviceList);
            }
            if(dataLive.ControlType=="User")
            {
                i = UserC.FindIndex(x => x.ID == dataLive.ControlName);
                UserC[i].release();
                cloud.IPmanager("Remove", "UserList", cloud.FindIp("UserList", UserC[i].ID));
                UserC[i].Unregister();
                if (i != -1) UserC.RemoveAt(i);
                Console.WriteLine("RemoveUser");
            }
        }
    }
}
