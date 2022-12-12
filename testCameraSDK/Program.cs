using JancsiVersionCameraController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testCameraSDK
{
    public class Program
    {
        static void Main(string[] args)
        {
            CognexCameraControl control = new CognexCameraControl();
            control.init();
        }
    }
}
