using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LargeGraphLayout.Algorithms;
using LargeGraphLayout.App_Start;

namespace LargeGraphLayout.Models
{
    public class Node
    {
        public double x { get; set; }
        public double y { get; set; }
        public int id { get; set; }
    }

    public class Link
    {
        public int source { get; set; }
        public int target { get; set; }
    }

    public class GraphCore
    {
        public string GraphName;
        public List<Node> Nodes;
        public List<Link> Links;
    }
    public class Graph
    {
        public GraphCore Core;
        [XmlIgnore]
        public List<int> IdSet;
        public List<List<int>> AdjList;
        public List<double> ChargeSet;
        [XmlIgnore]
        public List<List<double>> WeightList;
        [XmlIgnore]
        public Dictionary<int, int> ParentIdSet;
        public Dictionary<int, int> IdToIndex;
        [XmlIgnore]
        public bool IsLargest;
        [XmlIgnore]
        public Func<int, double> GetChargeFunc;
        [XmlIgnore]
        public Func<int, int, double> GetWeightFunc;

        #region for debug use

        public override string ToString()
        {
            return "{idSet: " +IdSetToString() 
                + ",\nadjSet: " + AdjListToString()
                + ",\nidToIndex: " + DictionaryToString(IdToIndex)
                + ",\nparentIdSet: " + DictionaryToString(ParentIdSet)
                + "}";
        }
        public static string DictionaryToString<S, T>(Dictionary<S, T> dictionary)
        {
            var content = "";
            if (dictionary != null)
                content = string.Join(",\n", from pair in dictionary?.OrderBy(p => p.Key) select pair.Key + ": " + pair.Value);
            return $"{{{content}}}";
        }
        public static string ListToString(List<int> list, bool reverse = false)
        {
            return "[" + string.Join(",\n", reverse ? list.OrderByDescending(p => p) : list.OrderBy(p => p)) + "]";
        }
        public string IdSetToString()
        {
            return ListToString(this.IdSet);
        }

        public string AdjListToString()
        {
            return "[" + string.Join(",\n", from adjSet in AdjList select ListToString(adjSet)) + "]";
        }
        #endregion
        
        public double GetCharge(int id)
        {
            return this.ChargeSet[id];
        }

        public double GetWeight(int idA, int idB)
        {
            return this.WeightList[idA][idB];
        }

        public Graph()
        {
            this.Core = new GraphCore();
        }

        public Graph(string root, string graphName)
        {
            this.Core = new GraphCore {};
            this.Core.GraphName = graphName;
            var filePath = root + (root.EndsWith("/") ? "" : "/") + graphName;
            var dataArr = File.ReadAllLines(filePath);
            var estimatedCount = dataArr.Length;
            this.IdSet = new List<int>(estimatedCount);

            var idToLineArr = new Dictionary<int, string []>(estimatedCount);
            this.IdToIndex = new Dictionary<int, int>(estimatedCount);
            var vertexCount = 0;
            foreach (var line in dataArr)
            {
                var tokens = line.Split('\t');
                if (tokens.Length < 2) continue;
                var id = int.Parse(tokens[0]);
                this.IdSet.Add(id);
                this.IdToIndex.Add(id, vertexCount);
                idToLineArr.Add(id, tokens);
                vertexCount ++;
            }
            this.AdjList =
                idToLineArr.Select(pair => pair.Value.Skip(1).Select(token => this.IdToIndex[int.Parse(token)]).ToList())
                    .ToList();
            
            this.GetChargeFunc = (index) => 1;
            this.GetWeightFunc = (idA, idB) => 1;
        }

        public Graph PersistentGraph(Dictionary<int, double> xArr, Dictionary<int, double> yArr)
        {
            this.Core.Nodes = new List<Node>(this.IdSet.Count);
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var id in this.IdSet)
            {
                if (xArr[id] < minX)
                    minX = xArr[id];
                if (xArr[id] > maxX)
                    maxX = xArr[id];
                if (yArr[id] < minY)
                    minY = yArr[id];
                if (yArr[id] > maxY)
                    maxY = xArr[id];
            }
            double width = Math.Max(maxX - minX, 1);
            double height = Math.Max(maxY - minY, 1);
            this.Core.Links = new List<Link>(this.IdSet.Count);
            foreach (var id in this.IdSet)
            {
                // Normalize
                xArr[id] = (xArr[id] - minX) / width;
                yArr[id] = (yArr[id] - minY) / height;
                this.Core.Nodes.Add(new Node()
                {
                    x = xArr[id],
                    y = yArr[id],
                    id = id
                });
                var sourceIndex = IdToIndex[id];
                var adjSet = AdjList[IdToIndex[id]];
                foreach (var adjId in adjSet)
                {
                    if (adjId < id)
                        continue;
                    this.Core.Links.Add(new Link()
                    {
                        source = sourceIndex,
                        target = IdToIndex[adjId]
                    });
                }
            }
            return this;
        }
    }
}