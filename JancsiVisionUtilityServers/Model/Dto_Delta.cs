using JancsiVisionPointCloudServers.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionUtilityServers.Model
{
    public class Dto_Delta
    {
        public Dto_PointCloud dtoCameraList { get; set; }
        public List<List<double>> listEqutions { get; set; }
    }
}
