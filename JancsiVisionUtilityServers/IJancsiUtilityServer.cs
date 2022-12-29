using JancsiVisionUtilityServers.Model;
using System;
using System.Collections.Generic;
using JancsiVisionPointCloudServers.Model;
using JancsiVisionCameraServers.Model;
using JancsiVisionConfigServices.Model;

namespace JancsiVisionUtilityServers
{
    public interface IJancsiUtilityServer
    {
        //# point clouds fusion
        Dto_Delta fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint);

        List<double> rotatePointCloud(Dto_Delta origin, Dto_Delta rotateMatrix);

        Dto_Delta CalibrationCubeCalibrate(string Name, string SerialNumber, List<List<double>> listEquations, List<LocationXYZ> ThreeMachineCalibration);

        Dto_Delta CalibrationCubeFitting(Dto_PointCloud dtoPointCloud);

    }
}
