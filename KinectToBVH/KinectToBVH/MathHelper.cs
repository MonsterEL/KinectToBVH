using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace KinectToBVH
{
    public class MathHelper
    {     

        /// <summary>
        /// convert quaternion to each angle of each axis
        /// rotation about X-axis: atan2(2(wx+yz), 1-2(xx+yy));
        /// rotation about Y-axis: arcsin(2(wy-zx));
        /// rotation about Z-axis: atan2(2(wz+xy), 1-2(yy+zz));
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="angleRotation"></param>
        public static Vector3D QuaternionToAxisAngles(Quaternion quaternion)
        {
            Vector3D angleRotation = new Vector3D(0, 0, 0);
            angleRotation.X = RadiusToAngle(Math.Atan2(2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z),
                1 - 2 * (Math.Pow(quaternion.X, 2) + Math.Pow(quaternion.Y, 2))));
            angleRotation.Y = RadiusToAngle(Math.Asin(2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X)));
            angleRotation.Z = RadiusToAngle(Math.Atan2(2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y),
                1 - 2 * (Math.Pow(quaternion.Y, 2) + Math.Pow(quaternion.Z, 2))));

            return angleRotation;
        }

        /// <summary>
        /// convert a vextor4 to quaternion
        /// </summary>
        /// <param name="vector4"></param>
        /// <returns></returns>
        public static Quaternion Vector4ToQuaternion(Vector4 vector4)
        {
            Quaternion quaternion = new Quaternion();
            quaternion.W = vector4.W;
            quaternion.X = vector4.X;
            quaternion.Y = vector4.Y;
            quaternion.Z = vector4.Z;
            return quaternion;
        }

        /// <summary>
        /// Convert a kinect rotation matrix to Matrix3D
        /// </summary>
        /// <param name="kinectMatrix"></param>
        /// <returns></returns>
        public static Matrix3D Matrix4ToMatrix3D(Matrix4 kinectMatrix)
        {
            Matrix3D converted = new Matrix3D();
            converted.M11 = kinectMatrix.M11;
            converted.M12 = kinectMatrix.M12;
            converted.M13 = kinectMatrix.M13;
            converted.M21 = kinectMatrix.M21;
            converted.M22 = kinectMatrix.M22;
            converted.M23 = kinectMatrix.M23;
            converted.M31 = kinectMatrix.M31;
            converted.M32 = kinectMatrix.M32;
            converted.M33 = kinectMatrix.M33;
            converted.M44 = kinectMatrix.M44;
            return converted;
        }

        /// <summary>
        /// convert a radius to angle
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static double RadiusToAngle(double radius)
        {
            return 180 * radius / Math.PI;
        }

        public static double AngleToRadius(double angle)
        {
            return Math.PI * angle / 180;
        }

        public static Quaternion XRotation(double angleInRadians)
        {
            double halfAngleInRadians = (angleInRadians * 0.5);
            Quaternion quaternion = new Quaternion(Math.Sin(halfAngleInRadians), 0, 0, Math.Cos(halfAngleInRadians));
            return quaternion;
        }

        public static Quaternion YRotation(double angleInRadians)
        {
            double halfAngleInRadians = (angleInRadians * 0.5);
            Quaternion quaternion = new Quaternion(0, Math.Sin(halfAngleInRadians), 0, Math.Cos(halfAngleInRadians));
            return quaternion;
        }

        public static Matrix3D GetRotationMatrix(double ax, double ay, double az)
        {
            Matrix3D my = Matrix3D.Identity;
            Matrix3D mz = Matrix3D.Identity;
            Matrix3D result = Matrix3D.Identity;
            if (ax != 0.0)
            {
                result = GetRotationMatrixX(ax);
            }
            if (ay != 0.0)
            {
                my = GetRotationMatrixY(ay);
            }
            if (az != 0.0)
            {
                mz = GetRotationMatrixZ(az);
            }
            if (my != null)
            {
                if (result != null)
                {
                    result *= my;
                }
                else
                {
                    result = my;
                }
            }
            if (mz != null)
            {
                if (result != null)
                {
                    result *= mz;
                }
                else
                {
                    result = mz;
                }
            }
            if (result != null)
            {
                return result;
            }
            else
            {
                return Matrix3D.Identity;
            }
        }

        public static Matrix3D GetRotationMatrixX(double angle)
        {
            if (angle == 0.0)
            {
                return Matrix3D.Identity;
            }
            double sin = (double)Math.Sin(angle);
            double cos = (double)Math.Cos(angle);
            return new Matrix3D(
         1, 0, 0, 0,
         0, cos, -sin, 0,
         0, sin, cos, 0,
         0, 0, 0, 1);
        }


        public static Matrix3D GetRotationMatrixY(double angle)
        {
            if (angle == 0.0)
            {
                return Matrix3D.Identity;
            }
            double sin = (double)Math.Sin(angle);
            double cos = (double)Math.Cos(angle);
            return new Matrix3D(
        cos, 0, sin, 0,
        0, 1, 0, 0,
        -sin, 0, cos, 0,
        0, 0, 0, 1);
        }

        public static Matrix3D GetRotationMatrixZ(double angle)
        {
            if (angle == 0.0)
            {
                return Matrix3D.Identity;
            }
            double sin = (double)Math.Sin(angle);
            double cos = (double)Math.Cos(angle);
            return new Matrix3D(
         cos, -sin, 0, 0,
         sin, cos, 0, 0,
         0, 0, 1, 0,
         0, 0, 0, 1);
        }

        public static Quaternion GetQuaternion(Vector3D v0, Vector3D v1)
        {
            Quaternion q = new Quaternion();
            // Copy, since cannot modify local
            v0.Normalize();
            v1.Normalize();

            double d = Vector3D.DotProduct(v0, v1);
            // If dot == 1, vectors are the same
            if (d >= 1.0f)
            {
                return Quaternion.Identity;
            }

            double s = Math.Sqrt((1 + d) * 2);
            double invs = 1 / s;

            Vector3D c = Vector3D.CrossProduct(v0, v1);

            q.X = c.X * invs;
            q.Y = c.Y * invs;
            q.Z = c.Z * invs;
            q.W = s * 0.5f;
            q.Normalize();

            return q;
        }

        public static Quaternion ZRotation(double angleInRadians)
        {
            double halfAngleInRadians = (angleInRadians * 0.5);
            Quaternion quaternion = new Quaternion(0, 0, Math.Sin(halfAngleInRadians), Math.Cos(halfAngleInRadians));
            return quaternion;
        }

        public static Vector3D SkeletonPosToVector3d(SkeletonPoint skelPoint)
        {
            return new Vector3D(skelPoint.X, skelPoint.Y, skelPoint.Z);
        }

        public static void MakeMatrixFromYX(Vector3D xUnnormalized, Vector3D yUnnormalized, ref Matrix3D jointOrientation)
        {
            //matrix columns
            Vector3D xCol;
            Vector3D yCol;
            Vector3D zCol;

            //set up the three different columns to be rearranged and flipped
            xCol = xUnnormalized;
            xCol.Normalize();
            yCol = yUnnormalized;
            yCol.Normalize();
            zCol = Vector3D.CrossProduct(xCol, yCol);
            zCol.Normalize();
            xCol = Vector3D.CrossProduct(yCol, zCol);

            //copy values into matrix
            UpdateMatrix(ref jointOrientation, xCol, yCol, zCol);
        }

        public static void UpdateMatrix(ref Matrix3D matrix, Vector3D xCol, Vector3D yCol, Vector3D zCol)
        {
            SetXColumn(ref matrix, xCol);
            SetYColumn(ref matrix, yCol);
            SetZColumn(ref matrix, zCol);
        }

        public static void SetXColumn(ref Matrix3D matrix, Vector3D vector)
        {
            matrix.M11 = vector.X;
            matrix.M21 = vector.Y;
            matrix.M31 = vector.Z;
        }

        public static Vector3D GetXColumn(Matrix3D matrix)
        {
            return new Vector3D(matrix.M11, matrix.M21, matrix.M31);
        }

        public static void SetYColumn(ref Matrix3D matrix, Vector3D vector)
        {
            matrix.M12 = vector.X;
            matrix.M22 = vector.Y;
            matrix.M32 = vector.Z;
        }

        public static Vector3D GetYColumn(Matrix3D matrix)
        {
            return new Vector3D(matrix.M12, matrix.M22, matrix.M32);
        }

        public static void SetZColumn(ref Matrix3D matrix, Vector3D vector)
        {
            matrix.M13 = vector.X;
            matrix.M23 = vector.Y;
            matrix.M33 = vector.Z;
        }

        public static Vector3D GetZColumn(Matrix3D matrix)
        {
            return new Vector3D(matrix.M13, matrix.M23, matrix.M33);
        }

        /// <summary>
        /// create quaternion as the rotation of two vectors, which should both be normalized
        /// great help from http://www.gamedev.net/topic/613595-quaternion-lookrotationlookat-up/
        /// </summary>
        /// <param name="lookAt"></param>
        /// <param name="upDirection"></param>
        /// <returns></returns>
        public static Quaternion LookRotation(Vector3D lookAt, Vector3D upDirection)
        {
            Quaternion quaternion = new Quaternion();

            Vector3D forward = lookAt;
            Vector3D up = upDirection;

            OrthoNormalize(forward, up);

            Vector3D right = Vector3D.CrossProduct(forward, up);

            quaternion.W = Math.Sqrt(1.0 + right.X + up.Y + forward.Z) * 0.5;
            double w4_recip = 1.0 / (4.0 * quaternion.W);
            quaternion.X = (up.Z - forward.Y) * w4_recip;
            quaternion.Y = (forward.X - right.Z) * w4_recip;
            quaternion.Z = (right.Y - up.X) * w4_recip;

            return quaternion;
        }

        /// <summary>
        /// normalize vector1, make vector2 orthogonal with vector2 and also make it normal
        /// great help from http://www.ituring.com.cn/article/120739
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        public static void OrthoNormalize(Vector3D vector1, Vector3D vector2)
        {
            vector1.Normalize();
            Vector3D thirdVector = Vector3D.CrossProduct(vector1, vector2);
            thirdVector.Normalize();
            vector2 = Vector3D.CrossProduct(vector1, thirdVector);
        }


        /// <summary>
        /// rotate on point by a specified quaternion
        /// </summary>
        /// <param name="pointBefore"></param>
        /// <param name="quaternion"></param>
        /// <param name="pointOut"></param>
        public static void RotatePoint(Point3D pointBefore, Quaternion quaternion, out Point3D pointOut)
        {
            double a = quaternion.W;
            double b = quaternion.X;
            double c = quaternion.Y;
            double d = quaternion.Z;
            double v1 = pointBefore.X;
            double v2 = pointBefore.Y;
            double v3 = pointBefore.Z;

            double t2 = a * b;
            double t3 = a * c;
            double t4 = a * d;
            double t5 = -b * b;
            double t6 = b * c;
            double t7 = b * d;
            double t8 = -c * c;
            double t9 = c * d;
            double t10 = -d * d;
            pointOut = new Point3D(2 * ((t8 + t10) * v1 + (t6 - t4) * v2 + (t3 + t7) * v3) + v1,
                2 * ((t4 + t6) * v1 + (t5 + t10) * v2 + (t9 - t2) * v3) + v2,
                2 * ((t7 - t3) * v1 + (t2 + t9) * v2 + (t5 + t8) * v3) + v3);
        }

        public static Quaternion ConvertToQuaternion(Vector3D rotation)
        {
            Quaternion xRotation = XRotation(AngleToRadius(rotation.X));
            Quaternion yRotation = YRotation(AngleToRadius(rotation.Y));
            Quaternion zRotation = ZRotation(AngleToRadius(rotation.Z));

            return yRotation * (xRotation * zRotation);
        }
    }
}