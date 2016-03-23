using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TestRail.ResultsImporter
{
    /// <summary>
    /// Represents a single test result
    /// </summary>
    public class TestResult
    {
        [JsonProperty("case_id")]
        public int CaseId { get; set; }

        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("elapsed")]
        public string Elapsed { get; set; }
    }
}
