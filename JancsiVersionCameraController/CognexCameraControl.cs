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
using System.Windows.Media.Media3D;
using System.Timers;
using JancsiVisionUtilityServers.Model;

namespace JancsiVersionCameraController
{
    public class CognexCameraControl : ICameraControlServer
    {
        private readonly ILogProvider _log;
        private readonly IConfigService _config;
        JancsiVisionConfigServices.Model.CameraConfig cameraConfig { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string _ErrorMessage { get; set; }
        /// <summary>
        /// 相机信息
        /// </summary>
        private List<CongexCameraServer> cameras;

        /// <summary>
        /// 相机配置对应点云信息
        /// </summary>
        private Dictionary<Dto_CameraOperation, Dto_PointCloud> dicCamera;

        /// <summary>
        /// 返回相机信息
        /// </summary>
        /// <returns></returns>
        public List<CongexCameraServer> GetListCameras()
        {

            return cameras;
        }

        public CognexCameraControl(ILogProvider log, IConfigService config)
        {

            _log = log;
            _config = config;
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
                    ConcurrentBag<Dictionary<Dto_CameraOperation, List<Point3D>>> resultCollection = new ConcurrentBag<Dictionary<Dto_CameraOperation, List<Point3D>>>();

                    cameras.AsParallel().ForAll(p =>
                    {
                        resultCollection.Add(p.connect());

                    });
                    //获取结束
                    if (resultCollection.Count != 0 && resultCollection.Count == cameras.Count)
                    {
                        //dicCamera
                        foreach (var resluts in resultCollection)
                        {
                            if (resluts == null)
                                continue;
                            Dto_PointCloud dto_PointCloud = new Dto_PointCloud();
                            dto_PointCloud.point3Ds = resluts.First().Value;

                            dicCamera[resluts.First().Key] = dto_PointCloud;
                        }
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
                _ErrorMessage = ex.ToString();
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
                cameraConfig = _config.GetEnvironmentConfig("camera");
                cameras = new List<CongexCameraServer>();
                _ErrorMessage = "";

                //获取相机
                //  CogFrameGrabberGigEs mframe = new CogFrameGrabberGigEs();
                // Enumerates available ICogFrameGrabber objects.
                //CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
                CogFrameGrabberImagingDevices frameGrabbers = new CogFrameGrabberImagingDevices();
                if (frameGrabbers.Count < 1)
                {
                    _ErrorMessage = "Was not able to find suitable device！";
                    Console.WriteLine("Was not able to find suitable device.");
                    Console.WriteLine("Finished. Press any key to close this window...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                //累加函数
                int index = 0;


                foreach (ICogFrameGrabber foundFrameGrabber in frameGrabbers)
                {

                    //Console.WriteLine("Found frame grabber " + foundFrameGrabber.Name + foundFrameGrabber.SerialNumber);
                    if (foundFrameGrabber.Name == "Device: Lion" || foundFrameGrabber.Name.Contains("Device: 3D-A"))
                    {

                        //实例化相机类
                        CongexCameraServer congexCamera = new CongexCameraServer(foundFrameGrabber, _log, _config);
                        //将相机实例加载到相机类
                        congexCamera._MainCogCrabber = foundFrameGrabber;
                        //初始化预热时间
                        congexCamera._PreheatingStartTime = DateTime.Now;

                        congexCamera._PreheatingEndTime = congexCamera._PreheatingStartTime.AddMinutes(40);

                        //对照相机位置配置相机信息
                        congexCamera._CameraOperation = new Dto_CameraOperation();
                        //这里扬奇逻辑根据postion 和 0，1，2做对照
                        congexCamera._CameraOperation.uuid = index;
                        //获取相机IP地址
                        //主机IP:foundFrameGrabber.OwnedGigEAccess.HostIPAddress;  
                        // congexCamera._CameraOperation.Ip = foundFrameGrabber.OwnedGigEAccess.CurrentIPAddress;
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
                        //congexCamera._MainCogFifo = foundFrameGrabber.CreateAcqFifo(foundFrameGrabber.AvailableVideoFormats[0], CogAcqFifoPixelFormatConstants.Format16Grey, 0, true);
                        // the pixel format is ignore here for the aik
                        congexCamera._MainCogFifo = foundFrameGrabber.CreateAcqFifo("Cognex NullFormat", CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);
                        // Create an AcqFifo.
                        // congexCamera._MainCogFifo = foundFrameGrabber.CreateAcqFifo(foundFrameGrabber.AvailableVideoFormats[0], CogAcqFifoPixelFormatConstants.Format16Grey, 0, true);

                        congexCamera.setCameraConfig(cameraConfig);

                        cameras.Add(congexCamera);

                        index++;
                        //遍历已连接相机信息，并添加进列表里（相机名，序列号，模式）
                        //log:cameras.Add(frameGrabber.Name + frameGrabber.SerialNumber + frameGrabber.AvailableVideoFormats[0]);
                    }
                    else
                    {
                        _ErrorMessage = "Not a spec camera model！";
                        //log not cognex
                        return;
                    }
                }
                //初始化返回结构
                if (cameras != null && cameras.Count > 0)
                {
                    foreach (CongexCameraServer devce in cameras)
                    {
                        dicCamera = new Dictionary<Dto_CameraOperation, Dto_PointCloud>();
                        if (!dicCamera.Keys.Contains(devce._CameraOperation))
                        {
                            dicCamera.Add(devce._CameraOperation, new Dto_PointCloud());
                        }
                    }
                }


                //log camera connect end
            }
            catch (Exception ex)
            {

                Console.WriteLine("Can not Found frame grabber " + ex.ToString());
            }
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //修改传出变量


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
                            device.setCameraConfig(cameraConfig);


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

        public List<Dto_RotateMatrix> InitMatrixByCamera()
        {



            // dto_RotateMatrix.dto_PointCloud.point3Ds[0]


            if (cameras != null && cameras.Count > 0)
            {

            }


            return null;

        }
    }
}
