using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using MocapView.Mocap;
using MocapView.View;

namespace MocapView {
    public partial class Page : UserControl {
        Motion motion1;
        Motion motion2;
        public Page() {
            InitializeComponent();
            
            // initialize viewport
            viewport1.Initialize(viewport1.Width, viewport1.Height);
            viewport2.Initialize(viewport2.Width, viewport2.Height);

            viewport1.Camera = new PerspectiveCamera(new Point3D(0, 0, 90), new Vector3D(0, 0, -25), new Vector3D(0, 1, 0), 45, viewport1);
            //viewport1.Camera = new PerspectiveCamera(new Point3D(100, 0, 0), new Vector3D(-10, 0, 0), new Vector3D(0, 1, 0), 45, viewport1);

            // for the winchun
            viewport2.Camera = new PerspectiveCamera(new Point3D(150, 250, 160), new Vector3D(-20, 0, -6), new Vector3D(0, 1, 0), 45, viewport2);
            //viewport2.Camera = new PerspectiveCamera(new Point3D(0, 220, 260), new Vector3D(0, 0, -20), new Vector3D(0, 1, 0), 45, viewport2);

            // load motion capture data
            //string data = GetResource("MocapView.09_03.bvh");
            //string data = GetResource("MocapView.16_05.bvh");
            LoadMotion(ref motion2, viewport2, GetResource("MocapView.B_winchun_combo2.bvh"));

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        DateTime _lastTimeRendered1, _lastTimeRendered2;
        void CompositionTarget_Rendering(object sender, EventArgs e) {
            //if (motion1 != null) {
            //    motion1.Skeleton.SetFrame(motion1.Frames[301], Quaternion.Identity);
            //    viewport1.Render();
            //    _lastTimeRendered1 = DateTime.Now;
            //}
            if ((motion1 != null) && (motion1.AdvanceTime((DateTime.Now - _lastTimeRendered1).TotalMilliseconds))) {
                viewport1.Render();
                _lastTimeRendered1 = DateTime.Now;
            }

            if (motion2.AdvanceTime((DateTime.Now - _lastTimeRendered2).TotalMilliseconds)) {
                viewport2.Render();
                _lastTimeRendered2 = DateTime.Now;
            }
        }

        private void LoadMotion(ref Motion motion, Viewport3D viewport, string data){
            motion = new Motion();
            motion = Loader.LoadFromBvh(data);
            List<Point3D> uniquePoints = new List<Point3D>(200);

            motion.Skeleton.StartPointWorld.X = motion.Skeleton.Offset.X;
            motion.Skeleton.StartPointWorld.Y = motion.Skeleton.Offset.Y;
            motion.Skeleton.StartPointWorld.Z = motion.Skeleton.Offset.Z;
            AddSkeleton(viewport, motion.Skeleton, uniquePoints); // !!! TODO: use unique points avoid duplicate computations
            viewport.Render();
            _lastTimeRendered1 = DateTime.Now;
            _lastTimeRendered2 = DateTime.Now;
        }

        private void AddSkeleton(Viewport3D viewport, Node root, List<Point3D> uniquePoints) {
            int childCount = root.Children.Count;
            uniquePoints.Add(root.StartPointWorld);
            if (childCount == 0) { // if leaf node
                return;
            }
            
            for (int i = 0; i < childCount; i++) {
                Node child = root.Children[i];
                child.StartPointWorld.X = root.StartPointWorld.X + child.Offset.X;
                child.StartPointWorld.Y = root.StartPointWorld.Y + child.Offset.Y;
                child.StartPointWorld.Z = root.StartPointWorld.Z + child.Offset.Z;
                SolidColorBrush brush;
                if (child.Children.Count == 0) { // if end node
                    brush = new SolidColorBrush(Colors.Green);
                }
                else
            /*        if (root.Name == "RightKnee") {
                        brush = new SolidColorBrush(Colors.Blue);
                    }
                    else*/
                {
                    brush = new SolidColorBrush(Colors.Red);
                }

                viewport.AddChild(new LineVisual3D(root.StartPointWorld, child.StartPointWorld, brush));
                AddSkeleton(viewport, child, uniquePoints);
            }
        }

        /// <summary>
        /// Retrieves an embedded resource and returns it as a string
        /// </summary>
        /// <param name="embeddedResourceName"></param>
        /// <returns></returns>
        public static string GetResource(string embeddedResourceName) {
            System.IO.Stream stream = typeof(Page).Assembly.GetManifestResourceStream(embeddedResourceName);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Biovision Motion Capture Files (*.bvh)|*.bvh";
            if (dialog.ShowDialog() == true) {
                using (StreamReader reader = dialog.File.OpenText()) {
                    string data = reader.ReadToEnd();
                    viewport1.ClearChildren();
                    LoadMotion(ref motion1, viewport1, data);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {

        }

        private void btnAddPerson_Click(object sender, RoutedEventArgs e) {
            viewport1.ClearChildren();
            LoadMotion(ref motion1, viewport1, GetResource("MocapView.dance01.bvh"));
        }

    }
}
