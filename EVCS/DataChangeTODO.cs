using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace EVCS
{
    public class DataChangeTODO
    {
        Boolean sendvolumeflag=true;
        DeviceData Data;
        public Special cloud;

        public DataChangeTODO(ref DeviceData data,ref Special c)
        {
            this.Data = data;
            this.cloud = c;
            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true; 
            check.Start();
        }
        void CreateThreadToCheckData()
        {
            try
            {
                while (true)
                {
                    if (Data.newdatachange())
                    {

                        switch (Data.messagetype)
                        {
                            case Messagetype.carinfomessage: ChangeCarinfoMessage(); break;
                            case Messagetype.order: OrderTODO(); break;
                            case Messagetype.update:updateTODO();break;
                        }
                        Data.flag = false;
                    }
                    else Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void ChangeCarinfoMessage()
        {
            Data.volume.carName = Data.Newdata.volume.carName;
            Data.volume.carNo = Data.Newdata.volume.carNo;
            Data.volume.carSN = Data.Newdata.volume.carSN;
            Data.volume.carVolume = Data.Newdata.volume.carVolume;
            Data.Newdata = null;
        }
        private void sendvolumedata()
        {
            
            while (sendvolumeflag)
            {
                string strData = "0";
                try
                {
                    string line;
                    // 创建一个 StreamReader 的实例来读取文件 ,using 语句也能关闭 StreamReader
                    using (System.IO.StreamReader sr = new System.IO.StreamReader("data.txt"))
                    {
                        // 从文件读取并显示行，直到文件的末尾 
                        while ((line = sr.ReadLine()) != null)
                        {
                            //Console.WriteLine(line);
                            strData = line;
                            Data.volume.volume = Convert.ToDecimal( strData);
                        }
                    }
                }
                catch (Exception e)
                {
                    // 向用户显示出错消息
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine(strData);
                try
                {
                    
                    cloud.cloudnet.Send(cloud.cloudnet.DeviceDataToPackage(cloud.Data,Messagetype.volumepackage));
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            sendvolumeflag = true;
        }
        private void monitor()
        {

            cloud.cloudnet.Send(cloud.cloudnet.DeviceDataToPackage(cloud.Data,Messagetype.carinfomessage));
        }


        private void OrderTODO()
        {
            switch (Data.codemode)
            {
                case Codemode.sendvolume:Thread sendvolumeData=new Thread(sendvolumedata);
                                         sendvolumeData.IsBackground = true; sendvolumeData.Start(); break;
                case Codemode.monitor:monitor(); break;
                case Codemode.stopsendvolume:sendvolumeflag=false;break;
                case Codemode.play: DeviceDO.Play(ref cloud) ;break;
                case Codemode.stop: DeviceDO.stop(ref cloud) ;break;
            }
        }
        private void updateTODO()
        {
            cloud.cloudnet.Send(cloud.cloudnet.DeviceDataToPackage(cloud.Data, Messagetype.package));
            Console.WriteLine("succeed");
        }
    }
}
    