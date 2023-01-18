using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using static JancsiVisionConfigServices.Model.CameraCalibrationSetting;

namespace JancsiVisionConfigServices.Model
{
    public class CalibrationSettings
    {
        CameraCalibrationSetting _CurrentCamera;
        string _CurrentName;
        ObservableCollection<CameraCalibrationSetting> _CamerasCali;
        public CalibrationSettings()
        {
            CamerasCali = new ObservableCollection<CameraCalibrationSetting>();
        }
        public ObservableCollection<CameraCalibrationSetting> CamerasCali { get { return _CamerasCali; } set { _CamerasCali = value; } }
        public string CurrentCameraName { get { return _CurrentName; } }

        [Newtonsoft.Json.JsonIgnore]
        public CameraCalibrationSetting CurrentCamera { get { return _CurrentCamera; } set { _CurrentCamera = value; _CurrentName = _CurrentCamera.Name; } }

        ObservableCollection<PhysicalCoordinateCalibrationSetting> _PhysicalCoordinate;

        public ObservableCollection<PhysicalCoordinateCalibrationSetting> physicalCoordinateCalibrationSetting { get { return _PhysicalCoordinate; } set { _PhysicalCoordinate = value; } }
    }

    public class CameraCalibrationSetting
    {
        string _Name;
        string _SerialNumber;
        //  string _Specification;
        int _CameraId;
        List<LocationXYZ> _ThreeMachineCalibration;
        RTTwoDimensionalMatrix _CameraAffineMatrixl;
        RTTwoDimensionalMatrixXYZ _CameraAffineMatrixlXyz;
        RTTwoDimensionalMatrixK _CameraAffineMatrixlK;
        public CameraCalibrationSetting()
        {
            _ThreeMachineCalibration = new List<LocationXYZ>();
            _CameraAffineMatrixl = new RTTwoDimensionalMatrix();
            _CameraAffineMatrixlXyz = new RTTwoDimensionalMatrixXYZ();
            _CameraAffineMatrixlK = new RTTwoDimensionalMatrixK();
        }

        public string Name { get { return _Name; } set { _Name = value; } }

        public string SerialNumber { get { return _SerialNumber; } set { _SerialNumber = value; } }
        //  public string Specification { get { return _Specification; } set { _Specification = value; } }
        public int CameraId { get { return _CameraId; } set { _CameraId = value; } }
        public List<LocationXYZ> ThreeMachineCalibration { get { return _ThreeMachineCalibration; } set { _ThreeMachineCalibration = value; } }

        public RTTwoDimensionalMatrix CameraAffineMatrixl { get { return _CameraAffineMatrixl; } set { _CameraAffineMatrixl = value; } }

        public RTTwoDimensionalMatrixXYZ CameraAffineMatrixlXyz { get { return _CameraAffineMatrixlXyz; } set { _CameraAffineMatrixlXyz = value; } }

        public RTTwoDimensionalMatrixK CameraAffineMatrixlK { get { return _CameraAffineMatrixlK; } set { _CameraAffineMatrixlK = value; } }


        public class LocationXYZ
        {

            double _X, _Y, _Z;
            public double X { get { return _X; } set { _X = value; } }
            public double Y { get { return _Y; } set { _Y = value; } }
            public double Z { get { return _Z; } set { _Z = value; } }

        }
        public class RTTwoDimensionalMatrix
        {

            List<List<double>> _Matrix;
            public List<List<double>> Matrix { get { return _Matrix; } set { _Matrix = value; } }

        }

        public class RTTwoDimensionalMatrixXYZ
        {

            double[,] _listAffineMatrixXYZ;
            public double[,] listAffineMatrixXYZ { get { return _listAffineMatrixXYZ; } set { _listAffineMatrixXYZ = value; } }

        }

        public class RTTwoDimensionalMatrixK
        {

            double[] _listAffineMatrixK;
            public double[] listAffineMatrixK { get { return _listAffineMatrixK; } set { _listAffineMatrixK = value; } }

        }


    }

    public class PhysicalCoordinateCalibrationSetting
    {
        int _Coordinate;

        List<List<LocationXYZ>> _ThreeMachine;
        public PhysicalCoordinateCalibrationSetting()
        {
            _ThreeMachine = new List<List<LocationXYZ>>();

        }
        public int Coordinate { get { return _Coordinate; } set { _Coordinate = value; } }

        public List<List<LocationXYZ>> ThreeMachineCalibration { get { return _ThreeMachine; } set { _ThreeMachine = value; } }


    }
}
