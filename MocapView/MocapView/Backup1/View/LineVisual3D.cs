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
    public class LineVisual3D : Visual3D {
        public Point3D Start;
        public Point3D End;

        private Line _visualObject = new Line();
        public LineVisual3D(Point3D start, Point3D end, SolidColorBrush brush) {
            Start = start;
            End = end;
            _visualObject.Stroke = brush;
            _visualObject.StrokeThickness = 2;
        }

        public override FrameworkElement VisualObject {
            get { return _visualObject; }
        }

        public override void UpdateOnScreen() {
            _visualObject.X1 = Start.ScreenCoords.X;
            _visualObject.Y1 = Start.ScreenCoords.Y;
            _visualObject.X2 = End.ScreenCoords.X;
            _visualObject.Y2 = End.ScreenCoords.Y;
        }

        public override void Project(Matrix3D matrix) {
            Start.ScreenCoords = matrix.Project(Start);
            End.ScreenCoords = matrix.Project(End);
        }
    }
}
