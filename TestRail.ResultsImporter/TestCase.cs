using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestRail.ResultsImporter
{
    /// <summary>
    /// Represents a test case. Used for serialization when adding new test cases.
    /// </summary>
    public class TestCase
    {
        [JsonProperty("title")]
        public string Title { get; set; }   
    }
}
