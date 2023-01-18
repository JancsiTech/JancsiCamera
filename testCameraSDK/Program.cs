using JancsiVersionCameraController;
using JancsiVisionCameraController;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices;
using JancsiVisionPointCloudServers.Model;
using JancsiVisionUtilityServers;
using JancsiVisionUtilityServers.Model;
using PclSharp;
using PclSharp.IO;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace testCameraSDK
{
    public class Program
    {
        private static object _syncRoot = new Object();
        static void Main(string[] args)
        { 
            
            var cloud = new PointCloudOfXYZ();
                using (var reader = new PCDReader())
                    reader.Read("", cloud);
            Console.WriteLine("请输入保存数据的数量");
            string trInfo = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(trInfo))
            {
                trInfo = "10";
            }
            int x = int.Parse(trInfo);
            //List<double> list = new List<double>();
            //List<double> list1 = new List<double>();
            //double[] lisDou = null;
            //double[] lisDou2 = null;
            //string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "1.txt");
            //string path2 = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "2.txt");

            //if (File.Exists(path))
            //{
            //    using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
            //    {
            //        string str;
            //        while ((str = sr.ReadLine()) != null)
            //        {

            //            string[] strs = str.Split(',');
            //            if (strs != null && strs.Count() > 0)
            //            {
            //                for (int p = 0; p < strs.Length; p++)
            //                {
            //                    list.Add(Convert.ToDouble(strs[p]));
            //                }

            //            }
            //        }
            //    }
            //}
            //lisDou = list.ToArray();

            //if (File.Exists(path2))
            //{
            //    using (System.IO.StreamReader sr = new System.IO.StreamReader(path2))
            //    {
            //        string str;
            //        while ((str = sr.ReadLine()) != null)
            //        {

            //            string[] strs = str.Split(',');
            //            if (strs != null && strs.Count() > 0)
            //            {
            //                for (int p = 0; p < strs.Length; p++)
            //                {
            //                    list1.Add(Convert.ToDouble(strs[p]));
            //                }

            //            }
            //        }
            //    }
            //}
            //lisDou2 = list1.ToArray();
            //if (File.Exists(path2))
            //{
            //    using (System.IO.StreamReader sr = new System.IO.StreamReader(path2))
            //    {
            //        string str;
            //        while ((str = sr.ReadLine()) != null)
            //        {

            //            string[] strs = str.Split(']');
            //            if (strs != null && strs.Count() > 0)
            //            {
            //                foreach (string item in strs)
            //                {
            //                    List<double> listDou = new List<double>();
            //                    string[] strEqution = item.ToString().Replace("[", "").Replace("]", "").Split(',');
            //                    foreach (string strDou in strEqution)
            //                    {
            //                        if (!string.IsNullOrWhiteSpace(strDou))
            //                        {
            //                            listDou.Add(Convert.ToDouble(strDou));

            //                        }

            //                    }
            //                    lisDou2.Add(listDou);
            //                }
            //            }
            //        }
            //    }
            //}
            JancsiUtilityServer jancsiUtilityServer = new JancsiUtilityServer();
            //Dto_Delta dto_Delta = new Dto_Delta();
            //dto_Delta.listEquations = new List<List<double>>();
            //dto_Delta.listEquations = lisDou2;
            //dto_Delta.doublePoints = new Dictionary<string, double[,]>();
            //while (true)
            //{
            //    var sas = jancsiUtilityServer.rotatePointCloudAsync(lisDou, lisDou2);
            //    // Console.WriteLine(string.Join(",", sas));
            //}

            //FileCongfigServer fileCongfigServer = new FileCongfigServer();
            //fileCongfigServer.SaveAffineMatrixConfig("Device: 3D-A5060", "1A2225XN004141", null);

            // jancsiUtilityServer.SaveAffineMatrixConfig(null);
            CameraControlbus control = new CameraControlbus();
            control.initData();
            int i = 0;
            Dto_Delta keyValues = new Dto_Delta();
            Dictionary<Dto_CameraOperation, Dto_PointCloud> PointDz = null;
            //Console.WriteLine("请输入保存数据的数量！");

            //int x = int.Parse(Console.ReadLine());

            //Console.WriteLine("请输入保存数据的数量：");

            //int x = Console.Read();


            while (true)
            {


                Stopwatch watch = new Stopwatch();
                watch.Start();



                if (i % 5 == 0 && i != 0)
                {
                    GC.Collect();
                }


                //去到多个相机的点云
                PointDz = control.StartTrigger();

                //Task<Dto_Delta> printRes = Task.Run(() =>
                //{

                //    var ss = jancsiUtilityServer.fusionPointClouds(PointDz);
                //    return ss;


                //});
                var ss = jancsiUtilityServer.fusionPointClouds(PointDz);
                //融合两次点云并比较
                watch.Stop();

                if (i <= x)
                {
                    jancsiUtilityServer.savePointNumber(PointDz, i);
                    jancsiUtilityServer.savePointCloudInMemory(ss.doublePoints, i);
                }
                if (i == x)
                {
                    jancsiUtilityServer.SaveFusionPointClouds();
                }
                // var sas = jancsiUtilityServer.rotatePointCloudAsync(lisDou, ss);

                //Dto_Delta s2s = jancsiUtilityServer.CalibrationCubeFitting(ss.dtoCameraList);
                //List<List<double>> lisDou = new List<List<double>>();
                //string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Config", "1.txt");

                //if (File.Exists(path))
                //{
                //    using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
                //    {
                //        string str;
                //        while ((str = sr.ReadLine()) != null)
                //        {
                //            Console.WriteLine(str);
                //        }
                //    }
                //}

                //Dto_Delta dto_Delta=new Dto_Delta();
                //dto_Delta.listEquations = new List<List<double>>();



                //var sas = jancsiUtilityServer.rotatePointCloud(lisDou, ss);
                //    keyValues = ss;
                //}
                i++;
                Console.WriteLine(string.Format("cloud pint  take time :{0}", watch.ElapsedMilliseconds.ToString()));
                //Console.WriteLine("1");
                //Console.ReadLine();

                //}


            }

        }

    }
}
