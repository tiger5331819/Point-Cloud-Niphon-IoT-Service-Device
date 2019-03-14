using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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
            set {  ip = value;  }
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

    public class CenterNetClass
     {
        public string ID;//设备编号
        public  TypeNet typenet;

        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Dictionary<string, Socket> dic = new Dictionary<string, Socket>();

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
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        public virtual bool Send(Package package, string ip)
        {
            ///orrived
            return true;
        }

        public delegate Package CreateDataToPackage(Messagetype message);
        public delegate Package TypeDataToPackage(Package package, Messagetype messagetype);

        public Package BytesToPackage(byte []buffer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(buffer, 0, buffer.Length);
                ms.Flush();
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                Package package = (Package)bf.Deserialize(ms);
                return package;
            }
        }


        public Package DeviceDataToPackage(TypeData data, Messagetype messagetype = Messagetype.package)
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
                Console.WriteLine(ex.Message);

            }
            return package;
        }
        /// <summary>
        /// 算了这里我帮你写了，省的你自己再定义，当然这里还未完工
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Package UserDataToPackage(TypeData data, Messagetype messagetype = Messagetype.package)
        {
            Package package = new Package();
            package.data = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    switch(messagetype)
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
                Console.WriteLine(ex.Message);

            }
            return package;
        }
        public Package ServerDataToPackage(TypeData data, Messagetype messagetype = Messagetype.package)
        {
            Package package = new Package();
            package.data = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, package);
                    ms.Flush();

                    package.message = messagetype;
                    package.data = ms.ToArray();
                }
                return package;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            return package;
        }

        public delegate void PackageToData(Package package,int DeviceID);
        public delegate void PackageToData2(Package package, int DeviceID,Socket o);
        public delegate DeviceData PackageToDeviceData(Package package);
        public delegate UserData PackageToUserData(Package package);
    }

    [Serializable]
    public class TypeData
    {
       public Datatype datatype;
       public string ID;
       public configtimexml [] configtime;
       public volumecontrol volume;

       public bool flag;
       public Messagetype messagetype;
       public Codemode codemode;

        public TypeData()
        {
            datatype = new Datatype();
            configtime = new configtimexml[3];
            volume = new volumecontrol();
        }
    }
    [Serializable]
    public struct Package
    {
        public Messagetype message;
        public byte[] data;
    }
}
