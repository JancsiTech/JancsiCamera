using Cognex.VisionPro;
using JancsiVersionCameraController;
using JancsiVisionCameraServers;
using JancsiVisionCameraServers.Interfaces;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices;
using JancsiVisionLogServers;
using JancsiVisionPointCloudServers.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionCameraController
{
    public class CameraControlbus
    {
        public ICameraControlServer cameraControl;
        /// <summary>
        /// 依赖注入方式执行相机获取
        /// </summary>
        public void initData()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ILogProvider, ConsoleLogProvider>();
            services.AddSingleton<IConfigService, FileCongfigServer>();
            services.AddSingleton<ICameraControlServer, CognexCameraControl>();
            using (ServiceProvider sp = services.BuildServiceProvider())
            {
                cameraControl = sp.GetRequiredService<ICameraControlServer>();
                cameraControl.init();

                Console.WriteLine("等操作结束");
                // Console.ReadLine();

            }
            Console.Read();


        }
        /// <summary>
        /// 开始获取
        /// </summary>
        public Dictionary<Dto_CameraOperation, Dto_PointCloud> StartTrigger()
        {
            Dictionary<Dto_CameraOperation, Dto_PointCloud> dicPin = new Dictionary<Dto_CameraOperation, Dto_PointCloud>();

            if (this.cameraControl != null)
            {
                dicPin = cameraControl.triggerCapture();
            }
            return dicPin;
        }
        /// <summary>
        /// 关闭相机链接
        /// </summary>
        public void DisConnectCamera()
        {
            cameraControl.disconnectAll();
        }
    }
}
