using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurock.TestRail;
using TestRail.ResultsImporter.TestRailModel;
using Xunit;

namespace TestRail.ResultsImporter.IntegrationTests
{
    public class ImportTestCaseTests
    {
        private const int SectionId = 5;

        [Fact]
        public async Task TestCanAddNewTestCase()
        {
            var client = new APIClient(ConfigurationManager.AppSettings["testrail-endpoint"])
            {
                User = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"]
            };

            await ImportManager.AddTestCase(new TestCase {Title = new string('A', 251)}, SectionId);
        }
    }
}
