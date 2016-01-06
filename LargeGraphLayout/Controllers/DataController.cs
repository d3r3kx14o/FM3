using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using LargeGraphLayout.App_Start;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.Controllers
{
    public class DataController : ApiController
    {
        [HttpPost]
        [ActionName("retrieve")]
        public JsonResult<String> GetData(RequestGraphLayerModel model)
        {
            List<Graph> graphs;
            if (!Data.Graphs.TryGetValue(model.DataSet, out graphs))
                graphs = Data.Graphs[Data.PreloadedGraph.First()];
            Trace.WriteLine($"Requesting {model.Layer}\t{model.DataSet}");
            var layer = Math.Max(0, model.Layer);

            layer = Math.Min(graphs.Count - 1, layer);
            layer = Math.Max(0, layer);
            Graph graph = graphs[layer];
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ScriptingJsonSerializationSection section = ConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization") as ScriptingJsonSerializationSection;
            if (section != null)
            {
                serializer.MaxJsonLength = 99999999;
                serializer.RecursionLimit = section.RecursionLimit;
            }
            return Json(serializer.Serialize(graph.Core));
        }
    }
}