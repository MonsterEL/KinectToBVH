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
    public struct Quaternion {
        private static Quaternion identity;
        private bool isNotDefaultQuaternion;
        private double x;
        private double y;
        private double z;
        private double w;
        private Matrix3D value;
        private bool valueIsDirty;

        static Quaternion() {
            Quaternion.identity = new Quaternion(0, 0, 0, 1);
        }

        public static Quaternion XRotation(double angleInRadians) {
            double halfAngleInRadians = (angleInRadians * 0.5);
            Quaternion q = new Quaternion(Math.Sin(halfAngleInRadians), 0, 0, Math.Cos(halfAngleInRadians));
            q.value = Matrix3D.Identity;
            q.valueIsDirty = true;
            return q;
        }

        public static Quaternion YRotation(double angleInRadians) {
            double halfAngleInRadians = (angleInRadians * 0.5);
            Quaternion q = new Quaternion(0, Math.Sin(halfAngleInRadians), 0, Math.Cos(halfAngleInRadians));
            q.value = Matrix3D.Identity;
            q.valueIsDirty = true;
            return q;
        }

        public static Quaternion ZRotation(double angleInRadians) {
            double halfAngleInRadians = (angleInRadians * 0.5);
            Quaternion q = new Quaternion(0, 0, Math.Sin(halfAngleInRadians), Math.Cos(halfAngleInRadians));
            q.value = Matrix3D.Identity;
            q.valueIsDirty = true;
            return q;
        }

        public void RotateTo(Point3D vector, Point3D result) {
            double a = this.W;
            double b = this.X;
            double c = this.Y;
            double d = this.Z;
            double v1 = vector.X;
            double v2 = vector.Y;
            double v3 = vector.Z;

            double t2 = a * b;
            double t3 = a * c;
            double t4 = a * d;
            double t5 = -b * b;
            double t6 = b * c;
            double t7 = b * d;
            double t8 = -c * c;
            double t9 = c * d;
            double t10 = -d * d;
            result.X = 2 * ((t8 + t10) * v1 + (t6 - t4) * v2 + (t3 + t7) * v3) + v1;
            result.Y = 2 * ((t4 + t6) * v1 + (t5 + t10) * v2 + (t9 - t2) * v3) + v2;
            result.Z = 2 * ((t7 - t3) * v1 + (t2 + t9) * v2 + (t5 + t8) * v3) + v3;
        }

        public Quaternion(Vector3D axisOfRotation, double angleInRadians) {
            double halfAngleInRadians = (angleInRadians * 0.5);
            this.isNotDefaultQuaternion = true;
            axisOfRotation.Normalize();
            this.x = axisOfRotation.X * System.Math.Sin(halfAngleInRadians);
            this.y = axisOfRotation.Y * System.Math.Sin(halfAngleInRadians);
            this.z = axisOfRotation.Z * System.Math.Sin(halfAngleInRadians);
            this.w = System.Math.Cos(halfAngleInRadians);
            this.value = Matrix3D.Identity;
            this.valueIsDirty = true;
        }

        public Quaternion(double x, double y, double z, double w) {
            this.isNotDefaultQuaternion = true;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            this.value = Matrix3D.Identity;
            this.valueIsDirty = true;
        }

        public static Quaternion Add(Quaternion left, Quaternion right) {
            return left + right;
        }

        public static Quaternion operator +(Quaternion left, Quaternion right) {
            if (!left.isNotDefaultQuaternion) {
                if (!right.isNotDefaultQuaternion) {
                    return new Quaternion(0, 0, 0, 2);
                }
                right.W = right.W + 1;
                return right;
            }
            if (!right.isNotDefaultQuaternion) {
                left.W = left.W + 1;
                return left;
            }

            return new Quaternion(left.X + right.X,
                                  left.Y + right.Y,
                                  left.Z + right.Z,
                                  left.W + right.W);
        }

        public static Quaternion Subtract(Quaternion left, Quaternion right) {
            return left - right;
        }

        public static Quaternion operator -(Quaternion left, Quaternion right) {
            if (!left.isNotDefaultQuaternion) {
                if (!right.isNotDefaultQuaternion) {
                    return new Quaternion(0, 0, 0, 0);
                }
                return new Quaternion(-right.X, -right.Y, -right.Z, 1 - right.W);
            }
            if (!right.isNotDefaultQuaternion) {
                return new Quaternion(left.X, left.Y, left.Z, left.W - 1);
            }

            return new Quaternion(left.X - right.X,
                                  left.Y - right.Y,
                                  left.Z - right.Z,
                                  left.W - right.W);
        }

        public static Quaternion Multiply(Quaternion left, Quaternion right) {
            return left * right;
        }

        public static Quaternion operator *(Quaternion left, Quaternion right) {
            if (!left.isNotDefaultQuaternion) {
                return right;
            }
            if (!right.isNotDefaultQuaternion) {
                return left;
            }

            //Derived from knowing:
            // q1q2 = w1w2 + w1q2 + w2q1 + q1 x q2 - q1.q2
            double xComponent = left.W * right.X + right.W * left.X + (left.Y * right.Z - left.Z * right.Y);
            double yComponent = left.W * right.Y + right.W * left.Y + (left.Z * right.X - left.X * right.Z);
            double zComponent = left.W * right.Z + right.W * left.Z + (left.X * right.Y - left.Y * right.X);
            double wComponent = left.W * right.W - (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
            return new Quaternion(xComponent, yComponent, zComponent, wComponent);
        }

        public static Quaternion Slerp(Quaternion from, Quaternion to, double t) {
            return Slerp(from, to, t, true);
        }

        public static Quaternion Slerp(Quaternion from, Quaternion to, double t, bool useShortestPath) {
            //For more information on how this works see:
            //Chapter 10 in "Essential Mathermatics for Games & Interactive Applications"
            //James M. Van Verth & Lars M. Bishop

            //Slerp(p,q,t) = (sin((1-t)angle) * p + sin(t*angle) * q) / sin(angle)

            //Interpolate along a sphere between the two quaternions

            //Find the angle between the two axis
            double fromNorm = from.Norm;
            double toNorm = to.Norm;
            from.Normalize();
            to.Normalize();
            double fromDotTo = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;

            if (useShortestPath) {
                if (fromDotTo < 0) {
                    //Need to negate one of the quaternions
                    to = new Quaternion(-to.X, -to.Y, -to.Z, -to.W);
                    fromDotTo *= -1;
                }
            }
            if (fromDotTo < -1) {
                fromDotTo = -1;
            }
            else if (fromDotTo > 1) {
                fromDotTo = 1;
            }

            double angleInRadians = System.Math.Acos(fromDotTo);
            double sinAngle = System.Math.Sin(angleInRadians);

            //Be careful about division by zero
            if (MathHelper.IsZero(sinAngle)) {
                //Just lerp instead of slerp
                return new Quaternion(from.X + (to.X - from.X) * t,
                                      from.Y + (to.Y - from.Y) * t,
                                      from.Z + (to.Z - from.Z) * t,
                                      from.W + (to.W - from.W) * t);
            }
            else {

                double sinFrom = System.Math.Sin((1.0 - t) * angleInRadians) / sinAngle;
                double sinTo = System.Math.Sin(t * angleInRadians) / sinAngle;

                //Scale from FromNorm to toNorm depending on t.  When t==0 then
                //scale == fromNorm, when t==1 scale == toNorm
                double scale = 1;//fromNorm * System.Math.Pow((toNorm / fromNorm), t);
                return new Quaternion(scale * (from.X * sinFrom + to.X * sinTo),
                                      scale * (from.Y * sinFrom + to.Y * sinTo),
                                      scale * (from.Z * sinFrom + to.Z * sinTo),
                                      scale * (from.W * sinFrom + to.W * sinTo));
            }
        }

        public static Quaternion Identity {
            get {
                return Quaternion.identity;
            }
        }

        public bool IsIdentity {
            get {
                return (!this.isNotDefaultQuaternion ||
                       ((this.X == 0) &&
                        (this.Y == 0) &&
                        (this.Z == 0) &&
                        (this.W == 1.0)));
            }
        }

        public bool IsNormalized {
            get {
                if (!this.isNotDefaultQuaternion) {
                    return true;
                }

                return MathHelper.IsZero(this.Norm - 1);
            }
        }

        private double Norm {
            get {
                return MathHelper.SquareRoot(this.X * this.X +
                                             this.Y * this.Y +
                                             this.Z * this.Z +
                                             this.W * this.W);
            }
        }

        private void MultiplyByScalar(double scalar) {
            this.X *= scalar;
            this.Y *= scalar;
            this.Z *= scalar;
            this.W *= scalar;
        }

        public Vector3D Axis {
            get {
                if (!this.isNotDefaultQuaternion ||
                   ((this.X == 0) &&
                    (this.Y == 0) &&
                    (this.Z == 0))) {
                    return new Vector3D(0, 1, 0);
                }

                Vector3D v = new Vector3D(this.X, this.Y, this.Z);
                v.Normalize();
                return v;
            }
        }

        public double Angle {
            get {
                if (!this.isNotDefaultQuaternion) {
                    return 0;
                }

                if ((this.X == 0) && (this.Y == 0) && (this.Z == 0) && (this.W == 0)) {
                    return 0;
                }

                Quaternion q = this;
                q.Normalize();
                return (180 * 2 * System.Math.Acos(q.W)) / System.Math.PI;
            }
        }

        public void Normalize() {
            if (!this.isNotDefaultQuaternion) {
                return;
            }

            double normInverse = 1.0 / this.Norm;
            this.X = this.X * normInverse;
            this.Y = this.Y * normInverse;
            this.Z = this.Z * normInverse;
            this.W = this.W * normInverse;
        }

        public void Invert() {
            double normSquaredInverse = 1.0 / (this.X * this.X +
                                               this.Y * this.Y +
                                               this.Z * this.Z +
                                               this.W * this.W);

            Quaternion q = this;
            q.Conjugate();
            this.X = q.X * normSquaredInverse;
            this.Y = q.Y * normSquaredInverse;
            this.Z = q.Z * normSquaredInverse;
            this.W = q.W * normSquaredInverse;
        }

        public void Conjugate() {
            if (this.isNotDefaultQuaternion) {
                this.X *= -1;
                this.Y *= -1;
                this.Z *= -1;
            }
        }

        public double X {
            get {
                return this.x;
            }
            set {
                if (!this.isNotDefaultQuaternion) {
                    this = Quaternion.identity;
                    this.isNotDefaultQuaternion = true;
                }
                this.x = value;

                this.ValueIsDirty = true;
            }
        }

        public double Y {
            get {
                return this.y;
            }
            set {
                if (!this.isNotDefaultQuaternion) {
                    this = Quaternion.identity;
                    this.isNotDefaultQuaternion = true;
                }
                this.y = value;

                this.ValueIsDirty = true;
            }
        }

        public double Z {
            get {
                return this.z;
            }
            set {
                if (!this.isNotDefaultQuaternion) {
                    this = Quaternion.identity;
                    this.isNotDefaultQuaternion = true;
                }
                this.z = value;

                this.ValueIsDirty = true;
            }
        }

        public double W {
            get {
                if (!this.isNotDefaultQuaternion) {
                    return 1.0;
                }
                return this.w;
            }
            set {
                if (!this.isNotDefaultQuaternion) {
                    this = Quaternion.identity;
                    this.isNotDefaultQuaternion = true;
                }
                this.w = value;

                this.ValueIsDirty = true;
            }
        }

        /// <summary>
        /// Returns the Matrix3D representation of the Quaternion.  NOTE - this
        /// property is not available in WPF
        /// </summary>
        public Matrix3D Value {
            get {
                if (this.ValueIsDirty) {
                    //TODO: Cache this if the quaternion has not changed
                    double n1 = 2 * this.Y * this.Y;
                    double n2 = 2 * this.Z * this.Z;
                    double n3 = 2 * this.X * this.X;
                    double n4 = 2 * this.X * this.Y;
                    double n5 = 2 * this.W * this.Z;
                    double n6 = 2 * this.X * this.Z;
                    double n7 = 2 * this.W * this.Y;
                    double n8 = 2 * this.Y * this.Z;
                    double n9 = 2 * this.W * this.X;

                    this.value = Matrix3D.Identity;
                    this.value.M11 = 1 - n1 - n2;
                    this.value.M12 = n4 + n5;
                    this.value.M13 = n6 - n7;
                    this.value.M21 = n4 - n5;
                    this.value.M22 = 1 - n3 - n2;
                    this.value.M23 = n8 + n9;
                    this.value.M31 = n6 + n7;
                    this.value.M32 = n8 - n9;
                    this.value.M33 = 1 - n3 - n1;
                    this.value.M44 = 1;

                    this.ValueIsDirty = false;
                }
                return this.value;
            }
        }

        /// <summary>
        /// Converts a rotation matrix to a quaternion.  Matrix must be orthogonal
        /// </summary>
        /// <returns></returns>
        public static Quaternion FromRotationMatrix(Matrix3D matrix) {
            double w = System.Math.Sqrt(1 + matrix.M11 + matrix.M22 + matrix.M33) / 2.0;
            double x = (matrix.M32 - matrix.M23) / (4 * w);
            double y = (matrix.M13 - matrix.M31) / (4 * w);
            double z = (matrix.M21 - matrix.M12) / (4 * w);

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Indicates that the value matrix is dirty and needs recalculating
        /// </summary>
        private bool ValueIsDirty {
            get { return this.valueIsDirty; }
            set { this.valueIsDirty = value; }
        }

        public override string ToString() {
            return string.Format("x:{0}, y:{1}, z:{2}, w:{3}", this.X, this.Y, this.Z, this.W);
        }

        public override int GetHashCode() {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.W.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is Quaternion)) {
                return false;
            }

            Quaternion q = (Quaternion)obj;
            return (this.X == q.X) &&
                   (this.Y == q.Y) &&
                   (this.Z == q.Z) &&
                   (this.W == q.W);
        }
    }
}
