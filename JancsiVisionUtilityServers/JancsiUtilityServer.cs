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
using JancsiVisionConfigServices.Model;
using Newtonsoft.Json.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Numerics;

namespace JancsiVisionUtilityServers
{
    public class JancsiUtilityServer : IJancsiUtilityServer
    {
        private readonly ILogProvider log;
        private readonly IConfigService config;
        private FileCongfigServer _fileCongfigServer;
        public JancsiUtilityServer()
        {
            _fileCongfigServer = new FileCongfigServer();
            //string pathToVirtualEnv = @"/pyd";
            //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);
            //PythonEngine.PythonHome = pathToVirtualEnv;
            //PythonEngine.PythonPath = Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
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
        public Dto_Delta CalibrationCubeCalibrate(Dto_CameraOperation operation, List<List<double>> listEquations, List<LocationXYZ> ThreeMachineCalibration)
        {

            Dto_Delta dto_Delta = new Dto_Delta();
            if (ThreeMachineCalibration != null && ThreeMachineCalibration.Count > 0)
            {
                List<LocationXYZ> locationXYZ = ThreeMachineCalibration;

                List<List<double>> PhysicalCoordinates = locationXYZ.Select((f, i) => new List<double>
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
                        dynamic calib = Py.Import("calibration");

                        dynamic calibrationArray = calib.Calibrate(listEquations, PhysicalCoordinates);

                        //IEnumerable rest = calibrationArray.Rest;
                        List<List<double>> lisAffineMatrix = new List<List<double>>();

                        foreach (var item in calibrationArray)
                        {
                            List<double> listDou = new List<double>();
                            string[] strEqution = item.ToString().Replace("[", "").Replace("]", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string strDou in strEqution)
                            {
                                listDou.Add(Convert.ToDouble(strDou));
                            }
                            lisAffineMatrix.Add(listDou);
                        }
                        dto_Delta.listEqutions = lisAffineMatrix;

                        //保存到配置文件
                        _fileCongfigServer.SaveAffineMatrixConfig(operation.Name, operation.SerialNumber, lisAffineMatrix);
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
            List<List<double>> lissEquations = new List<List<double>>();
            List<List<double>> lissPoint = TransformationStructure3D(dtoPointCloud.point3Ds);
            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.

                dynamic calib = Py.Import("calibration");

                dynamic calibrationArray = calib.CubeFitting(lissPoint);

                //IEnumerable rest = calibrationArray.Rest;

                string strPonit = calibrationArray.GetItem(0).ToString();
                //立方体三交面拟合后，每个点是否属于拟合的面，维度：N,数值类型：bool
                string strFittingSurfaceList = calibrationArray.GetItem(1).ToString();

                string[] strPont = strPonit.Substring(1, strPonit.Length - 2).Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string strPintEquations in strPont)
                {
                    List<double> listDou = new List<double>();
                    string[] strEqution = strPintEquations.Replace("[", "").Replace("]", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string strDou in strEqution)
                    {
                        listDou.Add(Convert.ToDouble(strDou));
                    }
                    lissEquations.Add(listDou);
                }

                List<List<double>> listDoubRealCoords = new List<List<double>>();
                string[] strEqutionTure = strFittingSurfaceList.Replace("[", "").Replace("]", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < strEqutionTure.Count(); i++)
                {
                    if (Convert.ToBoolean(strEqutionTure[i].Replace(',', ' ')))
                    {
                        listDoubRealCoords.Add(lissPoint[i]);
                    }
                }
                List<Point3D> lisPoin3d = TransformationStructure3D(listDoubRealCoords);
                Dto_PointCloud dto_PointCloud = new Dto_PointCloud();
                dto_PointCloud.point3Ds = lisPoin3d;


                dto_Delta.listEqutions = lissEquations;
                dto_Delta.dtoCameraList = dto_PointCloud;

            }
            return dto_Delta;
        }
        #region 点云算法
        ///// <summary>
        ///// 点云融合
        ///// </summary>
        ///// <param name="DicCameraAndPoint"></param>
        ///// <returns></returns>
        ///// <exception cref="NotImplementedException"></exception>
        //public Dto_Delta fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint)
        //{
        //    //    List<List<double>> pointsA = new List<List<double>>();
        //    //    pointsA.Add(new List<double>() { 1.76435536, -0.62500892, 0.10972233, -14.86004 });
        //    //    pointsA.Add(new List<double>() { 0.63284402, 1.70918776, -0.44023879, -50.66088782 });
        //    //    pointsA.Add(new List<double>() { 0.0467290115, 0.451293596, 1.81927867, 593.409915 });


        //    //    List<List<double>> pointsB = new List<List<double>>();
        //    //    pointsB.Add(new List<double>() { -1.05771195, 0.94722754, -1.2245939, -2.52334388 });
        //    //    pointsB.Add(new List<double>() { -1.54750176, -0.60286345, 0.87029831, 100.97457192 });
        //    //    pointsB.Add(new List<double>() { 0.0459242287, 1.50164588, 1.12186268, -72.1171435 });

        //    //List<List<double>> pointsC = new List<List<double>>();
        //    //pointsC.Add(new List<double>() { 84.134614109993, 249.198717951775, -250.586700439453, });
        //    //pointsC.Add(new List<double>() { 88.14102435112, 249.198717951775, -250.538635253906, });

        //    List<List<List<double>>> listAffineMatrix = new List<List<List<double>>>();
        //    List<List<List<double>>> listAllPouint = new List<List<List<double>>>();

        //    //listAllPouint.Add(pointsC);

        //    Dto_PointCloud dicPoint = new Dto_PointCloud();
        //    foreach (Dto_CameraOperation item in DicCameraAndPoint.Keys)
        //    {
        //        //if (item.SerialNumber.Contains("93906"))
        //        //{

        //        //    listAffineMatrix.Add(pointsB);
        //        //}
        //        //else
        //        //{
        //        //    listAffineMatrix.Add(pointsA);
        //        //}
        //        listAffineMatrix.Add(new List<List<double>>() { item._CameraAffineMatrixl.X, item._CameraAffineMatrixl.Y, item._CameraAffineMatrixl.Z, item._CameraAffineMatrixl.K });
        //        listAllPouint.Add(TransformationStructure3D(DicCameraAndPoint[item].point3Ds));
        //    }
        //    List<List<double>> lissEquations = new List<List<double>>();
        //    Dto_Delta dto_Delta = new Dto_Delta();
        //    using (Py.GIL())
        //    {
        //        //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
        //        //All python objects should be declared as dynamic type.

        //        dynamic calib = Py.Import("concatenate");

        //        var AfterFusionPoint = calib.Concatenate(listAllPouint, listAffineMatrix);

        //        foreach (var strPintEquations in AfterFusionPoint)
        //        {
        //            List<double> listDou = new List<double>();
        //            string[] strEqution = strPintEquations.ToString().Replace("[", "").Replace("]", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //            foreach (string strDou in strEqution)
        //            {
        //                listDou.Add(Convert.ToDouble(strDou));
        //            }
        //            lissEquations.Add(listDou);
        //        }
        //        dto_Delta.listEqutions = lissEquations;
        //        dto_Delta.dtoCameraList.point3Ds = TransformationStructure3D(lissEquations);            //结束计时

        //    }
        //    return dto_Delta;
        //}
        #endregion
        public Dto_Delta fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint)
        {

            List<List<double>> lissEquations = new List<List<double>>();
            Dto_Delta dto_Delta = new Dto_Delta();
            dto_Delta.dtoCameraList = new Dto_PointCloud();
            double[,] listAffineMatrixXYZ = new double[3, 3];
            double[] listAffineMatrixK = new double[3];


            // List<List<List<double>>> listAllPouint = new List<List<List<double>>>();

            Matrix<double> Point = null;
            Matrix<double> RMatrix = null;
            Vector<double> TMatrix = null;
            Matrix<double> p_prime = null;
            foreach (Dto_CameraOperation item in DicCameraAndPoint.Keys)
            {

                if (item._CameraAffineMatrixl.Matrix != null && item._CameraAffineMatrixl.Matrix.Count > 0)
                {
                    listAffineMatrixK[0] = item._CameraAffineMatrixl.Matrix[0][3];
                    listAffineMatrixK[1] = item._CameraAffineMatrixl.Matrix[1][3];
                    listAffineMatrixK[2] = item._CameraAffineMatrixl.Matrix[2][3];

                    for (int i = 0; i < listAffineMatrixXYZ.GetLength(0); i++) //遍历第一维
                    {
                        for (int j = 0; j < listAffineMatrixXYZ.GetLength(1); j++) //遍历第二维
                        {
                            listAffineMatrixXYZ[i, j] = item._CameraAffineMatrixl.Matrix[i][j];//为数组赋值
                        }

                    }

                }
                double[,] listPoints = new double[DicCameraAndPoint[item].point3Ds.Count, 3];

                for (int i = 0; i < listPoints.GetLength(0); i++) //遍历第一维
                {
                    for (int j = 0; j < listPoints.GetLength(1); j++) //遍历第二维
                    {

                        switch (j)
                        {
                            case 0:
                                listPoints[i, j] = DicCameraAndPoint[item].point3Ds[i].X;
                                break;
                            case 1:
                                listPoints[i, j] = DicCameraAndPoint[item].point3Ds[i].Y;
                                break;
                            case 2:
                                listPoints[i, j] = DicCameraAndPoint[item].point3Ds[i].Z;
                                break;
                            default:
                                break;
                        }//为数组赋值
                    }

                }
                Point = DenseMatrix.OfArray(listPoints);
                RMatrix = DenseMatrix.OfArray(listAffineMatrixXYZ);
                TMatrix = DenseVector.OfArray(listAffineMatrixK);
                p_prime = RMatrix * Point.Transpose();
                p_prime = p_prime.Transpose();



                Vector<double> rowbfr = Vector<double>.Build.Dense(3);

                for (int i = 0; i < p_prime.RowCount; i++)
                {
                    p_prime.Row(i, rowbfr);
                    rowbfr.Add(TMatrix, rowbfr);
                    p_prime.SetRow(i, rowbfr);

                }
                //收集点云
                //int mark = 1;
                //List<double> doublesPoint = new List<double>();
                //foreach (var ArrayPont in p_prime.ToArray())
                //{

                //    doublesPoint.Add(ArrayPont);
                //    if (mark % 3 == 0)
                //    {
                //        lissEquations.Add(doublesPoint);
                //        doublesPoint = new List<double>();
                //    }
                //    mark++;
                //}


                var leng = p_prime.ToArray();
                //  List<List<double>> result = Enumerable.Range(0, p_prime.ToArray().Length / 3).Select(x => p_prime.Enumerate().Skip(x * 3).Take(3).ToList()).ToList();
                // lissEquations = result;
                try
                {
                    for (int i = 0; i < leng.GetLength(0); i++) //遍历第一维
                    {
                        List<double> doublesPoint = new List<double>();
                        for (int j = 0; j < leng.GetLength(1); j++) //遍历第二维
                        {
                            doublesPoint.Add(leng[i, j]);


                        }
                        lissEquations.Add(doublesPoint);

                    }

                }
                catch (Exception ex)
                {

                    throw;
                }

            }
            dto_Delta.listEqutions = lissEquations;
            dto_Delta.dtoCameraList.point3Ds = TransformationStructure3D(lissEquations);            //结束计时
            return dto_Delta;
        }

        /// <summary>
        /// 6DoF误差计
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="rotateMatrix"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<double> rotatePointCloud(Dto_Delta refPointsCloud, Dto_Delta currentPointsCloud)
        {
            List<double> Dof6 = new List<double>();
            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.

                dynamic calib = Py.Import("six_dof");

                dynamic AfterErrorValue = calib.SixDofCalculate(refPointsCloud.listEqutions, currentPointsCloud.listEqutions);

                string strAfterErrorValue = AfterErrorValue.GetItem(0).ToString();
                foreach (var strPintEquations in AfterErrorValue)
                {

                    Dof6.Add(Convert.ToDouble(strPintEquations.ToString()));


                }
            }
            return Dof6;
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

            if (lisDoub3d != null && lisDoub3d.Count > 0)
            {
                foreach (List<double> point3d in lisDoub3d)
                {
                    Point3D pointCloud = new Point3D();
                    pointCloud.X = point3d[0];
                    pointCloud.Y = point3d[1];
                    pointCloud.Z = point3d[2];
                    point3Ds.Add(pointCloud);
                }
            }

            return point3Ds;

        }
    }
}
