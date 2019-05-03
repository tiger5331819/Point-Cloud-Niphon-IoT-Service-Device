using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using static System.Console;

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
        public DeviceData(string typedata, string typesystem)
            : base(typedata, typesystem)
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        public DeviceData() : base()
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        public DeviceData GetData()
        {
            DeviceData data = new DeviceData(this.TypeData, this.TypeSystem);
            data.ID = this.ID;
            data.volume = this.volume;
            for (int i = 0; i < 3; i++)
            {
                data.configtime[i] = this.configtime[i];
            }
            return data;
        }
    }

    public class Device : DeviceData
    {
        public string IP=null;
        public Boolean Live = false;
        public Messagetype messagetype;
        public Codemode codemode;
        public Socket socket = null;

        public Device():base() 
        {

        }
        public void Update(DeviceData data)
        {
            this.TypeData = data.TypeData;
            this.TypeSystem = data.TypeSystem;
            this.ID = data.ID;
            for (int i = 0; i < 3; i++)
            {
                configtime[i] = data.configtime[i];
            }
            volume = data.volume;
        }
    }

    [Serializable]
    public class UserData : IoT_Data
    {
        public configtimexml[] configtime;
        public volumecontrol volume;

        public UserData(string typedata, string typesystem)
            : base(typedata, typesystem)
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        public UserData():base()
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        public UserData GetData()
        {
            UserData data = new UserData(this.TypeData, this.TypeSystem);
            data.volume = this.volume;
            for (int i = 0; i < 3; i++)
            {
                data.configtime[i] = this.configtime[i];
            }
            return data;
        }
    }

    public class User : UserData
    {
        public string IP;
        public string DeviceID;
        public Boolean Live = false;
        public Socket socket = null;
        public Messagetype messagetype;
        public Codemode codemode;
        public User():base()
        {
            DeviceID = null;
        }
        public void Update(UserData data)
        {
            for (int i = 0; i < 3; i++)
            {
                configtime[i] = data.configtime[i];
            }
            volume = data.volume;
        }
    }

    public class ServerData:IoT_Data
    {
        public List<IPList> iplist = new List<IPList>(200);
        public List<IPList> UserList = new List<IPList>(200);
        public NetIP ip;
        

        public ServerData(string typedata, string typesystem) :base(typedata,typesystem)
        {
           ip=new NetIP();
        }


    }

    public class CenterServerNet:IoT_Net
    {
        ServerData Data;

        Special cloud;

        public CenterServerNet(ref ServerData data,ref Special s):base("PointCloud-EVCS")
        {
            this.ip = IPAddress.Parse(data.ip.IP);
            this.point = new IPEndPoint(ip, data.ip.Point);
            this.cloud = s;
            Data = data;

            LinkBind();
        }


        static public Package CreatIPListToPackage(Messagetype messagetype,List<IPList>ipl)
        {
            Package package = new Package();
            package.message = messagetype;
            package.data = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, ipl);
                    ms.Flush();
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

        public static User NewUser(Package package)
        {
            User data = new User();
            UserData user = new UserData();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(package.data, 0, package.data.Length);
                ms.Flush();
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                user = (UserData)bf.Deserialize(ms);
                data.Update(user);
            }
            return data;
        }
        public static Device NewDevice(Package package)
        {
            Device data = new Device();
            DeviceData Ddata = new DeviceData();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(package.data, 0, package.data.Length);
                ms.Flush();
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                Ddata = (DeviceData)bf.Deserialize(ms);
                data.Update(Ddata);
            }
            return data;
        }

       static public Package CreatCodeToPackage_ToDevice(Codemode codemode)
        {
            Package package = new Package();
            package.message = Messagetype.order;
            switch (codemode)
            {
                case Codemode.stop: package.data = Encoding.UTF8.GetBytes("0"); break;
                case Codemode.play: package.data = Encoding.UTF8.GetBytes("1"); break;
                case Codemode.monitor: package.data = Encoding.UTF8.GetBytes("2"); break;
                case Codemode.sendvolume: package.data = Encoding.UTF8.GetBytes("3"); break;
                case Codemode.stopsendvolume: package.data = Encoding.UTF8.GetBytes("4"); break;
            }
            return package;
        }
    }
}

