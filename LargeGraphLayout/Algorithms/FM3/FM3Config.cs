using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LargeGraphLayout.Algorithms.FM3
{
    public class FM3Config
    {
        public int CanvasWidth = 1000;
        public int CanvasHeight = 1000;
        public double LineWidth = 0.1;
        public double NodeRadius = 0.5;
        public string LineColor = "black";
        public string NodeColor = "blue";
        public double Alpha = 0.8;
        public double ScaleDown = 1.0;
        public bool DrawEveryLayout = false;
        /**
         * The maximal iteration times at each layout computation.
         * This value should be bigger as graphs becomes larger.
         * Its range should be [30, 500].
         */
        public int MaxIterations = 200;
        /**
        * The minimal iteration times at each layout computation.
        * This value should be bigger as graph becomes larger.
        * Its range should be [30, maxIterations].
        */
        public int MinIterations = 0;
        /**
        * When the value is true, the layout computation will be accelerated.
        * It is aimed to accelerate the computation when the layout has big enough vertices, which is set by the parameter "vertexCountOfAcceleration".
        * When the value is false, the layout computation will be calculated sufficiently.
        */
        public bool IsAccelerate = true;
        /**
         * When the vertices of graph G_i is more than this value, the layout computation is accerlerated.
         * It is a good practice to set this value big enough for producing a aesthetically-pleasing graph.
         * This value should become bigger as the graph becomes larger.
         */
        public int AccelerateThreshold = 1000;
        public int LayerMinimalNodeCount = 200;
        public double Epsilon0 = 1;
        public double Epsilon1 = 1;
    }
}