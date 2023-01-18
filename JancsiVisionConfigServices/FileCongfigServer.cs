using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using JancsiVisionConfigServices.Model;
using Newtonsoft.Json;
using static JancsiVisionConfigServices.Model.CameraCalibrationSetting;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Net.Http.Headers;

namespace JancsiVisionConfigServices
{
    public class FileCongfigServer : IConfigService
    {
        public string FilePath { get; set; }


        private string path;

        private string Calibrationpath;

        public CalibrationSettings _configCalibration;


        public FileCongfigServer()
        {
            path = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "Camera.json");

            Calibrationpath = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "CameraCalibrationSetting.json");
        }
        /// <summary>
        /// 可以从文件中读取默认相机配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetCameraConfig(string name)
        {
            return "拿到相机默认配置啦！";
        }
        /// <summary>
        /// 读取本地配置如日志路径，用户名，密码，相机版本多种类型配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CameraConfig GetEnvironmentConfig(string name)
        {
            /////从ini文件中获取
            //var kv = File.ReadAllLines(FilePath).Select(s => s.Split('=')).Select(strs => new { Name = strs[0], value = strs[1] }).SingleOrDefault(kc => kc.Name == name);
            //if (kv != null)
            //{
            //    return kv.value;
            //}
            //else
            //{
            //    return null;
            //}
            CameraConfig config = new CameraConfig();
            if (System.IO.File.Exists(path))
            {
                string jsonStr = File.ReadAllText(path, Encoding.UTF8);
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<CameraConfig>(jsonStr);
            }
            return config;

        }

        public CameraCalibrationSetting GetCameraCaCalibrationConfig(string Name, string SerialNumber)
        {
            CameraCalibrationSetting cameraCalibrationSetting = new CameraCalibrationSetting();
            if (_configCalibration == null)
            {
                _configCalibration = new CalibrationSettings();
                if (System.IO.File.Exists(Calibrationpath))
                {
                    string jsonStr = File.ReadAllText(Calibrationpath, Encoding.UTF8);
                    _configCalibration = Newtonsoft.Json.JsonConvert.DeserializeObject<CalibrationSettings>(jsonStr);
                }
            }

            if (_configCalibration.CamerasCali != null && _configCalibration.CamerasCali.Count > 0)
            {

                cameraCalibrationSetting = _configCalibration.CamerasCali.Where(o => o.Name == Name && o.SerialNumber == SerialNumber).FirstOrDefault();

            }
            return cameraCalibrationSetting;

        }



        public void SaveAffineMatrixConfig(string CameraName, string SerialNumber, List<List<double>> lisMatrixl, int specification)
        {
            try
            {
                //从ini文件中获取

                //CalibrationSettings configCalibration = new CalibrationSettings();
                //if (System.IO.File.Exists(Calibrationpath))
                //{
                //    string jsonStr = File.ReadAllText(Calibrationpath, Encoding.UTF8);
                //    configCalibration = Newtonsoft.Json.JsonConvert.DeserializeObject<CalibrationSettings>(jsonStr);
                //}

                CameraCalibrationSetting ChoseCameraSetting = _configCalibration.CamerasCali.Where(o => o.Name == CameraName && o.SerialNumber == SerialNumber).FirstOrDefault();

                var lisPsotions = _configCalibration.physicalCoordinateCalibrationSetting.Where(o => o.Coordinate == specification).FirstOrDefault().ThreeMachineCalibration[ChoseCameraSetting.CameraId];

                ChoseCameraSetting.ThreeMachineCalibration = lisPsotions;
                //CameraSetting ChoseCameraSetting = config.Cameras[0];
                //修改外参矩阵配置
                ChoseCameraSetting.CameraAffineMatrixl = new RTTwoDimensionalMatrix();
                ChoseCameraSetting.CameraAffineMatrixl.Matrix = lisMatrixl;
                //增加读取相机对应物理坐标信息
                ChoseCameraSetting.CameraAffineMatrixlXyz = new RTTwoDimensionalMatrixXYZ();
                ChoseCameraSetting.CameraAffineMatrixlXyz.listAffineMatrixXYZ = new double[3, 3];
                ChoseCameraSetting.CameraAffineMatrixlK = new RTTwoDimensionalMatrixK();
                ChoseCameraSetting.CameraAffineMatrixlK.listAffineMatrixK = new double[3];

                if (ChoseCameraSetting.CameraAffineMatrixl != null && ChoseCameraSetting.CameraAffineMatrixl.Matrix != null && ChoseCameraSetting.CameraAffineMatrixl.Matrix.Count > 0)
                {
                    ChoseCameraSetting.CameraAffineMatrixlK.listAffineMatrixK[0] = ChoseCameraSetting.CameraAffineMatrixl.Matrix[0][3];
                    ChoseCameraSetting.CameraAffineMatrixlK.listAffineMatrixK[1] = ChoseCameraSetting.CameraAffineMatrixl.Matrix[1][3];
                    ChoseCameraSetting.CameraAffineMatrixlK.listAffineMatrixK[2] = ChoseCameraSetting.CameraAffineMatrixl.Matrix[2][3];

                    for (int i = 0; i < ChoseCameraSetting.CameraAffineMatrixlXyz.listAffineMatrixXYZ.GetLength(0); i++) //遍历第一维
                    {
                        for (int j = 0; j < ChoseCameraSetting.CameraAffineMatrixlXyz.listAffineMatrixXYZ.GetLength(1); j++) //遍历第二维
                        {
                            ChoseCameraSetting.CameraAffineMatrixlXyz.listAffineMatrixXYZ[i, j] = ChoseCameraSetting.CameraAffineMatrixl.Matrix[i][j];//为数组赋值
                        }

                    }

                }
                //配置物理坐标





                string configString = Newtonsoft.Json.JsonConvert.SerializeObject(_configCalibration);
                //这里先加个创建文件好保存
                if (!File.Exists(Calibrationpath))
                {
                    FileStream fs = File.Create(Calibrationpath);//创建文件
                    fs.Close();
                }

                File.WriteAllLines(Calibrationpath, new string[] { configString }, Encoding.UTF8);
            }
            catch (Exception ex)
            {

                // Log.PLog.LogFull(this.GetType().ToString(), Log.MsgType.Error, string.Format("camera setting save faill, result={0}", ex.ToString()));
                // throw;
            }

        }
        /// <summary>
        /// 保存相机配置 没有则创建
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="SerialNumber"></param>
        /// <param name="uuid"></param>
        public void SaveConfig(string Name, string SerialNumber, int uuid)
        {
            try
            {

                //Task<int> printRes = Task.Run(() =>
                //{
                //    #region 配置文件
                //    CameraConfig config = new CameraConfig();
                //    if (!System.IO.File.Exists(path))
                //    {
                //        FileStream fs = File.Create(path);//创建文件
                //        fs.Close();
                //    }

                //    string jsonStr = File.ReadAllText(path, Encoding.UTF8);
                //    config = Newtonsoft.Json.JsonConvert.DeserializeObject<CameraConfig>(jsonStr);

                //    if (config.Cameras == null && config.Cameras.Count == 0)
                //    {
                //        config.Cameras = new System.Collections.ObjectModel.ObservableCollection<CameraSetting>();
                //    }

                //    CameraSetting cameraSettingNew = new CameraSetting();
                //    cameraSettingNew.IsAvailable = false;
                //    cameraSettingNew.Name = Name;
                //    cameraSettingNew.SerialNumber = SerialNumber;
                //    config.Cameras.Add(cameraSettingNew);
                //    config.CurrentCamera = cameraSettingNew;

                //    string configString = Newtonsoft.Json.JsonConvert.SerializeObject(config);

                //    File.WriteAllLines(path, new string[] { configString }, Encoding.UTF8);
                //    #endregion

                //    return 1;

                //});
                Task<int> printCalibrationReslut = Task.Run(() =>
                {

                    #region 物理坐标

                    CalibrationSettings calibrationSettings = new CalibrationSettings();
                    if (!System.IO.File.Exists(Calibrationpath))
                    {
                        FileStream fs = File.Create(Calibrationpath);//创建文件
                        fs.Close();
                    }
                    string jsonCalibrationStr = File.ReadAllText(Calibrationpath, Encoding.UTF8);
                    calibrationSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<CalibrationSettings>(jsonCalibrationStr);
                    if (calibrationSettings == null)
                    {
                        calibrationSettings = new CalibrationSettings();
                    }
                    if (calibrationSettings.CamerasCali == null && calibrationSettings.CamerasCali.Count == 0)
                    {
                        calibrationSettings.CamerasCali = new System.Collections.ObjectModel.ObservableCollection<CameraCalibrationSetting>();
                    }
                    calibrationSettings.physicalCoordinateCalibrationSetting = new System.Collections.ObjectModel.ObservableCollection<PhysicalCoordinateCalibrationSetting>();

                    //var CameraCalirationSel = calibrationSettings.CamerasCali.Where(o => o.Name == Name && o.SerialNumber == SerialNumber).FirstOrDefault();
                    //if (CameraCalirationSel != null)
                    //{
                    //    calibrationSettings.CurrentCamera = CameraCalirationSel;
                    //}
                    //else
                    //{
                    //    CameraCalibrationSetting cameraCalibrationSetting = new CameraCalibrationSetting();
                    //    cameraCalibrationSetting.Name = Name;
                    //    cameraCalibrationSetting.SerialNumber = SerialNumber;
                    //    calibrationSettings.CamerasCali.Add(cameraCalibrationSetting);
                    //    calibrationSettings.CurrentCamera = cameraCalibrationSetting;
                    //    calibrationSettings.physicalCoordinateCalibrationSetting = new System.Collections.ObjectModel.ObservableCollection<PhysicalCoordinateCalibrationSetting>();

                    //    PhysicalCoordinateCalibrationSetting py = new PhysicalCoordinateCalibrationSetting();
                    //    py.Coordinate = 50;
                    //    py.ThreeMachineCalibration = new List<List<LocationXYZ>>();
                    //    py.ThreeMachineCalibration.Add(location0XYZs);
                    //    py.ThreeMachineCalibration.Add(location90XYZs);
                    //    py.ThreeMachineCalibration.Add(location180XYZs);

                    //    calibrationSettings.physicalCoordinateCalibrationSetting.Add(py);

                    //    PhysicalCoordinateCalibrationSetting py1 = new PhysicalCoordinateCalibrationSetting();
                    //    py1.Coordinate = 100;
                    //    py1.ThreeMachineCalibration = new List<List<LocationXYZ>>();
                    //    py1.ThreeMachineCalibration.Add(location0XYZs);
                    //    py1.ThreeMachineCalibration.Add(location90XYZs);
                    //    py1.ThreeMachineCalibration.Add(location180XYZs);

                    //    calibrationSettings.physicalCoordinateCalibrationSetting.Add(py);

                    //    //switch (uuid)
                    //    //{
                    //    //    case 0:
                    //    //        cameraCalibrationSetting.ThreeMachineCalibration = location0XYZs;
                    //    //        break;
                    //    //    case 1:
                    //    //        cameraCalibrationSetting.ThreeMachineCalibration = location90XYZs;
                    //    //        break;
                    //    //    case 2:
                    //    //        cameraCalibrationSetting.ThreeMachineCalibration = location180XYZs;
                    //    //        break;
                    //    //    default:
                    //    //        break;
                    //    //}
                    //}




                    string configString = Newtonsoft.Json.JsonConvert.SerializeObject(calibrationSettings);

                    File.WriteAllLines(Calibrationpath, new string[] { configString }, Encoding.UTF8);
                    #endregion

                    return 1;

                });
            }
            catch (Exception ex)
            {

                // Log.PLog.LogFull(this.GetType().ToString(), Log.MsgType.Error, string.Format("camera setting save faill, result={0}", ex.ToString()));
                // throw;
            }

        }
    }
}
