using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EVCS.PointCloudControl;

namespace EVCS
{
    class PointCloudUserC
    {
        PointCloudCC cc;
        public User data;
        Special cloud;
        PointCloudDeviceC deviceC;
        PointCloudUserC userC;
        PointCloudMailBox MailBox;
        public string ID
        {
            get { return data.ID; }
        }

        public PointCloudUserC(ref Socket socket, ref Package package, ref Special s,ref PointCloudCC c, EventManager m)
        {
            cloud = s;
            cc = c;
            deviceC = null;
            userC = this;
            data = PointCloud_EVCS_Core.CreatUserData(ref socket, package);
            MailBox = new UserMailBox(ref data,s.Data.iplist);
            MailBox.Send(CenterServerNet.CreatIPListToPackage(Messagetype.codeus, s.Data.iplist));
            m.Event += new EventManager.NewEventHandler(sendiplist);

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
                if (!data.Live) cloud.Data.iplist.Remove(new IPList() { ID = data.ID, IP = data.IP });
                Thread.Sleep(400);
            }
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

           int i = cloud.Data.iplist.FindIndex(x => x.IP == data.DeviceID);
           deviceC = cc.DeviceC[i];
           deviceC.adduser(ref userC);
           deviceC.order.Enqueue(codemode);
            
        }
        void release()
        {
            if (deviceC.removeuser()) { data.DeviceID = null;deviceC = null; }
            else Console.WriteLine("user release error");
        }
        void codemode(Codemode codemode)
        {
            deviceC.order.Enqueue(codemode);
        }

        void updateTODO()
        {
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
        void sendiplist(object o,Send_IPList e)
        {
            MailBox.Send(CenterServerNet.CreatIPListToPackage(Messagetype.codeus, cloud.Data.iplist));
            Console.WriteLine("updatelist");
        }
        public void Unregister(EventManager m)
        {
            m.Event -= new EventManager.NewEventHandler(sendiplist);
        }
    }

}
