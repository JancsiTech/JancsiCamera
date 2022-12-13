using System;

namespace JancsiVisionConfigServices
{
    public interface IConfigService
    {
         string GetCameraConfig(string name);

         string GetEnvironmentConfig(string name);
    }
}
