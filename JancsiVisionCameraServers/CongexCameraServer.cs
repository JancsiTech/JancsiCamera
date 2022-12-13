using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro3D;
using JancsiVisionCameraServers.Interfaces;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices;
using JancsiVisionLogServers;
using JancsiVisionPointCloudServers.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Media.Media3D;
using static System.Net.Mime.MediaTypeNames;

namespace JancsiVisionCameraServers
{
    public class CongexCameraServer : ICameraServer
    {
        private readonly ILogProvider _log;
        private readonly IConfigService _config;

        /// <summary>
        /// Enumerates available ICogFrameGrabber objects.
        /// </summary>
        public ICogFrameGrabber _MainCogCrabber;

        /// <summary>
        /// an AcqFifo.
        /// </summary>
        public ICogAcqFifo _MainCogFifo;

        /// <summary>
        /// 相机配置集合 先留着逻辑看放在哪里
        /// </summary>
        public Dto_CameraConfig _CameraConfig;
        /// <summary>
        /// Preheating Start Time
        /// </summary>
        public DateTime _PreheatingStartTime { get; set; }
        /// <summary>
        /// Preheating End Time
        /// </summary>
        public DateTime _PreheatingEndTime { get; set; }
        /// <summary>
        /// 相机配置详情
        /// </summary>
        public Dto_CameraOperation _CameraOperation;

        public List<Point3D> _CloudData;

        public Dictionary<Dto_CameraOperation, List<Point3D>> _DicPoint3d;

        public CongexCameraServer(ILogProvider log, IConfigService config)
        {

            _log = log;
            _config = config;
        }
        public CongexCameraServer(ICogFrameGrabber icogGrabber)
        {
            _MainCogCrabber = icogGrabber;
            _CameraOperation = new Dto_CameraOperation();
            _DicPoint3d = new Dictionary<Dto_CameraOperation, List<Point3D>>();


        }
        public bool Calibrate()
        {
            throw new NotImplementedException();
        }
        CogImage16Range image;
        /// <summary>
        /// 实例化并获取点云
        /// </summary>
        /// <returns></returns>
        public Dictionary<Dto_CameraOperation, List<Point3D>> connect()
        {
            _CloudData = new List<Point3D>();

            try
            {

                // this._log.LogInfo(string.Format("log:当前线程id{0}，is{1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread));
                if (_CameraOperation.IsAvailable&& isReady())
                {
                    
                    // Start continuous acquisition.

                    while (_CloudData == null || _CloudData.Count == 0)
                    {

                        //CogStopwatch stop = new CogStopwatch();
                        //stop.Start();
                        _MainCogFifo.OwnedTriggerParams.TriggerEnabled = true;

                        // Acquire a single vision data container.
                        var acqInfo = new CogAcqInfo { Ticket = -1 }; // For FreeRun mode the ticket number is -1
                        CogImage16Range image = null;

                        image = (CogImage16Range)_MainCogFifo.CompleteAcquireEx(acqInfo);

                        // Stop continuous acquisition.

                        //stop.Stop();
                        //Console.WriteLine("fifotime" + stop.Milliseconds);

                        if (image != null)
                        {
                            if (GetCloudFromRange(image))
                            {
                                List<Point3D> copyList = new List<Point3D>(_CloudData);
                                _CloudData.Clear();
                                for (int i = 0; i < copyList.Count; i++)
                                {
                                    if (i % 5 == 0)
                                    {

                                        _CloudData.Add(copyList[i]);
                                    }
                                }
                                //if (rangeImageList.Count == 10)
                                //{
                                //    rangeImageList.RemoveAt(0);
                                //}
                                //rangeImageList.Add(_cloudData);
                                //保存点云
                                ////savePointCloudThreadFunc();

                                //Complate_Flag = false;
                            }

                        }
                        // Stop continuous acquisition.
                        //_MainCogFifo.OwnedTriggerParams.TriggerEnabled = false;
                    }

                }
                _DicPoint3d[_CameraOperation] = _CloudData;
                return _DicPoint3d;
            }
            catch (Exception ex)
            {
                return null;

            }

        }

        // Process an acquired pointcloud.
        static void ProcessPointCloud(CogImage8Grey image)
        {
            unsafe
            {
                // Get pointer to pixel memory.
                ICogImage8PixelMemory pixmem = image.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, image.Width, image.Height);
                byte* data = (byte*)pixmem.Scan0;

                // Pointer to the header structure.
                ESMHeader* header = (ESMHeader*)data;

                // Write timestamps to console.
                //LOG Console.WriteLine("Acquisition begin: " + header->acquisitionBeginTime.ToString());
                //LOG Console.WriteLine("Reconstruction end: " + header->reconstructionEndTime.ToString());

                // Early validity check.
                if (
                    header->magicNumber != 0xe5be7a00 ||
                    header->headerSize < 168
                )
                {
                    pixmem.Dispose();
                    //log
                    throw new Exception("Invalid ESM data");
                }

                // Calculate line pitch
                var pitch = (header->width * header->elementSize + header->alignment - 1) / header->alignment * header->alignment;

                // Check if the buffer is big enough
                var dataSize = (UInt64)(image.Width * image.Height);
                var requiredSize = header->pointsOffset + header->height * pitch;
                if (requiredSize > dataSize)
                {
                    pixmem.Dispose();
                    //log
                    throw new Exception("ESM buffer too small or incomplete");
                }

                // Pointer to begin of point data.
                var pointsData = data + header->pointsOffset;

                // Iterate through the points to calculate balance point.
                var balancePoint = new float[3];
                int numPoints = 0;
                var stopwatch = new CogStopwatch();
                stopwatch.Start();
                for (int i = 0; i < header->height; ++i)
                {
                    var line = (ESMPoint*)(pointsData + i * pitch);
                    for (int j = 0; j < header->width; ++j)
                    {
                        ESMPoint* point = &line[j];
                        if ((point->Flags & (byte)ESMPointFlags.Mask3D) == 0)
                        {
                            balancePoint[0] += point->X;
                            balancePoint[1] += point->Y;
                            balancePoint[2] += point->Z;
                            ++numPoints;
                        }
                    }
                }
                stopwatch.Stop();
                balancePoint[0] /= numPoints;
                balancePoint[1] /= numPoints;
                balancePoint[2] /= numPoints;
                Console.WriteLine("Read " + numPoints.ToString() + " points in " + stopwatch.Milliseconds.ToString() + "ms to calculate balance point: (" + balancePoint[0].ToString() + ", " + balancePoint[1].ToString() + ", " + balancePoint[2].ToString() + ")");

                pixmem.Dispose();
            }
        }

        CogImage16Grey rangeImage;
        CogImage8Grey maskImage;
        CogHistogram hist;
        CogHistogramResult histResult;
        ICogImage16PixelMemory rangePixels;
        ICogImage8PixelMemory maskPixels;
        public unsafe bool GetCloudFromRange(CogImage16Range imgInRange, CogImage8Grey imgInGrey = null, bool useSelectedSpace3D = true)
        {
            // _cloudData = new List<Point3D>();
            rangeImage = imgInRange.GetPixelData();
            maskImage = imgInRange.GetMaskData();

            //获取有效像素点数，预先申请空间提速
            hist = new CogHistogram();
            histResult = hist.Execute(maskImage, null);
            int[] histBins = histResult.GetHistogram();
            int noneZeroCount = histResult.NumSamples - histBins[0];

            int width = rangeImage.Width;
            int height = rangeImage.Height;
            int i = -1, j = -1;
            rangePixels = rangeImage.Get16GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, width, height);
            UInt16* pRangeRow = (UInt16*)rangePixels.Scan0;
            Int32 rangeStride = rangePixels.Stride;
            maskPixels = maskImage.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, width, height);
            byte* pMaskRow = (byte*)maskPixels.Scan0;
            Int32 maskStride = maskPixels.Stride;

            _CloudData.Capacity = noneZeroCount;

            for (j = 0; j < height; j++)
            {
                UInt16* pRangePel = pRangeRow;
                byte* pMaskPel = pMaskRow;

                for (i = 0; i < width; i++)
                {
                    if (*pMaskPel > 0)
                    {
                        //cloud.Add(new PclSharp.Struct.PointXYZ()
                        //{
                        //    X = i,
                        //    Y = j,
                        //    Z = *pRangePel,
                        //});
                        _CloudData.Add(new Point3D(i, j, *pRangePel));
                    }
                    pRangePel++;
                    pMaskPel++;
                }
                pRangeRow += rangeStride;

                pMaskRow += maskStride;
            }




            //range的像素坐标系转换至实际坐标系
            Cog3DTransformLinear transformTotal;
            if (useSelectedSpace3D == true)
            {
                try
                {
                    transformTotal = imgInRange.GetTransform3D(imgInRange.SelectedSpaceName3D, "#") as Cog3DTransformLinear;
                }
                catch (Exception e)
                {
                    transformTotal = imgInRange.GetTransform3D("Sensor3D", "#") as Cog3DTransformLinear;
                }
            }
            else
            {
                transformTotal = imgInRange.GetTransform3D("Sensor3D", "#") as Cog3DTransformLinear;
            }
            ApplyTransform(transformTotal);

            //设置像素大小
            Cog3DTransformLinear transform_Sensor3D = imgInRange.GetTransform3D("Sensor3D", "#") as Cog3DTransformLinear;
            Cog3DMatrix3x3 scaleMat = transform_Sensor3D.Inverse().GetMatrix();

            return true;
        }
        /// <summary>
        /// 3D转换
        /// </summary>
        /// <param name="transform"></param>
        public void ApplyTransform(Cog3DTransformLinear transform)
        {
            Cog3DMatrix3x3 matrix = transform.GetMatrix();
            Cog3DVect3 vect = transform.Translation;
            float e00 = (float)matrix.GetElement(0, 0);
            float e01 = (float)matrix.GetElement(0, 1);
            float e02 = (float)matrix.GetElement(0, 2);
            float e10 = (float)matrix.GetElement(1, 0);
            float e11 = (float)matrix.GetElement(1, 1);
            float e12 = (float)matrix.GetElement(1, 2);
            float e20 = (float)matrix.GetElement(2, 0);
            float e21 = (float)matrix.GetElement(2, 1);
            float e22 = (float)matrix.GetElement(2, 2);

            float e03 = (float)vect.X;
            float e13 = (float)vect.Y;
            float e23 = (float)vect.Z;

            double ptx, pty, ptz;
            double ptNewX, ptNewY, ptNewZ;
            int num = _CloudData.Count;
            for (int i = 0; i < num; i++)
            {
                ptx = _CloudData[i].X;
                pty = _CloudData[i].Y;
                ptz = _CloudData[i].Z;

                ptNewX = ptx * e00 + pty * e01 + ptz * e02 + e03;
                ptNewY = ptx * e10 + pty * e11 + ptz * e12 + e13;
                ptNewZ = ptx * e20 + pty * e21 + ptz * e22 + e23;

                _CloudData[i] = new Point3D(ptNewX, ptNewY, ptNewZ);
            }

            GC.Collect();
        }
        public bool disconnect()
        {
            try
            {
                Console.WriteLine(string.Format("{0}相机关闭啦！", _CameraOperation.Name));
                // this._log.LogInfo("log:相机关闭了");
                _MainCogCrabber.Disconnect(false);
                return true;
            }
            catch (Exception ex)
            {
                //log ；ex
                return false;

            }
        }

        public Dto_CameraOperation getCameraConfig()
        {
           return _CameraOperation;
        }
        /// <summary>
        /// 获取相机ROI参数
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_CameraROI getCameraROI()
        {
            return _CameraOperation.ROI;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool isEnabled()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 判断相机是否准备好 【预热】
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool isReady()
        {
            return getReadinessPercentile() >= 1;
        }

        // Multiple AcqFifo objects can exist for a single FrameGrabber. Each AcqFifo owns a set of parameters that are
        // guaranteed to be applied to the hardware when acquisitions are done using that instance. This function defines
        // what these parameters are.
        public bool setCameraConfig()
        {
            bool Setted = false;
            try
            {
                // CUDA device selection.
                //
                // This setting needs to be applied before any other setting. Changing it after any other settings have been
                // applied invalidates the AcqFifo.
                //
                // Although this property could potentially be set in the property list below this is not a good idea because of
                // the application delay this causes. For this reason we set this property directly on the frame grabber.
                //
                // By default the cudaDeviceString is null. To select a device different from the default run this
                // example once and choose one of the displayed valid options for the cuda_device string property.
                string cudaDeviceString = "[0] NVIDIA GeForce RTX 3090"; // Change this to "[0] GeForce GTX 1080" for example.
                if (cudaDeviceString != null)
                {
                    Console.WriteLine("Using non-default cuda device: " + cudaDeviceString);
                    _MainCogFifo.FrameGrabber.OwnedImagingDeviceAccess.SetFeature("cuda_device", cudaDeviceString);
                }

                // Disable the trigger so camera doesn't start acquiring right away when free run is enabled.
                _MainCogFifo.OwnedTriggerParams.TriggerEnabled = false;
                // Put the camera into continuous mode.
                _MainCogFifo.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.FreeRun;

                // Set the exposure time to 400µs (note that the exposure needs to be given in milliseconds).
                // The default value is 1000µs. The minimum value is 10µs. The maximum value is 50000µs.
                _MainCogFifo.OwnedExposureParams.Exposure = 1.1;

                // A lot of the device configuration is done using custom properties which are added to the
                // acqFifo.OwnedCustomPropertiesParams.CustomProps list. The following code provides comfortable
                // means of populating this list. A more concise approach will work just as well though.
                var deviceAccess = _MainCogFifo.FrameGrabber.OwnedImagingDeviceAccess;
                var propertyNames = deviceAccess.GetAvailableFeatures("root");
                var setProperties = new List<CogAcqCustomProperty>();
                if (false)
                {
                    Console.WriteLine("Custom properties:");
                }
                foreach (string propertyName in propertyNames)
                {
                    var propertyType = deviceAccess.GetFeatureType(propertyName);
                    if (propertyType == CogCustomPropertyTypeConstants.TypeCommand)
                    {
                        continue;
                    }

                    // The following list defines what properties the AcqFifo takes ownership of. To use the default setting from the
                    // config file just set propertyValue to 'null' in the respective case block.
                    string propertyValue = null;
                    switch (propertyName)
                    {
                        // Set the acquisition mode. A special mode ("esm_data") is used here to allow high-performance access to 3D data.
                        case "acquisition_mode": propertyValue = "range_image"; break;

                        // Enables binning which effectively combines pixels in the 2D camera for higher sensitivity.
                        // As a result, the images width and height are halved.
                        // In this mode, the region of interest is ignored and the full region of interest is used
                        // The default value is false.
                        case "binning": propertyValue = "True"; break;

                        // Compression is an alternative to HDR. It has less impact on acquisition time, but the results are not as precise.
                        // The effect of compression is a modified light intensity response of the cameras.
                        // The possible values are "None", "Low", "Medium" and "High".
                        // While "None" has a linear response, all other settings are not linear.Enabling Gradation Compression
                        // will require approximately a 4x longer exposure to take advantage of the larger dynamic range.
                        // The "Medium" and "High" settings are using 12 - bit acquisition which leads to a longer acquisition time
                        // compared to "None" and "Low" settings which use 10 - bit acquisition.The default value is "Low".
                        case "compression": propertyValue = "Low"; break;

                        // Set the minimum ratio of frame time to exposure (requires expert-mode).
                        // When frame_time_exposure_ratio_auto is set to true, the value of frame_time_exposure_ratio is ignored.
                        // frame_time_exposure_ratio ranges from 1.0 to 4.0.
                        // Choosing lower values means higher potential frame rates, but more blurry images, leading to higher noise in the 3D data.
                        // Choosing higher values means lower potential frame rates, but less blurry images, leading to lower noise in the 3D data.
                        // case "frame_time_exposure_ratio_auto": propertyValue = "True"; break;
                        case "frame_time_exposure_ratio": propertyValue = "1.0"; break;

                        // HDR is an optional feature that is useful for scenes with large variance in brightness.
                        // For example when the user wants to measure a shiny metal surface and a diffuse surface in the same scene.
                        // Depending on the set exposure time, the metal surface will either be over-exposed,
                        // or the diffuse surface will be under-exposed. The HDR mode allows the user to specify
                        // additional exposure times in microseconds, tuned for specific objects in the scene.
                        // The drawback of HDR is that the acquisition times are doubled/ tripled.
                        // The default setting is no HDR (both checkboxes unchecked). Both checkboxes can be checked at the same time.
                        // Low and High HDR are not mutually exclusive. This means, that the device will make 3 acquisitions.
                        // Enabling one of these using the checkbox makes the sensor acquire an additional sequence with another exposure time to use for reconstruction.
                        // Please be aware, that HDR has to be disabled in order to be able to use the FreeRun TriggerModel.
                        case "hdr_high_exposure": propertyValue = "6000"; break; // value in microseconds
                        case "hdr_high_exposure_enable": propertyValue = "False"; break;
                        case "hdr_low_exposure": propertyValue = "700"; break; // value in microseconds
                        case "hdr_low_exposure_enable": propertyValue = "False"; break;

                        // Sets the region of interest for the 2D cameras.
                        // These values are ignored when binning is enabled.
                        // The default values are device-specific. For 3D-A5000 devices:
                        // - 1440x1080 is the maximum resolution
                        // - 80x4 is the minimum resolution
                        // - increment of 'width',  'camera_1_offset_x' and 'camera_2_offset_x' is 8
                        // - increment of 'height', 'camera_1_offset_y' and 'camera_2_offset_y' is 4
                        case "width": propertyValue = "896"; break;
                        case "height": propertyValue = "800"; break;
                        case "camera_1_offset_x": propertyValue = "48"; break;
                        case "camera_1_offset_y": propertyValue = "0"; break;
                        case "camera_2_offset_x": propertyValue = "120"; break;
                        case "camera_2_offset_y": propertyValue = "0"; break;

                        // Number of 2D frames acquired for a 3D reconstruction. A higher number typically means better quality but also longer acquisition times.
                        // The default value is 24. The minimum value is 12. The maximum value is 60.
                        case "image_count": propertyValue = "20"; break;

                        // Set the LED brightness to 25% (requires expert-mode).
                        // It is recommended to not exceed a value of 25%, because extra pauses for thermal cooldown
                        // will be added between acquisitions, reducing the total rate of acquisition.
                        // Atttention: The full frame rate is only possible, if the CPMaxAvgPwrFan firmware parameter has been set to 64 (instead of the default value 12)
                        case "led_brightness": propertyValue = "25"; break;

                        // The outlier filter is a post-processing step that analyzes the neighborhood of each 3D point in order to find and remove outliers.
                        // The user can choose between "Disabled", "Permissive", "Balanced" and "Strict".The default value is "Permissive".
                        // This parameter is especially useful when the score threshold is lower than 0.9 or the pre - filtering value is lower than 4.
                        // Otherwise it is advisable to apply the "Disabled" setting.
                        case "outlier_filtering": propertyValue = "Disabled"; break;

                        // This value controls the reconstruction parameters.
                        // This setting does negatively affect the reconstruction time. The higher the setting, the higher the reconstruction time.
                        // The user can choose a value between 0 (lowest quality) and 9 (highest quality). The default value is 5.
                        case "reconstruction_quality": propertyValue = "4"; break;

                        // This value controls the filtering that is applied to the camera images before reconstruction.
                        // This setting does positively affect the reconstruction time. The higher the setting, the lower the reconstruction time.
                        // The user can choose a value between 0 (minimum filter intensity) and 9 (maximum filter intensity). The default value is 3.
                        case "pre_filtering": propertyValue = "3"; break;

                        // Each individual 3D point is reconstructed with a confidence score that ranges from 0.0 to 1.0.
                        // The user can choose a threshold to remove 3D points with a lower confidence score. The default value is 0.85.
                        case "score_threshold": propertyValue = "0.915"; break;

                        // Defines a frustum-shaped volume in which 3D points will be reconstructed.
                        // For 3D-A5000 devices, the user can choose between "Extended" and "Standard". The default value is "Extended".
                        // The extended volume is actually a combination of three frustums and may look somewhat distorted.
                        // This frustum volume can be visualized with the "Show Working Volume" button of the Point Cloud display.
                        case "working_volume": propertyValue = "Extended"; break;

                        // Defines a volume of interest in which 3D points will be reconstructed.
                        // This feature can be enabled or disabled.
                        // When enabled, the user can specify the boundaries of the volume manually (in millimeters).
                        case "volume_of_interest_enable": propertyValue = "true"; break;
                        case "volume_of_interest_max_x": propertyValue = "250.0"; break;
                        case "volume_of_interest_max_y": propertyValue = "250.0"; break;
                        case "volume_of_interest_max_z": propertyValue = "-50.0"; break;
                        case "volume_of_interest_min_x": propertyValue = "-250.0"; break;
                        case "volume_of_interest_min_y": propertyValue = "-250.0"; break;
                        case "volume_of_interest_min_z": propertyValue = "-400.0"; break;
                    }

                    if (propertyValue != null)
                    {
                        // Add the property to the list of properties this AcqFifo demands.
                        setProperties.Add(new CogAcqCustomProperty(propertyName, propertyValue, propertyType));
                    }

                    // Print the value of the property and possibly valid options for enum values to the console.
                    //if (printValues)
                    //{
                    //    if (propertyValue == null)
                    //    {
                    //        propertyValue = deviceAccess.GetFeature(propertyName);
                    //        Console.WriteLine("\t" + propertyName + " = " + propertyValue + " (default)");
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("\t" + propertyName + " = " + propertyValue);
                    //    }
                    //    if (propertyType == CogCustomPropertyTypeConstants.TypeEnum)
                    //    {
                    //        var validValues = deviceAccess.GetValidEnumValues(propertyName);
                    //        Console.WriteLine("\t\tValid values:");
                    //        foreach (var validValue in validValues)
                    //        {
                    //            Console.WriteLine("\t\t\t - " + validValue);
                    //        }
                    //    }
                    //}
                }

                // Apply the property-demands list to the AcqFifo.
                _MainCogFifo.OwnedCustomPropertiesParams.CustomProps = setProperties;
                _CameraOperation.IsAvailable = true;
                return true;
            }
            catch (Exception ex)
            {
                return Setted;

                //throw;
            }

        }

        public bool setCameraROI(Dto_CameraROI ROI)
        {
            bool isDown = false;
            if (ROI!=null)
            {
                _CameraOperation.ROI = ROI;
            }
            else
            {
                //log peizhiweikong
            }

            //重新设置 
            //setCameraConfig();
            return isDown;
        }

        public double getReadinessPercentile()
        {
            //根据预热开始时间，结束时间和当前时间计算百分比
            double Numerator = (_PreheatingEndTime.Hour-_PreheatingStartTime.Hour) * 60 * 60 + (_PreheatingEndTime.Minute - _PreheatingStartTime.Minute) * 60 +
                (_PreheatingEndTime.Second - _PreheatingStartTime.Second);
            DateTime dtNow = DateTime.Now;
            double Denominator = dtNow.Hour * 60 * 60 + dtNow.Minute * 60 + dtNow.Second;
            return Numerator / Denominator;
        }
    }
}
