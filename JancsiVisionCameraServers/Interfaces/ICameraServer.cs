using System;
using System.Collections.Generic;
using System.Text;
using JancsiVisionCameraServers.Model;
using JancsiVisionPointCloudServers.Model;

namespace JancsiVisionCameraServers.Interfaces
{
    public interface ICameraServer
    {

        /// <summary>
        /// 判断FiFo是否可获取
        /// </summary>
        /// <returns></returns>
        bool isEnabled();
        /// <summary>
        /// 相机准备状态
        /// </summary>
        /// <returns></returns>
        bool isReady();

        /// <summary>
        /// 链接相机
        /// </summary>
        /// <returns></returns>
        //# try to connect
        Dto_PointCloud connect();

        /// <summary>
        /// 断开链接
        /// </summary>
        /// <returns></returns>
        bool disconnect();
        /// <summary>
        /// 相机配置
        /// </summary>
        /// <returns></returns>
        Dto_CameraConfig getCameraConfig();

        /// <summary>
        /// 设置相机配置
        /// </summary>
        /// <returns></returns>
        bool setCameraConfig();

        /// <summary>
        /// 获取ROI配置
        /// </summary>
        /// <returns></returns>
        Dto_CameraROI getCameraROI();
        /// <summary>
        /// 设置ROI值
        /// </summary>
        /// <param name="ROI"></param>
        /// <returns></returns>
        bool setCameraROI(Dto_CameraROI ROI);
        /// <summary>
        /// 矫正
        /// </summary>
        /// <returns></returns>
        bool Calibrate();
    }
}
