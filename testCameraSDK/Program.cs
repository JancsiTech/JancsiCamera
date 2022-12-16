using JancsiVersionCameraController;
using JancsiVisionCameraController;
using JancsiVisionUtilityServers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testCameraSDK
{
    public class Program
    {
        static void Main(string[] args)
        {
            //JancsiUtilityServer jancsiUtilityServer = new JancsiUtilityServer();
            //jancsiUtilityServer.test();
            CameraControlbus control = new CameraControlbus();
            control.initData();
            int i = 0;
            while (true)
            {
                i++;
                if (i == 5)
                {
                    GC.Collect();
                }
                Stopwatch watch = new Stopwatch();
                watch.Start();
                //去到多个相机的点云
                var PointDz = control.StartTrigger();
                watch.Stop();
                //接入算法

                //生成图像

                //
                Console.WriteLine(string.Format("cloud pint  take time :{0}", watch.ElapsedMilliseconds.ToString()));
            }


        }
    }
}
