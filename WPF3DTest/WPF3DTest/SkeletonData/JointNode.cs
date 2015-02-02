using Microsoft.Kinect;
using System.Collections;
using System.Text;
using System.Windows.Media.Media3D;

namespace WPF3DTest.SkeletonData
{
    public class JointNode
    {
        public JointType JointIndex;

        // root || joint || end
        public NodeTypeEnum Type;

        // offset based on parent node
        public Point3D OriginalOffset;  // standard pose
        public Point3D Offset = new Point3D(0, 0, 0);

        public JointNode Parent = null;
        public ArrayList Children;

        // rotation relation to parent node
        public Vector3D Rotation = new Vector3D(0, 0, 0);

        public TransAxis Axis;

        public JointNode(JointType index, JointNode parent, TransAxis axis):
            this(NodeTypeEnum.JOINT, index, parent, axis)
        { }

        public JointNode(NodeTypeEnum type, JointType index, JointNode parent, TransAxis axis)
        {
            this.JointIndex = index;
            this.Type = type;
            if (NodeTypeEnum.END != Type)
                Children = new ArrayList();
            this.Parent = parent;
            if(parent != null)
                parent.AddChild(this);
            this.Axis = axis;
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
}
