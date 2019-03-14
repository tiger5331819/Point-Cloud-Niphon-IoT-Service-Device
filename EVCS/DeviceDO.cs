using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace EVCS
{
    class DeviceDO
    {
         static public void Play(ref Special cloud)
        {            
            cloud.process.Start();
            Console.WriteLine(cloud.process.StartInfo.FileName + "  play");
            NewMain.Nform.cloud.Data.volume.Begintime = DateTime.Now.ToString();
        }
        static public void stop(ref Special cloud)
        {
            //数据保存数据库
             NewMain.Nform.cloud.Data.volume.Endtime = DateTime.Now.ToString();           
            cloud.process.Kill();
            Console.WriteLine("杀死体积计算 " + NewMain.Nform.cloud.Volumev + "程序");       
        }
    }
}
