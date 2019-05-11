using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EVCS.PointCloudControl;

namespace EVCS
{
    /// <summary>
    /// 设备控制器
    /// </summary>
    class PointCloudDeviceC
    {
        Device data;
        PointCloudUserC user=null;
        PointCloudMailBox MailBox;
        Queue<Codemode> order = new Queue<Codemode>();
        EventManager manager;
        public string ID
        {
            get { return data.ID; }
        }
        public PointCloudDeviceC(Socket socket,Package package,EventManager m)
        {
            data = PointCloud_EVCS_Core.CreatDeviceData(socket, package);//DI
            MailBox = new DeviceMailBox(data);
            manager = m;

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
                else {Thread.Sleep(500);  sum++; }
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
            manager.SimulateNewDataLiveEvent(data.TypeData, data.ID, false);
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
        public bool adduser(PointCloudUserC d)
        {
            user = d;
            return false;
        }
        public bool removeuser()
        {
            user = null;
            return SendCode(Codemode.stopsendvolume);
        }
        public void userRelease()
        {
            if(user!=null)
            {
                user.release(0);
            }
        }
        public void updatemessage(volumecontrol newvolume,configtimexml[] newconfig)
        {
            data.volume.carName = newvolume.carName; data.configtime = newconfig;
            data.volume.carVolume = newvolume.carVolume;
            SendUpdate();
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
        public bool addorder(Codemode codemode)
        {
            try
            {
                order.Enqueue(codemode);
                return true;
            }
            catch(Exception ex)
            {
                ErrorMessage.GetError(ex);
                return false;
            }
        }
    }
}
