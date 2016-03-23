using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestRail.ResultsImporter
{
    /// <summary>
    /// Represents a single test run in TestRail. 
    /// </summary>
    public class TestRun
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
