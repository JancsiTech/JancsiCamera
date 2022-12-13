using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace JancsiVisionPointCloudServers.Model
{
    public class Dto_PointCloud
    {
        public List<Point3D> point3Ds;

        public Dto_PointCloud()
        {
            point3Ds = new List<Point3D>();

        }
    }
}
