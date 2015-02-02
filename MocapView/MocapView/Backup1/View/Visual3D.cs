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
    /// <summary>
    /// a simple 3D visual (e.g. line)
    /// </summary>
    public abstract class Visual3D {
        public abstract FrameworkElement VisualObject {
            get;
        }
        public abstract void UpdateOnScreen();
        public abstract void Project(Matrix3D matrix);
    }
}
