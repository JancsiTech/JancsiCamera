using Cognex.VisionPro;
using Cognex.VisionPro.ImagingDevice;
using JancsiVisionCameraController;
using JancsiVisionCameraServers;
using JancsiVisionCameraServers.Interfaces;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices;
using JancsiVisionLogServers;
using JancsiVisionPointCloudServers.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JancsiVersionCameraController
{
    public class CognexCameraControl : ICameraControlServer
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public string _ErrorMessage { get; set; }
        /// <summary>
        /// 相机信息
        /// </summary>
        private List<CongexCameraServer> cameras;
        // private List<string> cameras;

        private Dictionary<Dto_CameraOperation, Dto_PointCloud> dicCamera;
        /// <summary>
        /// 返回相机信息
        /// </summary>
        /// <returns></returns>
        private List<CongexCameraServer> listCameras()
        {

            return cameras;
        }
        //private static ConcurrentDictionary<int, string> Dic = new ConcurrentDictionary<int, string>();
        //# getOneFrame 
        //
        public Dictionary<Dto_CameraOperation, Dto_PointCloud> triggerCapture()
        {

            try
            {
                List<Task> TaskList = new List<Task>();
                //并行开始获取点云并返回
                if (cameras != null && cameras.Count > 0)
                {
                    //foreach (CongexCameraServer camServes in cameras)
                    //{
                    //    var LastTask = new Task(camServes.connect);
                    //    LastTask.Start();
                    //    TaskList.Add(LastTask);

                    //}

                    //Task.WaitAll(TaskList.ToArray());




                    // Await when all to get an array of result sets after all of then have finished
                    //var results = await Task.WhenAll(
                    //    Task.Run(() => Method1()), // Note that this leaves room for parameters to Method1...
                    //    Task.Run(Method2)          // While this shorthands if there are no parameters
                    //                               // Any further method calls can go here as more Task.Run calls
                    //    );

                    // Simply select many over the result sets to get each result
                    //return results.SelectMany(r => r);
                    //并行开启获取点云服务
                    ConcurrentBag<Dto_PointCloud> resultCollection = new ConcurrentBag<Dto_PointCloud>();
                    cameras.AsParallel().ForAll(p =>
                    {
                        resultCollection.Add(p.connect());
                    });
                    //获取结束
                    //
                    if (resultCollection.Count!=0&&resultCollection.Count==cameras.Count)
                    {

                    }
                    else
                    {
                        //log 对照失败
                        return null;
                    }

                }
                return dicCamera;
            }
            catch (Exception ex)
            {
                //会直接抛出 AggregateException 异常
                //throw;
                //log ex
                return null;
            }
        }
        /// <summary>
        /// cog-康耐视 初始化加载
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void init()
        {

            try
            {
                cameras = new List<CongexCameraServer>();
                _ErrorMessage = "";

                //获取相机
                //  CogFrameGrabberGigEs mframe = new CogFrameGrabberGigEs();
                // Enumerates available ICogFrameGrabber objects.
                CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
                if (frameGrabbers.Count < 1)
                {
                    _ErrorMessage = "没有找到相机设备！";
                    Console.WriteLine("Was not able to find suitable device.");
                    Console.WriteLine("Finished. Press any key to close this window...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                //累加函数
                int index = 0;
                foreach (ICogFrameGrabber foundFrameGrabber in frameGrabbers)
                {

                    Console.WriteLine("Found frame grabber " + foundFrameGrabber.Name);
                    if (foundFrameGrabber.Name == "Device: Lion" || foundFrameGrabber.Name.Contains("Device: 3D-A"))
                    {

                        //实例化相机类
                        CongexCameraServer congexCamera = new CongexCameraServer(foundFrameGrabber);
                        //将相机实例加载到相机类
                        congexCamera._MainCogCrabber = foundFrameGrabber;
                        //初始化预热时间

                        //对照相机位置配置相机信息
                        congexCamera._CameraOperation = new Dto_CameraOperation();
                        //这里扬奇逻辑根据postion 和 0，1，2做对照
                        congexCamera._CameraOperation.uuid = index;
                        //获取相机IP地址
                        //主机IP:foundFrameGrabber.OwnedGigEAccess.HostIPAddress;
                        congexCamera._CameraOperation.Ip = foundFrameGrabber.OwnedGigEAccess.CurrentIPAddress;
                        //配置中获取相机对照  ToDo...
                        congexCamera._CameraOperation.cameraId = index;

                        congexCamera._CameraOperation.Name = foundFrameGrabber.Name;

                        congexCamera._CameraOperation.SerialNumber = foundFrameGrabber.SerialNumber;

                        congexCamera._CameraOperation.Capture = new Capture();

                        congexCamera._CameraOperation.ROI = new Dto_CameraROI();

                        congexCamera._CameraOperation.ReconstructionQuality = new ReconstructionQuality();

                        congexCamera._CameraOperation.PreFilter = new PreFilter();

                        congexCamera._CameraOperation.PostFilter = new PostFiler();

                        congexCamera._CameraOperation.RegionExtract = new RegionExtract();

                        congexCamera._CameraOperation.Connected = true;
                        //暂时不可获取状态
                        congexCamera._CameraOperation.IsAvailable = false;
                        //位置对照 ToDo
                        congexCamera._CameraOperation.CameraPosotion = "0";

                        congexCamera._CameraOperation.calibrateTime = null;

                        // Create an AcqFifo.
                        congexCamera._MainCogFifo = foundFrameGrabber.CreateAcqFifo(foundFrameGrabber.AvailableVideoFormats[0], CogAcqFifoPixelFormatConstants.Format16Grey, 0, true);
                        cameras.Add(congexCamera);

                        index++;
                        //遍历已连接相机信息，并添加进列表里（相机名，序列号，模式）
                        //log:cameras.Add(frameGrabber.Name + frameGrabber.SerialNumber + frameGrabber.AvailableVideoFormats[0]);
                    }
                    else
                    {
                        _ErrorMessage = "不是康耐视相机！";
                        //log not cognex
                        return;
                    }
                }
                //log camera connect end
            }
            catch (Exception ex)
            {

                Console.WriteLine("Can not Found frame grabber " + ex.ToString());
            }
        }


        ///// <summary>
        ///// （扩展方法）如果字符串为空则返回""
        ///// </summary>
        ///// <param name="s"></param>
        ///// <returns></returns>
        //public static string ToStringEx(this object s)
        //{
        //    if (s == null)
        //        return "";
        //    else
        //        return s.ToString();
        //}
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddSingleton<ITool, EmailTool>();
        //    services.AddMvc();
        //}

        /// <summary>
        /// 预留判断是否在获取不到配置的情况加，将相机中的获取到的配置记录到配置文件中
        /// </summary>
        public bool autoCreateCamera = false;

        /// <summary>
        /// 重启相机
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void reset()
        {
            try
            {
                if (cameras != null && cameras.Count > 0)
                {
                    foreach (CongexCameraServer device in cameras)
                    {
                        if (device._MainCogCrabber != null)
                        {
                            Console.WriteLine("Performing device reset. This will take ~30 seconds.");
                            //Reset the device(requires the expert_mode registry keys).
                            device._MainCogCrabber.OwnedImagingDeviceAccess.ExecuteCommand("device_reset");
                            // The acquisition fifo is now invalid.
                            device._MainCogFifo = null;
                            // Sleep 30s to give the device time to boot up again
                            System.Threading.Thread.Sleep(30000);
                            //Recreate acq fifo.
                            //acqFifo = _MainCogCrabber.CreateAcqFifo(frameGrabber.AvailableVideoFormats[0], CogAcqFifoPixelFormatConstants.Format16Grey, 0, true);
                            // Configuration needs to be reapplied.
                            device.setCameraConfig();

                        }

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 相机数
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int summary()
        {
            return this.cameras.Count;
        }
        /// <summary>
        /// 断开链接
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void disconnectAll()
        {
            if (cameras != null && cameras.Count > 0)
            {
                foreach (CongexCameraServer device in cameras)
                {
                    device.disconnect();
                }

            }
            else
            {
                //log no cameras!
            }

        }
        //ToDo
        public string checkErrors()
        {
            return _ErrorMessage;
        }
        //ToDo
        public void Calibrate()
        {
            throw new NotImplementedException();
        }
        //ToDo
        public void CalibrateOne(string cameraUid)
        {
            throw new NotImplementedException();
        }

        public object getOneCameraDevice(string cameraUid)
        {
            var cameraDeivce = cameras.Where(o => o._CameraOperation.uuid.ToString() == cameraUid).FirstOrDefault();
            if (cameraDeivce != null)
            {
                _ErrorMessage = "can not find camera";
                return null;
                //log can not find camera
            }
            return cameraDeivce;
        }
    }
}
