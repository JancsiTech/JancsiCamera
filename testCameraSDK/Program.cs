using JancsiVersionCameraController;
using JancsiVisionCameraController;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices;
using JancsiVisionPointCloudServers.Model;
using JancsiVisionUtilityServers;
using JancsiVisionUtilityServers.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testCameraSDK
{
    public class Program
    {
        static void Main(string[] args)
        {
            //FileCongfigServer fileCongfigServer = new FileCongfigServer();
            //fileCongfigServer.SaveAffineMatrixConfig("Device: 3D-A5060", "1A2225XN004141", null);
            JancsiUtilityServer jancsiUtilityServer = new JancsiUtilityServer();
            //jancsiUtilityServer.SaveAffineMatrixConfig(null);
            CameraControlbus control = new CameraControlbus();
            control.initData();
            int i = 0;
            Dto_Delta keyValues = new Dto_Delta();
            while (true)
            {


                Stopwatch watch = new Stopwatch();
                watch.Start();
                //去到多个相机的点云
                var PointDz = control.StartTrigger();
                watch.Stop();
                if (i == 5)
                {
                    GC.Collect();
                }
                else if (i == 0)
                {
                    // Dto_Delta ss = jancsiUtilityServer.fusionPointClouds(PointDz);
                    keyValues = jancsiUtilityServer.fusionPointClouds(PointDz);
                }
                else if (i > 0)
                {
                    //融合两次点云并比较
                    //var lisRefPointsCloudPoints = keyValues.Select(o => o.Value.point3Ds).ToList();
                    //var lisCurrentPointsCloudPoints = PointDz.Select(o => o.Value.point3Ds).ToList();
                    //Dto_PointCloud dto_RefPointsCloud = new Dto_PointCloud();
                    //Dto_PointCloud dto_PointsCloudPoints = new Dto_PointCloud();
                    //dto_RefPointsCloud.point3Ds = new List<Point3D>();
                    //dto_RefPointsCloud.point3Ds.AddRange(lisRefPointsCloudPoints[0]);
                    //dto_RefPointsCloud.point3Ds.AddRange(lisRefPointsCloudPoints[1]);
                    //dto_PointsCloudPoints.point3Ds = new List<Point3D>();
                    //dto_PointsCloudPoints.point3Ds.AddRange(lisCurrentPointsCloudPoints[0]);
                    //dto_PointsCloudPoints.point3Ds.AddRange(lisCurrentPointsCloudPoints[1]);


                    var ss = jancsiUtilityServer.fusionPointClouds(PointDz);
                    var sas = jancsiUtilityServer.rotatePointCloud(keyValues, ss);
                    keyValues = ss;
                }
                //foreach (var item in PointDz.Keys)
                //{
                //    //接入算法
                //    Dto_Delta dto_Delta= jancsiUtilityServer.CalibrationCubeFitting(PointDz[item]);


                //    var ss = jancsiUtilityServer.CalibrationCubeCalibrate(dto_Delta.listEqutions, item._ThreeMachineCalibration);
                //    break;
                //}
                i++;
                //生成图像

                //
                Console.WriteLine(string.Format("cloud pint  take time :{0}", watch.ElapsedMilliseconds.ToString()));
            }


        }
    }
}
