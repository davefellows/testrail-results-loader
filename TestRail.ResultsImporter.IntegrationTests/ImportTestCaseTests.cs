using System.Threading.Tasks;

using TestRail.ResultsImporter.TestRailModel;
using Xunit;

namespace TestRail.ResultsImporter.IntegrationTests
{
    public class ImportTestCaseTests
    {
        private const int ProjectId = 2;
        private const int SectionId = 5;

        [Fact]
        public async Task TestCanAddNewTestCase()
        {
            await new ImportManager(ProjectId).AddTestCase(new TestCase { Title = new string('A', 251) }, SectionId);
        }
    }
}
