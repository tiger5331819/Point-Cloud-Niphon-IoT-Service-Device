using System;
using System.Collections.Generic;
using System.Threading;

namespace EVCS
{
    class Program
    {
        static void Main(string[] args)
        {
            //model
            Special PointCloudSever = new Special();
            //control
            PointCloudCC point=new PointCloudCC(ref PointCloudSever);
            //view
            PointCloud_EVCS_ServerView view = new PointCloud_EVCS_ServerView(ref PointCloudSever, ref point);
            view.shell();
        }
    }
}
