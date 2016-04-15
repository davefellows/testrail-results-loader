using Newtonsoft.Json;

namespace TestRail.ResultsImporter.TestRailModel
{

    public class Section
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public int Id { get; set; }
    }
}
