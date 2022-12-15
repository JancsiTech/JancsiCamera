using JancsiVisionConfigServices.Model;
using System;

namespace JancsiVisionConfigServices
{
    public interface IConfigService
    {
         string GetCameraConfig(string name);

        CameraConfig GetEnvironmentConfig(string name);
    }
}
