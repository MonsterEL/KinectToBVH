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
    public class QuaternionRotation3D : Rotation3D {
        private Quaternion quaternion;

        public QuaternionRotation3D() {
            this.Quaternion = Quaternion.Identity;
        }

        public QuaternionRotation3D(Quaternion quaternion) {
            this.Quaternion = quaternion;
        }

        public Quaternion Quaternion {
            get {
                return this.quaternion;
            }
            set {
                this.quaternion = value;
            }
        }

        public static Matrix3D GetMatrix(Quaternion q) {
            double n1 = 2 * q.Y * q.Y;
            double n2 = 2 * q.Z * q.Z;
            double n3 = 2 * q.X * q.X;
            double n4 = 2 * q.X * q.Y;
            double n5 = 2 * q.W * q.Z;
            double n6 = 2 * q.X * q.Z;
            double n7 = 2 * q.W * q.Y;
            double n8 = 2 * q.Y * q.Z;
            double n9 = 2 * q.W * q.X;

            Matrix3D result = Matrix3D.Identity;
            result.M11 = 1 - n1 - n2;
            result.M12 = n4 + n5;
            result.M13 = n6 - n7;
            result.M21 = n4 - n5;
            result.M22 = 1 - n3 - n2;
            result.M23 = n8 + n9;
            result.M31 = n6 + n7;
            result.M32 = n8 - n9;
            result.M33 = 1 - n3 - n1;
            result.M44 = 1;

            return result;
        }

        public override Matrix3D GetMatrix(Point3D center) {
            Quaternion q = this.Quaternion;
            double n1 = 2 * q.Y * q.Y;
            double n2 = 2 * q.Z * q.Z;
            double n3 = 2 * q.X * q.X;
            double n4 = 2 * q.X * q.Y;
            double n5 = 2 * q.W * q.Z;
            double n6 = 2 * q.X * q.Z;
            double n7 = 2 * q.W * q.Y;
            double n8 = 2 * q.Y * q.Z;
            double n9 = 2 * q.W * q.X;

            Matrix3D result = Matrix3D.Identity;
            result.M11 = 1 - n1 - n2;
            result.M12 = n4 + n5;
            result.M13 = n6 - n7;
            result.M21 = n4 - n5;
            result.M22 = 1 - n3 - n2;
            result.M23 = n8 + n9;
            result.M31 = n6 + n7;
            result.M32 = n8 - n9;
            result.M33 = 1 - n3 - n1;
            result.M44 = 1;

            //If this is not Center=(0,0,0) then have to take that into consideration
            if (center != null) {
                if ((center.X != 0) || (center.Y != 0) || (center.Z != 0)) {
                    result.OffsetX = (((-center.X * result.M11) -
                                       (center.Y * result.M21)) -
                                       (center.Z * result.M31)) + center.X;

                    result.OffsetY = (((-center.X * result.M12) -
                                       (center.Y * result.M22)) -
                                       (center.Z * result.M32)) + center.Y;

                    result.OffsetZ = (((-center.X * result.M13) -
                                       (center.Y * result.M23)) -
                                       (center.Z * result.M33)) + center.Z;
                }
            }

            return result;
        }
    }
}
