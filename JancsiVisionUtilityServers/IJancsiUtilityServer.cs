using JancsiVisionUtilityServers.Model;
using System;
using System.Collections.Generic;
using JancsiVisionPointCloudServers.Model;
using JancsiVisionCameraServers.Model;

namespace JancsiVisionUtilityServers
{
    public interface IJancsiUtilityServer
    {
        //# point clouds fusion
        Dto_PointCloud fusionPointClouds(Dictionary<Dto_CameraOperation, Dto_PointCloud> DicCameraAndPoint);

        Dto_PointCloud rotatePointCloud(Dto_PointCloud origin, Dto_RotateMatrix rotateMatrix);

        Dto_Delta CalibrationCubeCalibrate(Dto_PointCloud dtoPointCloud);

        Dto_Delta CalibrationCubeFitting(Dto_PointCloud dtoPointCloud); 

    }
}
