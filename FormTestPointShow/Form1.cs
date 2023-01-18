using JancsiVisionCameraController;
using JancsiVisionUtilityServers;
using JancsiVisionUtilityServers.Model;
using Kitware.VTK;
using PclSharp;
using PclSharp.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace FormTestPointShow
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 声明控件
        /// </summary>
        /// 
        private RenderWindowControl renderWindowControl1;
        private RenderWindowControl renderWindowControl2;

        private CameraControlbus control;

        private Dto_Delta keyValues;

        private JancsiUtilityServer jancsiUtilityServer;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            renderWindowControl1 = new RenderWindowControl();
            renderWindowControl1.AddTestActors = false;
            renderWindowControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            renderWindowControl1.Location = new System.Drawing.Point(0, 0);
            renderWindowControl1.Name = "renderWindowControl1";
            renderWindowControl1.Size = new System.Drawing.Size(100, 100);
            renderWindowControl1.TabIndex = 0;
            renderWindowControl1.TestText = null;

            renderWindowControl1 = new RenderWindowControl();
            renderWindowControl1.AddTestActors = false;
            renderWindowControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            renderWindowControl1.Location = new System.Drawing.Point(0, 0);
            renderWindowControl1.Name = "renderWindowControl1";
            renderWindowControl1.Size = new System.Drawing.Size(100, 100);
            renderWindowControl1.TabIndex = 0;
            renderWindowControl1.TestText = null;


            //将控件添加进Panel
            this.panelImdiat.Controls.Add(renderWindowControl1);
            this.panelPcd.Controls.Add(renderWindowControl2);


            keyValues = new Dto_Delta();
            control = new CameraControlbus();
            jancsiUtilityServer = new JancsiUtilityServer();
            control.initData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //读取点云数据
            //var cloud = new PointCloudOfXYZ();
            //using (var reader = new PCDReader())
            //    reader.Read(AppDomain.CurrentDomain.BaseDirectory + $"//pcd//rabbit.pcd", cloud);
            int i = 0;

            var PointDz = control.StartTrigger();
            keyValues = jancsiUtilityServer.fusionPointClouds(PointDz);

            var point3d = ConvertToPoint(keyValues.doublePoints);

            if (i == 5)
            {
                GC.Collect();
            }

            //显示点云
            ShowPointCloud(point3d, renderWindowControl1);
        }
        public List<Point3D> ConvertToPoint(Dictionary<string, double[,]> keyValuePairs)
        {
            MinZ = 0;
            MaxZ = 0;

            List<Point3D> lispoint3DsEquations = new List<Point3D>();
            if (keyValuePairs != null && keyValuePairs.Keys.Count > 0)
            {
                foreach (string cameraNumber in keyValuePairs.Keys)
                {

                    for (int i = 0; i < keyValuePairs[cameraNumber].GetLength(0); i++) //遍历第一维
                    {

                        Point3D pointCloud = new Point3D();
                        pointCloud.X = keyValuePairs[cameraNumber][i, 0];
                        pointCloud.Y = keyValuePairs[cameraNumber][i, 1];
                        pointCloud.Z = keyValuePairs[cameraNumber][i, 2];
                        if (i == 1 || MinZ > pointCloud.Z)
                        {
                            MinZ = pointCloud.Z;
                        }
                        if (i == 1 || MaxZ < pointCloud.Z)
                        {
                            MaxZ = pointCloud.Z;
                        }
                        lispoint3DsEquations.Add(pointCloud);
                    }
                }
            }

            return lispoint3DsEquations;
        }
        /// <summary>
        /// 异步线程的方法显示点云数据
        /// </summary>
        /// <param name="pointCloud"></param>
        public void ShowPointCloud(IList<Point3D> pointCloud, RenderWindowControl renderWindowControl)
        {
            Task.Run(() =>
            {
                try
                {
                    //PickUp();
                    ShowPointCloud(ConvertTovtkPoints(pointCloud), renderWindowControl);
                }
                catch (Exception)
                {
                }
            });

        }
        /// <summary>
        /// 异步线程的方法显示点云数据
        /// </summary>
        /// <param name="pointCloud"></param>
        public void ShowPointCloud(PointCloudOfXYZ pointCloud, RenderWindowControl renderWindowControl)
        {
            Task.Run(() =>
            {
                try
                {
                    //PickUp();
                    ShowPointCloud(ConvertTovtkPoints(pointCloud), renderWindowControl);
                }
                catch (Exception)
                {
                }
            });

        }
        private double MaxZ;

        private double MinZ;


        /// <summary>
        /// PointCloud 将转成vtk点集
        /// </summary>
        /// <returns></returns>
        private vtkPoints ConvertTovtkPoints(PointCloudOfXYZ pointCloud)
        {
            vtkPoints points = new vtkPoints();
            try
            {
                MinZ = pointCloud.Points[0].Z;
                MaxZ = pointCloud.Points[0].Z;
                for (int i = 0; i < pointCloud.Points.Count; i++)
                {
                    if (pointCloud.Points[i].Z > MaxZ) MaxZ = pointCloud.Points[i].Z;
                    if (pointCloud.Points[i].Z < MinZ) MinZ = pointCloud.Points[i].Z;
                    points.InsertPoint(i, pointCloud.Points[i].X, pointCloud.Points[i].Y, pointCloud.Points[i].Z);
                }
                return points;

            }
            catch (Exception)
            {
                return points;
            }
        }


        /// <summary>
        /// PointCloud 将转成vtk点集
        /// </summary>
        /// <returns></returns>
        private vtkPoints ConvertTovtkPoints(IList<Point3D> pointCloud)
        {
            Console.WriteLine("points count = {0}", pointCloud.Count);
            vtkPoints points = new vtkPoints();
            try
            {
                for (int i = 0; i < pointCloud.Count; i++)
                {
                    double x = pointCloud[i].X;
                    double y = pointCloud[i].Y;
                    double z = pointCloud[i].Z;
                    //Console.WriteLine("{0},{1},{2}", x, y, z);
                    points.InsertPoint(i, x, y, z);
                }
                return points;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return points;
            }


        }
        /// <summary>
        /// 显示点云
        /// </summary>
        /// <param name="points"></param>
        private void ShowPointCloud(vtkPoints points, RenderWindowControl renderWindowControl)
        {
            try
            {

                vtkPolyData polyData = vtkPolyData.New();
                polyData.SetPoints(points);
                //Color
                vtkVertexGlyphFilter glyphFilter = vtkVertexGlyphFilter.New();
                glyphFilter.SetInput(polyData);
                glyphFilter.Update();

                vtkElevationFilter ColoredGrid = vtkElevationFilter.New();  //帮点上颜色
                ColoredGrid.SetInputConnection(glyphFilter.GetOutputPort());
                ColoredGrid.SetLowPoint(0, 0, MaxZ);
                ColoredGrid.SetHighPoint(0, 0, MinZ);
                // Visualize
                vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(ColoredGrid.GetOutputPort());
                vtkActor triangulatedActor = triangulatedActor = vtkActor.New();
                triangulatedActor.SetMapper(mapper);

                //renderWindowControl1控件提供“渲染窗口” 
                vtkRenderWindow renderWindow = renderWindowControl.RenderWindow;
                //渲染器
                vtkRenderer renderer = renderWindow.GetRenderers().GetFirstRenderer();
                //设置背景色
                renderer.SetBackground(.1, .2, .3);
                //可以刷新显示
                //renderWindow.SetSize(520, 520);//设定窗口的大小

                renderWindow.SetSize(renderWindowControl.Width - 1, renderWindowControl.Height);//设定窗口的大小

                vtkActorCollection coll = renderer.GetActors();
                //int count = coll.GetNumberOfItems();
                //for (int i = 0; i <= count; i++)
                //{
                //    renderer.RemoveActor(renderer.GetActors().GetLastActor());
                //}
                coll.RemoveAllItems();//移除已有角色
                renderer.RemoveAllViewProps();

                renderer.AddActor(triangulatedActor);

                vtkInteractorStyleTrackballCamera style = vtkInteractorStyleTrackballCamera.New();  //移动摄像头

                //获取交互器
                vtkRenderWindowInteractor renderWindowInteractor = renderWindow.GetInteractor();
                renderWindowInteractor.Start();

                //设置"相机Camera"
                vtkCamera camera = renderer.GetActiveCamera();// 新建相机
                renderer.ResetCamera();

            }
            catch (Exception)
            {

                throw;
            }
        }
        string defaultPath = "";
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.pcd";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = dialog.FileName;
                //读取点云数据
                var cloud = new PointCloudOfXYZ();
                using (var reader = new PCDReader())
                    reader.Read(file, cloud);
                //显示点云
                ShowPointCloud(cloud, renderWindowControl2);
            }
        }
    }
}
