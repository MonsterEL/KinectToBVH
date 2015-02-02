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

namespace MocapView.View {
    public sealed class PerspectiveCamera : ProjectionCamera {
        private double fieldOfView;

        public PerspectiveCamera() {
            this.FieldOfView = 45;
        }
        /// <summary>
        /// The viewport associated with the camera
        /// </summary>
        internal Viewport3D Viewport;

        public PerspectiveCamera(Point3D position,
                                 Vector3D lookDirection,
                                 Vector3D upDirection,
                                 double fieldOfView, Viewport3D viewport) {
            this.Position = position;
            this.LookDirection = lookDirection;
            this.UpDirection = upDirection;
            this.FieldOfView = fieldOfView;
            this.Viewport = viewport;
        }

        /// <summary>
        /// Field of view - in degrees
        /// </summary>
        public double FieldOfView {
            get { return this.fieldOfView; }
            set {
                if (this.fieldOfView != value) {
                    //TODO: Lazy calculate the world transform
                    this.fieldOfView = value;
                }
            }
        }

        internal override void Invalidate() {
            //TODO: Cache internal matrix value and update cached version
            //      when this is called.
        }

        /// <summary>
        /// Calculates the matrix that transforms world coords to view coords
        /// </summary>
        /// <param name="lookDirection"></param>
        /// <param name="upDirection"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        internal Matrix3D ViewMatrix {
            get {
                //TODO: Cache this

                Vector3D cameraZAxis = -this.LookDirection;
                cameraZAxis.Normalize();

                Vector3D cameraXAxis = Vector3D.CrossProduct(this.UpDirection, cameraZAxis);
                cameraXAxis.Normalize();

                Vector3D cameraYAxis = Vector3D.CrossProduct(cameraZAxis, cameraXAxis);

                Vector3D cameraPosition = (Vector3D)this.Position;
                double offsetX = -Vector3D.DotProduct(cameraXAxis, cameraPosition);
                double offsetY = -Vector3D.DotProduct(cameraYAxis, cameraPosition);
                double offsetZ = -Vector3D.DotProduct(cameraZAxis, cameraPosition);

                return new Matrix3D(cameraXAxis.X, cameraYAxis.X, cameraZAxis.X, 0,
                                    cameraXAxis.Y, cameraYAxis.Y, cameraZAxis.Y, 0,
                                    cameraXAxis.Z, cameraYAxis.Z, cameraZAxis.Z, 0,
                                    offsetX, offsetY, offsetZ, 1);
            }
        }

        internal Matrix3D InverseViewMatrix {
            get {
                //TODO: Cache

                Matrix3D m = this.ViewMatrix;
                m.Invert();
                return m;
            }
        }

        /// <summary>
        /// Calculates the matrix that transforms view coords in 3D to 2D points on the
        /// projection plane
        /// </summary>
        /// <param name="nearPlaneDistance"></param>
        /// <param name="farPlaneDistance"></param>
        /// <param name="fieldOfView"></param>
        /// <returns></returns>
        internal Matrix3D ProjectionMatrix {
            get {
                //TODO: Cache this

                //Now we can create the projection matrix
                double aspectRatio = this.Viewport.AspectRatio;
                double xScale = 1.0 / System.Math.Tan(System.Math.PI * this.FieldOfView / 360);
                double yScale = aspectRatio * xScale;

                //Will produce values in range of znear to zfar of 0 to -1
                double zScale = (this.FarPlaneDistance == double.PositiveInfinity) ? -1 : this.FarPlaneDistance / (this.NearPlaneDistance - this.FarPlaneDistance);
                double zOffset = this.NearPlaneDistance * zScale;

                //TODO: zScale and zOffset negate?  What else will this affect
                return new Matrix3D(xScale, 0, 0, 0,
                                    0, yScale, 0, 0,
                                    0, 0, zScale, -1,
                                    0, 0, zOffset, 0);
            }
        }

        /// <summary>
        /// Returns the overall transformation applied to the world points by
        /// this camera.  In this case the points are multiplied by a view
        /// transform and perspective projection transform.
        /// </summary>
        internal override Matrix3D Value {
            get {
                //TODO: Cache computed values
                Matrix3D view = this.ViewMatrix;
                Matrix3D projection = this.ProjectionMatrix;
                //Debug.WriteLine("view=\n{0}\nproj=\n{1}", view.ToString(), projection.ToString());

                return view * projection;
            }
        }
    }
}
