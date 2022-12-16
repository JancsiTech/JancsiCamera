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
using System.Drawing;
using System.Linq;

namespace JancsiVisionUtilityServers
{
    public class JancsiUtilityServer : IJancsiUtilityServer
    {
        private readonly ILogProvider log;
        private readonly IConfigService config;
        List<Dto_RotateMatrix> _dtoMatrix;
        public JancsiUtilityServer()
        {
            InitData();
            //string pathToVirtualEnv = @"/pyd";

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
        public static class FormatStrings
        {
            public const string DoubleFixedPoint = "0.###################################################################################################################################################################################################################################################################################################################################################";
        }
        /// <summary>
        /// 标定后点云传回
        /// </summary>
        /// <param name="dtoPointCloud"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_Delta CalibrationCubeCalibrate(List<List<double>> listEquations)
        {
            Dto_Delta dto_Delta = new Dto_Delta();
            foreach (Dto_RotateMatrix dtoTate in _dtoMatrix)
            {
                List<List<double>> listzzz = new List<List<double>>();
                if (dtoTate.dto_PointCloud.point3Ds!=null&& dtoTate.dto_PointCloud.point3Ds.Count>0)
                {
                    List<List<double>> ss = dtoTate.dto_PointCloud.point3Ds.Select((f, i) => new List<double>
                    {
                        f.X,
                        f.Y, 
                        f.Z    
                    }).ToList();
                    if (listEquations != null && listEquations.Count > 0)
                    {
                        using (Py.GIL())
                        {
                            //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                            //All python objects should be declared as dynamic type.
                            Stopwatch stopwatch = new Stopwatch();
                            dynamic calib = Py.Import("calibration");
                            stopwatch.Start();
                            dynamic calibrationArray = calib.Calibrate(listEquations, ss);
                            // var gg = calibrationArray.ToPython();
                            //IEnumerable rest = calibrationArray.Rest;


                            //string strPonit = calibrationArray.GetItem(0).ToString();


                            //string[] strPont = strPonit.Substring(1, strPonit.Length - 2).Split(new string[] { "\n" }, StringSplitOptions.None);
                            //foreach (string strPintEquations in strPont)
                            //{
                            //    List<double> listDou = new List<double>();
                            //    string[] strEqution = strPintEquations.Substring(1, strPintEquations.Length - 2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            //    foreach (string strDou in strEqution)
                            //    {
                            //        listDou.Add(Convert.ToDouble(strDou));
                            //    }
                            //    lissDou.Add(listDou);
                            //}

                            //string strBol = calibrationArray.GetItem(1).ToString();


                            //string[] strPontBol = strBol.Substring(1, strBol.Length - 2).Split(new string[] { "\n" }, StringSplitOptions.None);

                            stopwatch.Stop();                             //结束计时
                            Console.WriteLine(stopwatch.Elapsed);
                            Console.ReadKey();
                        }

                    }

                }
            }
           
            return dto_Delta;
        }
        /// <summary>
        /// 单机标定 点云
        /// </summary>
        /// <param name="dtoPointCloud"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_Delta CalibrationCubeFitting(Dto_PointCloud dtoPointCloud)
        {
            Dto_Delta dto_Delta = new Dto_Delta();
            List<List<double>> lissDou = new List<List<double>>();
            List<List<double>> lissPoint = TransformationStructure3D(dtoPointCloud.point3Ds);
            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.
                Stopwatch stopwatch = new Stopwatch();
                dynamic calib = Py.Import("calibration");
                stopwatch.Start();
                dynamic calibrationArray = calib.CubeFitting(lissPoint);
                // var gg = calibrationArray.ToPython();
                //IEnumerable rest = calibrationArray.Rest;


                string strPonit = calibrationArray.GetItem(0).ToString();


                string[] strPont = strPonit.Substring(1, strPonit.Length - 2).Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string strPintEquations in strPont)
                {
                    List<double> listDou = new List<double>();
                    string[] strEqution = strPintEquations.Substring(1, strPintEquations.Length - 2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string strDou in strEqution)
                    {
                        listDou.Add(Convert.ToDouble(strDou));
                    }
                    lissDou.Add(listDou);
                }
                CalibrationCubeCalibrate(lissDou);
                //string strBol = calibrationArray.GetItem(1).ToString();


                //string[] strPontBol = strBol.Substring(1, strBol.Length - 2).Split(new string[] { "\n" }, StringSplitOptions.None);

                stopwatch.Stop();                             //结束计时
                Console.WriteLine(stopwatch.Elapsed);
                Console.ReadKey();
            }
            return dto_Delta;
        }

        public void test()
        {

            // string sss = "     [ 2.82130383e-01,  7.06257745e-01,  6.49309206e-01,\r\n         1.94080211e+02]";
            string s = "2.82130383e-01";
            double safa = Convert.ToDouble(s);


            //strPont[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
        }
        /// <summary>
        /// 点云融合
        /// </summary>
        /// <param name="DicCameraAndPoint"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dto_PointCloud fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint)
        {
            List<List<double>> pointsA = new List<List<double>>();
            pointsA.Add(new List<double>() { 48.0769219398499, 249.198717951775, -242.709350585938 });
            pointsA.Add(new List<double>() { 92.147434592247, 249.198717951775, -251.451873779297 });


            List<List<double>> pointsB = new List<List<double>>();
            pointsB.Add(new List<double>() { 76.121793627739, 249.198717951775, -248.274230957031 });
            pointsB.Add(new List<double>() { 101.762819170952, 249.198717951775, -253.935241699219 });

            List<List<double>> pointsC = new List<List<double>>();
            pointsC.Add(new List<double>() { 84.134614109993, 249.198717951775, -250.586700439453 });
            pointsC.Add(new List<double>() { 88.14102435112, 249.198717951775, -250.538635253906 });

            List<List<List<double>>> listAllPouint = new List<List<List<double>>>();
            listAllPouint.Add(pointsA);
            listAllPouint.Add(pointsB);
            listAllPouint.Add(pointsC);

            Dto_PointCloud dicPoint = new Dto_PointCloud();
            ////All calls to python should be inside a using (Py.GIL()) {/* Your code here */} block.
            //if (DicCameraAndPoint != null && DicCameraAndPoint.Count > 0)
            //{
            //    foreach (Dto_CameraOperation p in DicCameraAndPoint.Keys)
            //    {
            //        List<List<double>> lissDou = TransformationStructure3D(DicCameraAndPoint[p].point3Ds);
            //        listAllPouint.Add(lissDou);
            //    }
            //}






            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.
                Stopwatch stopwatch = new Stopwatch();
                //string strpth = Environment.CurrentDirectory + @"\pyd\";
                dynamic calib = Py.Import("concatenate");
                stopwatch.Start();
                var AfterFusionPoint = calib.Concatenate(listAllPouint);
                stopwatch.Stop();                             //结束计时
                Console.WriteLine(stopwatch.Elapsed);
                Console.ReadKey();
            }
            return dicPoint;
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
        public void InitData()
        {

            _dtoMatrix = new List<Dto_RotateMatrix>();
            Dto_RotateMatrix dto_RotateMatrix0 = new Dto_RotateMatrix();
            dto_RotateMatrix0.dto_PointCloud.point3Ds = new List<Point3D>();
            #region camera 0 
            Point3D carmera0Point3D1 = new Point3D();
            carmera0Point3D1.X = 0;
            carmera0Point3D1.Y = -106.06601717798123;
            carmera0Point3D1.Z = 150;
            Point3D carmera0Point3D2 = new Point3D();
            carmera0Point3D2.X = -106.06601717798123;
            carmera0Point3D2.Y = 0;
            carmera0Point3D2.Z = 150;
            Point3D carmera0Point3D3 = new Point3D();
            carmera0Point3D3.X = 106.06601717798123;
            carmera0Point3D3.Y = 0;
            carmera0Point3D3.Z = 150;
            Point3D carmera0Point3D4 = new Point3D();
            carmera0Point3D4.X = 0;
            carmera0Point3D4.Y = -106.06601717798123;
            carmera0Point3D4.Z = 0;

            dto_RotateMatrix0.dto_PointCloud.point3Ds.Add(carmera0Point3D1);
            dto_RotateMatrix0.dto_PointCloud.point3Ds.Add(carmera0Point3D2);
            dto_RotateMatrix0.dto_PointCloud.point3Ds.Add(carmera0Point3D3);
            dto_RotateMatrix0.dto_PointCloud.point3Ds.Add(carmera0Point3D4);
            #endregion

            #region 90
            Dto_RotateMatrix dto_RotateMatrix1 = new Dto_RotateMatrix();
            dto_RotateMatrix1.dto_PointCloud.point3Ds = new List<Point3D>();
            Point3D carmera1Point3D1 = new Point3D();
            carmera1Point3D1.X = -106.06601717798123;
            carmera1Point3D1.Y = 0;
            carmera1Point3D1.Z = 150;
            Point3D carmera1Point3D2 = new Point3D();
            carmera1Point3D2.X = 0;
            carmera1Point3D2.Y = 106.06601717798123;
            carmera1Point3D2.Z = 150;
            Point3D carmera1Point3D3 = new Point3D();
            carmera1Point3D3.X = 0;
            carmera1Point3D3.Y = -106.06601717798123;
            carmera1Point3D3.Z = 150;
            Point3D carmera1Point3D4 = new Point3D();
            carmera1Point3D4.X = -106.06601717798123;
            carmera1Point3D4.Y = 0;
            carmera1Point3D4.Z = 0;
            dto_RotateMatrix1.dto_PointCloud.point3Ds.Add(carmera1Point3D1);
            dto_RotateMatrix1.dto_PointCloud.point3Ds.Add(carmera1Point3D2);
            dto_RotateMatrix1.dto_PointCloud.point3Ds.Add(carmera1Point3D3);
            dto_RotateMatrix1.dto_PointCloud.point3Ds.Add(carmera1Point3D4);
            #endregion

            #region 180
            Dto_RotateMatrix dto_RotateMatrix2 = new Dto_RotateMatrix();
            dto_RotateMatrix2.dto_PointCloud.point3Ds = new List<Point3D>();
            Point3D carmera3Point3D1 = new Point3D();
            carmera3Point3D1.X = 106.06601717798123;
            carmera3Point3D1.Y = 0;
            carmera3Point3D1.Z = 150;
            Point3D carmera3Point3D2 = new Point3D();
            carmera3Point3D2.X = 0;
            carmera3Point3D2.Y = -106.06601717798123;
            carmera3Point3D2.Z = 150;
            Point3D carmera3Point3D3 = new Point3D();
            carmera3Point3D3.X = 0;
            carmera3Point3D3.Y = 106.06601717798123;
            carmera3Point3D3.Z = 150;
            Point3D carmera3Point3D4 = new Point3D();
            carmera3Point3D4.X = 106.06601717798123;
            carmera3Point3D4.Y = 0;
            carmera3Point3D4.Z = 0;
            dto_RotateMatrix2.dto_PointCloud.point3Ds.Add(carmera3Point3D1);
            dto_RotateMatrix2.dto_PointCloud.point3Ds.Add(carmera3Point3D2);
            dto_RotateMatrix2.dto_PointCloud.point3Ds.Add(carmera3Point3D3);
            dto_RotateMatrix2.dto_PointCloud.point3Ds.Add(carmera3Point3D4);
            #endregion

            _dtoMatrix.Add(dto_RotateMatrix0);
            _dtoMatrix.Add(dto_RotateMatrix1);
            _dtoMatrix.Add(dto_RotateMatrix2);
        }
    }
}
