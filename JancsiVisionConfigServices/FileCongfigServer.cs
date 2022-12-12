﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace JancsiVisionConfigServices
{
    public class FileCongfigServer : IConfigService
    {
        public string FilePath { get; set; }
        /// <summary>
        /// 可以从文件中读取默认相机配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetCameraConfig(string name)
        {
            return "拿到相机默认配置啦！";
        }
        /// <summary>
        /// 读取本地配置如日志路径，用户名，密码，相机版本多种类型配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetEnvironmentConfig(string name)
        {
            ///从ini文件中获取
            var kv = File.ReadAllLines(FilePath).Select(s => s.Split('=')).Select(strs => new { Name = strs[0], value = strs[1] }).SingleOrDefault(kc => kc.Name == name);
            if (kv != null)
            {
                return kv.value;
            }
            else
            {
                return null;
            }

        }
    }
}