using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KinectToBVH
{
    public class KinectSkeleton
    {
        // tracking node number of Kinect
        public const int INT_JOINTCOUNT = 20;

        public const int INT_KINECTFRAMERATE = 30;

        // root joint node
        public JointNode HipCenter;

        // store all joint nodes into an array, for the sake of easier access
        public Dictionary<string, JointNode> JointNodes;

        // original offset of each joint
        private Dictionary<string, Point3D> _initialOffsets;

        // original rotation of each joint
        private Dictionary<string, Vector3D> _initialRotations;

        // clibrating angles of each joint
        private Dictionary<string, Vector3D> _clibrationAngles;

        // all bones' lengthes - end joint as the key
        private Dictionary<JointType, Double> _boneLengths = new Dictionary<JointType, Double>();
        
        // track whether all bone original offsets are initialized
        private int _cliberateNum = 0;

        public KinectSkeleton()
        {
            initializeSkeletonStructure();
            initializeClibrateAngles();

            _initialRotations = new Dictionary<string, Vector3D>();
            _initialOffsets = new Dictionary<string, Point3D>();
        }

        // bone offsets should be clibrated at the first second
        public bool NeedBoneClibrated()
        {
            return _cliberateNum <= INT_KINECTFRAMERATE;
        }

        /// <summary>
        /// calculate length of each bone, get each joint's original position based on T pose;
        /// todo: give each bone a rotation as its start position, and calculate the original offset using this rotation and bone length;
        /// </summary>
        /// <param name="skeleton"></param>
        public void CliberateSkeleton(Skeleton skeleton)
        {
            _cliberateNum++; 
            if (NeedBoneClibrated())
            {
                initializeJoints(skeleton);
                //updateBoneLengths(skeleton);
            }
            else
            {
                // set each joint original offset - T pose
                foreach (JointNode node in JointNodes.Values)
                {
                    double nodeOffset = getJointStandardInitialOffset(node);
                    switch (node.BaseAxis)
                    {
                        case Axis.X:
                            node.OriginalOffset = new Point3D(nodeOffset, 0, 0);
                            break;
                        case Axis.nX:
                            node.OriginalOffset = new Point3D(-nodeOffset, 0, 0);
                            break;
                        case Axis.Y:
                            node.OriginalOffset = new Point3D(0, nodeOffset, 0);
                            break;
                        case Axis.nY:
                            node.OriginalOffset = new Point3D(0, -nodeOffset, 0);
                            break;
                        case Axis.Z:
                            node.OriginalOffset = new Point3D(0, 0, nodeOffset);
                            break;
                        case Axis.nZ:
                            node.OriginalOffset = new Point3D(0, 0, -nodeOffset);
                            break;
                        default: // root
                            node.OriginalOffset = new Point3D();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// update all nodes' offsets and rotations with the latest skeleton frame
        /// </summary>
        /// <param name="skeleton"></param>
        public void UpdateJoints(Skeleton skeleton)
        {
            foreach (JointNode node in JointNodes.Values)
            {
                if (NodeTypeEnum.END != node.Type)
                {
                    JointType rotationJoint = getRotationJoint(node);
                    Vector3D rotation;
                    Quaternion rotationQuat;
                    // root joint
                    if (node.Type == NodeTypeEnum.ROOT)
                    {
                        // offset
                        Point3D skeletonOffset = new Point3D(skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z);
                        BVHFile.BVHScalePoint(ref skeletonOffset);
                        node.Offset = new Point3D(skeletonOffset.X - _initialOffsets[node.Name].X,
                            skeletonOffset.Y - _initialOffsets[node.Name].Y + calRealHeightOfHipJoint(),
                            -(skeletonOffset.Z - _initialOffsets[node.Name].Z));                        

                        // rotation
                        rotationQuat = MathHelper.Vector4ToQuaternion(skeleton.BoneOrientations[rotationJoint].AbsoluteRotation.Quaternion);
                        rotation = MathHelper.QuaternionToAxisAngles(rotationQuat);
                        rotation.X = 0;
                        rotation.Y *= -1;
                        rotation.Z = 0;
                    }
                    else  // all other joints
                    {   // bones need no rotation
                        if (!(node.Name == "Spine" || node.Name == "Head"
                            || node.Name == "CollarLeft" || node.Name == "CollarRight" || node.Name == "AnkleLeft" || node.Name == "AnkleRight"))
                        {
                            if (node.Name == "HipCenter2"
                                || node.Name == "ShoulderLeft" || node.Name == "ShoulderRight"
                                || node.Name == "HipLeft" || node.Name == "HipRight")
                            {
                                Vector3D offset = new Vector3D();
                                Vector3D axis;
                                JointType parentJoint = node.JointIndex;
                                switch (node.Name)
                                {
                                    case "HipLeft":
                                        axis = new Vector3D(0, -1, 0);
                                        break;
                                    case "HipRight":
                                        axis = new Vector3D(0, -1, 0);
                                        break;
                                    case "ShoulderLeft":
                                        axis = new Vector3D(-1, 0, 0);
                                        break;
                                    case "ShoulderRight":
                                        axis = new Vector3D(1, 0, 0);
                                        break;
                                    case "HipCenter2":
                                        axis = new Vector3D(0, 1, 0);
                                        rotationJoint = JointType.ShoulderCenter;
                                        parentJoint = JointType.Spine;
                                        break;
                                    default:
                                        axis = new Vector3D();
                                        break;
                                }

                                offset.X = skeleton.Joints[rotationJoint].Position.X - skeleton.Joints[parentJoint].Position.X;
                                offset.Y = skeleton.Joints[rotationJoint].Position.Y - skeleton.Joints[parentJoint].Position.Y;
                                offset.Z = skeleton.Joints[rotationJoint].Position.Z - skeleton.Joints[parentJoint].Position.Z;

                                offset.Normalize();


                                if (node.Name == "ShoulderLeft" || node.Name == "ShoulderRight")
                                {
                                    Vector3D rotationOffset = MathHelper.QuaternionToAxisAngles(
                                        MathHelper.Vector4ToQuaternion(skeleton.BoneOrientations[JointType.ShoulderCenter].AbsoluteRotation.Quaternion));
                                    Matrix3D rotationMatrix = MathHelper.GetRotationMatrix(-rotationOffset.X * Math.PI / 180 - 180, 0, 0);
                                    rotation = Vector3D.Multiply(offset, rotationMatrix);
                                    rotationQuat = MathHelper.GetQuaternion(axis, rotation);
                                    rotation = MathHelper.QuaternionToAxisAngles(rotationQuat);
                                    rotation.Z *= -1;
                                }
                                else if (node.Name == "HipLeft" || node.Name == "HipRight")
                                {
                                    Vector3D rotationOffset = MathHelper.QuaternionToAxisAngles(
                                        MathHelper.Vector4ToQuaternion(skeleton.BoneOrientations[JointType.HipCenter].AbsoluteRotation.Quaternion));
                                    rotation = Vector3D.Multiply(offset, MathHelper.GetRotationMatrixY(-rotationOffset.Y * Math.PI / 180));
                                    rotationQuat = MathHelper.GetQuaternion(axis, rotation);
                                    rotation = MathHelper.QuaternionToAxisAngles(rotationQuat);
                                    rotation.X *= -1;
                                    rotation.Y *= -1;
                                    rotation.Z *= -1;
                                }
                                else
                                {
                                    Vector3D rotationOffset = MathHelper.QuaternionToAxisAngles(
                                        MathHelper.Vector4ToQuaternion(skeleton.BoneOrientations[JointType.HipCenter].AbsoluteRotation.Quaternion));
                                    rotation = Vector3D.Multiply(offset, MathHelper.GetRotationMatrixY(-rotationOffset.Y * Math.PI / 180));
                                    rotationQuat = MathHelper.GetQuaternion(axis, rotation);
                                    rotation = MathHelper.QuaternionToAxisAngles(rotationQuat);
                                    rotation.X *= -1;
                                    rotation.Y = 0;
                                    rotation.Z *= -1;
                                }
                            }
                            else
                            {
                                rotationQuat = MathHelper.Vector4ToQuaternion(skeleton.BoneOrientations[rotationJoint].HierarchicalRotation.Quaternion);
                                rotation = MathHelper.QuaternionToAxisAngles(rotationQuat);

                                if (node.BaseAxis == Axis.nY)
                                {
                                    rotation.X *= -1;
                                    rotation.Y *= -1;
                                }
                                if (node.BaseAxis == Axis.nX)
                                {
                                    Vector3D tempVector = new Vector3D(rotation.X, rotation.Y, rotation.Z);
                                    rotation.X = -tempVector.Z;
                                    rotation.Y = -tempVector.Y;
                                    rotation.Z = -tempVector.X;
                                }
                                if (node.BaseAxis == Axis.X)
                                {
                                    Vector3D tempVector = new Vector3D(rotation.X, rotation.Y, rotation.Z);
                                    rotation.X = tempVector.Z;
                                    rotation.Y = tempVector.Y;
                                    rotation.Z = tempVector.X;
                                }
                            }

                            rotation.X = illegalDoubleFilter(rotation.X);
                            rotation.Y = illegalDoubleFilter(rotation.Y);
                            rotation.Z = illegalDoubleFilter(rotation.Z);
                        }
                        else
                        {
                            rotation = new Vector3D();
                        }
                    }
                    if (_clibrationAngles.ContainsKey(node.Name))
                        node.Rotation = rotation + _clibrationAngles[node.Name];
                    else
                        node.Rotation = rotation;
                }                
            }
        }

        /// <summary>
        /// initialize human skeleton structure
        /// </summary>
        private void initializeSkeletonStructure()
        {
            JointNodes = new Dictionary<string, JointNode>();

            // root
            HipCenter = new JointNode(JointType.HipCenter.ToString(), JointType.HipCenter, null, Axis.None, NodeTypeEnum.ROOT, true);
            JointNodes.Add(HipCenter.Name, HipCenter);

            // hip, spine, shouderCenter, neck, head
            JointNode hipCenter2 = new JointNode("HipCenter2", JointType.HipCenter, HipCenter, Axis.Y, NodeTypeEnum.JOINT, false);
            JointNodes.Add(hipCenter2.Name, hipCenter2);
            JointNode spine = new JointNode(JointType.Spine, hipCenter2, Axis.Y);
            JointNodes.Add(spine.Name, spine);
            JointNode shoulderCenter = new JointNode(JointType.ShoulderCenter, spine, Axis.Y);
            JointNodes.Add(shoulderCenter.Name, shoulderCenter);
            JointNode neck = new JointNode("Neck", JointType.Head, shoulderCenter, Axis.Y, NodeTypeEnum.JOINT, false);
            JointNodes.Add(neck.Name, neck);
            JointNode head = new JointNode(JointType.Head, neck, Axis.Y);
            JointNodes.Add(head.Name, head);
            JointNode headEnd = new JointNode("HeadEnd", JointType.Head, head, Axis.None, NodeTypeEnum.END, false);
            JointNodes.Add(headEnd.Name, headEnd);

            // left arm
            JointNode collarLeft = new JointNode("CollarLeft", JointType.ShoulderLeft, shoulderCenter, Axis.X, NodeTypeEnum.JOINT, false);
            JointNodes.Add(collarLeft.Name, collarLeft);
            JointNode shoulderLeft = new JointNode(JointType.ShoulderLeft, collarLeft, Axis.X);
            JointNodes.Add(shoulderLeft.Name, shoulderLeft);
            JointNode elbowLeft = new JointNode(JointType.ElbowLeft, shoulderLeft, Axis.X);
            JointNodes.Add(elbowLeft.Name, elbowLeft);
            JointNode wristLeft = new JointNode(JointType.WristLeft, elbowLeft, Axis.X);
            JointNodes.Add(wristLeft.Name, wristLeft);
            JointNode handLeft = new JointNode(JointType.HandLeft.ToString(), JointType.HandLeft, wristLeft, Axis.X, NodeTypeEnum.END, true);
            JointNodes.Add(handLeft.Name, handLeft);

            // right arm
            JointNode collarRight = new JointNode("CollarRight", JointType.ShoulderRight, shoulderCenter, Axis.nX, NodeTypeEnum.JOINT, false);
            JointNodes.Add(collarRight.Name, collarRight);
            JointNode shoulderRight = new JointNode(JointType.ShoulderRight, collarRight, Axis.nX);
            JointNodes.Add(shoulderRight.Name, shoulderRight);
            JointNode elbowRight = new JointNode(JointType.ElbowRight, shoulderRight, Axis.nX);
            JointNodes.Add(elbowRight.Name, elbowRight);
            JointNode wristRight = new JointNode(JointType.WristRight, elbowRight, Axis.nX);
            JointNodes.Add(wristRight.Name, wristRight);
            JointNode handRight = new JointNode(JointType.HandRight.ToString(), JointType.HandRight, wristRight, Axis.nX, NodeTypeEnum.END, true);
            JointNodes.Add(handRight.Name, handRight);

            // left lower part
            JointNode hipLeft = new JointNode(JointType.HipLeft, HipCenter, Axis.X);
            JointNodes.Add(hipLeft.Name, hipLeft);
            JointNode KneeLeft = new JointNode(JointType.KneeLeft, hipLeft, Axis.nY);
            JointNodes.Add(KneeLeft.Name, KneeLeft);
            JointNode ankleLeft = new JointNode(JointType.AnkleLeft, KneeLeft, Axis.nY);
            JointNodes.Add(ankleLeft.Name, ankleLeft);
            JointNode footLeft = new JointNode(JointType.FootLeft.ToString(), JointType.FootLeft, ankleLeft, Axis.Z, NodeTypeEnum.END, true);
            JointNodes.Add(footLeft.Name, footLeft);

            // right lower part
            JointNode hipRight = new JointNode(JointType.HipRight, HipCenter, Axis.nX);
            JointNodes.Add(hipRight.Name, hipRight);
            JointNode kneeRight = new JointNode(JointType.KneeRight, hipRight, Axis.nY);
            JointNodes.Add(kneeRight.Name, kneeRight);
            JointNode ankleRight = new JointNode(JointType.AnkleRight, kneeRight, Axis.nY);
            JointNodes.Add(ankleRight.Name, ankleRight);
            JointNode footRight = new JointNode(JointType.FootRight.ToString(), JointType.FootRight, ankleRight, Axis.Z, NodeTypeEnum.END, true);
            JointNodes.Add(footRight.Name, footRight);
        }

        /// <summary>
        /// initialize some joints' angles which should be added to the real rotations each time
        /// </summary>
        private void initializeClibrateAngles()
        {
            _clibrationAngles = new Dictionary<string, Vector3D>();
            _clibrationAngles.Add("HipCenter2", new Vector3D(-30, 0, 0));
            _clibrationAngles.Add(JointType.Spine.ToString(), new Vector3D(30, 0, 0));
            _clibrationAngles.Add("Neck", new Vector3D(-20, 0, 0));
            _clibrationAngles.Add(JointType.ShoulderLeft.ToString(), new Vector3D(30, 0, 0));
            _clibrationAngles.Add(JointType.ShoulderRight.ToString(), new Vector3D(30, 0, 0));
            _clibrationAngles.Add(JointType.HipLeft.ToString(), new Vector3D(-10, 0, 0));
            _clibrationAngles.Add(JointType.HipRight.ToString(), new Vector3D(-10, 0, 0));
            _clibrationAngles.Add(JointType.KneeLeft.ToString(), new Vector3D(10, 0, 0));
            _clibrationAngles.Add(JointType.KneeRight.ToString(), new Vector3D(10, 0, 0));
        }

        /// <summary>
        /// usning multi-time average to calculate bone length
        /// </summary>
        /// <param name="skeleton"></param>
        private void initializeJoints(Skeleton skeleton)
        {
            Point3D nodeOffset;
            foreach (JointNode node in JointNodes.Values)
            {
                nodeOffset = getNodeOffset(node, skeleton);
                if (!_initialOffsets.ContainsKey(node.Name))
                {
                    _initialOffsets.Add(node.Name, nodeOffset);
                }
                else
                {
                    Point3D offsetUpdate = _initialOffsets[node.Name];
                    offsetUpdate.X = (offsetUpdate.X + nodeOffset.X) / 2;
                    offsetUpdate.Y = (offsetUpdate.Y + nodeOffset.Y) / 2;
                    offsetUpdate.Z = (offsetUpdate.Z + nodeOffset.Z) / 2;

                    _initialOffsets[node.Name] = offsetUpdate;
                }
            }
            updateBoneLengths(skeleton);
        }

        // get one joint's offset in kinect skeleton frame
        private Point3D getNodeOffset(JointNode node, Skeleton skeleton)
        {
            if (!node.IsKinectJoint)
                return new Point3D();

            Point3D offset;
            if (node.Type == NodeTypeEnum.ROOT)
                offset = new Point3D(skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z);
            else
            {
                JointNode parent = node.Parent;
                while (null != parent && !parent.IsKinectJoint)
                    parent = parent.Parent;
                offset = new Point3D(skeleton.Joints[node.JointIndex].Position.X - skeleton.Joints[parent.JointIndex].Position.X,
                    skeleton.Joints[node.JointIndex].Position.Y - skeleton.Joints[parent.JointIndex].Position.Y,
                    skeleton.Joints[node.JointIndex].Position.Z - skeleton.Joints[parent.JointIndex].Position.Z);
            }
            BVHFile.BVHScalePoint(ref offset);
            return offset;
        }

        /// <summary>
        /// get a bone's original offset based on its own offset and the symmetric node's offset if it has
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private double getJointStandardInitialOffset(JointNode node)
        {
            double offsetLength = getMaxAxisFromPoint(_initialOffsets[node.Name]);
            if (getSymmetricNode(node) == node)
                return offsetLength;
            else
            {
                double symmetricNodeOffsetLength = getMaxAxisFromPoint(_initialOffsets[getSymmetricNode(node).Name]);
                return (offsetLength + symmetricNodeOffsetLength) / 2;
            }
        }

        private JointNode getSymmetricNode(JointNode node)
        {
            if (node.Name.Contains("Left"))
            {
                return JointNodes[node.Name.Replace("Left", "Right")];
            }
            else if (node.Name.Contains("Right"))
            {
                return JointNodes[node.Name.Replace("Right", "Left")];
            }
            else
            {
                return node;
            }
        }

        private double getMaxAxisFromPoint(Point3D point)
        {
            return Math.Max(Math.Max(Math.Abs(point.X),
                        Math.Abs(point.Y)), Math.Abs(point.Z));
        }
        
        /// <summary>
        /// one joint's rotation is actually stored in its parent joint, so for a given joint, we should get its child joint's rotation
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        private JointType getRotationJoint(JointNode node)
        {
            switch (node.Name)
            {
                case "ShoulderLeft":
                    return JointType.ElbowLeft;
                case "ElbowLeft":
                    return JointType.WristLeft;
                case "WristLeft":
                    return JointType.HandLeft;
                case "ShoulderRight":
                    return JointType.ElbowRight;
                case "ElbowRight":
                    return JointType.WristRight;
                case "WristRight":
                    return JointType.HandRight;
                case "HipLeft":
                    return JointType.KneeLeft;
                case "KneeLeft":
                    return JointType.AnkleLeft;
                case "AnkleLeft":
                    return JointType.FootLeft;
                case "HipRight":
                    return JointType.KneeRight;
                case "KneeRight":
                    return JointType.AnkleRight;
                case "AnkleRight":
                    return JointType.FootRight;
                default:
                //case "HipCenter":
                //case "HipCenter2":
                //case "Spine":
                //case "ShoulderCenter":
                //case "Neck":
                //case "Head":
                //case "CollarLeft":
                //case "CollarRight":
                    return node.JointIndex;
            }
        }

        /// <summary>
        /// add all bones' lengths from foot to hip to calculate hip height
        /// </summary>
        /// <returns></returns>
        private double calRealHeightOfHipJoint()
        {
            double hipHeight = 0;
            // lengh of hipcenter-upleg
            hipHeight += _boneLengths[JointType.HipLeft];
            // lengh of upleg
            hipHeight += _boneLengths[JointType.KneeLeft];
            // lengh of leg
            hipHeight += _boneLengths[JointType.AnkleLeft];
            // lengh of ankle-foot
            hipHeight += _boneLengths[JointType.FootLeft];

            return hipHeight;
        }

        /// <summary>
        /// calcualte the average
        /// </summary>
        /// <param name="skeleton"></param>
        private void updateBoneLengths(Skeleton skeleton)
        {
            // hip to spine
            calCenterBoneLength(skeleton, JointNodes[JointType.Spine.ToString()]);
            // spine to shoulder
            calCenterBoneLength(skeleton, JointNodes[JointType.ShoulderCenter.ToString()]);
            // shoulder to head
            calCenterBoneLength(skeleton, JointNodes[JointType.Head.ToString()]);
            // shoulder to up arm
            calSideBoneLength(skeleton, JointNodes[JointType.ShoulderLeft.ToString()]);
            // up arm to elbow
            calSideBoneLength(skeleton, JointNodes[JointType.ElbowLeft.ToString()]);
            // elbow to wrist
            calSideBoneLength(skeleton, JointNodes[JointType.WristLeft.ToString()]);
            // wrist to hand
            calSideBoneLength(skeleton, JointNodes[JointType.HandLeft.ToString()]);
            // hip to up leg 
            calSideBoneLength(skeleton, JointNodes[JointType.HipLeft.ToString()]);
            // up leg to knee
            calSideBoneLength(skeleton, JointNodes[JointType.KneeLeft.ToString()]);
            // knee to ankle
            calSideBoneLength(skeleton, JointNodes[JointType.AnkleLeft.ToString()]);
            // ankle to foot 
            calSideBoneLength(skeleton, JointNodes[JointType.FootLeft.ToString()]);
        }

        /// <summary>
        /// length of hip center - spine - shoulder center - head
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="bone"></param>
        /// <param name="startJoint"></param>
        /// <param name="endJoint"></param>
        private void calCenterBoneLength(Skeleton skeleton, JointNode node)
        {
            double length = BVHFile.BVHScaledValue(calDescartesLength(skeleton.Joints[node.Parent.JointIndex].Position,
                skeleton.Joints[node.JointIndex].Position));
            if (_boneLengths.ContainsKey(node.JointIndex))
            {
                _boneLengths[node.JointIndex] = (length + _boneLengths[node.JointIndex]) / 2;
            }
            else
            {
                _boneLengths.Add(node.JointIndex, length);
            }
        }

        /// <summary>
        /// length of shoulder center - shoulder - elbow - wrist - hand, hip center - hip - knee - ankle - foot
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="bone"></param>
        /// <param name="startJoint"></param>
        /// <param name="endJoint"></param>
        private void calSideBoneLength(Skeleton skeleton, JointNode node)
        {
            double leftLength = BVHFile.BVHScaledValue(calDescartesLength(skeleton.Joints[node.Parent.JointIndex].Position,
                skeleton.Joints[node.JointIndex].Position));
            double rightLength = BVHFile.BVHScaledValue(calDescartesLength(skeleton.Joints[GetSymmetricJoint(node.Parent.JointIndex)].Position,
                skeleton.Joints[GetSymmetricJoint(node.JointIndex)].Position));
            double length = (leftLength + rightLength) / 2;
            if (_boneLengths.ContainsKey(node.JointIndex))
            {
                _boneLengths[node.JointIndex] = (length + _boneLengths[node.JointIndex]) / 2;
            }
            else
            {
                _boneLengths.Add(node.JointIndex, length);
            }
        }

        /// <summary>
        /// get symmetric joint node for a given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private JointType GetSymmetricJoint(JointType joint)
        {
            switch (joint)
            {
                case JointType.HipLeft:
                    return JointType.HipRight;
                case JointType.KneeLeft:
                    return JointType.KneeRight;
                case JointType.AnkleLeft:
                    return JointType.AnkleRight;
                case JointType.FootLeft:
                    return JointType.FootRight;
                case JointType.ShoulderLeft:
                    return JointType.ShoulderRight;
                case JointType.ElbowLeft:
                    return JointType.ElbowRight;
                case JointType.WristLeft:
                    return JointType.WristRight;
                case JointType.HandLeft:
                    return JointType.HandRight;

                case JointType.HipRight:
                    return JointType.HipLeft;
                case JointType.KneeRight:
                    return JointType.KneeLeft;
                case JointType.AnkleRight:
                    return JointType.AnkleLeft;
                case JointType.FootRight:
                    return JointType.FootLeft;
                case JointType.ShoulderRight:
                    return JointType.ShoulderLeft;
                case JointType.ElbowRight:
                    return JointType.ElbowLeft;
                case JointType.WristRight:
                    return JointType.WristLeft;
                case JointType.HandRight:
                    return JointType.HandLeft;

                default:
                    return joint;
            }
        }

        /// <summary>
        /// calculate Descartes distance for two 3D points
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private double calDescartesLength(SkeletonPoint point1, SkeletonPoint point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2)
                + Math.Pow(point2.Y - point1.Y, 2)
                + Math.Pow(point2.Z - point1.Z, 2));
        }

        private double illegalDoubleFilter(double num)
        {
            if (double.IsNaN(num))
                return 0;
            if (num > 180)
            {
                num -= 360;
            }
            else if (num < -180)
            {
                num += 360;
            }
            return num;
        }
    }
}
