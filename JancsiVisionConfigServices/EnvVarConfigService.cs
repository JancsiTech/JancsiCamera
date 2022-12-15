using JancsiVisionConfigServices.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionConfigServices
{

    public class EnvVarConfigService : IConfigService
    {
        public string GetCameraConfig(string name)
        {
            return "拿到相机默认配置啦！";
        }

        public CameraConfig GetEnvironmentConfig(string name)
        {
            throw new NotImplementedException();
        }
    }
}
