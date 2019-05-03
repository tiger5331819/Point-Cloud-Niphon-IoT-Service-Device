using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

/***
 *                   /88888888888888888888888888\
 *                   |88888888888888888888888888/
 *                    |~~____~~~~~~~~~"""""""""|
 *                   / \_________/"""""""""""""\
 *                  /  |              \         \
 *                 /   |  88    88     \         \
 *                /    |  88    88      \         \
 *               /    /                  \        |
 *              /     |   ________        \       |
 *              \     |   \______/        /       |
 *   /"\         \     \____________     /        |
 *   | |__________\_        |  |        /        /
 * /""""\           \_------'  '-------/       --
 * \____/,___________\                 -------/
 * ------*            |                    \
 *   ||               |                     \
 *   ||               |                 ^    \
 *   ||               |                | \    \
 *   ||               |                |  \    \
 *   ||               |                |   \    \
 *   \|              /                /"""\/    /
 *      -------------                |    |    /
 *      |\--_                        \____/___/
 *      |   |\-_                       |
 *      |   |   \_                     |
 *      |   |     \                    |
 *      |   |      \_                  |
 *      |   |        ----___           |
 *      |   |               \----------|
 *      /   |                     |     ----------""\
 * /"\--"--_|                     |               |  \
 * |_______/                      \______________/    )
 *                                               \___/
 */
//期待
//将所有音乐都打开
//拯救我不断摇摆的姿态
//期待
//我想要无限的精彩
//期待
//驱逐所有麻木倦怠
//给我你所有的信赖
//翻涌的律动让我醒过来
//期待
//我想要强烈的节拍
//期待
//充满新鲜感的未来
//(((((ી(･◡･)ʃ)))))

namespace EVCS
{
    public class Event:EventArgs
    {
        public int ID;
        public string TypeEvent;
        public Event(int id,string s)
        {
            ID = id;
            TypeEvent = s;
        }
        public Event(string s)
        {
            ID = 1;//临时变量
            TypeEvent = s;
        }
    }
    public class SerializableFind : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;  // 当前程序集
            return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        }
    }
    public class ErrorMessage
    {
        static string logPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/日志文件";
        static StreamWriter streamWriter = new StreamWriter(logPath + "/" + DateTime.Now.ToLongDateString().ToString() + "日志.txt", true);
        public static void GetError(Exception ex)
        {
            //创建日志文件夹
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss     ") + ex.Message);
            Console.WriteLine(ex);
            streamWriter.WriteLine(ex.Source + ":" + ex.TargetSite);
            Console.WriteLine(ex.Source + ":" + ex.TargetSite);
            streamWriter.WriteLine(ex.StackTrace);
            Console.WriteLine(ex.StackTrace);
        }
    }
    public class IoT_Net
    {
        public string TypeNet;
        protected Socket socket;
        protected IPAddress ip;
        protected IPEndPoint point;
        protected Boolean connectflag = true;
        public Queue<Socket> socketslist;

        public IoT_Net(string typenet)
        {
            TypeNet = typenet;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketslist = new Queue<Socket>(50);
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
        public bool Send(Package package, Socket s)
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
                s.Send(bytes, bytes.Length, 0);
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
                return false;
            }
            return true;
        }
        protected void LinkBind()
        {
            //创建监听用的Socket
            /*
               AddressFamily.InterNetWork：使用 IP4地址。
               SocketType.Stream：支持可靠、双向、基于连接的字节流，而不重复数据。
               此类型的 Socket 与单个对方主机进行通信，并且在通信开始之前需要远程主机连接。
               Stream 使用传输控制协议 (Tcp) ProtocolType 和 InterNetworkAddressFamily。
               ProtocolType.Tcp：使用传输控制协议。
             */
            try
            {
                socket.Bind(point);
                socket.Listen(10);
                Console.WriteLine("服务器开始监听");

                //这个线程用于实例化socket，每当一个子端connect时，new一个socket对象并保存到相关数据集合
                Thread acceptInfo = new Thread(AcceptInfo);
                acceptInfo.IsBackground = true;
                acceptInfo.Start();
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
            }
        }
        /// <summary>
        ///每有一个客户端连接，就会创建一个socket对象用于保存客户端传过来的套接字信息
        /// </summary>
        /// <param name="o"></param>
        public virtual void AcceptInfo()
        {
            while (true)
            {
                try
                {
                    //没有客户端连接时，accept会处于阻塞状态
                    Socket tSocket = socket.Accept();

                    string point = tSocket.RemoteEndPoint.ToString();
                    Console.WriteLine(point + "连接成功！");

                    socketslist.Enqueue(tSocket);
                }
                catch (Exception ex)
                {
                    ErrorMessage.GetError(ex);
                    break;
                }
            }
        }

        protected void Connect()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    if (connectflag)
                    {
                        try
                        {
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socket.Connect(point);

                            Thread waitcommand = new Thread(ReceiveCommand);
                            waitcommand.IsBackground = true;
                            waitcommand.Start();
                            Console.WriteLine("Link server");
                            connectflag = false;
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage.GetError(ex);
                            Thread.Sleep(100);
                        }
                    }
                    else Thread.Sleep(1000);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        public virtual void ReceiveCommand()
        {

        }

        static public Package BytesToPackage(byte[] buffer)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(buffer, 0, buffer.Length);
                    ms.Flush();
                    ms.Position = 0;
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Binder = new SerializableFind();
                    Package package = (Package)bf.Deserialize(ms);
                    return package;
                }
            }
            catch (Exception e)
            {
                ErrorMessage.GetError(e);
                return new Package();
            }
        }
    }

    [Serializable]
    public class IoT_Data
    {
        public string TypeData;
        public string TypeSystem;
        public string ID;

        public IoT_Data(string typedata, string typesystem, string id = "0")
        {
            TypeData = typedata;
            TypeSystem = typesystem;
            ID = id;
        }
        public IoT_Data()
        {

        }
    }

    [Serializable]
    public struct Package
    {
        public Messagetype message;
        public byte[] data;
    }
}
