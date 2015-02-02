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
    public abstract class Camera {
        protected Camera() {
        }

        /// <summary>
        /// Returns the overall transform that the camera applied to world points
        /// </summary>
        internal abstract Matrix3D Value {
            get;
        }

        /// <summary>
        /// Indicates that the camera should re-evaluate its internal state
        /// </summary>
        internal abstract void Invalidate();
    }
}
