using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using WPF3DTest.DebugBVH;
using WPF3DTest.MotionWriter;

namespace WPF3DTest.SkeletonData
{
    public class StructuredSkeleton
    {
        // tracking node number of Kinect
        public const int INT_JOINTCOUNT = 20;

        public const int INT_KINECTFRAMERATE = 30;

        // root joint node
        public JointNode hipRoot;

        // store all joint nodes into an array, for the sake of easier access
        public JointNode[] JointNodes;

        // original offset of each joint
        private Point3D[] _initialOffsets;

        // original rotation of each joint
        private Quaternion[] _initialRotations;

        // clibrating angles of each joint
        private Vector3D[] _clibrationAngles;

        // all bones' lengthes
        private Dictionary<HummanBoneEnum, Double> _boneLengths = new Dictionary<HummanBoneEnum, double>();

        // track whether all bone original offsets are initialized
        private int _cliberateNum = 0;

        // start position of the skeleton
        private Point3D _startPoint = new Point3D();
        
        public Point3D[] Positions_Debug = new Point3D[20];

        // local writer
        public StreamWriter sw = new StreamWriter("D:/rotations.txt");

        public StructuredSkeleton()
        {
            initializeSkeletonStructure();
        }

        // bone offsets should be clibrated at the first second
        public bool NeedBoneClibrated()
        {
            return _cliberateNum <= INT_KINECTFRAMERATE;
        }

        // clear the clibrate state if needed
        public void ClearBoneClibration()
        {
            _cliberateNum = 0;
        }

        /// <summary>
        /// initialize human skeleton structure
        /// </summary>
        private void initializeSkeletonStructure()
        {
            JointNodes = new JointNode[INT_JOINTCOUNT];
            
            // root
            hipRoot = new JointNode(NodeTypeEnum.ROOT, JointType.HipCenter, null, TransAxis.None);
            JointNodes[(int)JointType.HipCenter] = hipRoot;

            // spine, neck, head
            JointNode spine = new JointNode(JointType.Spine, hipRoot, TransAxis.Y);
            JointNodes[(int)JointType.Spine] = spine;
            JointNode shouderCenter = new JointNode(JointType.ShoulderCenter, spine, TransAxis.Y);
            JointNodes[(int)JointType.ShoulderCenter] = shouderCenter;
            JointNode head = new JointNode(JointType.Head, shouderCenter, TransAxis.Y);
            JointNodes[(int)JointType.Head] = head;
            JointNode headEnd = new JointNode(NodeTypeEnum.END, JointType.Head, head, TransAxis.None);

            // left arm
            JointNode leftShoulder = new JointNode(JointType.ShoulderLeft, shouderCenter, TransAxis.X);
            JointNodes[(int)JointType.ShoulderLeft] = leftShoulder;
            JointNode leftElbow = new JointNode(JointType.ElbowLeft, leftShoulder, TransAxis.X);
            JointNodes[(int)JointType.ElbowLeft] = leftElbow;
            JointNode leftWrist = new JointNode(JointType.WristLeft, leftElbow, TransAxis.X);
            JointNodes[(int)JointType.WristLeft] = leftWrist;
            JointNode leftHand = new JointNode(NodeTypeEnum.END, JointType.HandLeft, leftWrist, TransAxis.X);
            JointNodes[(int)JointType.HandLeft] = leftHand;

            // right arm
            JointNode rightShoulder = new JointNode(JointType.ShoulderRight, shouderCenter, TransAxis.nX);
            JointNodes[(int)JointType.ShoulderRight] = rightShoulder;
            JointNode rightElbow = new JointNode(JointType.ElbowRight, rightShoulder, TransAxis.nX);
            JointNodes[(int)JointType.ElbowRight] = rightElbow;
            JointNode rightWrist = new JointNode(JointType.WristRight, rightElbow, TransAxis.nX);
            JointNodes[(int)JointType.WristRight] = rightWrist;
            JointNode rightHand = new JointNode(NodeTypeEnum.END, JointType.HandRight, rightWrist, TransAxis.nX);
            JointNodes[(int)JointType.HandRight] = rightHand;

            // left lower part
            JointNode leftUpLeg = new JointNode(JointType.HipLeft, hipRoot, TransAxis.X);
            JointNodes[(int)JointType.HipLeft] = leftUpLeg;
            JointNode leftKnee = new JointNode(JointType.KneeLeft, leftUpLeg, TransAxis.nY);
            JointNodes[(int)JointType.KneeLeft] = leftKnee;
            JointNode leftAnkle = new JointNode(JointType.AnkleLeft, leftKnee, TransAxis.nY);
            JointNodes[(int)JointType.AnkleLeft] = leftAnkle;
            JointNode leftFoot = new JointNode(NodeTypeEnum.END, JointType.FootLeft, leftAnkle, TransAxis.Z);
            JointNodes[(int)JointType.FootLeft] = leftFoot;

            // right lower part
            JointNode rightUpLeg = new JointNode(JointType.HipRight, hipRoot, TransAxis.nX);
            JointNodes[(int)JointType.HipRight] = rightUpLeg;
            JointNode rightKnee = new JointNode(JointType.KneeRight, rightUpLeg, TransAxis.nY);
            JointNodes[(int)JointType.KneeRight] = rightKnee;
            JointNode rightAnkle = new JointNode(JointType.AnkleRight, rightKnee, TransAxis.nY);
            JointNodes[(int)JointType.AnkleRight] = rightAnkle;
            JointNode rightFoot = new JointNode(NodeTypeEnum.END, JointType.FootRight, rightAnkle, TransAxis.Z);
            JointNodes[(int)JointType.FootRight] = rightFoot;
        }

        /// <summary>
        /// get a specified joine node in the skeleton structure
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public JointNode GetJointNode(JointType joint)
        {
            return getJointNode(hipRoot, joint);
        }

        /// <summary>
        /// recurse search the skeleton structure to find the joint node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="joint"></param>
        /// <returns></returns>
        private JointNode getJointNode(JointNode root, JointType joint)
        {
            if (joint == root.JointIndex)
            {
                return root;
            }
            else
            {
                if (root == null || NodeTypeEnum.END == root.Type)
                {
                    return null;
                }
                else
                {
                    foreach (JointNode node in root.Children)
                    {
                        JointNode tempNode = getJointNode(node, joint);
                        if (tempNode == null)
                            continue;
                        else
                            return tempNode;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// get symmetric joint node for a given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public JointType GetSymmetricJoint(JointType joint)
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
        /// calculate length of each bone, get each joint's original position based on T pose;
        /// todo: give each bone a rotation as its start position, and calculate the original offset using this rotation and bone length;
        /// </summary>
        /// <param name="skeleton"></param>
        public void CliberateSkeleton(Skeleton skeleton)
        {
            _cliberateNum++; 
            if (NeedBoneClibrated())
            {
                //clibrateStartPosition(skeleton);
                updateBoneLengths(skeleton);
                initializeJoints(skeleton);
            }
            else
            {
                // set each joint original offset - T pose
                foreach (JointNode node in JointNodes)
                {
                    switch (node.JointIndex)
                    {
                        case JointType.HipCenter:
                            //node.OriginalOffset = new Point3D(BVHWriter.BVHScaledValue(_initialOffsets[(int)node.JointIndex].X),
                            //    BVHWriter.BVHScaledValue(_initialOffsets[(int)node.JointIndex].Y + calRealHeightOfHipJoint()), BVHWriter.BVHScaledValue(_initialOffsets[(int)node.JointIndex].Z));
                            node.OriginalOffset = new Point3D();
                            break;
                        default:
                            node.OriginalOffset = new Point3D(BVHWriter.BVHScaledValue(_initialOffsets[(int)node.JointIndex].X),
                                BVHWriter.BVHScaledValue(_initialOffsets[(int)node.JointIndex].Y), BVHWriter.BVHScaledValue(_initialOffsets[(int)node.JointIndex].Z));
                            node.OriginalOffset.X *= 2;
                            node.OriginalOffset.Y *= 2;
                            node.OriginalOffset.Z *= 2;
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
            //StringBuilder sb = new StringBuilder();
            //foreach (JointType joint in Enum.GetValues(typeof(JointType)))
            //{
            //    sb.Append(joint.ToString() + ", Start: " + skeleton.BoneOrientations[joint].StartJoint.ToString()
            //        + ", End : " + skeleton.BoneOrientations[joint].EndJoint.ToString()
            //        + " Rotation: " + getrotationString(skeleton.BoneOrientations[joint].HierarchicalRotation) + "\n");
            //}

            //MessageBox.Show(sb.ToString());


            /******* using position to calculate orientation ********/
            //Vector3D vectorX, vectorY, vectorZ;
            //if (node.JointIndex == JointType.HipCenter)
            //{
            //    vectorY = KinectDataConverter.SkeletonPosToVector3d(skeleton.Joints[JointType.Spine].Position)
            //        - KinectDataConverter.SkeletonPosToVector3d(skeleton.Joints[JointType.HipCenter].Position);
            //    vectorX = KinectDataConverter.SkeletonPosToVector3d(skeleton.Joints[JointType.HipLeft].Position)
            //        - KinectDataConverter.SkeletonPosToVector3d(skeleton.Joints[JointType.HipRight].Position);
            //    Matrix3D rotationMatrix = new Matrix3D();
            //    KinectDataConverter.MakeMatrixFromYX(vectorX, vectorY, ref rotationMatrix);
            //    Quaternion rotationQuaternion = KinectDataConverter.LookRotation(
            //        KinectDataConverter.GetZColumn(rotationMatrix), KinectDataConverter.GetYColumn(rotationMatrix));
            //}
            /******* using position to calculate orientation ********/

            foreach (JointNode node in JointNodes)
            {
                JointType rotationJoint = getRotationJoint(node);
                Quaternion rotation = new Quaternion();
                if (node.Type == NodeTypeEnum.ROOT)
                {
                    node.Offset.X = BVHWriter.BVHScaledValue(skeleton.Joints[node.JointIndex].Position.X - _initialOffsets[(int)node.JointIndex].X);
                    node.Offset.Y = BVHWriter.BVHScaledValue(skeleton.Joints[node.JointIndex].Position.Y - _initialOffsets[(int)node.JointIndex].Y + calRealHeightOfHipJoint());
                    node.Offset.Z = BVHWriter.BVHScaledValue(skeleton.Joints[node.JointIndex].Position.Z - _initialOffsets[(int)node.JointIndex].Z);

                    rotation = KinectDataConverter.Vector4ToQuaternion(skeleton.BoneOrientations[node.JointIndex].AbsoluteRotation.Quaternion);

                    Quaternion initialQuat = _initialRotations[(int)node.JointIndex];
                    initialQuat.Invert();
                    rotation = initialQuat * rotation;
                    node.Rotation = KinectDataConverter.QuaternionToAxisAngles(rotation);
                }
                else
                {
                    rotation = KinectDataConverter.Vector4ToQuaternion(skeleton.BoneOrientations[rotationJoint].HierarchicalRotation.Quaternion);
                    Quaternion initialQuat = _initialRotations[(int)node.JointIndex];
                    initialQuat.Invert();
                    rotation = initialQuat * rotation;
                    node.Rotation = KinectDataConverter.QuaternionToAxisAngles(rotation);

                    //if (node.Axis == TransAxis.X || node.Axis == TransAxis.nX)
                    //{
                    //    Vector3D tempRotation = node.Rotation;
                    //    node.Rotation.X = tempRotation.Z;
                    //    node.Rotation.Z = tempRotation.X;
                    //}
                }
            }                
        }

        /// <summary>
        /// one joint's rotation is actually stored in its parent joint, so for a given joint, we should get its child joint's rotation
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        private JointType getRotationJoint(JointNode node)
        {
            switch (node.JointIndex)
            {
                case JointType.ShoulderLeft:
                    return JointType.ElbowLeft;
                case JointType.ElbowLeft:
                    return JointType.WristLeft;
                case JointType.WristLeft:
                    return JointType.HandLeft;
                case JointType.ShoulderRight:
                    return JointType.ElbowRight;
                case JointType.ElbowRight:
                    return JointType.WristRight;
                case JointType.WristRight:
                    return JointType.HandRight;
                case JointType.HipLeft:
                    return JointType.KneeLeft;
                case JointType.KneeLeft:
                    return JointType.AnkleLeft;
                case JointType.AnkleLeft:
                    return JointType.FootLeft;
                case JointType.HipRight:
                    return JointType.KneeRight;
                case JointType.KneeRight:
                    return JointType.AnkleRight;
                case JointType.AnkleRight:
                    return JointType.FootRight;
                default:
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
            hipHeight += _boneLengths[HummanBoneEnum.HipToUpLeg];
            // lengh of upleg
            hipHeight += _boneLengths[HummanBoneEnum.UpLegToKnee];
            // lengh of leg
            hipHeight += _boneLengths[HummanBoneEnum.KneeToAnkle];
            // lengh of ankle-foot
            hipHeight += _boneLengths[HummanBoneEnum.AnkleToFoot];

            return hipHeight;
        }

        private void clibrateStartPosition(Skeleton skeleton)
        {
            if (0 == _startPoint.X && 0 == _startPoint.Y && 0 == _startPoint.Z)
            {
                _startPoint.X = skeleton.Joints[JointType.HipCenter].Position.X;
                _startPoint.Y = skeleton.Joints[JointType.HipCenter].Position.Y;
                _startPoint.Z = skeleton.Joints[JointType.HipCenter].Position.Z;
            }
            else
            {
                _startPoint.X = (_startPoint.X + skeleton.Joints[JointType.HipCenter].Position.X) / 2;
                _startPoint.Y = (_startPoint.Y + skeleton.Joints[JointType.HipCenter].Position.Y) / 2;
                _startPoint.Z = (_startPoint.Z + skeleton.Joints[JointType.HipCenter].Position.Z) / 2;
            }
        }

        private void initializeJoints(Skeleton skeleton)
        {
            foreach (JointNode node in JointNodes)
            {
                if (_cliberateNum == 1)
                {
                    _initialRotations = new Quaternion[INT_JOINTCOUNT];
                    _initialOffsets = new Point3D[INT_JOINTCOUNT];
                    if (node.JointIndex == JointType.HipCenter)
                    {
                        _initialOffsets[(int)node.JointIndex].X = skeleton.Joints[node.JointIndex].Position.X;
                        _initialOffsets[(int)node.JointIndex].Y = skeleton.Joints[node.JointIndex].Position.Y;
                        _initialOffsets[(int)node.JointIndex].Z = skeleton.Joints[node.JointIndex].Position.Z;

                        _initialRotations[(int)node.JointIndex] = KinectDataConverter.Vector4ToQuaternion(skeleton.BoneOrientations[node.JointIndex].AbsoluteRotation.Quaternion);
                    }
                    else
                    {
                        _initialOffsets[(int)node.JointIndex].X = skeleton.Joints[node.JointIndex].Position.X - skeleton.Joints[node.Parent.JointIndex].Position.X;
                        _initialOffsets[(int)node.JointIndex].Y = skeleton.Joints[node.JointIndex].Position.Y - skeleton.Joints[node.Parent.JointIndex].Position.Y;
                        _initialOffsets[(int)node.JointIndex].Z = skeleton.Joints[node.JointIndex].Position.Z - skeleton.Joints[node.Parent.JointIndex].Position.Z;
                        _initialRotations[(int)node.JointIndex] = KinectDataConverter.Vector4ToQuaternion(skeleton.BoneOrientations[getRotationJoint(node)].HierarchicalRotation.Quaternion);
                    }
                }
                else
                {
                    if (node.JointIndex == JointType.HipCenter)
                    {
                        _initialOffsets[(int)node.JointIndex].X = (_initialOffsets[(int)node.JointIndex].X + skeleton.Joints[node.JointIndex].Position.X) / 2;
                        _initialOffsets[(int)node.JointIndex].Y = (_initialOffsets[(int)node.JointIndex].Y + skeleton.Joints[node.JointIndex].Position.Y) / 2;
                        _initialOffsets[(int)node.JointIndex].Z = (_initialOffsets[(int)node.JointIndex].Z + skeleton.Joints[node.JointIndex].Position.Z) / 2;
                        _initialRotations[(int)node.JointIndex] = KinectDataConverter.Vector4ToQuaternion(skeleton.BoneOrientations[node.JointIndex].AbsoluteRotation.Quaternion);
                    }
                    else
                    {
                        _initialOffsets[(int)node.JointIndex].X = (_initialOffsets[(int)node.JointIndex].X + skeleton.Joints[node.JointIndex].Position.X - skeleton.Joints[node.Parent.JointIndex].Position.X) / 2;
                        _initialOffsets[(int)node.JointIndex].Y = (_initialOffsets[(int)node.JointIndex].Y + skeleton.Joints[node.JointIndex].Position.Y - skeleton.Joints[node.Parent.JointIndex].Position.Y) / 2;
                        _initialOffsets[(int)node.JointIndex].Z = (_initialOffsets[(int)node.JointIndex].Z + skeleton.Joints[node.JointIndex].Position.Z - skeleton.Joints[node.Parent.JointIndex].Position.Z) / 2;
                        _initialRotations[(int)node.JointIndex] = KinectDataConverter.Vector4ToQuaternion(skeleton.BoneOrientations[getRotationJoint(node)].HierarchicalRotation.Quaternion);
                    }
                }
            }
        }

        /// <summary>
        /// calcualte the average
        /// </summary>
        /// <param name="skeleton"></param>
        private void updateBoneLengths(Skeleton skeleton)
        {
            // hip to spine
            calCenterBoneLength(skeleton, HummanBoneEnum.HipToSpine, JointType.HipCenter, JointType.Spine);
            // spine to shoulder
            calCenterBoneLength(skeleton, HummanBoneEnum.SpineToShoulder, JointType.Spine, JointType.ShoulderCenter);
            // shoulder to head
            calCenterBoneLength(skeleton, HummanBoneEnum.ShoulderToHead, JointType.ShoulderCenter, JointType.Head);
            // shoulder to up arm
            calSideBoneLength(skeleton, HummanBoneEnum.ShoulderToUpArm, JointType.ShoulderCenter, JointType.ShoulderLeft);
            // up arm to elbow
            calSideBoneLength(skeleton, HummanBoneEnum.UpArmToElbow, JointType.ShoulderLeft, JointType.ElbowLeft);
            // elbow to wrist
            calSideBoneLength(skeleton, HummanBoneEnum.ElbowToWrist, JointType.ElbowLeft, JointType.WristLeft);
            // wrist to hand
            calSideBoneLength(skeleton, HummanBoneEnum.WristToHand, JointType.WristLeft, JointType.HandLeft);
            // hip to up leg
            calSideBoneLength(skeleton, HummanBoneEnum.HipToUpLeg, JointType.HipCenter, JointType.HipLeft);
            // up leg to knee
            calSideBoneLength(skeleton, HummanBoneEnum.UpLegToKnee, JointType.HipLeft, JointType.KneeLeft);
            // knee to ankle
            calSideBoneLength(skeleton, HummanBoneEnum.KneeToAnkle, JointType.KneeLeft, JointType.AnkleLeft);
            // ankle to foot
            calSideBoneLength(skeleton, HummanBoneEnum.AnkleToFoot, JointType.AnkleLeft, JointType.FootLeft);
        }

        /// <summary>
        /// length of hip center - spine - shoulder center - head
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="bone"></param>
        /// <param name="startJoint"></param>
        /// <param name="endJoint"></param>
        private void calCenterBoneLength(Skeleton skeleton, HummanBoneEnum bone, JointType startJoint, JointType endJoint)
        {
            double length = calDescartesLength(skeleton.Joints[startJoint].Position,
                skeleton.Joints[endJoint].Position);
            if (_boneLengths.ContainsKey(bone))
            {
                double oldLength;
                _boneLengths.TryGetValue(bone, out oldLength);
                _boneLengths[bone] = (length + oldLength) / 2;
            }
            else
            {
                _boneLengths.Add(bone, length);
            }
        }

        /// <summary>
        /// length of shoulder center - shoulder - elbow - wrist - hand, hip center - hip - knee - ankle - foot
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="bone"></param>
        /// <param name="startJoint"></param>
        /// <param name="endJoint"></param>
        private void calSideBoneLength(Skeleton skeleton, HummanBoneEnum bone, JointType startJoint, JointType endJoint)
        {
            double leftLength = calDescartesLength(skeleton.Joints[startJoint].Position,
                skeleton.Joints[endJoint].Position);
            double rightLength = calDescartesLength(skeleton.Joints[GetSymmetricJoint(startJoint)].Position,
                skeleton.Joints[GetSymmetricJoint(endJoint)].Position);
            double length = (leftLength + rightLength) / 2;
            if (_boneLengths.ContainsKey(bone))
            {
                double oldLength;
                _boneLengths.TryGetValue(bone, out oldLength);
                _boneLengths[bone] = (length + oldLength) / 2;
            }
            else
            {
                _boneLengths.Add(bone, length);
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

        public void getJointPositions_Debug(JointNode node)
        {
            if (NodeTypeEnum.END == node.Type)
                return;

            if (node.Type == NodeTypeEnum.ROOT)
            {
                Positions_Debug[(int)node.JointIndex] = new Point3D(320, 240, 0);
            }
            else
            {
                Positions_Debug[(int)node.JointIndex] = new Point3D(node.OriginalOffset.X + Positions_Debug[(int)node.Parent.JointIndex].X, 
                    node.OriginalOffset.Y + Positions_Debug[(int)node.Parent.JointIndex].Y, 0);
            }
            foreach (JointNode child in node.Children)
            {
                getJointPositions_Debug(child);
            }
        }
    }

    enum HummanBoneEnum
    {
        HipToSpine,
        SpineToShoulder,
        ShoulderToHead,
        HeadToEnd,
        ShoulderToUpArm,
        UpArmToElbow,
        ElbowToWrist,
        WristToHand,
        HandToEnd,
        HipToUpLeg,
        UpLegToKnee,
        KneeToAnkle,
        AnkleToFoot,
        FootToEnd
    }

    public enum TransAxis
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
