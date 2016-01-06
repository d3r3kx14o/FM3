using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LargeGraphLayout.Models;

namespace Test.Util
{
    class GraphUtil
    {
        public static Graph SubGraph(Graph graph, int nodeLimit)
        {
            var degreeList = graph.AdjList.Select(l => l.Count).ToList();
            var startId = degreeList.IndexOf(degreeList.Max());
            var res = new List<int>(nodeLimit);
            var idToIndex = new Dictionary<int, int>(nodeLimit);
            var queue = new Stack<int>();
            res.Add(startId);
            queue.Push(startId);
            while (queue.Count != 0 && res.Count <= nodeLimit)
            {
                if (res.Count%100 == 0)
                {
                    Console.WriteLine(res.Count);
                }
                var id = queue.Pop();
                var adjSet = graph.AdjList[id];
                //Console.WriteLine($"{adjSet.Count}\t{id}");
                if (adjSet.Count <= 1) continue;
                foreach (var adj in adjSet)
                {
                    if (res.Contains(adj)) continue;
                    res.Add(adj);
                    queue.Push(adj);
                }
            }
            var resSet = new HashSet<int>(res);
            if (resSet.Count != res.Count)
                throw new Exception("error");

            for (var i = 0; i < res.Count; i ++)
                idToIndex.Add(res[i], i);
            // remap id and retrieve links
            var adjLink = new List<HashSet<int>>(res.Count);
            for (var i = 0; i < res.Count; i ++)
            {
                adjLink.Add(new HashSet<int>());
            }
            for (var i = 0; i < res.Count; i ++)
            {
                for (var j = i + 1; j < res.Count; j ++)
                {
                    if (graph.AdjList[res[i]].Contains(res[j]))
                    {
                        adjLink[i].Add(j);
                        adjLink[j].Add(i);
                    }
                }
            }
            for (var i = 0; i < res.Count; i ++)
            {
                if (adjLink[i].Count == 0)
                {
                    
                }
            }
            //for (var i = 0; i < res.Count; i ++)
            //{
            //    var id = res[i];
            //    var adjSet = graph.AdjList[id];
            //    for (var j = 0; j < adjSet.Count; j ++)
            //    {

            //        var adjId = adjSet[j];
            //        // if not in the subgraph
            //        if (!res.Contains(adjId)) continue;

            //        var adjIndex = idToIndex[adjId];
            //        adjLink[i].Add(adjIndex);
            //        adjLink[adjIndex].Add(i);
            //    }
            //}
            return new Graph()
            {
                AdjList = adjLink.Select(p => p.ToList()).ToList(),
                IdSet = res,
                IdToIndex = idToIndex
            };
        }
    }
}
