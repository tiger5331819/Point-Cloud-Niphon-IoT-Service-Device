using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace EVCS
{
    [Serializable]
    public class DeviceData : TypeData
    {
       public DeviceData Newdata=null;
        public NetIP ip;
        public volumecontrol newvolumecontrol;
        public string IP;
        public Boolean Live = false;

        public DeviceData()
        {
            ip = new NetIP();
        }
        public bool newdatachange()
        {
            if (flag)
                return true;
            else return false;
        }

    }
   public class DeviceNet : DeviceNetClass
    {
        DeviceData Data;
        IPAddress ip;
        IPEndPoint point;
        Boolean connectflag = true;
        public DeviceNet(ref DeviceData data)
        {
            typenet = TypeNet.Device;

            ip = IPAddress.Parse(data.ip.IP);
            point = new IPEndPoint(ip, data.ip.Point);
            Data = data;
        }

       public bool userconnect()
        {
         
          while (true)
          {
                if (connectflag)
                {
                    try
                    {
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(point);


                        Send(DeviceDataToPackage(Data, Messagetype.ID));

                        Thread waitcommand = new Thread(ReceiveCommand);
                        waitcommand.IsBackground = true;
                        waitcommand.Start(socket);
                        Console.WriteLine("Link server");
                        connectflag = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else Thread.Sleep(1000);
          }
       }
        void ReceiveCommand(object s)
        {
            Socket command = s as Socket;
            PackageToData packageToData = new PackageToData(NewDeviceData);
            PackageToData packageToCode = new PackageToData(NewCode);
            PackageToData updateToData = new PackageToData(Updatemessage);
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    command.Receive(buffer);
                    Package package = BytesToPackage(buffer);
                    //根据接受的命令去做不同的任务
                    switch(package.message)
                    {
                        case Messagetype.order:packageToCode(package); break;
                        case Messagetype.package:packageToData(package);break;
                        case Messagetype.update:updateToData(package);break;
                    }                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    command.Shutdown(SocketShutdown.Both);
                    command.Close();
                    connectflag = true;
                    break;
                }
            }
        }
        /// <summary>
        /// example
        /// </summary>
        /// <param name="package"></param>
        void NewCode(Package package)
        {         
            Data.codemode=(Codemode)Convert.ToInt32(Encoding.UTF8.GetString(package.data, 0, package.data.Length));          
            Data.Newdata = null;
            Data.messagetype = package.message;

            Data.flag = true;         
        }
        /// <summary>
        /// example
        /// </summary>
        /// <param name="package"></param>
        void NewDeviceData(Package package)
        {
            DeviceData data = new DeviceData();          
            data.datatype = Datatype.Device;
            using (MemoryStream ms = new MemoryStream())
            {
               ms.Write(package.data, 0, package.data.Length);
               ms.Flush();
               ms.Position = 0;
               BinaryFormatter bf = new BinaryFormatter();
               data = (DeviceData)bf.Deserialize(ms);

               Data.Newdata = data;
               Data.messagetype = package.message;

               Data.flag = true;
            }            
        }
        void Updatemessage(Package package)
        {
            DeviceData data = new DeviceData();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(package.data, 0, package.data.Length);
                ms.Flush();
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                data = (DeviceData)bf.Deserialize(ms);

                Data.volume = data.volume;
                Data.configtime = data.configtime;
                Data.messagetype = package.message;

                Data.flag = true;
            }
        }
    }
}

