using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LargeGraphLayout.Algorithms;

namespace LargeGraphLayout.Models
{
    public class KDTree
    {
    }

    public class TreeNode
    {
        public List<Complex> SubtractArr;
        public List<int> IdSet;
        public TreeNode LeftChild;
        public TreeNode RightChild;
        public double Cx;
        public double Cy;
        public double Radius;
        public double Charges;
        public List<Complex> AkValueArr;
        public bool IsLeaf;
    }

    public class NodesRegion
    {
        public double Cx;
        public double Cy;
        public double Radius;
    }
}