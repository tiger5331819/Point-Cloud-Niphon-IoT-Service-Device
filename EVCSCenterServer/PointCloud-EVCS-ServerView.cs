using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace EVCS
{
    class PointCloud_EVCS_ServerView
    {
        string article;
        Special cloud;
        PointCloudCC cc;
        public PointCloud_EVCS_ServerView(ref Special s,ref PointCloudCC c)
        {
            cloud = s;
            cc = c;
        }
        public void shell()
        {
            while (true)
            {
                try
                {
                    article = Console.ReadLine();
                    switch (article)
                    {
                        case "DeviceList": foreach (IPList r in cloud.Data.iplist) WriteLine(r.ID + " " + r.IP); break;
                        case "UserList": foreach (IPList r in cloud.Data.UserList) WriteLine(r.ID + " " + r.IP); break;
                        case "Select": Select(); break;
                        default: break;
                    }
                    article = null;
                }
                catch (Exception ex)
                {
                    ErrorMessage.GetError(ex);
                }

            }
        }

        void Select()
        {

            article = null;
            article = ReadLine();

            PointCloudDeviceC d = cc.DeviceC.Find(x => x.ID == article);
            WriteLine(d.ID);

            while (true)
            {
                article = null;
                article = ReadLine();
                switch (article)
                {
                    case "back": return;
                    case "play": TODO(d, Codemode.play); break;
                    case "monitor": TODO(d, Codemode.monitor); break;
                    case "sendvolume": TODO(d, Codemode.sendvolume); break;
                    case "stopsendvolume": TODO(d, Codemode.stopsendvolume); break;
                    case "stop": TODO(d, Codemode.stop); break;
                }
            }

        }
        void TODO(PointCloudDeviceC Device, Codemode codemode)
        {
            Device.SendCode(codemode);
        }
    }
}
