using Cognex.VisionPro;
using JancsiVersionCameraController;
using JancsiVisionCameraServers;
using JancsiVisionCameraServers.Interfaces;
using JancsiVisionConfigServices;
using JancsiVisionLogServers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionCameraController
{
    public class CameraControlbus
    {

        /// <summary>
        /// 依赖注入方式执行相机获取
        /// </summary>
        public void initData()
        {

            ////并行执行相机参数配置，和点云获取
            // foreach (ICogFrameGrabber icogGrabber in cameras)
            // {

            //     CongexCameraServer cogCamera = new CongexCameraServer(icogGrabber);

            // }


            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ILogProvider, ConsoleLogProvider>();
            services.AddSingleton<ICameraControlServer, CognexCameraControl>();
            using (ServiceProvider sp = services.BuildServiceProvider())
            {
                var cameraInit = sp.GetRequiredService<ICameraControlServer>();
                cameraInit.init();

                Console.WriteLine("等操作结束");
                Console.ReadLine();

            }
            Console.Read();


        }
    }
}
