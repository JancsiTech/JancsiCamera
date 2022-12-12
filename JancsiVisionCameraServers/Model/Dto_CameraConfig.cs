using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace JancsiVisionCameraServers.Model
{
    public class Dto_CameraConfig
    {

        private Dto_CameraOperation _CurrentCamera;

        private string _CurrentName;

        private ObservableCollection<Dto_CameraOperation> _Cameras;

        public ObservableCollection<Dto_CameraOperation> Cameras
        {
            get
            {
                return _Cameras;
            }
            set
            {
                _Cameras = value;
               
            }
        }

        public string CurrentCameraName => _CurrentName;

        [JsonIgnore]
        public Dto_CameraOperation CurrentCamera
        {
            get
            {
                return _CurrentCamera;
            }
            set
            {
                _CurrentCamera = value;
              
                _CurrentName = _CurrentCamera.Name;
            }
        }

        public Dto_CameraConfig()
        {
            Cameras = new ObservableCollection<Dto_CameraOperation>();
        }
    }
}
