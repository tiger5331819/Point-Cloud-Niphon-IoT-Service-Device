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
    public class DeviceData : TypeData
    {
        public DeviceData Newdata = null;
        public volumecontrol newvolumecontrol;
        public NetIP ip;
        public string IP=null;
        public Boolean Live = false;

        public DeviceData()
        {
            ip = new NetIP();
        }
    }

    [Serializable]
    public class UserData : TypeData
    {
        public NetIP ip;
        public string IP;
        public string DeviceID;
        public Boolean Live = false;

        public UserData()
        {
            DeviceID = null;
            ip = new NetIP();
        }
    }

    public class ServerData
    {
        public IPList[] iplist = new IPList[200];
        public IPList[] UserList = new IPList[200];
        public DeviceData []Devicedata=new DeviceData[100];
        public UserData []Userdata=new UserData[100];
        public NetIP ip=new NetIP();
        public string ID;

        public ServerData()
        {
            for (int i=0;i<100;i++)
            {
                Devicedata[i] = null;
                Userdata[i] = null;

                iplist[i].ID = null;
                iplist[i].IP = null;

                UserList[i].ID = null;
                UserList[i].IP = null;
            }            
        }


    }

    public class CenterServerNet:CenterNetClass
    {
        ServerData Data;
        IPAddress ip;
        IPEndPoint point;
        Special cloud;

        public CenterServerNet(ref ServerData data,ref Special s)
        {
            typenet = TypeNet.CenterSever;
            ip = IPAddress.Parse(data.ip.IP);
            point = new IPEndPoint(ip, data.ip.Point);
            this.cloud = s;
            Data = data;
        }

        /// <summary>
        /// 创建一个服务器socket对象，走到监听端口这一步，新建线程，并将服务器socket对象传递过去，
        /// 用于实时创建连接的客户端socket
        /// </summary>
        /// <returns></returns>
        public bool serverLink()
        {
            //创建监听用的Socket
            /*
               AddressFamily.InterNetWork：使用 IP4地址。
               SocketType.Stream：支持可靠、双向、基于连接的字节流，而不重复数据。
               此类型的 Socket 与单个对方主机进行通信，并且在通信开始之前需要远程主机连接。
               Stream 使用传输控制协议 (Tcp) ProtocolType 和 InterNetworkAddressFamily。
               ProtocolType.Tcp：使用传输控制协议。
             */
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(point);
                socket.Listen(10);
                Console.WriteLine("服务器开始监听");

                //这个线程用于实例化socket，每当一个子端connect时，new一个socket对象并保存到相关数据集合
                Thread acceptInfo = new Thread(AcceptInfo);
                acceptInfo.IsBackground = true;
                acceptInfo.Start(socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        /// <summary>
        ///每有一个客户端连接，就会创建一个socket对象用于保存客户端传过来的套接字信息
        /// </summary>
        /// <param name="o"></param>
        void AcceptInfo(object o)
        {
            Socket socket = o as Socket;
            while (true)
            {
                try
                {
                    //没有客户端连接时，accept会处于阻塞状态
                    Socket tSocket = socket.Accept();

                    string point = tSocket.RemoteEndPoint.ToString();
                    Console.WriteLine(point + "连接成功！");
     
                    Thread th = new Thread(ReceiveMsg);
                    th.IsBackground = true;
                    th.Start(tSocket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }
        void ReceiveMsg(object o)
        {
            Socket client = o as Socket;

            void ipinfo()
            {
                byte[] buf = new byte[1024 * 1024];

                client.Receive(buf);
                Package package = BytesToPackage(buf);


                if (package.message == Messagetype.codeus)
                {
                    PackageToUserData packageToUserData = new PackageToUserData(NewUser);
                    int i = 0;
                    foreach (IPList ip in Data.UserList)
                    {              
                        if (ip.ID == null)
                        {
                            Data.Userdata[i] = packageToUserData(package);
                            Data.Userdata[i].IP = client.RemoteEndPoint.ToString();
                            Data.Userdata[i].Live = true;
                            Data.Userdata[i].socket = client;

                            Data.UserList[i].ID = Data.Userdata[i].ID;
                            Data.UserList[i].IP = client.RemoteEndPoint.ToString();                           
                            break;
                        }
                        i++;
                    }
                    Send(CreatIPListToPackage(Messagetype.codeus, Data.iplist), client);
                }
                else
                {
                    if (package.message == Messagetype.ID)
                    {
                        PackageToDeviceData packageToDeviceData = new PackageToDeviceData(NewDevice);

                        int i = 0;
                        foreach (IPList ip in Data.iplist)
                        {

                            if (ip.ID == null)
                            {
                                Data.Devicedata[i] = packageToDeviceData(package);
                                Data.Devicedata[i].IP= client.RemoteEndPoint.ToString();
                                Data.Devicedata[i].Live = true;
                                Data.Devicedata[i].socket = client;

                                Data.iplist[i].ID = Data.Devicedata[i].ID;
                                Data.iplist[i].IP = client.RemoteEndPoint.ToString();

                                //Thread thread = new Thread(DeviceReceive);
                                //thread.IsBackground = true;
                                //thread.Start(client);
                                break;
                            }
                            i++;
                        }
                    }
                }

            }
            ipinfo();
            
        }

        static public Package CreatIPListToPackage(Messagetype messagetype,IPList []ipl)
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
                Console.WriteLine(ex.Message);
            }
            return package;
        }

        UserData NewUser(Package package)
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
            return data;
        }
        DeviceData NewDevice(Package package)
        {
            DeviceData data = new DeviceData();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(package.data, 0, package.data.Length);
                ms.Flush();
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                data = (DeviceData)bf.Deserialize(ms);
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

