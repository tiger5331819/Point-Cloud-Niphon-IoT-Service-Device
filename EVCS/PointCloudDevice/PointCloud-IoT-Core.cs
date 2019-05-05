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
    /// <summary>
    /// 事件基类，所有的事件都继承与此
    /// </summary>
    public class Event : EventArgs
    {
        public int ID;
        public string TypeEvent;
        /// <summary>
        /// Event类的构造函数
        /// </summary>
        /// <param name="id">Event的编号</param>
        /// <param name="typeevent">Event的类别</param>
        public Event(int id, string typeevent)
        {
            ID = id;
            TypeEvent = typeevent;
        }
        //临时使用
        public Event(string s)
        {
            ID = 1;
            TypeEvent = s;
        }
    }
    /// <summary>
    /// 自定义序列化时所使用的程序集
    /// Type BindToType函数重载与基类SerializationBinder
    /// </summary>
    public class SerializableFind : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;  // 当前程序集
            return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        }
    }
    /// <summary>
    /// 错误消息处理器
    /// </summary>
    public class ErrorMessage
    {
        static string logPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/日志文件";
        static StreamWriter streamWriter;
        /// <summary>
        /// 获取错误并且将错误抛出与写入本地
        /// </summary>
        /// <param name="ex">异常</param>
        public static void GetError(Exception ex)
        {
            //创建日志文件夹
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            streamWriter= new StreamWriter(logPath + "/" + DateTime.Now.ToLongDateString().ToString() + "日志.txt", true);
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss     ") + ex.Message);
            Console.WriteLine(ex);
            streamWriter.WriteLine(ex.Source + ":" + ex.TargetSite);
            Console.WriteLine(ex.Source + ":" + ex.TargetSite);
            streamWriter.WriteLine(ex.StackTrace);
            Console.WriteLine(ex.StackTrace);
            streamWriter.Dispose();
        }
    }
    /// <summary>
    /// 核心类IoT_Net
    /// 负责网络功能实现的基类
    /// </summary>
    public class IoT_Net
    {
        public string TypeNet;
        protected Socket socket;
        protected IPAddress ip;
        protected IPEndPoint point;
        protected Boolean connectflag = true;
        public Queue<Socket> socketslist;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typenet">网络类别</param>
        public IoT_Net(string typenet)
        {
            TypeNet = typenet;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketslist = new Queue<Socket>(50);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="package">打包好的Package</param>
        /// <returns></returns>
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
        /// <summary>
        /// 紧急发送
        /// </summary>
        /// <param name="package">打包好的Package</param>
        /// <param name="s">需要使用的Socket端口</param>
        /// <returns></returns>
        public static bool Send(Package package, Socket s)
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
        /// <summary>
        /// 监听端口
        /// </summary>
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
        ///如果有自定义方法，需要重载此方法。
        /// </summary>
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
        /// <summary>
        /// 链接
        /// </summary>
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
        /// <summary>
        ///如果使用Connect链接服务器，子类必须重载此函数用于接收服务端传来的数据
        /// </summary>
        public virtual void ReceiveCommand()
        {

        }
        /// <summary>
        /// 基础服务：反序列化数据于Package包中
        /// </summary>
        /// <param name="buffer">Socket二进制流</param>
        /// <returns>打包好的Package包</returns>
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
    /// <summary>
    /// 核心类IoT_Data
    /// 系统中所有数据的基类
    /// </summary>
    [Serializable]
    public class IoT_Data
    {
        public string TypeData;
        public string TypeSystem;
        public string ID;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typedata">数据类型</param>
        /// <param name="typesystem">所属系统</param>
        /// <param name="id">类型ID，临时值为0</param>
        public IoT_Data(string typedata, string typesystem, string id = "0")
        {
            TypeData = typedata;
            TypeSystem = typesystem;
            ID = id;
        }
        /// <summary>
        /// 空构造函数
        /// </summary>
        public IoT_Data()
        {

        }
    }
    /// <summary>
    /// 数据传输基本单位Package
    /// </summary>
    [Serializable]
    public struct Package
    {
        public Messagetype message;
        public byte[] data;
    }
}
