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
    public class Point3D {
        public static Point3D Zero = new Point3D(0, 0, 0);

        /// <summary>
        /// X coordinate of this point in 3D space
        /// </summary>
        public double X;
        
        /// <summary>
        /// Y coordinate of this point in 3D space
        /// </summary>
        public double Y;
        
        /// <summary>
        /// Z coordinate of this point in 3D space
        /// </summary>
        public double Z;

        /// <summary>
        /// The coordinates of this point on screen
        /// </summary>
        public Point ScreenCoords = new Point();

        public Point3D(double x, double y, double z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        //For internal use only, not exposed in WPF libraries
        internal Point3D(Point p) {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = 1;
        }

        internal Point3D(Point3D p) {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        public override int GetHashCode() {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is Point3D)) {
                return false;
            }

            return Equals(this, (Point3D)obj);
        }

        public bool Equals(Point3D p) {
            return Equals(this, p);
        }

        public static bool Equals(Point3D p1, Point3D p2) {
            if (((object)p1 == null) && ((object) p2 == null)) {
                return true;
            }
            if (((object)p1 == null) || ((object) p2 == null)) {
                return false;
            }
            return p1.X.Equals(p2.X) && p1.Y.Equals(p2.Y) && p1.Z.Equals(p2.Z);
        }

        public override string ToString() {
            return "(" + this.X + ", " + this.Y + ", " + this.Z + ")";
        }

        public static explicit operator Vector3D(Point3D p) {
            return new Vector3D(p.X, p.Y, p.Z);
        }

        public static Vector3D Subtract(Point3D p1, Point3D p2) {
            return new Vector3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Vector3D operator -(Point3D p1, Point3D p2) {
            return Subtract(p1, p2);
        }

        public static Point3D Subtract(Point3D p, Vector3D v) {
            return new Point3D(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        }

        public static Point3D operator -(Point3D p, Vector3D v) {
            return Subtract(p, v);
        }

        public static Point3D Add(Point3D point, Vector3D v) {
            return new Point3D(point.X + v.X, point.Y + v.Y, point.Z + v.Z);
        }

        public static Point3D operator +(Point3D point, Vector3D v) {
            return Add(point, v);
        }

        public static bool operator ==(Point3D p1, Point3D p2) {
            if (((object)p1 == null) && ((object)p2 == null)) {
                return true;
            }
            if (((object)p1 == null) || ((object)p2 == null)) {
                return false;
            }
            return p1.Equals(p2);
        }

        public static bool operator !=(Point3D p1, Point3D p2) {
            if (((object)p1 == null) && ((object)p2 == null)) {
                return false;
            }
            if (((object)p1 == null) || ((object)p2 == null)) {
                return true;
            }
            return !p1.Equals(p2);
        }
    }
}
