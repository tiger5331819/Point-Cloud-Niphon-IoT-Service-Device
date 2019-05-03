using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EVCS
{
    public class PointCloud_EVCS_Core
    {
        public static Device CreatDeviceData(ref Socket socket,Package package)
        {
            Device data= CenterServerNet.NewDevice(package);
            data.IP = socket.RemoteEndPoint.ToString();
            data.Live = true;
            data.socket = socket;
            return data;
        }
        public static User CreatUserData(ref Socket socket,Package package)
        {
            User data = CenterServerNet.NewUser(package);
            data.IP = socket.RemoteEndPoint.ToString();
            data.Live = true;
            data.socket = socket;
            return data;
        }
    }

    
    public class Send_IPList:Event
    {
        public List<IPList> iPLists;
        public Send_IPList(List<IPList>iPL):base("IPList")
        {
            iPLists = iPL;
        }
    }
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
