using JancsiVisionConfigServices.Model;
using System;
using System.Collections.Generic;

namespace JancsiVisionConfigServices
{
    public interface IConfigService
    {
        string GetCameraConfig(string name);

        CameraConfig GetEnvironmentConfig(string name);

        void SaveAffineMatrixConfig(string cameraname, string SerialNumber, List<List<double>> lisMatrixl, int specification);

        void SaveConfig(string Name, string SerialNumber, int uuid);
    }
}
