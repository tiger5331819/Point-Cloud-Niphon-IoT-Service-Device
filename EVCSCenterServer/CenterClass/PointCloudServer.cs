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
    /// <summary>
    /// 定时功能配置数据结构
    /// </summary>
    [Serializable]
    public struct configtimexml
    {
        public string time;
        public string beginhour;
        public string beginminute;
        public string endminute;
        public string endhour;
    }
    /// <summary>
    /// 网络IP数据结构
    /// </summary>
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
    /// <summary>
    /// 体积信息数据结构
    /// </summary>
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
    /// <summary>
    /// 传输所用设备核心数据结构，继承自核心数据类
    /// </summary>
    [Serializable]
    public class DeviceData : IoT_Data
    {
        public configtimexml[] configtime;
        public volumecontrol volume;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typedata">数据类型</param>
        /// <param name="typesystem">所属系统</param>
        public DeviceData(string typedata, string typesystem)
            : base(typedata, typesystem)
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        /// <summary>
        /// 空构造函数
        /// </summary>
        public DeviceData() : base()
        {
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
        /// <summary>
        /// 获取核心数据
        /// </summary>
        /// <returns>DeviceData类型的核心数据</returns>
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
    /// <summary>
    /// 服务端所使用的设备数据
    /// </summary>
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
        /// <summary>
        /// 将接受得到的设备核心数据更新到服务端设备数据中
        /// </summary>
        /// <param name="data">DeviceData类型的核心数据</param>
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
    /// <summary>
    /// 传输所用用户核心数据结构，继承自核心数据类
    /// </summary>
    [Serializable]
    public class UserData : IoT_Data
    {
        public configtimexml[] configtime;
        public volumecontrol volume;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typedata">数据类型</param>
        /// <param name="typesystem">所属系统</param>
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
        /// <summary>
        /// 获取核心数据
        /// </summary>
        /// <returns>UserData类型的核心数据</returns>
        public UserData GetData()
        {
            UserData data = new UserData(this.TypeData, this.TypeSystem);
            data.ID = ID;
            data.volume = volume;
            for (int i = 0; i < 3; i++)
            {
                data.configtime[i] = this.configtime[i];
            }
            return data;
        }
    }
    /// <summary>
    /// 服务端所使用的用户数据
    /// </summary>
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
        /// <summary>
        /// 将接受得到的用户核心数据更新到服务端用户数据中
        /// </summary>
        /// <param name="data">UserData类型的核心数据</param>
        public void Update(UserData data)
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
    /// <summary>
    /// 服务端所使用的数据，继承自核心数据类
    /// </summary>
    public class ServerData:IoT_Data
    {
        public List<IPList> DeviceList = new List<IPList>(200);//设备链接表
        public List<IPList> UserList = new List<IPList>(200);//用户链接表
        public NetIP ip;
        public ServerData(string typedata, string typesystem) :base(typedata,typesystem)
        {
           ip=new NetIP();
        }
    }
    /// <summary>
    /// 服务端网络服务类，实现最基本的网络服务，继承自核心类IoT_Net
    /// </summary>
    public class CenterServerNet:IoT_Net
    {
        ServerData Data;
        Special cloud;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data">服务端数据</param>
        /// <param name="s">服务端总控制映射</param>
        public CenterServerNet(ServerData data,Special s):base("PointCloud-EVCS")
        {
            ip = IPAddress.Parse(data.ip.IP);
            point = new IPEndPoint(ip, data.ip.Point);
            cloud = s;

            Data = data;
            LinkBind();
        }
        /// <summary>
        /// 基础服务：序列化设备链接表并打包
        /// </summary>
        /// <param name="messagetype">消息类型</param>
        /// <param name="ipl">设备链接表</param>
        /// <returns></returns>
        static public Package CreatIPListToPackage(Messagetype messagetype,IPList[] ipl)
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
        /// <summary>
        /// 基础服务：反序列化用户核心数据
        /// </summary>
        /// <param name="package">Package包</param>
        /// <returns></returns>
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
        /// <summary>
        /// 基础服务：反序列化设备核心数据
        /// </summary>
        /// <param name="package">Package包</param>
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
        /// <summary>
        /// 序列化用户命令并打包
        /// </summary>
        /// <param name="codemode">用户命令</param>
        /// <returns>Package包</returns>
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

