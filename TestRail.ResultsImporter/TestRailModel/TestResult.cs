using Newtonsoft.Json;

namespace TestRail.ResultsImporter.TestRailModel
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
