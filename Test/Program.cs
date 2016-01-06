using System;
using System.Collections.Generic;
using System.IO;
using LargeGraphLayout.Algorithms;
using LargeGraphLayout.Models;
using Test.Util;

namespace Test
{
    class Program
    {
        private static string root = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        static void Main(string[] args)
        {
            //IRandomTest();
            //SubGraphTest();
            ToStringTest();
        }

        static void ToStringTest()
        {
            var d = new List<string>();
            StreamWriter w = new StreamWriter(root + "/" + "tostring.txt");
            for (var i = 0; i < 1000; i++)
            {
                w.WriteLine(Utils.ToString(i * 1.0 / 3, 2));
            }
            w.Close();
        }

        static void IRandomTest()
        {
            StreamWriter w = new StreamWriter(root + "/" + "random.txt");
            for (var i = 0; i < 20000; i ++)
            {
                w.WriteLine(IRandom.NextDouble().ToString("F10"));
            }
            w.Close();
        }

        static void SubGraphTest()
        {
            Graph g = new Graph(root, "wechat.txt");
            int[] l = new[] { 1000};
            for (var i = 0; i < l.Length; i++)
            {
                var subG = GraphUtil.SubGraph(g, l[i]);
                StreamWriter w = new StreamWriter(root + "/" + "sample_" + subG.IdSet.Count + ".txt");
                for (var j = 0; j < subG.AdjList.Count; j++)
                {
                    if (subG.AdjList[j].Count == 0)
                        throw new Exception("error");
                    var line = j + "\t" + string.Join("\t", subG.AdjList[j]);
                    if (j % 100 == 0)
                        Console.WriteLine(line);
                    w.WriteLine(line);
                }
                w.Close();
            }
            Console.WriteLine(root);
        }
    }
}
