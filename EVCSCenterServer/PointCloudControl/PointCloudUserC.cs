using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EVCS.PointCloudControl;

namespace EVCS
{
    class PointCloudUserC
    {
        PointCloudCC cc;
        public UserData data;
        Special cloud;
        PointCloudDeviceC deviceC;
        PointCloudUserC userC;
        PointCloudMailBox MailBox;
        int UserID;

        public PointCloudUserC(ref UserData d,ref Special s,ref PointCloudCC c,int i)
        {
            data = d;
            cloud = s;
            cc = c;
            deviceC = null;
            userC = this;
            MailBox = new UserMailBox(ref d, s.Data.iplist);
            UserID = i;

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
                if (!data.Live) cloud.Data.iplist[UserID].ID = null;
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
            for(int i=0;i<200;i++)
            {
                IPList ip = cloud.Data.iplist[i];
                if (ip.IP == data.DeviceID)
                {
                    deviceC = cc.DeviceC[i];
                    deviceC.adduser(ref userC,ip.ID);
                    deviceC.order.Enqueue(codemode);
                }
            }
            
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
            if (MailBox.Send(CenterServerNet.UserDataToPackage(data, messagetype))) return true;
            else return false;
        }

    }
}
