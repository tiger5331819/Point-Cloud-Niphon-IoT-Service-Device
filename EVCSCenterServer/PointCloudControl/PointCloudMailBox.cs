using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EVCS.PointCloudControl
{
    /// <summary>
    /// 邮箱接口
    /// 接口实现：接收数据、发送数据、打包序列化
    /// </summary>
    public interface PointCloudMailBox
    {
        Task<bool> DOReceive();
        bool Send(Package package);
        Package DataToPackage(Messagetype messagetype = Messagetype.package);
    }

    public class DeviceMailBox:PointCloudMailBox
    {
        Socket socket=null;
        Device Data = null;

        delegate void PackageToData(Package package);
        public DeviceMailBox( Device d)
        {
            socket = d.socket;
            Data = d;
        }

        public bool Send(Package package)
        {
            try
            {
                byte[] bytes = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, package);
                    ms.Flush();
                    bytes = ms.ToArray();
                }
                socket.Send(bytes, bytes.Length, 0);
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
                return false;
            }
            return true;
        }

        public Task<bool>DOReceive()
        {
            return Task.Run<bool>(() => { return Receive(); });
        }


        /// <summary>
        /// 设备信息接收
        /// </summary>
        /// <param name="o"></param>
        bool Receive()
        {
            PackageToData packageToData = new PackageToData(NewvolumeData);
            //接受设备数据
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int n = socket.Receive(buffer);
                    Package package = CenterServerNet.BytesToPackage(buffer);

                    switch (package.message)
                    {
                        case Messagetype.package: NewDeviceData(package);return true;
                        case Messagetype.carinfomessage: packageToData(package); return true;
                        case Messagetype.volumepackage: packageToData(package); return true;
                        default:return false;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.GetError(ex);
                    Data.Live = false;
                    return false;
                }
        }

        void NewDeviceData(Package package)
        {
            try
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
                    Data.messagetype = package.message;
                    Data.IP = socket.RemoteEndPoint.ToString();
                    Data.Live = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 更改体积等信息
        /// </summary>
        /// <param name="package"></param>
        void NewvolumeData(Package package)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(package.data, 0, package.data.Length);
                    ms.Flush();
                    ms.Position = 0;
                    BinaryFormatter bf = new BinaryFormatter();

                    Data.volume = (volumecontrol)bf.Deserialize(ms);

                    Data.messagetype = package.message;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public Package DataToPackage(Messagetype messagetype = Messagetype.package)
        {
            Package package = new Package();
            package.data = null;
            DeviceData data = Data.GetData();
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
                        default: bf.Serialize(ms, data); break;
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
    }

    public class UserMailBox : PointCloudMailBox
    {
        Socket socket = null;
        User Data = null;

        public UserMailBox(User d)
        {
            socket = d.socket;
            Data = d;
        }
        public delegate void PackageToData(Package package,Messagetype messagetype);

        public bool Send(Package package)
        {
            try
            {
                byte[] bytes = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, package);
                    ms.Flush();
                    bytes = ms.ToArray();
                }
                socket.Send(bytes, bytes.Length, 0);
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
                return false;
            }
            return true;
        }

        public Task<bool> DOReceive()
        {
            return Task.Run<bool>(() => { return Receive(); });
        }
        /// <summary>
        /// 用户信息接收
        /// </summary>
        /// <param name="o"></param>
        public bool Receive()
        {
            PackageToData packageToData = new PackageToData(NewUserData);
            //接受用户数据
            try
            {
                byte[] buffer = new byte[1024 * 1024];
                int n = socket.Receive(buffer);
                Package package = CenterServerNet.BytesToPackage(buffer);

                if (package.message == Messagetype.codeus)
                {
                    string receive = Encoding.UTF8.GetString(package.data, 0, package.data.Length);
                    Data.DeviceID = receive;
                    return false;
                }
                else
                    switch (package.message)
                    {
                        case Messagetype.package:packageToData(package,package.message); return true;
                        case Messagetype.order:NewCode(package); return true;
                        case Messagetype.update:packageToData(package, package.message); return true;
                        default:return false;
                    }
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
                Data.Live = false;
                return false;
            }
        }

        void NewUserData(Package package,Messagetype messagetype)
        {
            try
            {
                UserData data = new UserData();
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(package.data, 0, package.data.Length);
                    ms.Flush();
                    ms.Position = 0;
                    BinaryFormatter bf = new BinaryFormatter();
                    data = (UserData)bf.Deserialize(ms);
                }
                switch(messagetype)
                {
                    case Messagetype.package:Data.Update(data);break;
                    case Messagetype.update:
                        Data.messagetype = package.message;
                        Data.configtime = data.configtime;
                        Data.volume = data.volume;break;
                    default:Console.WriteLine("Func:NewUserData.messagetype is null"); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Func(NewUserData) error:" + ex.ToString());
            }
        }
        void NewCode(Package package)
        {
            Data.codemode = (Codemode)Convert.ToInt32(Encoding.UTF8.GetString(package.data, 0, package.data.Length));
            Data.messagetype = package.message;
            Console.WriteLine(Data.codemode);
        }
        public Package DataToPackage(Messagetype messagetype = Messagetype.package)
        {
            Package package = new Package();
            package.data = null;
            UserData data = Data.GetData();
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
                        default: bf.Serialize(ms, data); break;
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
    }
}
