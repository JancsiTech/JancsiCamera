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
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Headers;
using System.Collections;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace JancsiVisionUtilityServers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class JancsiUtilityServer : IJancsiUtilityServer
    {
        private readonly ILogProvider log;

        private readonly IConfigService config;

        private FileCongfigServer _fileCongfigServer;

        [DllImport("libSixDof.dll", EntryPoint = "SixDofCalculate", CallingConvention = CallingConvention.Cdecl)]
        extern static void SixDofCalculate(double[] refPoints, int refPointsNumber, double[] currentPoints, int currentPointsNumber, double[] resData);


        public JancsiUtilityServer()
        {
            _fileCongfigServer = new FileCongfigServer();
            //string pathToVirtualEnv = @"/pyd";
            //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);
            //PythonEngine.PythonHome = pathToVirtualEnv;
            //PythonEngine.PythonPath = Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
            keyValuePairs = new System.Collections.Generic.Dictionary<string, double[,]>();
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
        public Dto_Delta CalibrationCubeCalibrate(string Name, string SerialNumber, List<List<double>> listEquations, int specification)
        {

            Dto_Delta dto_Delta = new Dto_Delta();

            var CameraCaCalibration = _fileCongfigServer.GetCameraCaCalibrationConfig(Name, SerialNumber);

            List<List<double>> PhysicalCoordinates = CameraCaCalibration.ThreeMachineCalibration.Select((f, i) => new List<double>
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
                    //dto_Delta.listEquations = lisAffineMatrix;

                    //保存到配置文件
                    _fileCongfigServer.SaveAffineMatrixConfig(Name, SerialNumber, lisAffineMatrix, specification);

                    _fileCongfigServer._configCalibration = null;
                }


            }

            return dto_Delta;
        }
        public List<List<double>> ConvertStructer(Dictionary<string, double[,]> doublePoints)
        {
            List<List<double>> listDouPoint = new List<List<double>>();
            foreach (string strKey in doublePoints.Keys)
            {
                for (int i = 0; i < doublePoints[strKey].GetLength(0); i++) //遍历第一维
                {
                    List<double> pointCloud = new List<double>();
                    pointCloud.Add(doublePoints[strKey][i, 0]);
                    pointCloud.Add(doublePoints[strKey][i, 1]);
                    pointCloud.Add(doublePoints[strKey][i, 2]);


                }
            }
            return listDouPoint;

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


                dto_Delta.listEquations = lissEquations;
                dto_Delta.dtoCameraList = dto_PointCloud;

            }
            return dto_Delta;
        }
        private Dictionary<string, double[,]> keyValuePairs;
        public Dto_Delta fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint)
        {

            // List<List<double>> lissEquations = new List<List<double>>();

            Dto_Delta dto_Delta = new Dto_Delta();
            dto_Delta.dtoCameraList = new Dto_PointCloud();
            dto_Delta.doublePoints = new Dictionary<string, double[,]>();

            // List<List<List<double>>> listAllPouint = new List<List<List<double>>>();

            if (DicCameraAndPoint == null)
            {
                return null;
            }
            List<Point3D> lisPoints = new List<Point3D>();

            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 4;


            if (_fileCongfigServer._configCalibration == null)
            {
                var CameraCaCalibration = _fileCongfigServer.GetCameraCaCalibrationConfig("", "");
            }
            Action<int> addContour = k =>
            {
                Matrix<double> Point = null;
                Matrix<double> RMatrix = null;
                Vector<double> TMatrix = null;
                Matrix<double> p_prime = null;
                var dicItem = DicCameraAndPoint.ElementAt(k);
                var item = dicItem.Key;

                // keyValuePairs.Add(item.SerialNumber, new double[1, 3]);
                lisPoints = DicCameraAndPoint[item].point3Ds;

                // double[,] listPoints = new double[DicCameraAndPoint[item].point3Ds.Count, 3];
                double[,] listPoints = lisPoints.To2DArray(x => x.X, x => x.Y, x => x.Z);
                //double[,] listPoints = new double[objectInfoss.GetLength(0), objectInfoss.GetLength(1)];
                //Array.Copy(objectInfoss, listPoints, objectInfoss.Length);
                var CameraCaCalibration = _fileCongfigServer.GetCameraCaCalibrationConfig(item.Name, item.SerialNumber);


                Point = DenseMatrix.OfArray(listPoints);
                RMatrix = DenseMatrix.OfArray(CameraCaCalibration.CameraAffineMatrixlXyz.listAffineMatrixXYZ);
                TMatrix = DenseVector.OfArray(CameraCaCalibration.CameraAffineMatrixlK.listAffineMatrixK);
                p_prime = RMatrix * Point.Transpose();
                p_prime = p_prime.Transpose();


                Vector<double> rowbfr = Vector<double>.Build.Dense(3);

                for (int i = 0; i < p_prime.RowCount; i++)
                {
                    p_prime.Row(i, rowbfr);
                    rowbfr.Add(TMatrix, rowbfr);
                    p_prime.SetRow(i, rowbfr);

                }

                double[,] leng = p_prime.ToArray();

                lock (keyValuePairs)
                {
                    if (!keyValuePairs.Keys.Contains(item.SerialNumber))
                    {
                        keyValuePairs.Add(item.SerialNumber, null);
                    }
                    keyValuePairs[item.SerialNumber] = leng;
                }

            };
            Parallel.For(0, DicCameraAndPoint.Keys.Count, parallelOptions, addContour);


            dto_Delta.doublePoints = keyValuePairs;
            // dto_Delta.dtoCameraList.point3Ds = TransformationStructure3D(lissEquations);            //结束计时
            return dto_Delta;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ResData
        {

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public IntPtr data6dofAndMag;

        }
        private delegate void CloseDevDelegate();
        /// <summary>
        /// 6DoF误差计
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="rotateMatrix"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public async Task<List<double>> rotatePointCloudAsync(double[] refPoints, Dto_Delta currentPointsCloud)
        {
            //var ss = GetInfoAsync(currentPointsCloud);

            double[] t1 = GetInfoAsync(currentPointsCloud);
            // Task<double[]> t2 = GetrefPointslCoudInfoAsync(refPointslCoud);

            //var results = await Task.WhenAll(t1);


            //double[] refPoints = results[1];
            int refPointsNumber = refPoints.Length / 3;


            double[] currentPoints = t1;
            int currentPointsNumber = currentPoints.Length / 3;
            ResData resData = new ResData();
            resData.data6dofAndMag = ArrayToIntptr(new double[7]);

            double[] double6Dof = new double[7];
            //IntPtr refPointsIndouble6Dof = ArrayToIntptr(double6Dof);
            ////指针转换
            // IntPtr refPointsIn = ArrayToIntptr(refPoints);
            // IntPtr currentPointsIn = ArrayToIntptr(currentPoints);

            //IntPtr refPointsIn = new IntPtr();
            //Marshal.Copy(refPoints, 0, refPointsIn, refPoints.Length);
            //Marshal.Copy(currentPoints, 0, currentPointsIn, currentPoints.Length);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            SixDofCalculate(refPoints, refPointsNumber, currentPoints, currentPointsNumber, double6Dof);
            watch.Stop();
            //Console.WriteLine(string.Format("cloud pint  take time :{0}", watch.ElapsedMilliseconds.ToString()));
            Console.WriteLine(string.Join(",", double6Dof));


            //if (resData.data6dofAndMag != null && resData.data6dofAndMag.Count() > 0)
            //{
            //    Dof6 = resData.data6dofAndMag.ToList();
            //}
            //var sss = IntPtrToStruct<double[]>(resData.data6dofAndMag);
            //Marshal.ReleaseComObject(refPointsIn);
            //Marshal.ReleaseComObject(currentPointsIn);
            //public static void Copy(IntPtr source, double[] destination, int startIndex, int length);

            // Marshal.Copy(resData.data6dofAndMag, double6Dof, 0, 7);
            return double6Dof.ToList();

            //return new List<double>() { 0.1, 0.1, 0.1, 0.1, 0.3, 0.2, 0.1, 0.1 };
        }
        public async Task<List<double>> rotatePointCloudAsync(double[] refPoints, double[] currentPointsCloud)
        {
            //var ss = GetInfoAsync(currentPointsCloud);

            //double[] t1 = GetInfoAsync(currentPointsCloud);
            // Task<double[]> t2 = GetrefPointslCoudInfoAsync(refPointslCoud);

            //var results = await Task.WhenAll(t1);


            //double[] refPoints = results[1];
            int refPointsNumber = refPoints.Length;


            double[] currentPoints = currentPointsCloud;
            int currentPointsNumber = currentPoints.Length;
            ResData resData = new ResData();
            resData.data6dofAndMag = ArrayToIntptr(new double[7]);

            double[] double6Dof = new double[7];
            //IntPtr refPointsIndouble6Dof = ArrayToIntptr(double6Dof);
            ////指针转换
            // IntPtr refPointsIn = ArrayToIntptr(refPoints);
            // IntPtr currentPointsIn = ArrayToIntptr(currentPoints);

            //IntPtr refPointsIn = new IntPtr();
            //Marshal.Copy(refPoints, 0, refPointsIn, refPoints.Length);
            //Marshal.Copy(currentPoints, 0, currentPointsIn, currentPoints.Length);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            SixDofCalculate(refPoints, refPointsNumber, currentPoints, currentPointsNumber, double6Dof);
            watch.Stop();
            //Console.WriteLine(string.Format("cloud pint  take time :{0}", watch.ElapsedMilliseconds.ToString()));
            Console.WriteLine(string.Join(",", double6Dof));


            //if (resData.data6dofAndMag != null && resData.data6dofAndMag.Count() > 0)
            //{
            //    Dof6 = resData.data6dofAndMag.ToList();
            //}
            //var sss = IntPtrToStruct<double[]>(resData.data6dofAndMag);
            //Marshal.ReleaseComObject(refPointsIn);
            //Marshal.ReleaseComObject(currentPointsIn);
            //public static void Copy(IntPtr source, double[] destination, int startIndex, int length);

            // Marshal.Copy(resData.data6dofAndMag, double6Dof, 0, 7);
            return double6Dof.ToList();

            //return new List<double>() { 0.1, 0.1, 0.1, 0.1, 0.3, 0.2, 0.1, 0.1 };
        }

        public double[] GetInfoAsync(Dto_Delta currentPointsCloud)
        {

            JancsiUtilityServer jancsiUtilityServer = new JancsiUtilityServer();
            //await Task.Delay(TimeSpan.FromSeconds(seconds));
            //await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(seconds)));
            Dto_Delta AfterFusion = TransExpV2<Dto_Delta, Dto_Delta>.Trans(currentPointsCloud);
            double[] currentPoints = jancsiUtilityServer.ConvertToPoint(AfterFusion.doublePoints);

            return currentPoints;
        }

        async static Task<double[]> GetrefPointslCoudInfoAsync(List<List<double>> refPointslCoud)
        {
            List<double> refPointsClouds = refPointslCoud.SelectMany(i => i).ToList();

            return refPointsClouds.Cast<double>().ToArray();
        }

        public static class TransExpV2<TIn, TOut>
        {

            private static readonly Func<TIn, TOut> cache = GetFunc();
            private static Func<TIn, TOut> GetFunc()
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
                List<MemberBinding> memberBindingList = new List<MemberBinding>();

                foreach (var item in typeof(TOut).GetProperties())
                {
                    if (!item.CanWrite)
                        continue;

                    MemberExpression property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);
                }

                MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
                Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });

                return lambda.Compile();
            }

            public static TOut Trans(TIn tIn)
            {
                return cache(tIn);
            }

        }

        public double[] ConvertToPoint(Dictionary<string, double[,]> keyValuePairs)
        {
            double[] doublTotal = new double[0];

            foreach (string cameraNumber in keyValuePairs.Keys)
            {

                double[] doublePoint = TwoD_1(keyValuePairs[cameraNumber]);
                doublTotal = Combine2(doublTotal, doublePoint);
            }

            return doublTotal;
        }
        /// <summary>
        /// 将数组a,b进行合并
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private static double[] Combine2(double[] a, double[] b)
        {
            double[] c = new double[a.Length + b.Length];

            Array.Copy(a, 0, c, 0, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);

            return c;
        }
        /// <summary>
        /// 二维数组转一维数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T[] TwoD_1<T>(T[,] obj)
        {
            T[] obj2 = new T[obj.Length];
            for (int i = 0; i < obj.Length; i++)
                obj2[i] = obj[i / obj.GetLength(1), i % obj.GetLength(1)];
            return obj2;
        }

        public static T IntPtrToStruct<T>(IntPtr info)
        {
            return (T)Marshal.PtrToStructure(info, typeof(T));
        }
        static IntPtr ArrayToIntptr(double[] source)

        {

            if (source == null)

            {

                return IntPtr.Zero;

            }


            unsafe

            {

                fixed (double* point = source)

                {

                    IntPtr ptr = new IntPtr(point);

                    return ptr;

                }

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
        /// <summary>
        /// 
        /// </summary>
        public void savePointNumber(Dictionary<Dto_CameraOperation, Dto_PointCloud> PointDz, int i)
        {

            StringBuilder stringBuilderPint = new StringBuilder();
            // ConcurrentBag<Dictionary<Dto_CameraOperation, List<Point3D>>> resultCollection = new ConcurrentBag<Dictionary<Dto_CameraOperation, List<Point3D>>>();

            PointDz.Keys.AsParallel().ForAll(p =>
            {
                // resultCollection.Add(p.connect());
                string pathCamera = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", p.SerialNumber + "." + i.ToString() + ".txt");
                if (System.IO.File.Exists(pathCamera))
                {
                    System.IO.File.Delete(pathCamera);
                }
                var stringPointCloud = PointDz[p].point3Ds.Select((f, k) =>
                    string.Format($"{f.X.ToString()},{f.Y.ToString()},{f.Z.ToString()}")
                    ).ToList();

                string filerWrite = string.Join("\r\n", stringPointCloud);

                System.IO.File.AppendAllText(pathCamera, filerWrite);

            });

            //foreach (Dto_CameraOperation item in PointDz.Keys)
            //{
            //    string pathCamera0 = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", item.SerialNumber + "." + i.ToString() + ".txt");

            //    string pathCamera1 = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", item.SerialNumber + "." + i.ToString() + ".txt");
            //}


        }

        public void SaveFusionPointClouds()
        {
            if (dickeyValuePairs != null && dickeyValuePairs.Keys.Count > 0)
            {

                dickeyValuePairs.Keys.AsParallel().ForAll(p =>
                {
                    List<Point3D> point3Ds = ConvertTo3dPoint(keyValuePairs);
                    string pathCamera = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "FusionPoint." + p.ToString() + ".txt");
                    if (System.IO.File.Exists(pathCamera))
                    {
                        System.IO.File.Delete(pathCamera);
                    }
                    var stringPointCloud = point3Ds.Select((f, k) =>
                        string.Format($"{f.X.ToString()},{f.Y.ToString()},{f.Z.ToString()}")
                        ).ToList();

                    string filerWrite = string.Join("\r\n", stringPointCloud);

                    System.IO.File.AppendAllText(pathCamera, filerWrite);

                });

            }



        }

        public Dictionary<int, Dictionary<string, double[,]>> dickeyValuePairs;

        public void savePointCloudInMemory(Dictionary<string, double[,]> keyValuePairs, int i)
        {
            if (dickeyValuePairs == null)
            {
                dickeyValuePairs = new Dictionary<int, Dictionary<string, double[,]>>();
            }

            if (!dickeyValuePairs.Keys.Contains(i))
            {
                dickeyValuePairs.Add(i, new Dictionary<string, double[,]>());
            }
            dickeyValuePairs[i] = FromBinary(ToBinary(keyValuePairs)) as Dictionary<string, double[,]>;

        }

        public List<Point3D> ConvertTo3dPoint(Dictionary<string, double[,]> keyValuePairs)
        {


            List<Point3D> lispoint3DsEquations = new List<Point3D>();
            if (keyValuePairs != null && keyValuePairs.Keys.Count > 0)
            {
                foreach (string cameraNumber in keyValuePairs.Keys)
                {

                    for (int i = 0; i < keyValuePairs[cameraNumber].GetLength(0); i++) //遍历第一维
                    {
                        Point3D pointCloud = new Point3D();
                        pointCloud.X = keyValuePairs[cameraNumber][i, 0];
                        pointCloud.Y = keyValuePairs[cameraNumber][i, 1];
                        pointCloud.Z = keyValuePairs[cameraNumber][i, 2];
                        lispoint3DsEquations.Add(pointCloud);


                    }
                }
            }

            return lispoint3DsEquations;
        }

        public Byte[] ToBinary(Dictionary<string, double[,]> keyValuePairs)
        {
            MemoryStream ms = null;
            Byte[] byteArray = null;
            try
            {
                BinaryFormatter serializer = new BinaryFormatter();
                ms = new MemoryStream();
                serializer.Serialize(ms, keyValuePairs);
                byteArray = ms.ToArray();
            }
            catch (Exception unexpected)
            {
                Trace.Fail(unexpected.Message);
                throw;
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }
            return byteArray;
        }

        public object FromBinary(Byte[] buffer)
        {
            MemoryStream ms = null;
            object deserializedObject = null;

            try
            {
                BinaryFormatter serializer = new BinaryFormatter();
                ms = new MemoryStream();
                ms.Write(buffer, 0, buffer.Length);
                ms.Position = 0;
                deserializedObject = serializer.Deserialize(ms);
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }
            return deserializedObject;
        }
    }
    /// 
    /// 实现泛型集合到数组对象转换的静态扩展类
    /// 
    public static class ConverterExtension
    {
        /// 
        /// 实现泛型集合到数组对象转换的静态扩展方法
        /// 
        /// 泛型对象
        /// 泛型集合
        /// 需要转换的泛型对象属性
        /// 数据对象
        public static double[,] To2DArray(this List<Point3D> lines, params Func<Point3D, double>[] lambdas)
        {
            var array = new double[lines.Count(), lambdas.Count()];
            var lineCounter = 0;
            lines.ForEach(line =>
            {
                for (var i = 0; i < lambdas.Length; i++)
                {
                    array[lineCounter, i] = lambdas[i](line);
                }
                lineCounter++;
            });
            return array;
        }

    }


}

