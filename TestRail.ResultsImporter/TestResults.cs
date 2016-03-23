using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestRail.ResultsImporter
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
