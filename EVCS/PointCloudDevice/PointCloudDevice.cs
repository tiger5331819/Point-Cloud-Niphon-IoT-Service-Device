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
    public struct configtimexml
    {
        public string time;
        public string beginhour;
        public string beginminute;
        public string endminute;
        public string endhour;
    }
    [Serializable]
    public struct NetIP
    {
        string ip;
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }
        int point;
        public int Point
        {
            get { return point; }
            set { point = value; }
        }
    }
    [Serializable]
    public struct volumecontrol
    {
        public int carNo;
        public string carName;
        public decimal? carVolume;
        public string carSN;
        public decimal? volume;

        public string count;
        public int Loadingrate;
        public string Endtime;
        public string Begintime;

    }
    [Serializable]
    public class DeviceData : IoT_Data
    {
        public configtimexml[] configtime;
        public volumecontrol volume;
        public DeviceData(string typedata,string typesystem)
            :base(typedata,typesystem)
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        public DeviceData():base()
        {

        }
        public DeviceData GetData()
        {
            DeviceData data = new DeviceData(this.TypeData,this.TypeSystem);
            data.ID = this.ID;
            data.volume = this.volume;
            for(int i=0;i<3;i++)
            {
                data.configtime[i] = this.configtime[i];
            }
            return data;
        }
    }
    public class Device : DeviceData
    {
       public DeviceData Newdata=null;
        public NetIP ip;
        public volumecontrol newvolumecontrol;
        public Boolean Live = false;
        public bool flag;
        public Messagetype messagetype;
        public Codemode codemode;
        public Device(string typedata, string typesystem):base(typedata, typesystem)
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
   public class DeviceNet : IoT_Net
    {
        Device Data;
        public DeviceNet(ref Device data,string typenet):base(typenet)
        {
            ip = IPAddress.Parse(data.ip.IP);
            point = new IPEndPoint(ip, data.ip.Point);
            Data = data;
            Connect();
        }

        static public Package DeviceDataToPackage(DeviceData data, Messagetype messagetype = Messagetype.package)
        {
            Package package = new Package();
            package.data = null;
            
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    switch (messagetype)
                    {
                        case Messagetype.carinfomessage: bf.Serialize(ms, data.volume); break;
                        case Messagetype.volumepackage: bf.Serialize(ms, data.volume); break;
                        case Messagetype.package: bf.Serialize(ms, data); break;
                        case Messagetype.ID: bf.Serialize(ms, data); break;
                    }
                    
                    ms.Flush();
                    package.message = messagetype;
                    
                    package.data = ms.ToArray();
                    
                }
                return package;
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
            }
            return package;
        }
        public override void ReceiveCommand()
        {
            Send(DeviceDataToPackage(Data.GetData(), Messagetype.ID));
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    socket.Receive(buffer);
                    Package package = BytesToPackage(buffer);

                    //根据接受的命令去做不同的任务
                    switch (package.message)
                    {
                        case Messagetype.order: NewCode(package); break;
                        case Messagetype.package: NewDeviceData(package);break;
                        case Messagetype.update: Updatemessage(package);break;
                    }                   
                }
                catch (Exception ex)
                {
                    ErrorMessage.GetError(ex);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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

