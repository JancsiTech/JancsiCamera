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

namespace JancsiVisionUtilityServers
{
    public class JancsiUtilityServer : IJancsiUtilityServer
    {
        private readonly ILogProvider log;
        private readonly IConfigService config;

        public JancsiUtilityServer()
        {

        }

        public JancsiUtilityServer(ILogProvider log, IConfigService config)
        {
            this.log = log;
            this.config = config;
        }

        public Dto_PointCloud fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint)
        {
            throw new NotImplementedException();
        }

        public Dto_Delta getDeltaForTwoPointClouds(Dto_PointCloud p1, Dto_PointCloud p2)
        {
            throw new NotImplementedException();
        }

        public Dto_PointCloud rotatePointCloud(Dto_PointCloud origin, Dto_RotateMatrix rotateMatrix)
        {
            throw new NotImplementedException();
        }

        public void text() {
            //All calls to python should be inside a using (Py.GIL()) {/* Your code here */} block.
            using (Py.GIL())
            {
                //Import python modules using dynamic mod = Py.Import("mod"), then you can call functions as normal.
                //All python objects should be declared as dynamic type.
                Stopwatch stopwatch = new Stopwatch();
                string strpth = Environment.CurrentDirectory;
                int[,] map = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
                int[][] newArray = { new int[] { 1, 3, 4, 5, 6 }, new int[] { 2, 4, 6, 8, 2 } };
                dynamic calib = Py.Import("one_cam");
                stopwatch.Start();
                calib.calibration(0);
                stopwatch.Stop();                             //结束计时
                Console.WriteLine(stopwatch.Elapsed);
                Console.ReadKey();
            }

        }
    }
}
