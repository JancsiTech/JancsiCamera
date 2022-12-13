using JancsiVisionConfigServices;
using JancsiVisionLogServers;
using JancsiVisionUtilityServers.Model;
using JancsiVisionPointCloudServers.Model;
using System;
using System.Collections.Generic;
using System.Text;
using JancsiVisionCameraServers.Model;
using Python.Runtime;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.IO;

namespace JancsiVisionUtilityServers
{
    public class JancsiUtilityServer : IJancsiUtilityServer
    {
        private readonly ILogProvider log;
        private readonly IConfigService config;

        public JancsiUtilityServer()
        {
            string pathToVirtualEnv = @"/pyd";

            //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            // Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);


            //PythonEngine.PythonHome = pathToVirtualEnv;
            // PythonEngine.PythonPath = Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
        }

        public JancsiUtilityServer(ILogProvider log, IConfigService config)
        {
            this.log = log;
            this.config = config;
        }

        /// <summary>
        /// 点云融合
        /// </summary>
        /// <param name="DicCameraAndPoint"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_PointCloud fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint)
        {
            //List<List<double>> pointsA=new List<List<double>>();
            //List<List<double>> pointsB=new List<List<double>>();
            //List<List<double>>  pointsC =new List<List<double>>();

            List<List<List<double>>> listAllPouint = new List<List<List<double>>>();

            Dto_PointCloud dicPoint = new Dto_PointCloud();
            //All calls to python should be inside a using (Py.GIL()) {/* Your code here */} block.
            if (DicCameraAndPoint != null && DicCameraAndPoint.Count > 0)
            {
                foreach (Dto_CameraOperation p in DicCameraAndPoint.Keys)
                {
                    List<List<double>> lissDou = TransformationStructure3D(DicCameraAndPoint[p].point3Ds);
                    listAllPouint.Add(lissDou);
                }
            }

            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.
                Stopwatch stopwatch = new Stopwatch();
                //string strpth = Environment.CurrentDirectory + @"\pyd\";
                dynamic calib = Py.Import("Concatenate");
                stopwatch.Start();
                var ss = calib.calibration(listAllPouint[0], listAllPouint[1], listAllPouint[2]);
                stopwatch.Stop();                             //结束计时
                Console.WriteLine(stopwatch.Elapsed);
                Console.ReadKey();
            }
            return dicPoint;
        }

        /// <summary>
        /// 单机标定
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_Delta getDeltaForTwoPointClouds(Dto_PointCloud p1, Dto_PointCloud p2)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 6DoF误差计
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="rotateMatrix"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_PointCloud rotatePointCloud(Dto_PointCloud origin, Dto_RotateMatrix rotateMatrix)
        {
            throw new NotImplementedException();
        }

        public void text()
        {
            //All calls to python should be inside a using (Py.GIL()) {/* Your code here */} block.
            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.
                Stopwatch stopwatch = new Stopwatch();
                string strpth = Environment.CurrentDirectory + @"\pyd\";

                int[,] map = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
                int[][] newArray = { new int[] { 1, 3, 4, 5, 6 }, new int[] { 2, 4, 6, 8, 2 } };
                //dynamic calib = Py.Import("one_cam");
                dynamic calib = Py.Import("Concatenate");
                stopwatch.Start();
                calib.calibration(0);
                stopwatch.Stop();                             //结束计时
                Console.WriteLine(stopwatch.Elapsed);
                Console.ReadKey();





            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point3Ds"></param>
        /// <returns></returns>
        public List<List<double>> TransformationStructure3D(List<Point3D> point3Ds)
        {

            List<List<double>> lisDoub3d = new List<List<double>>();

            if (point3Ds != null && point3Ds.Count > 0)
            {
                foreach (Point3D point3d in point3Ds)
                {
                    List<double> lisDou = new List<double> { point3d.X, point3d.Y, point3d.Z };
                    lisDoub3d.Add(lisDou);
                }
            }


            return lisDoub3d;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lisDoub3d"></param>
        /// <returns></returns>
        public List<Point3D> TransformationStructure3D(List<List<double>> lisDoub3d)
        {
            List<Point3D> point3Ds = new List<Point3D>();

            return point3Ds;

        }
    }
}
