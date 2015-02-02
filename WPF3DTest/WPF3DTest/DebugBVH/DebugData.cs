using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using WPF3DTest.MotionWriter;
using WPF3DTest.SkeletonData;

namespace WPF3DTest.DebugBVH
{
    public class DebugData
    {
        public static StreamWriter sw = new StreamWriter("D:/debug.txt");

        private static RenderingSolution _renderingSolution = new RenderingSolution(640, 480, OriginCoordinate.LeftTop);

        public static void UpdateSkeletonForDebug(JointNode node)
        {
            // rotation of a bone is actually the rotation of joint's parent joint
            Quaternion parentRotation;
            if (node.JointIndex == JointType.HipCenter)
                parentRotation = ConvertToQuaternion(node.Rotation);
            else
                parentRotation = ConvertToQuaternion(node.Parent.Rotation);

            RotatePoint(node.OriginalOffset, ConvertToQuaternion(node.Rotation), out node.Offset);

            // update node rotation to absolute
            node.Rotation = KinectDataConverter.QuaternionToAxisAngles(ConvertToQuaternion(node.Rotation) * parentRotation);


            if (NodeTypeEnum.ROOT == node.Type)
            {
                node.Offset.X += _renderingSolution.OriginX;
                node.Offset.Y += _renderingSolution.OriginY;
                node.Offset.Z += 0;
            }
            else
            {
                node.Offset.X += node.Parent.Offset.X;
                node.Offset.Y += node.Parent.Offset.Y;
                node.Offset.Z += node.Parent.Offset.Z;
            }
            if (node.Type == NodeTypeEnum.END)
                return;

            foreach (JointNode child in node.Children)
            {
                UpdateSkeletonForDebug(child);
            }
        }
        
        public static Quaternion ConvertToQuaternion(Vector3D rotation)
        {
            Quaternion xRotation = XRotation(AngleToRadius(rotation.X));
            Quaternion yRotation = YRotation(AngleToRadius(rotation.Y));
            Quaternion zRotation = ZRotation(AngleToRadius(rotation.Z));

            return yRotation*(xRotation*zRotation);
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

        /// <summary>
        /// generate a quaternion for rotating some angle by x-coordinate
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        public static Quaternion XRotation(double angleInRadians)
        {
            double halfAngleInRadians = angleInRadians / 2;
            Quaternion quaternion = new Quaternion();
            quaternion.X = (float)Math.Sin(halfAngleInRadians);
            quaternion.Y = 0;
            quaternion.Z = 0;
            quaternion.W = (float)Math.Cos(halfAngleInRadians);

            return quaternion;
        }

        /// <summary>
        /// generate a quaternion for rotating some angle by y-coordinate
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        public static Quaternion YRotation(double angleInRadians)
        {
            double halfAngleInRadians = angleInRadians / 2;
            Quaternion quaternion = new Quaternion();
            quaternion.X = 0;
            quaternion.Y = (float)Math.Sin(halfAngleInRadians);
            quaternion.Z = 0;
            quaternion.W = (float)Math.Cos(halfAngleInRadians);

            return quaternion;

        }

        /// <summary>
        /// generate a quaternion for rotating some angle by z-coordinate
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        public static Quaternion ZRotation(double angleInRadians)
        {
            double halfAngleInRadians = angleInRadians / 2;
            Quaternion quaternion = new Quaternion();
            quaternion.X = 0;
            quaternion.Y = 0;
            quaternion.Z = (float)Math.Sin(halfAngleInRadians);
            quaternion.W = (float)Math.Cos(halfAngleInRadians);

            return quaternion;
        }

        public static double AngleToRadius(double angle)
        {
            return Math.PI * angle / 180;
        }

        /// <summary>
        /// product of two quaternions
        /// </summary>
        /// <param name="quaternion1"></param>
        /// <param name="quanernion2"></param>
        /// <returns></returns>
        public static Quaternion ProductOfTwoQuaternion(Quaternion quaternion1, Quaternion quanernion2)
        {
            Quaternion product = new Quaternion();
            //Derived from knowing:
            // q1q2 = w1w2 + w1q2 + w2q1 + q1 x q2 - q1.q2
            product.X = quaternion1.W * quanernion2.X + quaternion1.X * quanernion2.W + quaternion1.Y * quanernion2.Z - quaternion1.Z * quanernion2.Y;
            product.Y = quaternion1.W * quanernion2.Y + quaternion1.Y * quanernion2.W + quaternion1.Z * quanernion2.X - quaternion1.X * quanernion2.Z;
            product.Z = quaternion1.W * quanernion2.Z + quaternion1.Z * quanernion2.W + quaternion1.X * quanernion2.Y - quaternion1.Y * quanernion2.X;
            product.W = quaternion1.W * quanernion2.W - quaternion1.X * quanernion2.X - quaternion1.Y * quanernion2.Y - quaternion1.Z * quanernion2.Z;
            return product;
        }

    }


    public class RenderingSolution
    {
        private double _width;
        private double _height;
        private OriginCoordinate _origin;

        // inorder to render skeleton in the middle of the screen, need to give these values to skeleton center
        private double _originX;
        private double _originY;

        public RenderingSolution(double width, double height, OriginCoordinate origin)
        {
            _width = width;
            _height = height;
            _origin = origin;
            updateOrigin();
        }

        public double Width
        {
            get { return _width; }
            set
            {
                this._width = value;
                updateOrigin();
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                this._height = value;
                updateOrigin();
            }
        }

        public OriginCoordinate Origin
        {
            get { return _origin; }
            set
            {
                this._origin = value;
                updateOrigin();
            }
        }

        public double OriginX
        {
            get { return _originX; }
        }

        public double OriginY
        {
            get { return _originY; }
        }

        /// <summary>
        /// update coordinate origin based on rendering width and height
        /// </summary>
        private void updateOrigin()
        {
            switch (_origin)
            {
                case OriginCoordinate.Center:
                    _originX = 0;
                    _originY = 0;
                    break;
                case OriginCoordinate.LeftTop:
                case OriginCoordinate.LeftBottom:
                    _originX = _width / 2;
                    _originY = _height / 2;
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// enum of coordinate origin for screen
    /// </summary>
    public enum OriginCoordinate
    {
        Center,
        LeftTop,
        LeftBottom
    }
}
