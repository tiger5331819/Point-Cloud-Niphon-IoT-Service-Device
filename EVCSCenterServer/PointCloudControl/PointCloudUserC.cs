using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EVCS.PointCloudControl;

namespace EVCS
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    class PointCloudUserC
    {
        PointCloudCC cc;
        User data;
        PointCloudDeviceC deviceC=null;
        PointCloudMailBox MailBox;
        EventManager manager;
        public string ID
        {
            get { return data.ID; }
        }
        public PointCloudUserC(Socket socket,Package package,PointCloudCC c, EventManager m)
        {
            cc = c;
            data = PointCloud_EVCS_Core.CreatUserData(socket, package);
            MailBox = new UserMailBox(data);

            manager = m;
            manager.IPLEvent += new EventManager.IPLHandler(sendiplist);//事件注册

            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true;
            check.Start();
        }
        void CreateThreadToCheckData()
        {
            async void Receive()
            {
                while(data.Live)
                if (await MailBox.DOReceive())
                {
                    switch (data.messagetype)
                    {
                        case Messagetype.order:orderTODO(); break;
                        case Messagetype.update:updateTODO();break;
                    }
                }
                     
            }
            Receive();
            while (data.Live)
            {
                
                Thread.Sleep(1000);
            }
            manager.SimulateNewDataLiveEvent(data.TypeData, data.ID, false);
        }
        void orderTODO()
        {
            switch(data.codemode)
            {
                case Codemode.monitor:monitor(data.codemode); break;
                case Codemode.release:release();break;
                default:codemode(data.codemode); break;
            }
        }
        void monitor(Codemode codemode)
        {
            deviceC = cc.FindDeviceC(data.DeviceID);
            if (deviceC == null) { Console.WriteLine("没有此设备");return; }
            deviceC.adduser(this);
            deviceC.addorder(codemode);
        }
        public void release(int flag=1)
        {
            if(deviceC!=null)
                if (flag==0||deviceC.removeuser()) { data.DeviceID = null;deviceC = null;Console.WriteLine("{0}:移除设备",data.ID); }
                else Console.WriteLine("user can not release");
            else Console.WriteLine("DeviceC is null");
        }
        void codemode(Codemode codemode)
        {
            if (deviceC != null)
                deviceC.addorder(codemode);
        }
        void updateTODO()
        {
            if (deviceC != null)
                deviceC.updatemessage(data.volume, data.configtime);
        }
        public void UpdateVolume(volumecontrol v)
        {
            data.volume = v;
            if (SendMessage(Messagetype.volumepackage)) Console.WriteLine("发送成功给用户！");
            else Console.WriteLine("error！");
        }
        public bool SendMessage(Messagetype messagetype)
        {
            if (MailBox.Send(MailBox.DataToPackage(messagetype))) return true;
            else return false;
        }

        /// <summary>
        /// 发送设备列表事件
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void sendiplist(object o,Send_IPList e)
        {
            MailBox.Send(CenterServerNet.CreatIPListToPackage(Messagetype.codeus, e.iPLists));
        }
        /// <summary>
        /// 注销事件
        /// </summary>
        public void Unregister()
        {
            manager.IPLEvent -= new EventManager.IPLHandler(sendiplist);
        }
    }

}
