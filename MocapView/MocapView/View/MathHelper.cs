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
    public static class MathHelper {
        public const double Epsilon = 0.00001f;

        public const double OneDivThree = 1.0 / 3;

        public static bool IsZero(double value) {
            return System.Math.Abs(value) < Epsilon;
        }

        public static bool AreEqual(double x, double y) {
            return IsZero(x - y);
        }

        public static double SquareRoot(double value) {
            return System.Math.Sqrt(value);
        }

        public static double InverseSquareRoot(double value) {
            return (1.0 / System.Math.Sqrt(value));
        }
    }
}
