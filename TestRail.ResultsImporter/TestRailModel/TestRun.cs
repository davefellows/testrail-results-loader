using Newtonsoft.Json;

namespace TestRail.ResultsImporter.TestRailModel
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
