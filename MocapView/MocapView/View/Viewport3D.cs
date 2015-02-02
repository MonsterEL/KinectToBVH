using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace MocapView.View {
    public sealed class Viewport3D : UserControl {
        public Camera Camera;
        private List<Visual3D> _children = new List<Visual3D>();
        private Canvas _renderTarget;
        private UIElementCollection currentOrderedTriangles;

        public void ClearChildren() {
            _children.Clear();
            _renderTarget.Children.Clear();
        }

        public void AddChild(Visual3D child) {
            _children.Add(child);
            _renderTarget.Children.Add(child.VisualObject);
        }

        public Viewport3D() {
            this._renderTarget = new Canvas();
            this.currentOrderedTriangles = this._renderTarget.Children;
            this.Content = _renderTarget;
            this.SizeChanged += new SizeChangedEventHandler(Viewport3D_SizeChanged);
        }

        public void Initialize(double width, double height) {
            IsViewportDirty = true;
            VPWidth = width;
            VPHeight = height;
            if (this.Camera != null) {
                this.Camera.Invalidate();
            }
        }

        void Viewport3D_SizeChanged(object sender, SizeChangedEventArgs e) {
            this.IsViewportDirty = true;
            this.VPWidth = this.ActualWidth;
            this.VPHeight = this.ActualHeight;

            //Indicate to the camera it may have to refresh itself, since the
            //viewport size has changed
            if (this.Camera != null) {
                this.Camera.Invalidate();
            }
        }

        public void Render() {
            
            Matrix3D worldToNDC = this.Camera.Value;
            Matrix3D ndcToScreen = this.NDCToScreenTransform;
            Matrix3D worldToScreen = worldToNDC * ndcToScreen;

            for (int i = _children.Count - 1; i >= 0; i--) {
                _children[i].Project(worldToScreen);
                _children[i].UpdateOnScreen();
            }

            this.IsViewportDirty = false;
            this.IsContentDirty = false;
        }

        /// <summary>
        /// Indicates that the content the viewport is displaying is now
        /// dirty and needs to be redrawn
        /// </summary>
        internal bool IsContentDirty = true;

        /// <summary>
        /// Indicates that the viewport is dirty, i.e. the camera with which it is 
        /// associated is dirty, so we ned to redraw all of the models, even it they
        /// are not dirty internally.
        /// </summary>
        internal bool IsViewportDirty = true;

        /// <summary>
        /// Returns the matrix which converts NDC coords to viewport screen
        /// coords.  Point 0,0 is the top left of the viewport and vw,vh is
        /// the bottom right of the viewport.  The NDC coord has 0,0 at the
        /// center of the viewport, -1,1 at the top left of the viewport and
        /// 1,-1 at the bottom right of the viewport.  
        /// </summary>
        internal Matrix3D NDCToScreenTransform {
            get {
                //TODO: Cache

                //Create the NDC to screen matrix, know NDC is kept at 2 units high
                // y' = - (hs / 2) * Yndc + hs/2 + sx
                // x' = (ws / 2) * Xndc + ws/2 + sy
                // z' = (ds / 2) * Zndc + ds/2   //ds is [0,1] where our ndc z was from -1 to 1
                double screenDepth = 1;
                Matrix3D ndcToScreen = new Matrix3D(this.VPWidth / 2, 0, 0, 0,
                                                    0, -this.VPHeight / 2, 0, 0,
                                                    0, 0, screenDepth / 2, 0,
                                                    this.VPWidth / 2, this.VPHeight / 2, screenDepth / 2, 1);

                return ndcToScreen;
            }
        }

        /// <summary>
        /// Transform to turn screen points to points in the view plane, with a z value of -d
        /// where d is the distance from the camera to the view plane.
        /// //TODO: Need to rethink this - people not always using the perspective projection matrix
        /// </summary>
        public Matrix3D ScreenToViewTransform {
            get {
                double fieldOfViewInDegrees = 0;
                PerspectiveCamera camera = this.Camera as PerspectiveCamera;
                if (camera == null) {
                    throw new NotSupportedException("Unsupported camera type");
                }

                fieldOfViewInDegrees = camera.FieldOfView;

                double width = this.VPWidth;
                double height = this.VPHeight;
                double aspectRatio = this.AspectRatio;
                double depth = 1.0 / System.Math.Tan(System.Math.PI * fieldOfViewInDegrees / 360.0);

                return new Matrix3D(2 / width, 0, 0, 0,
                                    0, -2 / (height * aspectRatio), 0, 0,
                                    -1, 1 / aspectRatio, -depth, 0,
                                    0, 0, 0, 1);
            }
        }

        /// <summary>
        /// Returns the ratio of the width to the height of the viewport on the screen
        /// </summary>
        internal double AspectRatio {
            get {
                return this.VPWidth / this.VPHeight;
            }
        }

        public double VPWidth {
            get;
            set;
        }

        public double VPHeight {
            get;
            set;
        }
    }

}
