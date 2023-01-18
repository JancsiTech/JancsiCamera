using JancsiVisionUtilityServers.Model;
using System;
using System.Collections.Generic;
using JancsiVisionPointCloudServers.Model;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices.Model;
using System.Threading.Tasks;

namespace JancsiVisionUtilityServers
{
    public interface IJancsiUtilityServer
    {
        //# point clouds fusion
        Dto_Delta fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint);

        Task<List<double>> rotatePointCloudAsync(double[] refPoints, Dto_Delta rotateMatrix);

        Dto_Delta CalibrationCubeCalibrate(string Name, string SerialNumber, List<List<double>> listEquations, int specification);

        Dto_Delta CalibrationCubeFitting(Dto_PointCloud dtoPointCloud);

    }
}
