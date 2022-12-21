using JancsiVisionConfigServices.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionCameraServers.Model
{
    public class Dto_CameraOperation
    {

        /// <summary>
        /// 相机编号对照  目前累加模式
        /// </summary>
        public int uuid;
        /// <summary>
        ///  camera has its own static ip address
        /// </summary>
        public string Ip;
        /// <summary>
        ///  when calibration, querying real world coordinates
        /// </summary>
        public int cameraId;
        /// <summary>
        /// 相机编号
        /// </summary>
        public string SerialNumber;

        //GPU加速显卡名称 例： [0] NVIDIA GeForce RTX 3090"; // Change this to "[0] GeForce GTX 1080" for example.
        public string cudaDeviceString = "";

        /// <summary>
        /// 相机名称-同系列产品可能相同 # not null
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Acquisition Parameters 采集参数
        /// </summary>
        public Capture Capture { get; set; }
        /// <summary>
        /// Region of Interest 参数
        /// </summary>
        public Dto_CameraROI ROI { get; set; }

        /// <summary>
        /// ReconstructionQuality 参数
        /// </summary>
        public ReconstructionQuality ReconstructionQuality { get; set; }
        /// <summary>
        /// 预先过滤
        /// </summary>
        public PreFilter PreFilter { get; set; }
        /// <summary>
        /// 后续过滤
        /// </summary>
        public PostFiler PostFilter { get; set; }
        /// <summary>
        /// 提取区域
        /// </summary>
        public RegionExtract RegionExtract { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 对照业务逻辑 相机位置角度 0，90，180
        /// </summary>
        public string CameraPosotion { get; set; }

        /// <summary>
        /// 校准时间
        /// </summary>
        public DateTime? calibrateTime { get; set; }
        /// <summary>
        /// 相机物理坐标
        /// </summary>
        public List<LocationXYZ> _ThreeMachineCalibration;
        //Matrix affineMatrix; // persist on disk read from disk at loading time

    }
    public class Capture
    {
        //unit:ms
        //# important params regarding FPS,
        //# most of time same across all cameras.
        int _ImagesCount = 20;
        /// <summary>
        /// 曝光度
        /// </summary>
        double _ExposureLevel = 1.1;
        bool _EnableLowExposure = false;
        //# HDR should be disabled by default
        /// <summary>
        /// HDR 低曝光度
        /// </summary>
        double _LowExposureLevel = 700;
        bool _EnableHighExposure = false;
        /// <summary>
        /// HDR 高曝光度
        /// </summary>
        double _HighExposureLevel = 6000;
        Compression _Compress = Compression.Low;

        public int ImagesCount { get { return _ImagesCount; } set { _ImagesCount = value; } }
        public double ExposureLevel { get { return _ExposureLevel; } set { _ExposureLevel = value; } }
        public bool EnableLowExposure { get { return _EnableLowExposure; } set { _EnableLowExposure = value; } }
        public double LowExposureLevel { get { return _LowExposureLevel; } set { _LowExposureLevel = value; } }
        public bool EnableHighExposure { get { return _EnableHighExposure; } set { _EnableHighExposure = value; } }
        public double HighExposureLevel { get { return _HighExposureLevel; } set { _HighExposureLevel = value; } }
        public Compression Compress { get { return _Compress; } set { _Compress = value; } }

        [Newtonsoft.Json.JsonIgnore]
        public int MinExposureLevel { get { return 10; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MaxExposureLevel { get { return 50000; } }

      
    }
    public enum Compression
    {
        None,
        Low
    }
    /// <summary>
    /// 质量参数
    /// </summary>
    public class ReconstructionQuality
    {
        int _Quality = 4;
        public int Quality { get { return _Quality; } set { _Quality = value; } }

        [Newtonsoft.Json.JsonIgnore]
        public int MinQuality { get { return 0; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MaxQuality { get { return 9; } }

    }
    /// <summary>
    /// 预过滤器参数
    /// </summary>
    public class PreFilter
    {
        int _Threshold = 3;
        public int Threshold { get { return _Threshold; } set { _Threshold = value; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MinThreshold { get { return 0; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MaxThreshold { get { return 9; } }

    }
    /// <summary>
    /// 发布参数
    /// </summary>
    public class PostFiler
    {

        float _Threshold = 0.915f;
        OutlierFilter _ExceptionValueFilter = OutlierFilter.Disabled;
        WorkingVolumeValue _Workout = WorkingVolumeValue.Extended;
        public float Threshold { get { return _Threshold; } set { _Threshold = value; } }
        public OutlierFilter ExceptionValueFilter { get { return _ExceptionValueFilter; } set { _ExceptionValueFilter = value; } }
        public WorkingVolumeValue WorkingVolume { get { return _Workout; } set { _Workout = value; } }

        [Newtonsoft.Json.JsonIgnore]
        public float MinThreshold { get { return 0; } }
        [Newtonsoft.Json.JsonIgnore]
        public float MaxThreshold { get { return 1; } }
    }
    /// <summary>
    /// By default the 3D sensor generates a 3D range image from all data within the field of view of both cameras. Enable a Volume of Interest and use the values X limits, Y limits and Z limits to extract a portion of the available 3D object volume. The settings allow you configure the smallest and highest limits for each axis.
    /// </summary>
    public class RegionExtract
    {
        //unit:mm
        bool _EnableVOI = true;
        int _LimitationXMin = -250;
        int _LimitationXMax = 250;
        int _LimitationYMin = -250;
        int _LimitationYMax = 250;
        int _LimitationZMin = -400;
        int _LimitationZMax = -50;
        bool _Preview;

        public bool EnableVOI { get { return _EnableVOI; } set { _EnableVOI = value; } }
        public int LimitationXMin { get { return _LimitationXMin; } set { _LimitationXMin = value; } }
        public int LimitationXMax { get { return _LimitationXMax; } set { _LimitationXMax = value; } }
        public int LimitationYMin { get { return _LimitationYMin; } set { _LimitationYMin = value; } }
        public int LimitationYMax { get { return _LimitationYMax; } set { _LimitationYMax = value; } }
        public int LimitationZMin { get { return _LimitationZMin; } set { _LimitationZMin = value; } }
        public int LimitationZMax { get { return _LimitationZMax; } set { _LimitationZMax = value; } }
        public bool Preview { get { return _Preview; } set { _Preview = value; } }

    }

    /// <summary>
    /// The options are Disabled, Permissive, Balanced and Strict, in that order. As you increase the policy from one to the next, the number of outliers included in the 3D range image decrease while increasing the risk of filtering out valid 3D points. 
    /// </summary>
    public enum OutlierFilter
    {
        Disabled,
        Permissive,
        Balanced,
        Strict
    }
    /// <summary>
    /// Define the volume of space within the field of view of both GigE Vision cameras in which 3D points will be constructed: 
    /// Standard: The limited volume with the highest accuracy for the 3D sensor
    /// Extended: A greater volume of space but at the cost of some accuracy in the 3D points
    /// </summary>
    public enum WorkingVolumeValue
    {
        Standard,
        Extended
    }

}
