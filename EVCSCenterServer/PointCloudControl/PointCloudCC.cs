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
        public List<PointCloudDeviceC>DeviceC = new List<PointCloudDeviceC>(50);//设备控制器表
        List<PointCloudUserC>UserC = new List<PointCloudUserC>(50);//用户控制器表
        
        PointCloudCC cc;

        public PointCloudCC(ref Special c)
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
                            DeviceC.Add(deviceC);
                            //断线重连原型
                            //if (DeviceC.Exists(x => x.ID == deviceC.ID))
                            //{
                            //    int i = DeviceC.FindIndex(x => x.ID == deviceC.ID);
                            //    DeviceC[i].giveLive = true;
                            //}
                            //else

                            IPList iP = new IPList();
                            iP.ID = deviceC.ID;
                            iP.IP = socket.RemoteEndPoint.ToString();
                            cloud.Data.iplist.Add(iP);
                        }
                    }
                    m.SimulateNewEvent(cloud.Data.iplist);
                }
                //当设备或用户离开时，从链接表中移除相关控制器
                int i = -1,j=-1;
                 i=DeviceC.FindIndex(x => x.giveLive == false);
                j = UserC.FindIndex(x => x.data.Live == false);
                if(i!=-1)DeviceC.RemoveAt(i);
                if (j != -1) {UserC[j].Unregister(m); UserC.RemoveAt(j); }
                Thread.Sleep(100);
            }
        }
    }
}
