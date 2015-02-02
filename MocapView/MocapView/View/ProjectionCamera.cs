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
    public abstract class ProjectionCamera : Camera {
        private double farPlaneDistance;
        private double nearPlaneDistance;
        private Vector3D lookDirection;
        private Vector3D upDirection;
        private Point3D position;

        protected ProjectionCamera() {
            //These are the same values that WPF uses for its initial values
            //for projection based cameras
            this.FarPlaneDistance = double.PositiveInfinity;
            this.NearPlaneDistance = 0.125;
            this.LookDirection = new Vector3D(0, 0, -1);
            this.UpDirection = new Vector3D(0, 1, 0);
            this.Position = new Point3D(0, 0, 0);
        }

        public double FarPlaneDistance {
            get { return this.farPlaneDistance; }
            set {
                if (this.farPlaneDistance != value) {
                    this.farPlaneDistance = value;
                }
            }
        }

        public double NearPlaneDistance {
            get { return this.nearPlaneDistance; }
            set {
                if (this.nearPlaneDistance != value) {
                    this.nearPlaneDistance = value;
                }
            }
        }

        public Vector3D LookDirection {
            get { return this.lookDirection; }
            set {
                if (this.lookDirection != value) {
                    this.lookDirection = value;
                    this.lookDirection.Normalize();
                }
            }
        }

        public Point3D Position {
            get { return this.position; }
            set {
                if (this.position != value) {
                    this.position = value;
                }
            }
        }

        public Vector3D UpDirection {
            get { return this.upDirection; }
            set {
                if (this.upDirection != value) {
                    this.upDirection = value;
                    this.upDirection.Normalize();
                }
            }
        }
    }

}
