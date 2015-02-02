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

namespace MocapView.Mocap {
    /// <summary>
    /// Class used to load motion capture data
    /// </summary>
    public static class Loader {
        const string STR_HIERARCHY = "HIERARCHY";
        const string STR_ROOT = "ROOT";
        const string STR_JOINT = "JOINT";
        const string STR_END = "END";
        const string STR_SITE = "SITE";
        const string STR_OFFSET = "OFFSET";
        const string STR_MOTION = "MOTION";
        const string STR_FRAMES = "FRAMES:";
        const string STR_FRAME = "FRAME";
        const string STR_TIME = "TIME:";

        readonly static int LEN_HIERARCHY = STR_HIERARCHY.Length;
        readonly static int LEN_ROOT = STR_ROOT.Length;
        readonly static int LEN_JOINT = STR_JOINT.Length;
        readonly static int LEN_OFFSET = STR_OFFSET.Length;

        const char CHAR_SPACE = ' ';
        const char CHAR_TAB = '\t';
        const char CHAR_CR = '\r';
        const char CHAR_LF = '\n';
        const string STR_OPEN_BRACE = "{";
        const string STR_CLOSE_BRACE = "}";
        const string STR_XPOSITION = "Xposition";
        const string STR_YPOSITION = "Yposition";
        const string STR_ZPOSITION = "Zposition";

        const string STR_XROTATION = "Xrotation";
        const string STR_YROTATION = "Yrotation";
        const string STR_ZROTATION = "Zrotation";

        private static List<string> _errors = new List<string>();

        /// <summary>
        /// Loads motion capture data from the Biovision .BVH file format
        /// for quick overview of the format see: http://www.cs.wisc.edu/graphics/Courses/cs-838-1999/Jeff/BVH.html
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static Motion LoadFromBvh(string contents) {
            ClearErrors();

            string[] tokens = contents.Split(new char[] {CHAR_TAB, CHAR_CR, CHAR_LF, CHAR_SPACE}, StringSplitOptions.RemoveEmptyEntries);
            int tokenCount = tokens.Length;

            if (tokenCount == 0) {
                AddErrorMessage("No data found");
                return null;
            }

            if (!string.Equals(STR_HIERARCHY, tokens[0], StringComparison.OrdinalIgnoreCase)) {
                AddErrorMessage("HIERARCHY tag expected");
                return null;
            }

            Motion motion = new Motion();
            motion.Skeleton = new Node();
            int channelCount = 0;
            int tokenIndex = 1;

            // parse the hierarchy -------------------------------------------------------------------------------------
            if (!GetNodeData(tokens, ref tokenIndex, tokenCount, motion.Skeleton, ref channelCount)) {
                return null;
            }
            
            motion.ChannelCount = channelCount;

            // parse the motion data -------------------------------------------------------------------------------------
            if (!ParseNextToken(tokens, tokenCount, ref tokenIndex, STR_MOTION)) { // MOTION
                return null;
            }
            if (!ParseNextToken(tokens, tokenCount, ref tokenIndex, STR_FRAMES)) { // Frames: 
                return null;
            }

            if (!MoveToNextTokenAndParseInt(tokens, tokenCount, ref tokenIndex, out motion.FrameCount)) { // frame count
                return null;
            }

            // 'Frame Time:' is parsed as 2 tokens:
            if (!ParseNextToken(tokens, tokenCount, ref tokenIndex, STR_FRAME)) { // Frame
                return null;
            }
            if (!ParseNextToken(tokens, tokenCount, ref tokenIndex, STR_TIME)) { // Time: 
                return null;
            }

            if (!MoveToNextTokenAndParseDouble(tokens, tokenCount, ref tokenIndex, out motion.FrameTime)) { // frame time
                return null;
            }
            motion.FrameTime *= 1000; // convert from seconds to milliseconds

            // parse frames data
            motion.Frames = new double[motion.FrameCount][];
            double tempDouble;

            for (int i = 0; i < motion.FrameCount; i++) {
                motion.Frames[i] = new double[motion.ChannelCount];
                for (int j = 0; j < motion.ChannelCount; j++) {
                    if (!MoveToNextTokenAndParseDouble(tokens, tokenCount, ref tokenIndex, out tempDouble)) { // value
                        return null;
                    }
                    if (j < 3) { // if offset data
                        motion.Frames[i][j] = tempDouble; 
                    }
                    else { // angle data
                        //tempDouble = (((int)tempDouble) % 360) + tempDouble-Math.Floor(tempDouble);
                        motion.Frames[i][j] = (tempDouble * Math.PI) / 180; // convert angle to radians
                    }
                }
            }

            return motion;
        }

        enum NodeType {
            Root,
            Joint,
            EndSite
        }

        /// <summary>
        /// Parses a specific token
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="?"></param>
        /// <param name="tokenCount"></param>
        /// <param name="tokenName"></param>
        /// <returns></returns>
        private static bool ParseNextToken(string[] tokens, int tokenCount, ref int tokenIndex, string tokenName){
            if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                return false;
            }

            if (!tokens[tokenIndex].Equals(tokenName, StringComparison.OrdinalIgnoreCase)) {
                AddErrorMessage("Expected '" + tokenName + "'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the information for a given mocap node
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="lineIndex"></param>
        /// <param name="lineCount"></param>
        /// <param name="node"></param>
        /// <param name="channelCount"></param>
        /// <param name="rootChannelLocations"></param>
        /// <returns></returns>
        private static bool GetNodeData(string[] tokens, ref int tokenIndex, int tokenCount, Node node, ref int channelCount) {
            if (tokenIndex >= tokenCount) {
                AddErrorMessage("Unexpected end of data encountered");
                return false;
            }

            string token = tokens[tokenIndex].Trim();
            NodeType nodeType;
            if (token.Equals(STR_JOINT, StringComparison.OrdinalIgnoreCase)) {
                nodeType = NodeType.Joint;
                if (!ParseNodeName(tokens, tokenCount, ref tokenIndex, out node.Name)) {
                    return false;
                }
            }
            else
                if (token.Equals(STR_END, StringComparison.OrdinalIgnoreCase)) {
                    nodeType = NodeType.EndSite;
                    if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                        return false;
                    }
                    if (!tokens[tokenIndex].Equals(STR_SITE, StringComparison.OrdinalIgnoreCase)) {
                        AddErrorMessage("Expected 'SITE'");
                        return false;
                    }
                    if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                        return false;
                    }
                }
                else
                    if (token.StartsWith(STR_ROOT, StringComparison.OrdinalIgnoreCase)) {
                        nodeType = NodeType.Root;
                        if (!ParseNodeName(tokens, tokenCount, ref tokenIndex, out node.Name)) {
                            return false;
                        }
                    }
                    else {
                        AddErrorMessage("Unrecognized node:" + token);
                        return false;
                    }

            // parse the open brace
            if (!tokens[tokenIndex].Equals(STR_OPEN_BRACE)) {
                AddErrorMessage("Missing '{'");
                return false;
            }

            // parse Offset tag
            if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                return false;
            }
            if (!tokens[tokenIndex].Equals(STR_OFFSET, StringComparison.OrdinalIgnoreCase)) {
                AddErrorMessage("OFFSET tag expected: " + tokens[tokenIndex]);
                return false;
            }

            if (!MoveToNextTokenAndParseDouble(tokens, tokenCount, ref tokenIndex, out node.Offset.X)) { // X
                return false;
            }
            if (!MoveToNextTokenAndParseDouble(tokens, tokenCount, ref tokenIndex, out node.Offset.Y)) { // Y
                return false;
            }
            if (!MoveToNextTokenAndParseDouble(tokens, tokenCount, ref tokenIndex, out node.Offset.Z)) { // Z
                return false;
            }

            node.OriginalOffset.X = node.Offset.X;
            node.OriginalOffset.Y = node.Offset.Y;
            node.OriginalOffset.Z = node.Offset.Z;

            if (nodeType == NodeType.EndSite) {
                // parse the close brace
                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                if (!tokens[tokenIndex].Equals(STR_CLOSE_BRACE)) {
                    AddErrorMessage("Missing '}'");
                    return false;
                }

                return true;
            }

            // Node type is JOINT or ROOT here:

            // parse Channels tag
            if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                return false;
            }
            token = tokens[tokenIndex].Trim().Replace(" ", String.Empty).Replace("\t", String.Empty);

            if (nodeType == NodeType.Root) {
                channelCount += 3;
                // ensure the 6-channel definition is supported: CHANNELS 6 Xposition Yposition Zposition ...
                
                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                if (tokens[tokenIndex] != "6") {
                    AddErrorMessage("Unsupported number of channels for ROOT element: " + tokens[tokenIndex]);
                    return false;
                }

                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                if (!tokens[tokenIndex].Equals(STR_XPOSITION)) {
                    AddErrorMessage("Unsupported CHANNELS definition for ROOT element: " + tokens[tokenIndex]);
                    return false;
                }

                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                if (!tokens[tokenIndex].Equals(STR_YPOSITION)) {
                    AddErrorMessage("Unsupported CHANNELS definition for ROOT element: " + tokens[tokenIndex]);
                    return false;
                }

                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                if (!tokens[tokenIndex].Equals(STR_ZPOSITION)) {
                    AddErrorMessage("Unsupported CHANNELS definition for ROOT element: " + tokens[tokenIndex]);
                    return false;
                }
            }
            else {
                // ensure the 3-channel definition is supported: CHANNELS 3 ...
                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                if (tokens[tokenIndex] != "3") {
                    AddErrorMessage("Unsupported number of channels for ROOT element: " + tokens[tokenIndex]);
                    return false;
                }
            }

            // parse the rotation channels
            if (!MoveToAndParseRotationChannel(tokens, tokenCount, ref tokenIndex, channelCount, node)) {
                return false;
            }
            channelCount++;
            if (!MoveToAndParseRotationChannel(tokens, tokenCount, ref tokenIndex, channelCount, node)) {
                return false;
            }
            channelCount++;
            if (!MoveToAndParseRotationChannel(tokens, tokenCount, ref tokenIndex, channelCount, node)) {
                return false;
            }
            channelCount++;

            // parse nested children tags, if any
            while (true) {
                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }

                if (tokens[tokenIndex].Equals(STR_CLOSE_BRACE)) {
                    return true;
                }

                Node child = new Node();
                child.Parent = node;
                if (!GetNodeData(tokens, ref tokenIndex, tokenCount, child, ref channelCount)){
                    return false;
                }

                node.Children.Add(child);
            }
        }

        private static bool ParseNodeName(string[] tokens, int tokenCount, ref int tokenIndex, out string name) {
            name = null;

            while (true) {
                if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                    return false;
                }
                
                if (tokens[tokenIndex] == STR_OPEN_BRACE) {
                    return true;
                }

                if (name == null) {
                    name = tokens[tokenIndex];
                }
                else {
                    name = name + ' ' + tokens[tokenIndex];
                }
            }
        }

        /// <summary>
        /// Parses one of the rotation channel definitions and assigns it to a given node
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="tokenCount"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="channelIndexToAssign"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool MoveToAndParseRotationChannel(string[] tokens, int tokenCount, ref int tokenIndex, int channelIndexToAssign, Node node) {
            if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                return false;
            }
            string token = tokens[tokenIndex];

            if (token.Equals(STR_XROTATION, StringComparison.OrdinalIgnoreCase)) {
                node.XRotationIndex = channelIndexToAssign;
                return true;
            }

            if (token.Equals(STR_YROTATION, StringComparison.OrdinalIgnoreCase)) {
                node.YRotationIndex = channelIndexToAssign;
                return true;
            }

            if (token.Equals(STR_ZROTATION, StringComparison.OrdinalIgnoreCase)) {
                node.ZRotationIndex = channelIndexToAssign;
                return true;
            }

            AddErrorMessage("Unrecognized rotation token: " + token);
            return false;
        }

        private static bool MoveToNextTokenAndParseDouble(string[] tokens, int tokenCount, ref int tokenIndex, out double result) {
            if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                result = 0;
                return false;
            }

            if (!double.TryParse(tokens[tokenIndex], out result)) {
                AddErrorMessage("Number expected");
                return false;
            }

            return true;
        }

        private static bool MoveToNextTokenAndParseInt(string[] tokens, int tokenCount, ref int tokenIndex, out int result) {
            if (!MoveToNextToken(tokenCount, ref tokenIndex)) {
                result = 0;
                return false;
            }

            if (!Int32.TryParse(tokens[tokenIndex], out result)) {
                AddErrorMessage("Number expected");
                return false;
            }

            return true;
        }

        private static bool MoveToNextToken(int tokenCount, ref int tokenIndex) {
            tokenIndex++;
            if (tokenIndex >= tokenCount) {
                AddErrorMessage("Unexpected end of data encountered");
                return false;
            }

            return true;
        }

        private static void ClearErrors() {
            _errors.Clear();
        }

        private static void AddErrorMessage(string errorMesssage) {
            _errors.Add(errorMesssage);
        }
    }
}
