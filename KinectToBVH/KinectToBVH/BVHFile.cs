using System.Text;
using Microsoft.Kinect;
using System.IO;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Media.Media3D;

namespace KinectToBVH
{
    public class BVHFile
    {
        // const strings/chars/integers for generating BVH file
        public const string STR_HIERARCHY = "HIERARCHY";
        public const string STR_OFFSET = "OFFSET";
        public const string STR_CHANNELS = "CHANNELS";
        public const string STR_END_SITE = "End Site";
        public const string STR_MOTION = "MOTION";
        public const string STR_FRAMES = "Frames:";
        public const string STR_FRAME_TIME = "Frame Time:";
        public const string STR_XPOSITION = "Xposition";
        public const string STR_YPOSITION = "Yposition";
        public const string STR_ZPOSITION = "Zposition";
        public const string STR_XROTATION = "Xrotation";
        public const string STR_YROTATION = "Yrotation";
        public const string STR_ZROTATION = "Zrotation";

        public const char CHAR_SPACE = ' ';
        public const char CHAR_TAB = '\t';
        public const char CHAR_LF = '\n';
        public const char CHAR_OPEN_BRACE = '{';
        public const char CHAR_CLOSE_BRACE = '}';

        public const int JOINT_CHANNEL_NUM = 3;
        public const int ROOT_CHANNEL_NUM = 6;
        public const double DOUBLE_FRAMERATE = 0.1 / 3.0;

        public const string STRING_DEFAULTPATH = "d:/Kinect.bvh";

        //convert meter in kinect to centimeter in bvh
        public const double DOUBLE_KINECTTOBVHSCALERATE = 100;

        private string _filePath;
        
        // buffer of skeleton structure
        private StringBuilder _bvhSkeletonBuffer;

        // buffer of all motion frames
        private StringBuilder _bvhMotionFrameBuffer;

        // count of frames
        private int _frameCount = 0;

        // reference to skeleton structure
        private KinectSkeleton _structuredSkeleton;

        public BVHFile(KinectSkeleton structuredSkeleton, string filePath = STRING_DEFAULTPATH)
        {
            _structuredSkeleton = structuredSkeleton;
            FilePath = filePath;
            _bvhSkeletonBuffer = new StringBuilder();
            _bvhMotionFrameBuffer = new StringBuilder();
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                // judge the file path legality
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    _filePath = value;
                }
                else // default path
                {
                    _filePath = STRING_DEFAULTPATH;
                }
            }
        }

        /// <summary>
        /// Write bvh data into a file
        /// </summary>
        /// <param name="path"></param>
        public void OutputBVHToFile()
        {
            // if skeleton struture not clibrated or there is no motion frame, just return
            if (_structuredSkeleton.NeedBoneClibrated() || _frameCount == 0)
            {
                return;
            }

            StringBuilder wholeBVH = new StringBuilder();
            // HIRARCHY
            wholeBVH.Append(STR_HIERARCHY + CHAR_LF);

            // all the joints
            buildBVHOutputSkeleton(_structuredSkeleton.HipCenter);
            wholeBVH.Append(_bvhSkeletonBuffer);

            // MOTION
            wholeBVH.Append(STR_MOTION + CHAR_LF);

            // frame info
            wholeBVH.Append(STR_FRAMES + CHAR_SPACE + _frameCount + CHAR_LF);
            wholeBVH.Append(STR_FRAME_TIME + CHAR_SPACE + DOUBLE_FRAMERATE + CHAR_LF);

            // frames
            wholeBVH.Append(_bvhMotionFrameBuffer);

            // write out
            using (StreamWriter bvhStreamWriter = new StreamWriter(FilePath))
            {
                bvhStreamWriter.Write(wholeBVH.ToString());
            }
        }

        public static double BVHScaledValue(double origin)
        {
            return origin * DOUBLE_KINECTTOBVHSCALERATE;
        }

        public static void BVHScalePoint(ref Point3D point)
        {
            point.X = BVHScaledValue(point.X);
            point.Y = BVHScaledValue(point.Y);
            point.Z = BVHScaledValue(point.Z);
        }

        /// <summary>
        /// Append one motion frame to motion buffer
        /// </summary>
        /// <param name="structuredSkeleton"></param>
        /// <param name="skeleton"></param>
        public void AppendOneMotionFrame()
        {
            foreach (JointNode node in _structuredSkeleton.JointNodes.Values)
            {
                if (NodeTypeEnum.ROOT == node.Type)
                    appendOffsetToBVHBuffer(node.Offset);
                if(NodeTypeEnum.END != node.Type)
                    appendRotationToBVHBuffer(node.Rotation);
            }
            _bvhMotionFrameBuffer.Append(CHAR_LF);
            _frameCount++;
        }
        
        /// <summary>
        /// Append an offset to buffer
        /// </summary>
        /// <param name="offset"></param>
        private void appendOffsetToBVHBuffer(Point3D offset)
        {
            _bvhMotionFrameBuffer.Append(offset.X.ToString() + CHAR_SPACE +
                    offset.Y.ToString() + CHAR_SPACE + offset.Z.ToString() + CHAR_SPACE);
        }

        /// <summary>
        /// Append a joint rotation to buffer
        /// </summary>
        /// <param name="rotation"></param>
        private void appendRotationToBVHBuffer(Vector3D rotation)
        {
            _bvhMotionFrameBuffer.Append(rotation.Z.ToString() + CHAR_SPACE +
                    rotation.X.ToString() + CHAR_SPACE + rotation.Y.ToString() + CHAR_SPACE);
        }

        /// <summary>
        /// output a given joint in BVH format
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="sbOut"></param>
        /// <param name="indents"></param>
        private void buildBVHOutputSkeleton(JointNode root, string indents = "")
        {
            _bvhSkeletonBuffer.Append(indents);
            _bvhSkeletonBuffer.Append(root.Type != NodeTypeEnum.END ?
                root.Type.ToString() + CHAR_SPACE + root.Name : STR_END_SITE).Append(CHAR_LF);
            _bvhSkeletonBuffer.Append(indents).Append(CHAR_OPEN_BRACE).Append(CHAR_LF);
            _bvhSkeletonBuffer.Append(indents).Append(CHAR_SPACE).Append(CHAR_SPACE).Append(STR_OFFSET).Append(CHAR_SPACE)
                .Append(root.OriginalOffset.X.ToString()).Append(CHAR_SPACE)
                .Append(root.OriginalOffset.Y.ToString()).Append(CHAR_SPACE)
                .Append(root.OriginalOffset.Z.ToString()).Append(CHAR_LF);
            if (root.Type != NodeTypeEnum.END)
            {
                _bvhSkeletonBuffer.Append(indents).Append(CHAR_SPACE).Append(CHAR_SPACE).Append(STR_CHANNELS).Append(CHAR_SPACE)
                    .Append(root.Type == NodeTypeEnum.ROOT ? ROOT_CHANNEL_NUM : JOINT_CHANNEL_NUM);
                if (root.Type == NodeTypeEnum.ROOT)
                {
                    _bvhSkeletonBuffer.Append(CHAR_SPACE).Append(STR_XPOSITION)
                        .Append(CHAR_SPACE).Append(STR_YPOSITION)
                        .Append(CHAR_SPACE).Append(STR_ZPOSITION);
                }
                _bvhSkeletonBuffer.Append(CHAR_SPACE).Append(STR_ZROTATION)
                .Append(CHAR_SPACE).Append(STR_XROTATION)
                .Append(CHAR_SPACE).Append(STR_YROTATION).Append(CHAR_LF);
                foreach (JointNode childJoint in root.Children)
                {
                    buildBVHOutputSkeleton(childJoint, indents + CHAR_SPACE + CHAR_SPACE);
                }
            }
            _bvhSkeletonBuffer.Append(indents).Append(CHAR_CLOSE_BRACE).Append(CHAR_LF);
        }
    }
}
