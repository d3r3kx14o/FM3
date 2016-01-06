using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LargeGraphLayout.Algorithms.FM3;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.App_Start
{
    public class Data
    {
        public static string ServerDataRoot = HttpContext.Current.Server.MapPath("~/App_Data") + "/";
        public static Dictionary<string, List<Graph>> Graphs;
        public static List<string> PreloadedGraph = new List<string> { "sample_1004" };
        public static string RuntimeIdentifier;
        public static void Initialize()
        {
            RuntimeIdentifier = DateTime.Now.ToString("yyyyMMdd-HHmmss.ffff");
            FM3Config config = new FM3Config();
            Graphs = PreloadedGraph.ToDictionary(graphName => graphName,
                graphName => new FM3(new Graph(ServerDataRoot, graphName + ".txt"), config).GraphLayers);
        }
    }
}