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
            PointCloudSever.cloudnet.serverLink();
            //control
            PointCloudCC point=new PointCloudCC(ref PointCloudSever);
        }
    }
}
