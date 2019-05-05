using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace EVCS
{
    /// <summary>
    /// 用于存储IP的元数据结构
    /// </summary>
    [Serializable]
    public struct IPList
    {
        public string ID;
        public string IP;
    }
    /// <summary>
    /// EVCS服务端总控制器
    /// </summary>
    public class Special
    {
        public ServerData Data;
        #region EVCS版本号
        string EVCSServerVersion;
        public string EVCSv
        {
            get { return EVCSServerVersion; }
            set { EVCSServerVersion = value; }
        }
        #endregion

        public Special cloud;
        public CenterServerNet cloudnet;
        
        /// <summary>
        /// 加载xml
        /// 子端结构体赋值为空
        /// 实例化Net类
        /// </summary>
        public Special()
        {
            this.Data = new ServerData("Server", "PointCloud-EVCS");
            loadxml();   
            cloud = this;
            this.cloudnet = new CenterServerNet(ref Data,ref cloud);
        }

        public void loadxml()
        {
            //将XML文件加载进来
            XDocument document = XDocument.Load("Serverconfig.xml");
            //获取到XML的根元素进行操作
            XElement root = document.Root;

            XElement Server = root.Element("Server");
            XElement ID = Server.Element("ID");
            Data.ID = ID.Value;

            XElement version = Server.Element("EVCSServerVersion");
            EVCSServerVersion = version.Value;

            XElement NetLink = root.Element("NetLink");
            XElement IP = NetLink.Element("IP");
            XElement server = IP.Element("server");
            Data.ip.IP = server.Value;
            XElement Point = IP.Element("serverpoint");
            Data.ip.Point = int.Parse(Point.Value);

        }
        public void writexml()
        {
            //获取根节点对象
            XDocument document = new XDocument();
            XElement root = new XElement("EVCS");
            XElement EVCSv = new XElement("EVCSServerVersion");
            EVCSv.Value = EVCSServerVersion;
            root.Add(EVCSv);           

            XElement NetLink = new XElement("NetLink");
            XElement IP = new XElement("IP");
            IP.SetElementValue("server", Data.ip.IP);
            IP.SetElementValue("serverpoint", Data.ip.Point);
            NetLink.Add(IP);
            root.Add(NetLink);
            root.Save("config.xml");
        }

    }
    
}
