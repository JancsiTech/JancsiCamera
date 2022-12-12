using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionCameraServers.Model
{
    /// <summary>
    /// Use Offset values to move the region of interest for either GigE Vision camera: 
    /// </summary>
    public class Dto_CameraROI
    {
        //# unit:px
        /// <summary>
        /// 宽度 
        /// </summary>
        int _Width = 896;
        /// <summary>
        /// 高度
        /// </summary>
        int _Height = 800;
        /// <summary>
        /// 
        /// </summary>
        bool _Size2X2 = false;
        /// <summary>
        /// 偏移量X
        /// </summary>
        int _Camera1OffsetX = 40;
        /// <summary>
        /// 偏移量Y
        /// </summary>
        int _Camera1OffsetY = 0;
        /// <summary>
        /// 偏移量X
        /// </summary>
        int _Camera2OffsetX = 120;
        /// <summary>
        /// 偏移量Y
        /// </summary>
        int _Camera2OffsetY = 0;
        public int Width { get { return _Width; } set { _Width = value;  } }
        public int Height { get { return _Height; } set { _Height = value;  } }
        public bool Size2X2 { get { return _Size2X2; } set { _Size2X2 = value; } }
        public int Camera1OffsetX { get { return _Camera1OffsetX; } set { _Camera1OffsetX = value; } }
        public int Camera1OffsetY { get { return _Camera1OffsetY; } set { _Camera1OffsetY = value; } }
        public int Camera2OffsetX { get { return _Camera2OffsetX; } set { _Camera2OffsetX = value; } }
        public int Camera2OffsetY { get { return _Camera2OffsetY; } set { _Camera2OffsetY = value; } }
    }
}
