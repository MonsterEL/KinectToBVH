using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF3DTest.DebugBVH;
using WPF3DTest.MotionWriter;
using WPF3DTest.SkeletonData;

namespace WPF3DTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // connected kinect sensor
        private KinectSensor sensor;
        
        // under recording state or not
        private bool underRecording = true;

        // edge brush
        private Brush BRUSH_EDGE = Brushes.Red;
        private const double DOUBLE_THICKNESS_EDGE = 10;
        // bone brush
        private Brush BRUSH_BONE = Brushes.Green;
        private const double DOUBLE_THICKNESS_BONE = 5;
        private Brush BRUSH_JOINT = Brushes.White;
        private const double DOUBLE_RADIUS_JOINT = 10;

        // skeleton structure
        private StructuredSkeleton _structuredSkeleton;

        // for debug
        private Brush BTUSH_BONE_DEBUG = Brushes.Black;
        private const double DOUBLE_THICKNESS_BONE_DEBUG = 10;

        //BVH writer;
        private BVHWriter bvhWriter;

        public MainWindow()
        {
            InitializeComponent();
            _structuredSkeleton = new StructuredSkeleton();
            bvhWriter = new BVHWriter(_structuredSkeleton);
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new frame data
                this.sensor.SkeletonFrameReady += SensorSkeletonFrameReady;
                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                // todo: error log
            }

        }
        public void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            long frameTimeStamp = 0;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    frameTimeStamp = skeletonFrame.Timestamp;
                }
            }

            if (skeletons.Length != 0)
            {
                canvas.Children.Clear();
                foreach (Skeleton skel in skeletons)
                {
                    // render clipped edges
                    renderClippedEdges(skel);

                    if (SkeletonTrackingState.Tracked == skel.TrackingState)
                    {
                        // clibrate the bones if not done yet
                        _structuredSkeleton.CliberateSkeleton(skel);

                        // render the skeleton on the canvas
                        addBones(skel);

                        if (!_structuredSkeleton.NeedBoneClibrated())
                        {
                            this.grid.Background = Brushes.AntiqueWhite;
                            _structuredSkeleton.UpdateJoints(skel);

                            // record motion data to bvh file
                            if (underRecording)
                            {
                                bvhWriter.AppendOneMotionFrame();
                            }

                            // render bones for debug
                            //addTPose_Debug();

                            DebugData.UpdateSkeletonForDebug(_structuredSkeleton.hipRoot);
                            addBVHBones_Debug();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// add extra edge to viewport if the related edge is clipped by kinect
        /// </summary>
        /// <param name="skeleton"></param>
        private void renderClippedEdges(Skeleton skeleton)
        {
            double halfThickness = DOUBLE_THICKNESS_EDGE / 2;
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
                addLineToWindow(halfThickness, 0, halfThickness, canvas.Height, BRUSH_EDGE, DOUBLE_THICKNESS_EDGE);
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
                addLineToWindow(0, halfThickness, canvas.Width, halfThickness, BRUSH_EDGE, DOUBLE_THICKNESS_EDGE);
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
                addLineToWindow(canvas.Width - halfThickness, 0, canvas.Width - halfThickness, canvas.Height, BRUSH_EDGE, DOUBLE_THICKNESS_EDGE);
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
                addLineToWindow(halfThickness, canvas.Height - halfThickness, canvas.Width, canvas.Height - halfThickness, BRUSH_EDGE, DOUBLE_THICKNESS_EDGE);
        }

        /// <summary>
        /// Add one bone to screen
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="joint1"></param>
        /// <param name="joint2"></param>
        private void addBone(Skeleton skeleton, JointType joint1, JointType joint2)
        {
            if (JointTrackingState.Tracked == skeleton.Joints[joint1].TrackingState
                && JointTrackingState.Tracked == skeleton.Joints[joint2].TrackingState)
            {
                DepthImagePoint depthPoint1 = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[joint1].Position, 
                    DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint depthPoint2 = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[joint2].Position,
                    DepthImageFormat.Resolution640x480Fps30);
                addLineToWindow(depthPoint1.X, depthPoint1.Y, depthPoint2.X, depthPoint2.Y,
                    BRUSH_BONE, DOUBLE_THICKNESS_BONE);
            }
        }

        /// <summary>
        /// Add line to window
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="brush"></param>
        /// <param name="thickness"></param>
        private void addLineToWindow(double x1, double y1, double x2, double y2, Brush brush, double thickness)
        {
            // add bone
            Line line = new Line();
            line.Stroke = brush;
            line.StrokeThickness = thickness;
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            canvas.Children.Add(line);

            // add joint
            Ellipse jointPot = new Ellipse();
            jointPot.Fill = BRUSH_JOINT;
            jointPot.Width = DOUBLE_RADIUS_JOINT;
            jointPot.Height = DOUBLE_RADIUS_JOINT;
            jointPot.Margin = new Thickness(x1 - DOUBLE_RADIUS_JOINT / 2, y1 - DOUBLE_RADIUS_JOINT / 2, 0, 0);
            canvas.Children.Add(jointPot);
        }

        /// <summary>
        /// Add all bones to screen
        /// </summary>
        /// <param name="skeleton"></param>
        private void addBones(Skeleton skeleton)
        {
            // add torso
            addBone(skeleton, JointType.HipCenter, JointType.HipLeft);
            addBone(skeleton, JointType.HipCenter, JointType.HipRight);
            addBone(skeleton, JointType.HipCenter, JointType.Spine);
            addBone(skeleton, JointType.Spine, JointType.ShoulderCenter);
            addBone(skeleton, JointType.ShoulderCenter, JointType.Head);
            addBone(skeleton, JointType.ShoulderCenter, JointType.ShoulderLeft);
            addBone(skeleton, JointType.ShoulderCenter, JointType.ShoulderRight);

            // add left arm
            addBone(skeleton, JointType.ShoulderLeft, JointType.ElbowLeft);
            addBone(skeleton, JointType.ElbowLeft, JointType.WristLeft);
            addBone(skeleton, JointType.WristLeft, JointType.HandLeft);

            // add right arm
            addBone(skeleton, JointType.ShoulderRight, JointType.ElbowRight);
            addBone(skeleton, JointType.ElbowRight, JointType.WristRight);
            addBone(skeleton, JointType.WristRight, JointType.HandRight);

            // add left leg
            addBone(skeleton, JointType.HipLeft, JointType.KneeLeft);
            addBone(skeleton, JointType.KneeLeft, JointType.AnkleLeft);
            addBone(skeleton, JointType.AnkleLeft, JointType.FootLeft);

            // add right leg
            addBone(skeleton, JointType.HipRight, JointType.KneeRight);
            addBone(skeleton, JointType.KneeRight, JointType.AnkleRight);
            addBone(skeleton, JointType.AnkleRight, JointType.FootRight);
        }

        private void addTPose_Debug()
        {
            _structuredSkeleton.getJointPositions_Debug(_structuredSkeleton.hipRoot);
            // add torso
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.HipCenter], _structuredSkeleton.Positions_Debug[(int)JointType.HipLeft]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.HipCenter], _structuredSkeleton.Positions_Debug[(int)JointType.HipRight]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.HipCenter], _structuredSkeleton.Positions_Debug[(int)JointType.Spine]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.Spine], _structuredSkeleton.Positions_Debug[(int)JointType.ShoulderCenter]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ShoulderCenter], _structuredSkeleton.Positions_Debug[(int)JointType.ShoulderLeft]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ShoulderCenter], _structuredSkeleton.Positions_Debug[(int)JointType.ShoulderRight]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ShoulderCenter], _structuredSkeleton.Positions_Debug[(int)JointType.Head]);
                                             
            // add left arm                  
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ShoulderLeft], _structuredSkeleton.Positions_Debug[(int)JointType.ElbowLeft]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ElbowLeft], _structuredSkeleton.Positions_Debug[(int)JointType.WristLeft]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.WristLeft], _structuredSkeleton.Positions_Debug[(int)JointType.HandLeft]);
                                             
            // add right arm                 
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ShoulderRight], _structuredSkeleton.Positions_Debug[(int)JointType.ElbowRight]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.ElbowRight], _structuredSkeleton.Positions_Debug[(int)JointType.WristRight]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.WristRight], _structuredSkeleton.Positions_Debug[(int)JointType.HandRight]);
                                             
            // add left leg                  
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.HipLeft], _structuredSkeleton.Positions_Debug[(int)JointType.KneeLeft]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.KneeLeft], _structuredSkeleton.Positions_Debug[(int)JointType.AnkleLeft]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.AnkleLeft], _structuredSkeleton.Positions_Debug[(int)JointType.FootLeft]);
                                             
            // add right leg                 
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.HipRight], _structuredSkeleton.Positions_Debug[(int)JointType.KneeRight]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.KneeRight], _structuredSkeleton.Positions_Debug[(int)JointType.AnkleRight]);
            addTPoseBone_Debug(_structuredSkeleton.Positions_Debug[(int)JointType.AnkleRight], _structuredSkeleton.Positions_Debug[(int)JointType.FootRight]);
        }

        private void addBVHBones_Debug()
        {
            foreach (JointNode node in _structuredSkeleton.JointNodes)
            {
                if (node.Parent == null) continue;
                AddBVHBone_Debug(node.Parent, node);
            }
        }

        private void AddBVHBone_Debug(JointNode node1, JointNode node2)
        {
            addLineToWindow(node1.Offset.X, node1.Offset.Y,
                node2.Offset.X, node2.Offset.Y,
                BTUSH_BONE_DEBUG, DOUBLE_THICKNESS_BONE_DEBUG);
        }

        private void addTPoseBone_Debug(Point3D node1, Point3D node2)
        {
            // need to mirror the left-right, for example: left shoulder should be rendered as right shoulder
            addLineToWindow(node1.X, node1.Y,
                node2.X, node2.Y,
                BTUSH_BONE_DEBUG, DOUBLE_THICKNESS_BONE_DEBUG);
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }

            bvhWriter.OutputBVHToFile();
            _structuredSkeleton.sw.Close();
            DebugData.sw.Close();
        }
    }
}
