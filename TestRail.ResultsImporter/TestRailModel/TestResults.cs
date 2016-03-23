using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestRail.ResultsImporter.TestRailModel
{
    public class TestResults
    {
        [JsonProperty("results")]
        public List<TestResult> Results 
        {
            get;
            set;
        }


    }
}
