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
using System.Collections.Generic;
using MocapView.View;

namespace MocapView.Mocap {
    /// <summary>
    /// The basic motion capture node - e.g. an End Site, Joint, or root
    /// </summary>
    public class Node {
        public Point3D OriginalOffset = new Point3D(0, 0, 0);

        /// <summary>
        /// X offset of the node compared to its parent node
        /// </summary>
        public Point3D Offset = new Point3D(0, 0, 0);

        /// <summary>
        /// The start point of the current line/joint in world coordinates
        /// </summary>
        public Point3D StartPointWorld = new Point3D(0, 0, 0);

        /// <summary>
        /// Parent node of the current node
        /// </summary>
        public Node Parent;

        /// <summary>
        /// Chlidren of this node or empty list if this is an end site node
        /// </summary>
        public List<Node> Children = new List<Node>();

        /// <summary>
        /// Node name (e.g. "Hips") or empty string if the node is unnamed
        /// </summary>
        public string Name = String.Empty;

        /// <summary>
        /// Index for the X rotation component in the motion data
        /// </summary>
        public int XRotationIndex = -1;

        /// <summary>
        /// Index for the Y rotation component in the motion data
        /// </summary>
        public int YRotationIndex = -1;

        /// <summary>
        /// Index for the Z rotation component in the motion data
        /// </summary>
        public int ZRotationIndex = -1;

        /// <summary>
        /// Calculates the updated offsets for a given frame
        /// </summary>
        /// <param name="frame"></param>
        public void SetFrame(double[] frame, Quaternion prevRotation) {
            int childCount = Children.Count;

            Quaternion  result = prevRotation;

            /*Matrix3D matrix = QuaternionRotation3D.GetMatrix(result);
            matrix.TransformTo(OriginalOffset, Offset);*/

            result.RotateTo(OriginalOffset, Offset);
            if (childCount != 0) {
                Quaternion rotX = Quaternion.XRotation(frame[XRotationIndex]);
                Quaternion rotY = Quaternion.YRotation(frame[YRotationIndex]);
                Quaternion rotZ = Quaternion.ZRotation(frame[ZRotationIndex]);
                /*if (Name == "RightKnee") {
                    rotX = Quaternion.Identity;
                    rotY = Quaternion.Identity;
                    rotZ = Quaternion.Identity;
                }*/
                result = prevRotation * rotZ * rotX * rotY;
            }
/*            if (Parent != null) {
                Quaternion rotX = Quaternion.XRotation(frame[Parent.XRotationIndex]);
                Quaternion rotY = Quaternion.YRotation(frame[Parent.YRotationIndex]);
                Quaternion rotZ = Quaternion.ZRotation(frame[Parent.ZRotationIndex]);
                result = prevRotation * rotZ * rotX * rotY;

            }
            else {
                result = prevRotation;
            }*/


            if (Parent == null) { // if root node
                StartPointWorld.X = frame[0] + Offset.X;
                StartPointWorld.Y = frame[1] + Offset.Y;
                StartPointWorld.Z = frame[2] + Offset.Z;
            }
            else 
            {
                StartPointWorld.X = Parent.StartPointWorld.X + Offset.X;
                StartPointWorld.Y = Parent.StartPointWorld.Y + Offset.Y;
                StartPointWorld.Z = Parent.StartPointWorld.Z + Offset.Z;
            }

            for (int i = 0; i < childCount; i++) 
            {
                Children[i].SetFrame(frame, result);
            }
        }

    }
}
