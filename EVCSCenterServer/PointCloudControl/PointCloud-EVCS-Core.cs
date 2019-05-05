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
        public static Device CreatDeviceData(ref Socket socket,Package package)
        {
            Device data= CenterServerNet.NewDevice(package);
            data.IP = socket.RemoteEndPoint.ToString();
            data.Live = true;
            data.socket = socket;
            return data;
        }
        /// <summary>
        /// 创建用户数据并注入
        /// </summary>
        /// <param name="socket">Socket节点</param>
        /// <param name="package">获得的数据包</param>
        /// <returns></returns>
        public static User CreatUserData(ref Socket socket,Package package)
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
        public List<IPList> iPLists;
        public Send_IPList(List<IPList>iPL):base("IPList")
        {
            iPLists = iPL;
        }
    }
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager
    {
        public delegate void NewEventHandler(object sender, Send_IPList e);
        public event NewEventHandler Event;
        public virtual void OnNewEvent(Send_IPList e)
        {
            Event?.Invoke(this, e);
        }
        public void SimulateNewEvent(List<IPList> iPL)
        {
            Send_IPList e = new Send_IPList(iPL);
            OnNewEvent(e);
        }
    }
}
