using Newtonsoft.Json;

namespace TestRail.ResultsImporter.TestRailModel
{
    /// <summary>
    /// Represents a test case. Used for serialization when adding new test cases.
    /// </summary>
    public class TestCase
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        public int Id { get; set; }
    }
}
