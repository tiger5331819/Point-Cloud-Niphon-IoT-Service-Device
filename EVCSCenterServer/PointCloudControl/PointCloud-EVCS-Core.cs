using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EVCS
{
    /// <summary>
    /// EVCS服务器核心服务
    /// </summary>
    public class PointCloud_EVCS_Core
    {
        /// <summary>
        /// 创建设备数据并注入
        /// </summary>
        /// <param name="socket">Socket节点</param>
        /// <param name="package">获得的数据包</param>
        /// <returns></returns>
        public static Device CreatDeviceData(Socket socket,Package package)
        {
            Device data= CenterServerNet.NewDevice(package);
            data.IP = socket.RemoteEndPoint.ToString();
            data.Live = true;
            data.socket = socket;
            return data;
        }
        public static User CreatUserData(Socket socket,Package package)
        {
            User data = CenterServerNet.NewUser(package);
            data.IP = socket.RemoteEndPoint.ToString();
            data.Live = true;
            data.socket = socket;
            return data;
        }
    }
    /// <summary>
    /// 发送设备链接表事件，继承自事件基类
    /// </summary>
    public class Send_IPList:Event
    {
        public IPList[] iPLists;
        public Send_IPList(IPList[] iPL):base(1,"IPList")
        {
            iPLists = iPL;
        }
    }
    public class DataLive : Event
    {
        public string ControlType;
        public string ControlName;
        public bool Live;
        public DataLive(string type,string name,bool live):base(2,"DataLive")
        {
            ControlType = type;
            ControlName = name;
            Live = live;
        }
    }
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager
    {
        public delegate void IPLHandler(object sender, Send_IPList e);
        public delegate void DataLiveHandler(object sender, DataLive e);
        public event DataLiveHandler DataLiveEvent;
        public event IPLHandler IPLEvent;
        public void OnIPLEvent(Send_IPList e)
        {
            IPLEvent?.Invoke(this, e);
        }
        public void OnDataLiveEvent(DataLive e)
        {
            DataLiveEvent?.Invoke(this, e);
        }
        public void SimulateNewIPLEvent(IPList[] iPL)
        {
            Send_IPList e = new Send_IPList(iPL);
            OnIPLEvent(e);
        }
        public void SimulateNewDataLiveEvent(string type,string name,bool live)
        {
            DataLive e = new DataLive(type,name,live);
            OnDataLiveEvent(e);
        }
    }
}
