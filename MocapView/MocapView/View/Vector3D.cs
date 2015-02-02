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
    public class Vector3D {
        public double X;
        public double Y;
        public double Z;

        public Vector3D(double x, double y, double z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }


        public double Length {
            get {
                return MathHelper.SquareRoot(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
            }
        }

        public double LengthSquared {
            get {
                return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
            }
        }


        #region Methods

        public override int GetHashCode() {
            return this.X.GetHashCode() ^
                   this.Y.GetHashCode() ^
                   this.Z.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is Vector3D)) {
                return false;
            }

            return Equals(this, (Vector3D)obj);
        }

        public bool Equals(Vector3D v) {
            return Equals(this, v);
        }

        public static bool Equals(Vector3D v1, Vector3D v2) {
            if (((object)v1 == null) && ((object) v2 == null)) {
                return true;
            }
            if (((object)v1 == null) || ((object) v2 == null)) {
                return false;
            }

            return v1.X.Equals(v2.X) &&
                   v1.Y.Equals(v2.Y) &&
                   v1.Z.Equals(v2.Z);
        }

        public override string ToString() {
            return X + ", " + Y + ", " + Z;
        }

        public static double DotProduct(Vector3D v1, Vector3D v2) {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static Vector3D CrossProduct(Vector3D v1, Vector3D v2) {
            return new Vector3D(v1.Y * v2.Z - v1.Z * v2.Y,
                                v1.Z * v2.X - v1.X * v2.Z,
                                v1.X * v2.Y - v1.Y * v2.X);
        }

        public void Negate() {
            this.X *= -1;
            this.Y *= -1;
            this.Z *= -1;
        }

        public void Normalize() {
            double lengthSquared = this.LengthSquared;
            double inverseSquareRoot = MathHelper.InverseSquareRoot(lengthSquared);
            this.X *= inverseSquareRoot;
            this.Y *= inverseSquareRoot;
            this.Z *= inverseSquareRoot;
        }

        public static Point3D Add(Vector3D v, Point3D p) {
            return new Point3D(v.X + p.X, v.Y + p.Y, v.Z + p.Z);
        }

        public static Vector3D Add(Vector3D v1, Vector3D v2) {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Point3D operator +(Vector3D v, Point3D p) {
            return Add(v, p);
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2) {
            return Add(v1, v2);
        }

        public static Point3D Subtract(Vector3D v, Point3D p) {
            return new Point3D(v.X - p.X, v.Y - p.Y, v.Z - p.Z);
        }

        public static Vector3D Subtract(Vector3D v1, Vector3D v2) {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Point3D operator -(Vector3D v, Point3D p) {
            return Subtract(v, p);
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2) {
            return Subtract(v1, v2);
        }

        public static Vector3D Multiply(double scalar, Vector3D v) {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D Multiply(Vector3D v, double scalar) {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D operator *(double scalar, Vector3D v) {
            return Multiply(scalar, v);
        }

        public static Vector3D operator *(Vector3D v, double scalar) {
            return Multiply(v, scalar);
        }

        public static Vector3D Divide(Vector3D v, double scalar) {
            double div = 1.0 / scalar;
            return new Vector3D(v.X * div, v.Y * div, v.Z * div);
        }

        public static Vector3D operator /(Vector3D v, double scalar) {
            return Divide(v, scalar);
        }

        public static bool operator ==(Vector3D v1, Vector3D v2) {
            return Equals(v1, v2);
        }

        public static bool operator !=(Vector3D v1, Vector3D v2) {
            return !Equals(v1, v2);
        }

        public static explicit operator Point3D(Vector3D v) {
            return new Point3D(v.X, v.Y, v.Z);
        }

        public static Vector3D operator -(Vector3D v) {
            return new Vector3D(-v.X, -v.Y, -v.Z);
        }

        #endregion
    }
}
