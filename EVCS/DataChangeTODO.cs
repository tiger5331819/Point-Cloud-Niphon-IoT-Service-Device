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
    /// <summary>
    /// 当从服务端获得数据或指令时的工作类
    /// </summary>
    public class DataChangeTODO
    {
        Boolean sendvolumeflag=true;
        Device Data;
        Special cloud;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data">设备数据</param>
        /// <param name="s">总控制端的映射</param>
        public DataChangeTODO(Device data,Special s)
        {
            Data = data;
            cloud = s;

            Thread check = new Thread(CreateThreadToCheckData);
            check.IsBackground = true; 
            check.Start();
        }
        #region CreateThreadToCheckData()
        /// <summary>
        /// 用于检测是否收到服务器的指令与数据，并怎么做
        /// </summary>
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
                            case Messagetype.order:  OrderTODO(); break;
                            case Messagetype.update:updateTODO();break;
                        }
                        Data.flag = false;
                    }
                    else Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.GetError(ex);
            }
        }
        #endregion

        #region DO
        /// <summary>
        /// 车辆信息更改
        /// </summary>
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
                    
                    cloud.cloudnet.Send(DeviceNet.DeviceDataToPackage(Data.GetData(),Messagetype.volumepackage));
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    ErrorMessage.GetError(ex);
                    return;
                }
            }
            sendvolumeflag = true;
        }
        /// <summary>
        /// 服务端传达监控指令
        /// </summary>
        private void monitor()
        {
            cloud.cloudnet.Send(DeviceNet.DeviceDataToPackage(Data.GetData(),Messagetype.carinfomessage));
        }
        private void OrderTODO()
        {
            switch (Data.codemode)
            {
                case Codemode.sendvolume:Thread sendvolumeData=new Thread(sendvolumedata);
                                         sendvolumeData.IsBackground = true; sendvolumeData.Start(); break;
                case Codemode.monitor:monitor(); break;
                case Codemode.stopsendvolume:sendvolumeflag=false;break;
                case Codemode.play: DeviceDO.Play(cloud) ;break;
                case Codemode.stop: DeviceDO.stop(cloud) ;break;
            }
        }
        /// <summary>
        /// 更新服务端上的设备核心数据
        /// </summary>
        private void updateTODO()
        {
            cloud.cloudnet.Send(DeviceNet.DeviceDataToPackage(cloud.Data.GetData(), Messagetype.package));
            Console.WriteLine("succeed");
        }
        #endregion DO
    }
}
    