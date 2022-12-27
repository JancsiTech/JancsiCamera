using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using JancsiVisionConfigServices.Model;
using Newtonsoft.Json;

namespace JancsiVisionConfigServices
{
    public class FileCongfigServer : IConfigService
    {
        public string FilePath { get; set; }


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
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "Camera.json");
            CameraConfig config = new CameraConfig();
            if (System.IO.File.Exists(path))
            {
                string jsonStr = File.ReadAllText(path, Encoding.UTF8);
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<CameraConfig>(jsonStr);
            }
            return config;

        }
        public void SaveAffineMatrixConfig(string CameraName, string SerialNumber, List<List<double>> lisMatrixl)
        {
            List<List<double>> pointsA = new List<List<double>>();
            pointsA.Add(new List<double>() { 1.76435536, -0.62500892, 0.10972233, -14.86004 });
            pointsA.Add(new List<double>() { 0.63284402, 1.70918776, -0.44023879, -50.66088782 });
            pointsA.Add(new List<double>() { 0.0467290115, 0.451293596, 1.81927867, 593.409915 });


            //List<List<double>> pointsB = new List<List<double>>();
            //pointsB.Add(new List<double>() { -1.05771195, 0.94722754, -1.2245939, -2.52334388 });
            //pointsB.Add(new List<double>() { -1.54750176, -0.60286345, 0.87029831, 100.97457192 });
            //pointsB.Add(new List<double>() { 0.0459242287, 1.50164588, 1.12186268, -72.1171435 });

            //List<List<double>> pointsC = new List<List<double>>();
            //pointsC.Add(new List<double>() { 84.134614109993, 249.198717951775, -250.586700439453, });
            //pointsC.Add(new List<double>() { 88.14102435112, 249.198717951775, -250.538635253906, });

            //listAffineMatrix.Add(pointsA);
            //listAffineMatrix.Add(pointsA);
            //listAffineMatrix.Add(pointsA);

            try
            {            //从ini文件中获取
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "Camera.json");
                CameraConfig config = new CameraConfig();
                if (System.IO.File.Exists(path))
                {
                    string jsonStr = File.ReadAllText(path, Encoding.UTF8);
                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<CameraConfig>(jsonStr);

                }

                CameraSetting ChoseCameraSetting = config.Cameras.Where(o => o.Name == CameraName && o.SerialNumber == SerialNumber).FirstOrDefault();
                //CameraSetting ChoseCameraSetting = config.Cameras[0];
                //修改外参矩阵配置
                ChoseCameraSetting.CameraAffineMatrixl = new RTTwoDimensionalMatrix();
                ChoseCameraSetting.CameraAffineMatrixl.Matrix = pointsA;
                //ChoseCameraSetting.CameraAffineMatrixl.Matrix2 = lisMatrixl[1];
                //ChoseCameraSetting.CameraAffineMatrixl.Matrix3 = lisMatrixl[2];

                string configString = Newtonsoft.Json.JsonConvert.SerializeObject(config);
                //这里先加个创建文件好保存
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);//创建文件
                    fs.Close();
                }

                File.WriteAllLines(path, new string[] { configString }, Encoding.UTF8);
            }
            catch (Exception ex)
            {

                // Log.PLog.LogFull(this.GetType().ToString(), Log.MsgType.Error, string.Format("camera setting save faill, result={0}", ex.ToString()));
                // throw;
            }

        }

    }
}
