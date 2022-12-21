using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JancsiVisionConfigServices.Model
{
    public class CameraConfig
    {
        CameraSetting _CurrentCamera;
        string _CurrentName;
        ObservableCollection<CameraSetting> _Cameras;
        public CameraConfig()
        {
            Cameras = new ObservableCollection<CameraSetting>();
        }
        public ObservableCollection<CameraSetting> Cameras { get { return _Cameras; } set { _Cameras = value; } }
        public string CurrentCameraName { get { return _CurrentName; } }

        [Newtonsoft.Json.JsonIgnore]
        public CameraSetting CurrentCamera { get { return _CurrentCamera; } set { _CurrentCamera = value; _CurrentName = _CurrentCamera.Name; } }


    }
    public class CameraSetting
    {
        string _Name;
        string _SerialNumber;
        Capture _Capture;
        ROI _ROI;
        ReconstructionQuality _ReconstructionQuality;
        PreFilter _PreFilter;
        PostFiler _PostFilter;
        RegionExtract _RegionExtract;
        bool _IsAvailable;
        List<LocationXYZ> _ThreeMachineCalibration;
        public CameraSetting()
        {
            Capture = new Capture();
            ROI = new ROI();
            ReconstructionQuality = new ReconstructionQuality();
            PreFilter = new PreFilter();
            PostFilter = new PostFiler();
            RegionExtract = new RegionExtract();
            IsAvailable = false;
            _ThreeMachineCalibration = new List<LocationXYZ>();
        }
        [Newtonsoft.Json.JsonIgnore]
        public bool IsAvailable { get { return _IsAvailable; } set { _IsAvailable = value; } }
        public string Name { get { return _Name; } set { _Name = value; } }
        public string SerialNumber { get { return _SerialNumber; } set { _SerialNumber = value; } }
        public Capture Capture { get { return _Capture; } set { _Capture = value; } }
        public ROI ROI { get { return _ROI; } set { _ROI = value; } }
        public ReconstructionQuality ReconstructionQuality { get { return _ReconstructionQuality; } set { _ReconstructionQuality = value; } }
        public PreFilter PreFilter { get { return _PreFilter; } set { _PreFilter = value; } }
        public PostFiler PostFilter { get { return _PostFilter; } set { _PostFilter = value; } }
        public RegionExtract RegionExtract { get { return _RegionExtract; } set { _RegionExtract = value; } }

        public List<LocationXYZ> ThreeMachineCalibration { get { return _ThreeMachineCalibration; } set { _ThreeMachineCalibration = value; } }
        
    }
    public enum Compression
    {
        None,
        Low
    }
    public class Capture
    {

        float _ImagesCount = 24;
        double _ExposureLevel = 1.1;
        bool _EnableLowExposure = false;
        int _LowExposureLevel = 1000;
        bool _EnableHighExposure = false;
        int _HighExposureLevel = 1000;
        Compression _Compress = Compression.Low;

        public float ImagesCount { get { return _ImagesCount; } set { _ImagesCount = value; } }
        public double ExposureLevel { get { return _ExposureLevel; } set { _ExposureLevel = value; } }
        public bool EnableLowExposure { get { return _EnableLowExposure; } set { _EnableLowExposure = value; } }
        public int LowExposureLevel { get { return _LowExposureLevel; } set { _LowExposureLevel = value; } }
        public bool EnableHighExposure { get { return _EnableHighExposure; } set { _EnableHighExposure = value; } }
        public int HighExposureLevel { get { return _HighExposureLevel; } set { _HighExposureLevel = value; } }
        public Compression Compress { get { return _Compress; } set { _Compress = value; } }

        [Newtonsoft.Json.JsonIgnore]
        public int MinExposureLevel { get { return 10; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MaxExposureLevel { get { return 50000; } }

    }
    public class ROI
    {
        int _Width = 1440;
        int _Height = 1080;
        bool _Size2X2 = false;
        int _Camera1OffsetX = 0;
        int _Camera1OffsetY = 0;
        int _Camera2OffsetX = 0;
        int _Camera2OffsetY = 0;
        public int Width { get { return _Width; } set { _Width = value; } }
        public int Height { get { return _Height; } set { _Height = value; } }
        public bool Size2X2 { get { return _Size2X2; } set { _Size2X2 = value; } }
        public int Camera1OffsetX { get { return _Camera1OffsetX; } set { _Camera1OffsetX = value; } }
        public int Camera1OffsetY { get { return _Camera1OffsetY; } set { _Camera1OffsetY = value; } }
        public int Camera2OffsetX { get { return _Camera2OffsetX; } set { _Camera2OffsetX = value; } }
        public int Camera2OffsetY { get { return _Camera2OffsetY; } set { _Camera2OffsetY = value; } }


    }
    public class ReconstructionQuality
    {
        int _Quality = 4;
        public int Quality { get { return _Quality; } set { _Quality = value; } }

        [Newtonsoft.Json.JsonIgnore]
        public int MinQuality { get { return 0; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MaxQuality { get { return 9; } }

    }
    public class PreFilter
    {
        int _Threshold = 3;
        public int Threshold { get { return _Threshold; } set { _Threshold = value; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MinThreshold { get { return 0; } }
        [Newtonsoft.Json.JsonIgnore]
        public int MaxThreshold { get { return 9; } }

    }
    public enum OutlierFilter
    {
        Disable,
        Permissive,
        Balanced,
        Strict
    }
    public enum WorkingVolumeValue
    {
        Standard,
        Extended
    }
    public class PostFiler
    {

        float _Threshold = 0.85f;
        OutlierFilter _ExceptionValueFilter = OutlierFilter.Permissive;
        WorkingVolumeValue _Workout = WorkingVolumeValue.Extended;
        public float Threshold { get { return _Threshold; } set { _Threshold = value; } }
        public OutlierFilter ExceptionValueFilter { get { return _ExceptionValueFilter; } set { _ExceptionValueFilter = value; } }
        public WorkingVolumeValue WorkingVolume { get { return _Workout; } set { _Workout = value; } }

        [Newtonsoft.Json.JsonIgnore]
        public float MinThreshold { get { return 0; } }
        [Newtonsoft.Json.JsonIgnore]
        public float MaxThreshold { get { return 1; } }
    }
    public class RegionExtract
    {
        bool _EnableVOI = false;
        int _LimitationXMin = -340;
        int _LimitationXMax = 340;
        int _LimitationYMin = -265;
        int _LimitationYMax = 265;
        int _LimitationZMin = -400;
        int _LimitationZMax = 400;
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

    public class LocationXYZ
    {

        double _X, _Y, _Z;
        public double X { get { return _X; } set { _X = value; } }
        public double Y { get { return _Y; } set { _Y = value; } }
        public double Z { get { return _Z; } set { _Z = value; } }

    }
}
