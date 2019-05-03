using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EVCS.PointCloudControl;

namespace EVCS
{
    class PointCloudDeviceC
    {
        Special cloud;
        Device data;
        PointCloudUserC user;
        PointCloudMailBox MailBox;
        public Queue<Codemode> order = new Queue<Codemode>();
        public string ID
        {
            get { return data.ID; }
        }
        public bool giveLive
        {
            get { return data.Live; }
            set { data.Live = true; }
        }

        public PointCloudDeviceC(ref Socket socket,ref Package package,ref Special s)
        {
            cloud = s;
            user = null;
            data = PointCloud_EVCS_Core.CreatDeviceData(ref socket, package);
            MailBox = new DeviceMailBox(ref data);

            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true;
            check.Start();
        }

        void CreateThreadToCheckData()
        {
            int sum = 0;
            async void Receive()
            {
                while(data.Live)
                if (await MailBox.DOReceive())
                {
                    switch (data.messagetype)
                    {
                       case Messagetype.carinfomessage: ChangeCarMessage(); break;
                       case Messagetype.volumepackage: ChangeCarMessage(); break;
                       case Messagetype.package: ChangeCarMessage(); break;
                    }
                }
                else {Thread.Sleep(100);  sum++; }
            }
            Receive();
            while (data.Live)
            {
                if (sum == 100) { SendCode(Codemode.monitor);sum = 0; }

                Codemode code;
                if(order.TryDequeue(out code))
                {
                    SendCode(code);
                }
            }
            Console.WriteLine("data is dead");
            cloud.Data.iplist.Remove(new IPList() { ID = data.ID, IP = data.IP });
        }

        void ChangeCarMessage()
        {
            Console.WriteLine(data.ID);
            Console.WriteLine(data.volume.carName);
            Console.WriteLine(data.volume.carVolume);
            Console.WriteLine(data.volume.volume);
            if (user!=null)
            {
               user.UpdateVolume(data.volume);
            }
        }
        public bool adduser(ref PointCloudUserC d)
        {
            user = d;
            Console.WriteLine(user.data.ID);
            return false;
        }
        public bool removeuser()
        {
            try
            {
                user = null;
                SendCode(Codemode.stopsendvolume);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        public void updatemessage(volumecontrol newvolume,configtimexml[] newconfig)
        {
            data.volume.carName = newvolume.carName; data.configtime = newconfig;
            data.volume.carVolume = newvolume.carVolume;
            SendUpdate();
        }
       public bool SendMessage(Messagetype messagetype)
        {
            if (MailBox.Send(MailBox.DataToPackage(messagetype))) return true;
            else return false;
        }
        public bool SendCode(Codemode code)
        {
            if (MailBox.Send(CenterServerNet.CreatCodeToPackage_ToDevice(code))) return true;
            else { Console.WriteLine("fail"); return false; }
        }
        bool SendUpdate(Messagetype messagetype=Messagetype.update)
        {
            Codemode code;
            if (order.TryDequeue(out code))
            {
                Console.WriteLine("Orderqueue is not null.");
                return false;
            }
            if (MailBox.Send(MailBox.DataToPackage(messagetype))) return true;
            else return false;
        }
    }
}
