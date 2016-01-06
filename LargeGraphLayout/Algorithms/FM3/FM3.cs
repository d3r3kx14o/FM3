using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.Algorithms.FM3
{
    public class FM3
    {
        public FM3Config Config;
        public Dictionary<int, double> XCoordArr;
        public Dictionary<int, double> YCoordArr;
        public int Interval;
        public List<Graph> GraphLayers = new List<Graph>();
        /**
         * This is the maximum iterations for large graph which has hundreds of thousands vertices.
         * If the maxIterations is bigger than the _maximumIterations, it will be reassigned as the _maximumInterations.
         */
        private const int MaxIterations = 500;

        /**
         * This is the minimum iterations for small graph which has serveral vertices.
         * If the minIterations is smaller than the _minimumIterations, it will be reassigned as the _minimumIterations.
         */
        private const int MinIterations = 0;
        /**
         * Every line of the input file should be in the format like:
         * fromId \t toId \t toId \t ...
         */
         
        public FM3(Graph largestGraph, FM3Config config)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            this.Config = config;
            var maxIteration = config.MaxIterations;
            var minIteration = config.MinIterations;
            var isAccelerate = config.IsAccelerate;
            var accelerateCount = config.AccelerateThreshold;

            if (maxIteration > MaxIterations)
                maxIteration = MaxIterations;
            if (maxIteration < MinIterations)
                maxIteration = MinIterations;

            if (minIteration < MinIterations)
                minIteration = MinIterations;
            if (minIteration > maxIteration)
                minIteration = maxIteration;

            int i = 0, threshold = 8, len;
            var graphArr = new List<Graph> {largestGraph};
            while ((len = graphArr[i].IdSet.Count) > threshold)
            {
                Trace.WriteLine("Layout: " + i + "\t\tVertices count: " + len);
                graphArr.Add(this.Coarsening(graphArr[i]));
                i++;
            }
            if (len == 1)
            {
                graphArr.RemoveAt(graphArr.Count - 1);
                i--;
            }
            else
                Trace.WriteLine("Layout: " + i + "\t\tVertices count: " + len);
            
            this.XCoordArr = new Dictionary<int, double>(len);
            this.YCoordArr = new Dictionary<int, double>(len);
            var initRange = Math.Sqrt(largestGraph.IdSet.Count);
            var smallestSet = graphArr[i].IdSet;
            Dictionary<int, double> xArr = this.XCoordArr, yArr = this.YCoordArr;
            len = smallestSet.Count;
            //Random random = new Random(3145);
            for (var k = 0; k < len; k ++)
            {
                var id = smallestSet[k];
                xArr[id] = IRandom.NextDouble() * initRange;
                yArr[id] = IRandom.NextDouble() * initRange;
            }

            
            int maxI = i, j = i;
            var iterationsArr = Enumerable.Repeat(0.0, j + 1).ToList();
            var tValueArr = Enumerable.Repeat(0.0, j + 1).ToList();
            var globalForceArr = new List<double>();
            double kValue = 0.1, tDecrease = 0.95, forceDecreaseBound = 0.99;
            var iterRange = maxIteration - minIteration;
            var biggestLen = largestGraph.IdSet.Count;
            while (j > -1)
            {
                iterationsArr[j] = iterRange * (1.0 * j / maxI) + minIteration;
                tValueArr[j] = biggestLen * (1.0 * j / maxI);
                j--;
            }
            var minI = (maxI > 1) ? 0 : -1;
            while (i > minI)
            {
                var currIter = 0;
                var iterations = iterationsArr[i];
                var currGraph = graphArr[i];
                var tValue = Math.Pow(tValueArr[i], 0.45) * kValue;
                var useK = !(isAccelerate && currGraph.IdSet.Count > accelerateCount);
                TreeNode kdTree = new TreeNode();
                while (currIter < iterations)
                {
                    if (currIter < 3 || currIter % 20 == 0)
                    {
                        kdTree = this.GetKDTree(currGraph, 100);
                    }
                    globalForceArr.Add(this.MoveVertex(currGraph, kdTree, tValue, useK));
                    if (globalForceArr.Count == 3)
                    {
                        if (globalForceArr[2] / globalForceArr[0] > forceDecreaseBound)
                        {
                            tValue *= tDecrease;
                        }
                        globalForceArr.RemoveAt(0); // js: shift
                    }
                    currIter++;
                }
                Utils.Log("Layout: " + i + "\t\tIteration times: " + currIter);
                Trace.WriteLine("Layout: " + i + "\t\tIteration times: " + currIter);
                //if (config.drawEveryLayout)
                //    this._drawGraphWithCanvas(currGraph, context, config.nodeRadius, config.scaleDown, (maxI - i) * config.canvasHeight);
                if (i > 0)
                {
                    this.Interpolation(graphArr[i - 1], 50);
                    graphArr.RemoveAt(i);
                }
                i--;
            }
            //foreach (var id in this.XCoordArr.Keys.OrderBy(p => p))
            //{
            //    Utils.Log($"{id}\t{this.XCoordArr[id]}\t{this.YCoordArr[id]}");
            //}
            this.GraphLayers.Add(graphArr[0].PersistentGraph(this.XCoordArr, this.YCoordArr));
            s.Stop();
            Trace.WriteLine("Total" + s.Elapsed);
            Utils.Log("Total" + s.Elapsed);
        }

        private static void IncrementPointer(ref int pointer, bool[] isSelected)
        {
            do
            {
                pointer++;
                if (pointer >= isSelected.Length)
                    pointer -= isSelected.Length;
            }
            while (isSelected[pointer]);
        }

        public static T[] GetRandomOrderItems<T>(IEnumerable<T> items, Random random = null)
        {
            if (random == null)
                random = new Random();

            int itemCnt = items.Count();
            bool[] isSelected = new bool[itemCnt];

            T[] orderedItems = new T[itemCnt];

            int freeItemCnt = itemCnt;
            int pointer = 0;
            while (freeItemCnt > 0)
            {
                int dPointer = random.Next(freeItemCnt);
                for (int i = 0; i < dPointer; i++)
                {
                    IncrementPointer(ref pointer, isSelected);
                }
                isSelected[pointer] = true;
                orderedItems[freeItemCnt - 1] = items.ElementAt(pointer);
                if (freeItemCnt > 1)
                    IncrementPointer(ref pointer, isSelected);

                freeItemCnt--;
            }
            return orderedItems;
        }
        
        private Graph Coarsening(Graph graph)
        {
            //Utils.Log(graph.ToString());
            var idSet = graph.IdSet;
            var adjList = graph.AdjList;
            var oldIdToIndex = graph.IdToIndex;
            var getChargeFunc = graph.GetChargeFunc;
            var getWeightFunc = graph.GetWeightFunc;
            var newIdSet = new List<int>();
            var chargeSet = new List<double>();
            var deleteSet = new HashSet<int>();
            var parentIdSet = new Dictionary<int, int>();
            var newIdToIndex = new Dictionary<int, int>();
            int newIndex = 0, edgeCount = 0;
            for (var j = 0; j < idSet.Count; j++)
            {
                var currId = idSet[j];
                if (deleteSet.Contains(currId))
                    continue;

                var adjSet = adjList[j];
                if (adjSet.Count == 0)
                {
                    parentIdSet.Add(currId, 0);
                    continue;
                }

                newIdSet.Add(currId);
                chargeSet.Add(getChargeFunc(j));
                newIdToIndex.Add(currId, newIndex);
                parentIdSet.Add(currId, currId);
                deleteSet.Add(currId);
                for (var k = 0; k < adjSet.Count; k++)
                {
                    var adjId = adjSet[k];
                    if (!deleteSet.Contains(adjId))
                    {
                        deleteSet.Add(adjId);
                        parentIdSet.Add(adjId, currId);
                        chargeSet[newIndex] += getChargeFunc(oldIdToIndex[adjId]);
                    }
                    edgeCount++;
                }
                newIndex++;
            }
            graph.ParentIdSet = parentIdSet;

            var neighbors = Math.Floor(2.0 * edgeCount / newIdSet.Count);
            
            var newGraph = new Graph
            {
                IdSet = newIdSet,
                ChargeSet = chargeSet,
                WeightList = new List<List<double>>(newIdSet.Count),
                AdjList = new List<List<int>>(newIdSet.Count),
                IdToIndex = newIdToIndex
            };
            newGraph.GetChargeFunc = (index) => newGraph.ChargeSet[index];
            newGraph.GetWeightFunc = (index1, index2) => newGraph.WeightList[index1][index2];
            this.GetNewAdj(newGraph, neighbors, adjList, getWeightFunc, oldIdToIndex, parentIdSet);


            return newGraph;
        }

        private void GetNewAdj(Graph newGraph, double nbrs, List<List<int>> oldAdjList, Func<int, int, double> getWeightFunc, Dictionary<int, int> oldIdToIndex,
            Dictionary<int, int> parentIdSet)
        {
            var newIdSet = newGraph.IdSet;
            var newIdToIndex = newGraph.IdToIndex;
            var newAdjList = newGraph.AdjList;
            var newWeightList = newGraph.WeightList;

            for (var j = 0; j < newIdSet.Count; j++)
            {
                newAdjList.Add(new List<int>());
                newWeightList.Add(new List<double>());
            }

            var idCount = newIdSet.Count;
            for (var j = 0; j < idCount; j++)
            {
                var newId = newIdSet[j];
                var newAdjSet = newAdjList[j];
                var newWeightSet = newWeightList[j];
                var adjCount = newAdjSet.Count;
                if (adjCount >= nbrs)
                    continue;

                var newAdjMap = new Dictionary<int, int>();
                for (var v = 0; v < adjCount; v++)
                    newAdjMap.Add(newAdjSet[v], 1);

                var idQueue = new List<int>();
                var distanceQueue = new List<double>();
                var searchHistory = new HashSet<int>();
                idQueue.Add(newId);
                distanceQueue.Add(0);
                searchHistory.Add(newId);

                while (idQueue.Count > 0)
                {
                    var currId = idQueue.First();
                    idQueue.RemoveAt(0);
                    var currDistance = distanceQueue.First();
                    distanceQueue.RemoveAt(0);

                    var oldIndex = oldIdToIndex[currId];
                    var oldAdjSet = oldAdjList[oldIndex];
                    var len = oldAdjSet.Count;
                    for (var k = 0; k < len; k++)
                    {
                        var adjId = oldAdjSet[k];
                        if (searchHistory.Contains(adjId))
                            continue;

                        searchHistory.Add(adjId);
                        var newDistance = currDistance + getWeightFunc(oldIndex, k);
                        if (parentIdSet[adjId] == adjId)
                        {
                            if (newAdjMap.ContainsKey(adjId)) continue;

                            newAdjSet.Add(adjId);

                            newWeightSet.Add(newDistance);
                            var newIndex = newIdToIndex[adjId];
                            newAdjList[newIndex].Add(newId);
                            newWeightList[newIndex].Add(newDistance);
                            adjCount++;
                            if (!nbrs.Equals(adjCount)) continue;
                            idQueue = new List<int>();
                            break;
                        }
                        else
                        {
                            idQueue.Add(adjId);
                            distanceQueue.Add(newDistance);
                        }
                    }
                }
            }
        }

        private void Interpolation(Graph graph, int iterations)
        {
            var idSet = graph.IdSet;
            var parentIdSet = graph.ParentIdSet;
            Dictionary<int, double> xArr = this.XCoordArr, yArr = this.YCoordArr;
            var len1 = idSet.Count;
            for (var j = 0; j < len1; j++)
            {
                var id = idSet[j];
                var parentId = parentIdSet[id];
                xArr[id] = xArr[parentId];
                yArr[id] = yArr[parentId];
            }

            var iter = 0;
            var adjList = graph.AdjList;
            
            while (iter < Math.Min(iterations, 50))
            {
                len1 = idSet.Count;
                for (var j = 0; j < len1; j++)
                {
                    var id = idSet[j];
                    if (id == parentIdSet[id])
                        continue;

                    var adjSet = adjList[j];
                    var xSum = 0.0;
                    var ySum = 0.0;
                    var adjCount = 0;
                    var len2 = adjSet.Count;
                    for (var k = 0; k < len2; k++)
                    {
                        var adjId = adjSet[k];
                        xSum += xArr[adjId];
                        ySum += yArr[adjId];
                        adjCount++;
                    }

                    if (adjCount > 0)
                    {
                        xArr[id] = (xArr[id] + xSum / adjCount) / 2;
                        yArr[id] = (yArr[id] + ySum / adjCount) / 2;
                    }
                }
                iter++;
            }
        }

        private TreeNode GetKDTree(Graph graph, int shift)
        {
            double minX = 0, minY = 0, maxX = double.MinValue, maxY = double.MinValue;
            var idSet = graph.IdSet;
            var len = idSet.Count;
            Dictionary<int, double> xArr = this.XCoordArr, yArr = this.YCoordArr;

            for (var j = 0; j < len; j++)
            {
                var id = idSet[j];
                double x = xArr[id], y = yArr[id];
                
                if (x < minX)
                    minX = x;
                if (y < minY)
                    minY = y;
                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
            }
            if (minX < 0 || minY < 0)
            {
                maxX -= minX;
                maxY -= minY;
                len = idSet.Count;
                for (var j = 0; j < len; j++)
                {
                    var id = idSet[j];
                    xArr[id] -= minX;
                    yArr[id] -= minY;
                }
            }
            var maxLength = (maxX > maxY) ? maxX : maxY;
            long rank = Utils.ToString(maxLength*shift, 2).Length;
            //long rank = Convert.ToString(Convert.ToInt64(Math.Round(maxLength * shift)), 2).Length;

            var xBinArr = Enumerable.Repeat("", len).ToList();
            var yBinArr = Enumerable.Repeat("", len).ToList();
            
            for (var j = 0; j < len; j++)
            {
                var id = idSet[j];
                var xBin = Utils.ToString(Math.Round(xArr[id] * shift), 2);
                //xBin = Convert.ToString(Convert.ToInt64(Math.Round(xArr[id] * shift)), 2);
                var binLen = xBin.Length;
                while (binLen < rank)
                {
                    xBin = '0' + xBin;
                    binLen++;
                }
                var yBin = Utils.ToString(Math.Round(yArr[id] * shift), 2);
                //yBin = Convert.ToString(Convert.ToInt64(Math.Round(yArr[id] * shift)), 2);
                binLen = yBin.Length;
                while (binLen < rank)
                {
                    yBin = '0' + yBin;
                    binLen++;
                }
                xBinArr[j] = xBin;
                yBinArr[j] = yBin;
            }

            return this.ConstructKDTree(idSet, 0, rank, xBinArr, yBinArr, graph.IdToIndex, graph.GetChargeFunc);
        }

        private TreeNode ConstructKDTree(List<int> idSet, int depth, long rank, IReadOnlyList<string> xBinArr, IReadOnlyList<string> yBinArr,
            IReadOnlyDictionary<int, int> idToIndex, Func<int, double> getChargeFunc)
        {
            var len = idSet.Count;
            if (len < 4)
                return new TreeNode { IdSet = idSet, IsLeaf = true};

            var axis = depth % 2;
            var sigBit = depth / 2;
            double charges = 0;
            List<int> leftSet = new List<int>(), rightSet = new List<int>();

            while (leftSet.Count == 0 || rightSet.Count == 0)
            {
                if ((sigBit * 1.0).Equals(rank))
                    return new TreeNode(){ IdSet = idSet, IsLeaf = true};

                charges = 0;
                leftSet = new List<int>();
                rightSet = new List<int>();
                for (var j = 0; j < len; j++)
                {
                    var id = idSet[j];
                    var index = idToIndex[id];
                    charges += getChargeFunc(index);
                    var binNum = (axis == 0) ? xBinArr[index] : yBinArr[index];

                    if (binNum[sigBit] == '0')
                        leftSet.Add(id);
                    else
                        rightSet.Add(id);
                }
                axis = (axis + 1) % 2;
                if (axis == 0)
                    sigBit += 1;
            }

            var circle = this.GetNodesRegion(idSet);

            var akValueArr = new List<Complex>();
            var subtractArr = Enumerable.Repeat(new Complex(), len).ToList();
            var powArr = Enumerable.Repeat(new Complex(), len).ToList();
            var z0 = new Complex
            {
                Real = circle.Cx,
                Imaginary = circle.Cy
            };
            var complexCharge = new Complex();
            Dictionary<int, double> xArr = this.XCoordArr, yArr = this.YCoordArr;

            for (var j = 0; j < len; j++)
            {
                var id = idSet[j];
                subtractArr[j] = Complex.Subtract(new Complex(xArr[id], yArr[id]), z0);
            }
            for (var k = 0; k < 4; k++)
            {
                var akValue = new Complex();
                for (var j = 0; j < len; j++)
                {
                    complexCharge.Real = -getChargeFunc(idToIndex[idSet[j]]);
                    powArr[j] = k == 0 ? subtractArr[j] : Complex.Multiply(powArr[j], subtractArr[j]);
                    akValue = Complex.Add(akValue, Complex.Multiply(complexCharge, powArr[j]));
                }
                akValueArr.Add(akValue);
            }

            return new TreeNode {
                IdSet = idSet,
                LeftChild = this.ConstructKDTree(leftSet, depth + 1, rank, xBinArr, yBinArr, idToIndex, getChargeFunc),
                RightChild = this.ConstructKDTree(rightSet, depth + 1, rank, xBinArr, yBinArr, idToIndex, getChargeFunc),
                Cx = circle.Cx,
                Cy = circle.Cy,
                Radius = circle.Radius,
                Charges = charges,
                AkValueArr = akValueArr,
                SubtractArr = subtractArr
            };
        }

        private NodesRegion GetNodesRegion(List<int> idSet)
        {
            Dictionary<int, double> xArr = this.XCoordArr, yArr = this.YCoordArr;
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            var len = idSet.Count;
            for (var j = 0; j < len; j++)
            {
                var id = idSet[j];
                var x = xArr[id];
                var y = yArr[id];
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
            }
            var centerX = (minX + maxX) / 2;
            var centerY = (minY + maxY) / 2;
            var radius = Math.Sqrt(Math.Pow(maxX - minX, 2) + Math.Pow(maxY - minY, 2)) / 2;
            return new NodesRegion()
            {
                Cx = centerX,
                Cy = centerY,
                Radius = radius
            };
        }

        private Complex CalculateRepulsion(int id, TreeNode kdTree, bool useK, IDictionary<int, double> xArr,
            IDictionary<int, double> yArr, IReadOnlyDictionary<int, int> idToIndex, Func<int, double> getChargeFunc)
        {
            double vertexX = xArr[id], vertexY = yArr[id];
            var vertexCharge = getChargeFunc(idToIndex[id]);
            Complex repulsion = new Complex(this.Config.Epsilon0, this.Config.Epsilon0), complexDistNorm = new Complex(), complexRepNorm = new Complex(), complexCharge = new Complex(), coordinate = new Complex(vertexX, vertexY);
            var queue = new List<TreeNode> { kdTree };
            var leaf = new List<double>();
            var nonLeaf = new List<double>();
            while (queue.Count > 0)
            {
                var node = queue.Last();
                queue.RemoveAt(queue.Count - 1);

                // get distance between node center and the current vertex
                var distance = Complex.Subtract(coordinate, new Complex
                {
                    Real = node.Cx,
                    Imaginary = node.Cy
                });
                if (node.IsLeaf)
                {
                    foreach (var _id in node.IdSet)
                    {
                        var charge = getChargeFunc(idToIndex[_id]);
                        var source = new Complex(xArr[id], yArr[_id]);
                        var dist = source.Clone().Subtract(coordinate);
                        if (dist.Norm().Equals(0))
                            dist = new Complex(-1, -1);
                        var t = charge/Math.Pow(dist.Norm(), 1);
                        //Trace.WriteLine($"Leaf: {charge}\t{t}");
                        repulsion.Add(dist.Normalize().Multiply(t));
                        leaf.Add(node.Charges);
                    }
                }
                else
                {
                    if (distance.NormSquare() > node.Radius * node.Radius)
                    {
                        distance.Log().AddTo(repulsion);
                        var distanceInverted = new Complex(1, 0).Divide(distance);
                        var distancePow = distanceInverted;
                        for (var i = 0; i < 4; i++)
                        {
                            var t = node.AkValueArr[i].Clone().Multiply(distancePow);
                            //Trace.WriteLine(t.ToString());
                            t.AddTo(repulsion);
                            distancePow.Multiply(distanceInverted);
                        }

                        nonLeaf.Add(node.Charges);
                    }
                    else
                    {
                        if (node.LeftChild != default(TreeNode))
                            queue.Add(node.LeftChild);
                        if (node.RightChild != default(TreeNode))
                            queue.Add(node.RightChild);
                    }
                }
            }
                
            return repulsion.Multiply(this.Config.Epsilon0);
        }

        private Complex CalculateRepulsion1(int id, TreeNode kdTree, bool useK, IDictionary<int, double> xArr,
            IDictionary<int, double> yArr, IReadOnlyDictionary<int, int> idToIndex, Func<int, double> getChargeFunc)
        {
            double vertexX = xArr[id], vertexY = yArr[id];
            var vertexCharge = getChargeFunc(idToIndex[id]);
            Complex repulsion = new Complex(), complexDistNorm = new Complex(), complexRepNorm = new Complex(), complexCharge = new Complex(), coordinate = new Complex(vertexX, vertexY);
            var queue = new List<TreeNode> { kdTree };

            while (queue.Count > 0)
            {
                var node = queue.Last();
                queue.RemoveAt(queue.Count - 1);
                if (node.LeftChild == default(TreeNode))
                {
                    var idSet = node.IdSet;
                    var len = idSet.Count;
                    for (var j = 0; j < len; j++)
                    {
                        var otherId = idSet[j];
                        var distance1 = Complex.Subtract(coordinate, new Complex
                        {
                            Real = xArr[otherId],
                            Imaginary = yArr[otherId]
                        });
                        var dist1Norm = distance1.Norm();

                        if (dist1Norm.Equals(0))
                        {
                            if (id == otherId)
                                continue;
                            xArr[otherId] += 1;
                            yArr[otherId] += 1;
                            distance1 = new Complex(-1, -1);
                            dist1Norm = Math.Sqrt(2);
                        }
                        complexDistNorm.Real = dist1Norm;
                        complexRepNorm.Real = getChargeFunc(idToIndex[otherId]) / dist1Norm;
                        repulsion = Complex.Add(repulsion,
                            Complex.Multiply(Complex.Divide(distance1, complexDistNorm), complexRepNorm));
                    }
                }
                else
                {
                    var cx = node.Cx;
                    var cy = node.Cy;
                    var tmp1 = vertexX - cx;
                    var tmp2 = vertexY - cy;
                    var distance2 = tmp1 * tmp1 + tmp2 * tmp2;

                    if (distance2 > node.Radius * node.Radius)
                    {
                        var akValueArr = node.AkValueArr;
                        complexCharge.Real = node.Charges;
                        var complexDist = Complex.Subtract(coordinate, new Complex(cx, cy));
                        var complexPow = complexDist;
                        var centerRepulsion = Complex.Divide(complexCharge, complexDist);

                        if (useK)
                        {
                            foreach (var akValue in akValueArr)
                            {
                                // TODO: i think the multiply operation should be put behind the subtract
                                complexPow = Complex.Multiply(complexPow, complexDist);
                                var ratio = Complex.Divide(akValue, complexPow);
                                centerRepulsion = Complex.Subtract(centerRepulsion, ratio);
                            }
                        }
                        var t = new Complex(centerRepulsion.Real*vertexCharge, -centerRepulsion.Imaginary*vertexCharge);
                        //Trace.WriteLine($"{t.Real / t.Imaginary}\t{complexDist.Real / complexDist.Imaginary}");
                        repulsion = Complex.Add(repulsion, t);
                    }
                    else
                    {
                        // TODO: ?
                        if (node.LeftChild != null)
                            queue.Add(node.RightChild);
                        if (node.RightChild != null)
                            queue.Add(node.LeftChild);
                    }
                }
            }
            return repulsion.Multiply(this.Config.Epsilon0); //.Normalize(100);
        }

        private Complex CalculateAttraction(int id, List<int> adjSet, Func<int, int, double> getWeightFunc, int firstIndex, Dictionary<int, double> xArr,
            Dictionary<int, double> yArr)
        {
            Complex attraction = new Complex(),
                coordinate = new Complex(xArr[id], yArr[id]),
                complexDistNorm = new Complex(),
                complexAttrNorm = new Complex();
            var len = adjSet.Count;
            for (var j = 0; j < len; j++)
            {
                var otherId = adjSet[j];
                var distance = Complex.Subtract(new Complex(xArr[otherId], yArr[otherId]), coordinate);
                var distNorm = distance.Norm();
                if (distNorm.Equals(0))
                {
                    xArr[id] += 1;
                    yArr[id] += 1;
                    distance = new Complex(1, 1);
                    distNorm = Math.Sqrt(2);
                }
                complexDistNorm.Real = distNorm;
                complexAttrNorm.Real = distNorm * distNorm * Math.Log(distNorm / getWeightFunc(firstIndex, j));
                attraction = Complex.Add(attraction, Complex.Multiply(Complex.Divide(distance, complexDistNorm), complexAttrNorm));
            }
            //for (var j = 0; j < len; j++)
            //{
            //    var otherId = adjSet[j];
            //    var desiredDistance = getWeightFunc(firstIndex, j);
            //    var source = new Complex(xArr[otherId], yArr[otherId]);
            //    VertexPairAttraction(source, coordinate, desiredDistance).AddTo(attraction);
            //}
            return attraction;//.Normalize(100);
        }

        private static Complex VertexPairAttraction(Complex distance, double desiredDistance)
        {
            var distNorm = distance.Norm();

            if (distNorm.Equals(0))
            {
                distance = new Complex(1, 1);
                distNorm = Math.Sqrt(2);
            }
            var complexAttrNorm = new Complex(Math.Log(distNorm / desiredDistance), 0);
            return complexAttrNorm.Multiply(Math.Pow(distNorm, 2)).Multiply(distance.Normalize());
        }

        private static Complex VertexPairAttraction(Complex source, Complex target, double desiredDistance)
        {
            var distance = Complex.Subtract(source, target);
            return VertexPairAttraction(distance, desiredDistance);
        }

        private double MoveVertex(Graph graph, TreeNode kdTree, double t, bool useK)
        {
            var idSet = graph.IdSet;
            var idToIndex = graph.IdToIndex;
            var adjList = graph.AdjList;
            var getChargeFunc = graph.GetChargeFunc;
            var getWeightFunc = graph.GetWeightFunc;
            Dictionary<int, double> xArr = this.XCoordArr, yArr = this.YCoordArr;
            double globalForce = 0;
            Complex complexForceNorm = new Complex(),
                complexMin = new Complex();

            //var newXArr = xArr.ToDictionary(p => p.Key, p => p.Value);
            //var newYArr = yArr.ToDictionary(p => p.Key, p => p.Value);
            //Parallel.ForEach(idSet, (id) =>
            //foreach (var id in idSet)
            for (var i = 0; i < idSet.Count; i ++)
            {
                var id = idSet[i];
                var index = idToIndex[id];
                var force = CalculateRepulsion1(id, kdTree, useK, xArr, yArr, idToIndex, getChargeFunc);
                var attraction = this.CalculateAttraction(id, adjList[index], getWeightFunc, index, xArr, yArr);
                //Trace.WriteLine("attraction: " + attraction);
                force = Complex.Add(force, attraction);
                
                var forceNorm = force.Norm();

                globalForce += forceNorm;

                var min = t;
                if (forceNorm < min)
                {
                    min = forceNorm;
                }
                complexMin.Real = min;
                complexForceNorm.Real = forceNorm;
                var moveStep = Complex.Multiply(complexMin, Complex.Divide(force, complexForceNorm));
                if (id == 165)
                {
                    Utils.Log($"{id}\t{this.XCoordArr[id]}\t{this.YCoordArr[id]}\n");
                }
                this.XCoordArr[id] += moveStep.Real;
                this.YCoordArr[id] += moveStep.Imaginary;
                //Utils.Log($"{id}\t{this.XCoordArr[id]}\t{this.YCoordArr[id]}\n");
            }
            //this.XCoordArr = newXArr;
            //this.YCoordArr = newYArr;
            //Trace.WriteLine($"{CurrLayer}\t{globalForce / idSet.Count}");
            return globalForce;
        }
    }
}