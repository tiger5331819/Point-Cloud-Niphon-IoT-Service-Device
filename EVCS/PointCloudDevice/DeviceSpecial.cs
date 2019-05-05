using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace EVCS
{
    /// <summary>
    /// EVCS设备端总控制器
    /// </summary>
    public class Special
    {
        #region 体积与版本相关
        decimal flagvolume = 0;//体积锁
        public decimal? Volume
        {
            get { return Data.volume.volume; }
        }
        public bool receivevolume(decimal a)
        {
            Data.volume.volume = a;
            return true;
        }
        public bool receivevolume(string path = "0")
        {
            if (path == "0") { Data.volume.volume = 0; flagvolume = 0; }
            else Data.volume.volume = Convert.ToDecimal(File.ReadAllText(path)) - flagvolume;
            return true;
        }
        public bool changevolume(decimal change)
        {
            flagvolume = change;
            return true;
        }
        string EVCSversion;//EVCS版本号
        string Volumeversion;//体积计算程序版本号
        public string EVCSv
        {
            get { return EVCSversion; }
            set { EVCSversion = value; }
        }
        public string Volumev
        {
            get { return Volumeversion; }
            set { Volumeversion = value; }
        }
        #endregion

        public Special cloud;      
        public DeviceNet cloudnet;
        public DataChangeTODO changeTODO;
        public Process process;
        public Device Data;

        public void loadcardata()
        {
            try
            {
                string line;
                // 创建一个 StreamReader 的实例来读取文件 ,using 语句也能关闭 StreamReader
                using (System.IO.StreamReader sr = new System.IO.StreamReader("carinfo.txt"))
                {
                    // 从文件读取并显示行，直到文件的末尾 
                    //while ((line = sr.ReadLine()) != null)
                    //{
                    //    strData = line;
                    //}

                    if ((line = sr.ReadLine()) != null)
                    {
                        Data.volume.carName = line;
                    }
                    else
                    {
                        Console.WriteLine("读取车名异常");
                    }

                    if ((line = sr.ReadLine()) != null)
                    {
                        Data.volume.carSN = line;

                    }
                    else
                    {
                        Console.WriteLine("读取货运单号异常");
                    }


                    if ((line = sr.ReadLine()) != null)
                    {
                        Data.volume.carVolume = System.Convert.ToDecimal(line);
                    }
                    else
                    {
                        Console.WriteLine("读取车辆总体积异常");
                    }
                }
            }
            catch (Exception e)
            {
                // 向用户显示出错消息
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public Special()
        {
            this.Data = new Device("Device","PointCloud-EVCS");
            loadxml();
            loadcardata();
            
            cloud = this;
        
           process = new Process();
           process.StartInfo.FileName = "体积计算 " +Volumev + ".exe";

            this.cloudnet = new DeviceNet(ref Data,"PointCloud-EVCS");
            this.changeTODO = new DataChangeTODO(ref Data,ref cloud);
        }
        #region 获取已配置好的自动管理时间属性
        public int gethour(int a, bool flag)
        {
            if (flag) return Convert.ToInt32(Data.configtime[a].beginhour);
            else return Convert.ToInt32(Data.configtime[a].endhour);
        }
        public string gethour(int a, bool flag, bool isstring)
        {
            if (flag) return Data.configtime[a].beginhour;
            else return Data.configtime[a].endhour;
        }
        public int getminute(int a, bool flag)
        {
            if (flag) return Convert.ToInt32(Data.configtime[a].beginminute);
            else return Convert.ToInt32(Data.configtime[a].endminute);
        }
        public string getminute(int a, bool flag, bool isstring)
        {
            if (flag) return Data.configtime[a].beginminute;
            else return Data.configtime[a].endminute;
        }
        public bool sethour(int a, bool flag, string t)
        {
            if (flag) Data.configtime[a].beginhour = t;
            else Data.configtime[a].endhour = t;
            return true;
        }
        public bool setminute(int a, bool flag, string t)
        {
            if (flag) Data.configtime[a].beginminute = t;
            else Data.configtime[a].endminute = t;
            return true;
        }
        #endregion
        public void loadxml()
        {
            //将XML文件加载进来
            XDocument document = XDocument.Load("config.xml");
            //获取到XML的根元素进行操作
            XElement root = document.Root;

            XElement Device = root.Element("Device");
            XElement ID = Device.Element("ID");
            Data.ID = ID.Value;

            XElement version = Device.Element("EVCSversion");
            EVCSversion = version.Value;
            version = Device.Element("Volumeversion");
            Volumeversion = version.Value;
            
            XElement NetLink = root.Element("NetLink");
            XElement IP = NetLink.Element("IP");
            XElement server = IP.Element("server");
            
            Data.ip.IP = server.Value;
            XElement Point = IP.Element("serverpoint");
            Data.ip.Point = int.Parse(Point.Value);

            //获取根元素下的所有子元素
            IEnumerable<XElement> ele = root.Elements("time");
            IEnumerable<XElement> enumerable = ele.Elements();
            int i = 0;
            foreach (XElement item in enumerable)
            {
                Data.configtime[i].time = item.Name.ToString();
                XElement timefind = item.Element("beginhour");
                Data.configtime[i].beginhour = timefind.Value;
                timefind = item.Element("beginminute");
                Data.configtime[i].beginminute = timefind.Value;
                timefind = item.Element("endhour");
                Data.configtime[i].endhour = timefind.Value;
                timefind = item.Element("endminute");
                Data.configtime[i].endminute = timefind.Value;
                i++;
            }
        }
        public void writexml()
        {
            //获取根节点对象
            XDocument document = new XDocument();
            XElement root = new XElement("EVCS");

            XElement Device = new XElement("Device");
            XElement ID = new XElement("ID");
            ID.Value = Data.ID;
           
            XElement EVCSv = new XElement("EVCSversion");
            EVCSv.Value = EVCSversion;
            Device.Add(EVCSv);
            XElement version = new XElement("Volumeversion");
            version.Value = Volumeversion;
            Device.Add(version);

            root.Add(Device);

            XElement NetLink = new XElement("NetLink");
            XElement IP = new XElement("IP");
            IP.SetElementValue("server", Data.ip.IP);
            IP.SetElementValue("serverpoint", Data.ip.Point);
            NetLink.Add(IP);
            root.Add(NetLink);

            XElement time = new XElement("time");

            foreach (configtimexml x in Data.configtime)
            {
                XElement addtime = new XElement(x.time);
                addtime.SetElementValue("beginhour", x.beginhour);
                addtime.SetElementValue("beginminute", x.beginminute);
                addtime.SetElementValue("endhour", x.endhour);
                addtime.SetElementValue("endminute", x.endminute);
                time.Add(addtime);
            }

            root.Add(time);
            root.Save("config.xml");
        }

    }
}
