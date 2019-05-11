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
            PointCloudCC point=new PointCloudCC(PointCloudSever);
            //view
            PointCloud_EVCS_ServerView view = new PointCloud_EVCS_ServerView(PointCloudSever,point);
            view.shell();
        }
    }
}
