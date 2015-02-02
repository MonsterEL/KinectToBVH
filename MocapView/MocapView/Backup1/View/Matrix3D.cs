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
    public struct Matrix3D {
        //TODO: Look at caching these values once they have been computed.

        private double m11, m12, m13, m14;
        private double m21, m22, m23, m24;
        private double m31, m32, m33, m34;
        private double offsetX, offsetY, offsetZ, m44;
        private bool isNotDefaultMatrix;
        private static Matrix3D identity;

        static Matrix3D() {
            Matrix3D.identity = new Matrix3D(1, 0, 0, 0,
                                             0, 1, 0, 0,
                                             0, 0, 1, 0,
                                             0, 0, 0, 1);
        }

        public Matrix3D(double m11, double m12, double m13, double m14,
                        double m21, double m22, double m23, double m24,
                        double m31, double m32, double m33, double m34,
                        double offsetX, double offsetY, double offsetZ, double m44) {
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m14 = m14;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m24 = m24;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
            this.m34 = m34;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.offsetZ = offsetZ;
            this.m44 = m44;
            this.isNotDefaultMatrix = true;
        }

        #region Properties

        public double M11 {
            get {
                //WPF version of the Matrix3D structure is the identity matrix
                //when the default constructor is used, so we need to do a bit 
                //of housekeeping here to make sure that is the case for us as well.
                if (!this.isNotDefaultMatrix) {
                    return 1;
                }
                else {
                    return this.m11;
                }
            }
            set {
                if (!this.isNotDefaultMatrix) {
                    this = Matrix3D.identity;
                    this.isNotDefaultMatrix = true;
                }
                this.m11 = value;
            }
        }

        public double M12 {
            get {
                return this.m12;
            }
            set {
                this.m12 = value;
            }
        }

        public double M13 {
            get {
                return this.m13;
            }
            set {
                this.m13 = value;
            }
        }

        public double M14 {
            get {
                return this.m14;
            }
            set {
                this.m14 = value;
            }
        }

        public double M21 {
            get {
                return this.m21;
            }
            set {
                this.m21 = value;
            }
        }

        public double M22 {
            get {
                if (!this.isNotDefaultMatrix) {
                    return 1;
                }
                else {
                    return this.m22;
                }
            }
            set {
                if (!this.isNotDefaultMatrix) {
                    this = Matrix3D.identity;
                    this.isNotDefaultMatrix = true;
                }
                this.m22 = value;
            }
        }

        public double M23 {
            get {
                return this.m23;
            }
            set {
                this.m23 = value;
            }
        }

        public double M24 {
            get {
                return this.m24;
            }
            set {
                this.m24 = value;
            }
        }

        public double M31 {
            get {
                return this.m31;
            }
            set {
                this.m31 = value;
            }
        }

        public double M32 {
            get {
                return this.m32;
            }
            set {
                this.m32 = value;
            }
        }

        public double M33 {
            get {
                if (!this.isNotDefaultMatrix) {
                    return 1;
                }
                else {
                    return this.m33;
                }
            }
            set {
                if (!this.isNotDefaultMatrix) {
                    this = Matrix3D.identity;
                    this.isNotDefaultMatrix = true;
                }
                this.m33 = value;
            }
        }

        public double M34 {
            get {
                return this.m34;
            }
            set {
                this.m34 = value;
            }
        }

        public double OffsetX {
            get {
                return this.offsetX;
            }
            set {
                this.offsetX = value;
            }
        }

        public double OffsetY {
            get {
                return this.offsetY;
            }
            set {
                this.offsetY = value;
            }
        }

        public double OffsetZ {
            get {
                return this.offsetZ;
            }
            set {
                this.offsetZ = value;
            }
        }

        public double M44 {
            get {
                if (!this.isNotDefaultMatrix) {
                    return 1;
                }
                else {
                    return this.m44;
                }
            }
            set {
                if (!this.isNotDefaultMatrix) {
                    this = Matrix3D.identity;
                    this.isNotDefaultMatrix = true;
                }
                this.m44 = value;
            }
        }

        public bool IsIdentity {
            get {
                if (!this.isNotDefaultMatrix) {
                    return true;
                }

                return m11 == 1 && m22 == 1 && m33 == 1 && m44 == 1 &&
                       m12 == 0 && m13 == 0 && m14 == 0 &&
                       m21 == 0 && m23 == 0 && m24 == 0 &&
                       m31 == 0 && m32 == 0 && m34 == 0 &&
                       offsetX == 0 && offsetY == 0 && offsetZ == 0;
            }
        }

        /// <summary>
        /// Returns an identity matrix instance
        /// </summary>
        public static Matrix3D Identity {
            get {
                return Matrix3D.identity;
            }
        }

        public bool IsAffine {
            get {
                if (!this.isNotDefaultMatrix) {
                    return true;
                }
                else {
                    return this.m14 == 0 &&
                           this.m24 == 0 &&
                           this.m34 == 0 &&
                           this.m44 == 1;
                }
            }
        }

        /// <summary>
        /// Returns true if the matrix has an inverse
        /// </summary>
        public bool HasInverse {
            get {
                return !MathHelper.IsZero(this.Determinant);
            }
        }

        /// <summary>
        /// Returns the determinant of the matrix
        /// </summary>
        public double Determinant {
            get {
                if (!this.isNotDefaultMatrix) {
                    return 1;
                }

                //TODO: Implement affine determinant

                double num1 = m33 * m44 - m34 * offsetZ;
                double num2 = m32 * m44 - m34 * offsetY;
                double num3 = m31 * m44 - m34 * offsetX;
                double num4 = m32 * offsetZ - m33 * offsetY;
                double num5 = m31 * offsetZ - m33 * offsetX;
                double num6 = m31 * offsetY - m32 * offsetX;

                return m11 * (m22 * num1 - m23 * num2 + m24 * num4) -
                       m12 * (m21 * num1 - m23 * num3 + m24 * num5) +
                       m13 * (m21 * num2 - m22 * num3 + m24 * num6) -
                       m14 * (m21 * num4 - m22 * num5 + m23 * num6);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Appends the matrix to the current matrix
        /// </summary>
        /// <param name="m"></param>
        public void Append(Matrix3D m) {
            this = this * m;
        }

        public void Prepend(Matrix3D m) {
            this = m * this;
        }

        public static Matrix3D Multiply(Matrix3D m1, Matrix3D m2) {
            return m1 * m2;
        }

        public static Matrix3D operator *(Matrix3D m1, Matrix3D m2) {
            if (!m1.isNotDefaultMatrix) {
                return m2;
            }
            else if (!m2.isNotDefaultMatrix) {
                return m1;
            }
            else if (m1.IsAffine) {
                return new Matrix3D(m1.m11 * m2.m11 + m1.m12 * m2.m21 + m1.m13 * m2.m31,
                                    m1.m11 * m2.m12 + m1.m12 * m2.m22 + m1.m13 * m2.m32,
                                    m1.m11 * m2.m13 + m1.m12 * m2.m23 + m1.m13 * m2.m33,
                                    m1.m11 * m2.m14 + m1.m12 * m2.m24 + m1.m13 * m2.m34,
                                    m1.m21 * m2.m11 + m1.m22 * m2.m21 + m1.m23 * m2.m31,
                                    m1.m21 * m2.m12 + m1.m22 * m2.m22 + m1.m23 * m2.m32,
                                    m1.m21 * m2.m13 + m1.m22 * m2.m23 + m1.m23 * m2.m33,
                                    m1.m21 * m2.m14 + m1.m22 * m2.m24 + m1.m23 * m2.m34,
                                    m1.m31 * m2.m11 + m1.m32 * m2.m21 + m1.m33 * m2.m31,
                                    m1.m31 * m2.m12 + m1.m32 * m2.m22 + m1.m33 * m2.m32,
                                    m1.m31 * m2.m13 + m1.m32 * m2.m23 + m1.m33 * m2.m33,
                                    m1.m31 * m2.m14 + m1.m32 * m2.m24 + m1.m33 * m2.m34,
                                    m1.offsetX * m2.m11 + m1.offsetY * m2.m21 + m1.offsetZ * m2.m31 + m1.m44 * m2.offsetX,
                                    m1.offsetX * m2.m12 + m1.offsetY * m2.m22 + m1.offsetZ * m2.m32 + m1.m44 * m2.offsetY,
                                    m1.offsetX * m2.m13 + m1.offsetY * m2.m23 + m1.offsetZ * m2.m33 + m1.m44 * m2.offsetZ,
                                    m1.offsetX * m2.m14 + m1.offsetY * m2.m24 + m1.offsetZ * m2.m34 + m1.m44 * m2.m44);
            }

            return new Matrix3D(m1.m11 * m2.m11 + m1.m12 * m2.m21 + m1.m13 * m2.m31 + m1.m14 * m2.offsetX,
                                m1.m11 * m2.m12 + m1.m12 * m2.m22 + m1.m13 * m2.m32 + m1.m14 * m2.offsetY,
                                m1.m11 * m2.m13 + m1.m12 * m2.m23 + m1.m13 * m2.m33 + m1.m14 * m2.offsetZ,
                                m1.m11 * m2.m14 + m1.m12 * m2.m24 + m1.m13 * m2.m34 + m1.m14 * m2.m44,
                                m1.m21 * m2.m11 + m1.m22 * m2.m21 + m1.m23 * m2.m31 + m1.m24 * m2.offsetX,
                                m1.m21 * m2.m12 + m1.m22 * m2.m22 + m1.m23 * m2.m32 + m1.m24 * m2.offsetY,
                                m1.m21 * m2.m13 + m1.m22 * m2.m23 + m1.m23 * m2.m33 + m1.m24 * m2.offsetZ,
                                m1.m21 * m2.m14 + m1.m22 * m2.m24 + m1.m23 * m2.m34 + m1.m24 * m2.m44,
                                m1.m31 * m2.m11 + m1.m32 * m2.m21 + m1.m33 * m2.m31 + m1.m34 * m2.offsetX,
                                m1.m31 * m2.m12 + m1.m32 * m2.m22 + m1.m33 * m2.m32 + m1.m34 * m2.offsetY,
                                m1.m31 * m2.m13 + m1.m32 * m2.m23 + m1.m33 * m2.m33 + m1.m34 * m2.offsetZ,
                                m1.m31 * m2.m14 + m1.m32 * m2.m24 + m1.m33 * m2.m34 + m1.m34 * m2.m44,
                                m1.offsetX * m2.m11 + m1.offsetY * m2.m21 + m1.offsetZ * m2.m31 + m1.m44 * m2.offsetX,
                                m1.offsetX * m2.m12 + m1.offsetY * m2.m22 + m1.offsetZ * m2.m32 + m1.m44 * m2.offsetY,
                                m1.offsetX * m2.m13 + m1.offsetY * m2.m23 + m1.offsetZ * m2.m33 + m1.m44 * m2.offsetZ,
                                m1.offsetX * m2.m14 + m1.offsetY * m2.m24 + m1.offsetZ * m2.m34 + m1.m44 * m2.m44);
        }

        /// <summary>
        /// Multiples the vector by the matrix i.e. v * m
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3D Transform(Vector3D v) {
            if (!this.isNotDefaultMatrix) {
                return v;
            }

            return new Vector3D(v.X * this.m11 + v.Y * this.m21 + v.Z * this.m31,
                                v.X * this.m12 + v.Y * this.m22 + v.Z * this.m32,
                                v.X * this.m13 + v.Y * this.m23 + v.Z * this.m33);
        }

        /// <summary>
        /// Transforms a 3D point using the matrix and returns only the 2d component of it
        /// Usable for projections on 2-d space
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point Project(Point3D p) {
            Point newPoint = new Point(p.X * this.m11 + p.Y * this.m21 + p.Z * this.m31 + this.offsetX,
                             p.X * this.m12 + p.Y * this.m22 + p.Z * this.m32 + this.offsetY);

            if (!this.IsAffine) {
                double d = 1.0 / (p.X * this.m14 + p.Y * this.m24 + p.Z * this.m34 + this.m44);
                newPoint.X *= d;
                newPoint.Y *= d;
            }

            return newPoint;
        }

        public void TransformTo(Point3D p, Point3D result) {
            if (!this.isNotDefaultMatrix) {
                result.X = p.X;
                result.Y = p.Y;
                result.Z = p.Z;
                return;
            }
            result.X = p.X * this.m11 + p.Y * this.m21 + p.Z * this.m31 + this.offsetX;
            result.Y = p.X * this.m12 + p.Y * this.m22 + p.Z * this.m32 + this.offsetY;
            result.Z = p.X * this.m13 + p.Y * this.m23 + p.Z * this.m33 + this.offsetZ;

            if (!this.IsAffine) {
                double d = 1.0 / (p.X * this.m14 + p.Y * this.m24 + p.Z * this.m34 + this.m44);
                result.X *= d;
                result.Y *= d;
                result.Z *= d;
            }
        }

        /// <summary>
        /// Multiplies the point by the matrix i.e. p * m
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point3D Transform(Point3D p) {
            if (!this.isNotDefaultMatrix) {
                return p;
            }

            Point3D newPoint = new Point3D(0,0,0);
            TransformTo(p, newPoint);

            return newPoint;
        }

        public void Scale(Vector3D v) {
            if (!this.isNotDefaultMatrix) {
                this.m11 = v.X;
                this.m22 = v.Y;
                this.m33 = v.Z;
                this.m44 = 1;
                this.isNotDefaultMatrix = true;
            }
            else {
                this.m11 *= v.X;
                this.m12 *= v.Y;
                this.m13 *= v.Z;
                this.m21 *= v.X;
                this.m22 *= v.Y;
                this.m23 *= v.Z;
                this.m31 *= v.X;
                this.m32 *= v.Y;
                this.m33 *= v.Z;
                this.offsetX *= v.X;
                this.offsetY *= v.Y;
                this.offsetZ *= v.Z;
            }
        }

        public void ScalePrepend(Vector3D v) {
            if (!this.isNotDefaultMatrix) {
                this.m11 = v.X;
                this.m22 = v.Y;
                this.m33 = v.Z;
                this.m44 = 1;
                this.isNotDefaultMatrix = true;
            }
            else {
                this.m11 *= v.X;
                this.m21 *= v.Y;
                this.m31 *= v.Z;
                this.m12 *= v.X;
                this.m22 *= v.Y;
                this.m32 *= v.Z;
                this.m13 *= v.X;
                this.m23 *= v.Y;
                this.m33 *= v.Z;
                this.m14 *= v.X;
                this.m24 *= v.Y;
                this.m34 *= v.Z;
            }
        }

        /// <summary>
        /// Transposes the current matrix
        /// </summary>
        public void Transpose() {
            double temp = this.m12;
            this.m12 = this.m21;
            this.m21 = temp;

            temp = this.m13;
            this.m13 = this.m31;
            this.m31 = temp;

            temp = this.m14;
            this.m14 = this.offsetX;
            this.offsetX = temp;

            temp = this.m23;
            this.m23 = this.m32;
            this.m32 = temp;

            temp = this.m24;
            this.m24 = this.offsetY;
            this.offsetY = temp;

            temp = this.m34;
            this.m34 = this.offsetZ;
            this.offsetZ = temp;
        }

        /// <summary>
        /// Inverts this instance of the matrix
        /// </summary>
        public void Invert() {
            if (!this.isNotDefaultMatrix) {
                //The matrix is the identity matrix, nothing to do
                return;
            }
            /*else if(this.IsAffine)
            {
                //TODO: Optimize for the affine case
                //TODO: Can use the determinant calculations here to improve
                //      performance - do this.
                double determinant = this.Determinant;
                if (MathHelper.IsZero(determinant))
                {
                    throw new InvalidOperationException("The matrix does not have an inverse");
                }

            }*/
            else {
                //double determinant = this.Determinant;

                double num1 = m33 * m44 - m34 * offsetZ;
                double num2 = m32 * m44 - m34 * offsetY;
                double num3 = m31 * m44 - m34 * offsetX;
                double num4 = m32 * offsetZ - m33 * offsetY;
                double num5 = m31 * offsetZ - m33 * offsetX;
                double num6 = m31 * offsetY - m32 * offsetX;

                double determinant = m11 * (m22 * num1 - m23 * num2 + m24 * num4) -
                                     m12 * (m21 * num1 - m23 * num3 + m24 * num5) +
                                     m13 * (m21 * num2 - m22 * num3 + m24 * num6) -
                                     m14 * (m21 * num4 - m22 * num5 + m23 * num6);

                if (MathHelper.IsZero(determinant)) {
                    return;
                    //throw new InvalidOperationException("The matrix does not have an inverse");
                }

                /*
                double num1 = m33 * m44 - m34 * offsetZ;
                double num2 = m32 * m44 - m34 * offsetY;
                double num3 = m31 * m44 - m34 * offsetX;
                double num4 = m32 * offsetZ - m33 * offsetY;
                double num5 = m31 * offsetZ - m33 * offsetX;
                double num6 = m31 * offsetY - m32 * offsetX;
                */

                double num7 = m33 * m44 - m34 * offsetZ;
                double num8 = m32 * m44 - m34 * offsetY;
                double num9 = m31 * m44 - m34 * offsetX;
                double num10 = m32 * offsetZ - m33 * offsetY;
                double num11 = m31 * offsetZ - m33 * offsetX;
                double num12 = m31 * offsetY - m32 * offsetX;

                double num13 = m23 * m44 - m24 * offsetZ;
                double num14 = m22 * m44 - m24 * offsetY;
                double num15 = m21 * m44 - m24 * offsetX;
                double num16 = m22 * offsetZ - m23 * offsetY;
                double num17 = m21 * offsetZ - m23 * offsetX;
                double num18 = m21 * offsetY - m22 * offsetX;

                double num19 = m23 * m34 - m24 * m33;
                double num20 = m22 * m34 - m24 * m32;
                double num21 = m21 * m34 - m24 * m31;
                double num22 = m22 * m33 - m23 * m32;
                double num23 = m21 * m33 - m23 * m31;
                double num24 = m21 * m32 - m22 * m31;

                double cofactor11 = (m22 * num1 - m23 * num2 + m24 * num4);
                double cofactor12 = -(m21 * num1 - m23 * num3 + m24 * num5);
                double cofactor13 = (m21 * num2 - m22 * num3 + m24 * num6);
                double cofactor14 = -(m21 * num4 - m22 * num5 + m23 * num6);

                double cofactor21 = -(m12 * num7 - m13 * num8 + m14 * num10);
                double cofactor22 = (m11 * num7 - m13 * num9 + m14 * num11);
                double cofactor23 = -(m11 * num8 - m12 * num9 + m14 * num12);
                double cofactor24 = (m11 * num10 - m12 * num11 + m13 * num12);

                double cofactor31 = (m12 * num13 - m13 * num14 + m14 * num16);
                double cofactor32 = -(m11 * num13 - m13 * num15 + m14 * num17);
                double cofactor33 = (m11 * num14 - m12 * num15 + m14 * num18);
                double cofactor34 = -(m11 * num16 - m12 * num17 + m13 * num18);

                double cofactor41 = -(m12 * num19 - m13 * num20 + m14 * num22);
                double cofactor42 = (m11 * num19 - m13 * num21 + m14 * num23);
                double cofactor43 = -(m11 * num20 - m12 * num21 + m14 * num24);
                double cofactor44 = (m11 * num22 - m12 * num23 + m13 * num24);

                double inverseDet = 1.0 / determinant;
                this.m11 = cofactor11 * inverseDet;
                this.m12 = cofactor21 * inverseDet;
                this.m13 = cofactor31 * inverseDet;
                this.m14 = cofactor41 * inverseDet;
                this.m21 = cofactor12 * inverseDet;
                this.m22 = cofactor22 * inverseDet;
                this.m23 = cofactor32 * inverseDet;
                this.m24 = cofactor42 * inverseDet;
                this.m31 = cofactor13 * inverseDet;
                this.m32 = cofactor23 * inverseDet;
                this.m33 = cofactor33 * inverseDet;
                this.m34 = cofactor43 * inverseDet;
                this.offsetX = cofactor14 * inverseDet;
                this.offsetY = cofactor24 * inverseDet;
                this.offsetZ = cofactor34 * inverseDet;
                this.m44 = cofactor44 * inverseDet;
            }
        }

        public void ScaleAt(Vector3D scale, Point3D center) {
            if (!this.isNotDefaultMatrix) {
                //Simple case
                this.m11 = scale.X;
                this.m22 = scale.Y;
                this.m33 = scale.Z;
                this.m44 = 1.0;
                this.offsetX = center.X - (center.X * scale.X);
                this.offsetY = center.Y - (center.Y * scale.Y);
                this.offsetZ = center.Z - (center.Z * scale.Z);
                this.isNotDefaultMatrix = true;
            }
            else {
                //Need to multiple the current matrix with the scaling matrix
                this.m11 = this.m11 * scale.X + this.m14 * center.X - this.m14 * scale.X * center.X;
                this.m12 = this.m12 * scale.Y + this.m14 * center.Y - this.m14 * scale.Y * center.Y;
                this.m13 = this.m13 * scale.Z + this.m14 * center.Z - this.m14 * scale.Z * center.Z;

                this.m21 = this.m21 * scale.X + this.m24 * center.X - this.m24 * scale.X * center.X;
                this.m22 = this.m22 * scale.Y + this.m24 * center.Y - this.m24 * scale.Y * center.Y;
                this.m23 = this.m23 * scale.Z + this.m24 * center.Z - this.m24 * scale.Z * center.Z;

                this.m31 = this.m31 * scale.X + this.m34 * center.X - this.m34 * scale.X * center.X;
                this.m32 = this.m32 * scale.Y + this.m34 * center.Y - this.m34 * scale.Y * center.Y;
                this.m33 = this.m33 * scale.Z + this.m34 * center.Z - this.m34 * scale.Z * center.Z;

                this.offsetX = this.offsetX * scale.X + this.m44 * center.X - this.m44 * scale.X * center.X;
                this.offsetY = this.offsetY * scale.Y + this.m44 * center.Y - this.m44 * scale.Y * center.Y;
                this.offsetZ = this.offsetZ * scale.Z + this.m44 * center.Z - this.m44 * scale.Z * center.Z;
            }
        }

        #endregion

        #region Overrides

        public override int GetHashCode() {
            if (!this.isNotDefaultMatrix) {
                return 0;
            }
            else {
                return this.m11.GetHashCode() ^
                       this.m12.GetHashCode() ^
                       this.m13.GetHashCode() ^
                       this.m14.GetHashCode() ^
                       this.m21.GetHashCode() ^
                       this.m22.GetHashCode() ^
                       this.m23.GetHashCode() ^
                       this.m24.GetHashCode() ^
                       this.m31.GetHashCode() ^
                       this.m32.GetHashCode() ^
                       this.m33.GetHashCode() ^
                       this.M34.GetHashCode() ^
                       this.offsetX.GetHashCode() ^
                       this.offsetY.GetHashCode() ^
                       this.offsetZ.GetHashCode() ^
                       this.m44.GetHashCode();
            }
        }

        public override string ToString() {
            if (this.IsIdentity) {
                return "Identity";
            }
            else {
                return String.Format("{0},{1},{2},{3},\n{4},{5},{6},{7},\n{8},{9},{10},{11},\n{12},{13},{14},{15}",
                                     m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34,
                                     offsetX, offsetY, offsetZ, m44);
            }
        }

        public void SetIdentity() {
            this = Matrix3D.identity;
        }

        public override bool Equals(object obj) {
            if (obj == null | !(obj is Matrix3D)) {
                return false;
            }

            return Equals(this, (Matrix3D)obj);
        }

        public bool Equals(Matrix3D m) {
            return Equals(this, m);
        }

        public static bool Equals(Matrix3D m1, Matrix3D m2) {
            if (!m1.isNotDefaultMatrix || !m2.isNotDefaultMatrix) {
                return m1.IsIdentity == m2.IsIdentity;
            }
            else {
                return (m1.m11 == m2.m11) &&
                       (m1.m12 == m2.m12) &&
                       (m1.m13 == m2.m13) &&
                       (m1.m14 == m2.m14) &&
                       (m1.m21 == m2.m21) &&
                       (m1.m22 == m2.m22) &&
                       (m1.m23 == m2.m23) &&
                       (m1.m24 == m2.m24) &&
                       (m1.m31 == m2.m31) &&
                       (m1.m32 == m2.m32) &&
                       (m1.m33 == m2.m33) &&
                       (m1.m34 == m2.m34) &&
                       (m1.offsetX == m2.offsetX) &&
                       (m1.offsetY == m2.offsetY) &&
                       (m1.offsetZ == m2.offsetZ) &&
                       (m1.m44 == m2.m44);
            }
        }

        public static bool operator ==(Matrix3D m1, Matrix3D m2) {
            return Equals(m1, m2);
        }

        public static bool operator !=(Matrix3D m1, Matrix3D m2) {
            return !Equals(m1, m2);
        }

        #endregion

    }
}
