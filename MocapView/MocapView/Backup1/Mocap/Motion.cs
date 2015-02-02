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
using MocapView.View;

namespace MocapView.Mocap {
    /// <summary>
    /// Contains information about a specific motion
    /// </summary>
    public class Motion {
        /// <summary>
        /// Root node describing the skeleton that goes with the data
        /// </summary>
        public Node Skeleton = null;

        /// <summary>
        /// Frames in this animation
        /// </summary>
        public int FrameCount = 0;

        /// <summary>
        /// Channels per frame
        /// </summary>
        public int ChannelCount = 0;

        /// <summary>
        /// Time to display each frame in milliseconds, default 33.3333 (30 FPS)
        /// </summary>
        public double FrameTime = 1000 / 30.0;

        /// <summary>
        /// Current frame number
        /// </summary>
        public double CurrentFrame = -1;

        /// <summary>
        /// List of frames
        /// </summary>
        public double[][] Frames = null;

        public bool AdvanceTime(double msec) {
            if ((msec == 0) || (FrameCount <= 0)) {
                return false;
            }

            CurrentFrame = CurrentFrame + msec / FrameTime;
            while (CurrentFrame > FrameCount) {
                CurrentFrame -= FrameCount;
            }

            Skeleton.SetFrame(Frames[(int)CurrentFrame], Quaternion.Identity);
            return true;
        }

        public void NextFrame() {
            if (FrameCount <= 0) {
                return;
            }
            
            CurrentFrame++;
            if (FrameCount > 0) {
                while (CurrentFrame > FrameCount) {
                    CurrentFrame -= FrameCount;
                }
            }

            Matrix3D identity = new Matrix3D();
            identity.SetIdentity();
            Skeleton.SetFrame(Frames[(int)CurrentFrame], Quaternion.Identity);
        }
    }
}
