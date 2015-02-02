using Microsoft.Kinect;
using System.Collections;
using System.Windows.Media.Media3D;

namespace KinectToBVH
{
    public class JointNode
    {
        // root || joint || end
        public string Name;
        public NodeTypeEnum Type;
        public JointType JointIndex;
        public JointNode Parent = null;
        public ArrayList Children;
        public bool IsKinectJoint;

        // offset based on parent node
        public Point3D OriginalOffset;  // standard pose
        public Point3D Offset = new Point3D(0, 0, 0);
        // rotation relation to parent node
        public Vector3D Rotation = new Vector3D(0, 0, 0);
        
        // orientation of joint tranformation
        public Axis BaseAxis;

        public JointNode(JointType index, JointNode parent, Axis axis, bool isKinectJoint = true):
            this(index.ToString(), index, parent, axis, NodeTypeEnum.JOINT, true){ }

        public JointNode(string name, JointType index, JointNode parent, Axis axis, NodeTypeEnum type, bool isKinectJoint)
        {
            this.IsKinectJoint = isKinectJoint;
            this.JointIndex = index;
            Name = isKinectJoint ? index.ToString() : name;
            this.BaseAxis = axis;
            this.Type = type;
            if(Type != NodeTypeEnum.END)
                Children = new ArrayList();
            if (null != parent)
            {
                this.Parent = parent;
                parent.AddChild(this);
            }
        }

        public bool AddChild(JointNode node)
        {
            return Children.Add(node) >= 0;
        }
    }

    public enum NodeTypeEnum
    {
        ROOT,
        JOINT,
        END
    }

    public enum Axis
    {
        None,
        X,
        Y,
        Z,
        nX,
        nY,
        nZ
    }
}
