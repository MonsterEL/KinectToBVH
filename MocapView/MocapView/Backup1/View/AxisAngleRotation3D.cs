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
    public class NormalizedAxisAngleRotation3D : Rotation3D {
        public Vector3D NormalizedAxis = new Vector3D(0, 1, 0);
        public double AngleRad = 0;

        public NormalizedAxisAngleRotation3D() {
        }

        public NormalizedAxisAngleRotation3D(Vector3D normalizedAxis, double angleInRadians) {
            this.NormalizedAxis = normalizedAxis;
            this.AngleRad = angleInRadians;
        }

        public override Matrix3D GetMatrix(Point3D center) {
            //Calculations from Essential Mathematics for games & interactive applications
            //James M. Van Verth & Lars M. Bishop

            Vector3D a = NormalizedAxis;

            //TODO: Internally use a Quaternion since it is more efficient to calculate

            //TODO: Look into caching all this
            double angle = AngleRad;
            double t = 1.0 - System.Math.Cos(angle);
            double c = System.Math.Cos(angle);
            double s = System.Math.Sin(angle);
            double x = a.X;
            double y = a.Y;
            double z = a.Z;

            //TODO: Optimize, do not multiple matrices, just expand out terms
            //Matrix3D m = new Matrix3D(1, 0, 0, 0,
            //                          0, 1, 0, 0,
            //                          0, 0, 1, 0,
            //                          -center.X, -center.Y, -center.Z, 1);

            //m.Append(
            Matrix3D m = new Matrix3D(t * x * x + c, t * x * y + s * z, t * x * z - s * y, 0,
                              t * x * y - s * z, t * y * y + c, t * y * z + s * x, 0,
                              t * x * z + s * y, t * y * z - s * x, t * z * z + c, 0,
                              0, 0, 0, 1);

            //m.Append(new Matrix3D(1, 0, 0, 0,
            //                      0, 1, 0, 0,
            //                      0, 0, 1, 0,
            //                      center.X, center.Y, center.Z, 1));

            return m;
        }

    }

    // TODO: !!! optimize these 

/*    public class XAxisRotation3D : QuaternionRotation3D {
        public XAxisRotation3D(double angleInRadians) : base(new Quaternion(new Vector3D(1,0,0), angleInRadians)) {
        }
    }

    public class YAxisRotation3D : QuaternionRotation3D {
        public YAxisRotation3D(double angleInRadians) : base(new Quaternion(new Vector3D(0,1,0), angleInRadians)) {
        }
    }
    public class ZAxisRotation3D : QuaternionRotation3D {
        public ZAxisRotation3D(double angleInRadians) : base(new Quaternion(new Vector3D(0,0,1), angleInRadians)) {
        }
    }*/

}
