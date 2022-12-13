using JancsiVisionCameraServers;
using JancsiVisionCameraServers.Model;
using JancsiVisionPointCloudServers.Model;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Media.Media3D;

namespace JancsiVisionCameraController
{
    public interface ICameraControlServer
    {
        /// <summary>
        /// 检查错误
        /// </summary>
        string checkErrors();

        /// <summary>
        /// 初始化
        /// </summary>
        void init();

        /// <summary>
        /// 重启
        /// </summary>
        void reset();

        /// <summary>
        /// 相机数
        /// </summary>
        /// <returns></returns>
        int summary();

        /// <summary>
        /// 全部关闭链接
        /// </summary>
        void disconnectAll();

        /// <summary>
        /// 多相机矫正
        /// </summary>
        void Calibrate();
        /// <summary>
        /// 单相机矫正 顺序执行，不要并行，存在阴影问题
        /// </summary>
        /// <param name="cameraUid"></param>
        void CalibrateOne(string cameraUid);
        /// <summary>
        /// 获取单个相机配置
        /// </summary>
        /// <param name="cameraUid"></param>
        object getOneCameraDevice(string cameraUid);


        /// <summary>
        /// 获取相机 点云对照
        /// </summary>
        /// <returns></returns>
        Dictionary<Dto_CameraOperation, Dto_PointCloud> triggerCapture();
        

    }
}
