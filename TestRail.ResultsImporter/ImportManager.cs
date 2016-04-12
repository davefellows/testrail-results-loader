using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Gurock.TestRail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    using System.IO;

    internal class ImportManager
    {
        private const string XUnitResultsFileExtension = ".trx";

        private static APIClient _client;
        private readonly int _projectId;
        private readonly int _sectionId;


        public ImportManager(int projectId, int sectionId)
        {
            _projectId = projectId;
            _sectionId = sectionId;

            _client = new APIClient(ConfigurationManager.AppSettings["testrail-endpoint"])
            {
                User = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"]
            };
        }

        public async Task Run(string testResultsPath, string branchAndBuildLabel)
        {
            int testRunId = 0;

            var resultsFiles = Directory.GetFiles(testResultsPath, $"*{XUnitResultsFileExtension}");

            if (!resultsFiles.Any())
            {
                Log.Error($"No {XUnitResultsFileExtension} results files in given path: {testResultsPath}");
            }

            foreach (string resultsFile in resultsFiles)
            {
                ResultsParser resultsParser = new TrxResultsParser(resultsFile);

                if (testRunId == 0)
                {
                    var testRunName = $"{branchAndBuildLabel} - {resultsParser.StartTime.ToString(new CultureInfo("en-US"))}";
                    // Add a TestRail test run for this instantiation
                    testRunId = AddTestRun(testRunName, _projectId).Result;
                }
                
                // Retrieve all existing "unit test" test cases from TestRail (for Azure Batch project)
                var testCases = (await GetTestCases(_sectionId)).ToList();

                // Get only those tests that don't already exist in TestRail. 
                var missingTests = resultsParser.GetMissingTests(testCases).ToList();

                if (missingTests.Any())
                {
                    // Add the missing test cases
                    await AddMissingTestCases(missingTests, _sectionId);

                    // Refresh list of test cases in TestRail
                    testCases = (await GetTestCases(_sectionId)).ToList();
                }

                // Combine the Test report results with the Test Case id from TestRail
                var testResultsWithCaseIds = resultsParser.GetTestResultsWithCaseIds(testCases);

                // Add the test results to TestRail
                await AddTestResults(testResultsWithCaseIds, testRunId);
            }
        }


        private static async Task<IEnumerable<TestCase>> GetTestCases(int sectionId)
        {
            try
            {
                // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
                var testCasesResponse = (JArray)await _client.SendGet("get_cases/1&section_id=" + sectionId);

                var testCasesList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testCasesResponse.ToString());

                return testCasesList.Select(
                    test => new TestCase
                    {
                        Title = test["title"].ToString(),
                        Id = int.Parse(test["id"].ToString())
                    });
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting TestRail Test Cases for Section Id '{sectionId}'.", ex);
                throw;
            }
        }

        private static async Task AddMissingTestCases(IEnumerable<TestCase> missingTests, int sectionId)
        {
            var tasks = missingTests.Select(test => AddTestCase(test, sectionId));
            await Task.WhenAll(tasks);
        }

        internal static async Task AddTestCase(TestCase testCase, int sectionId)
        {
            try
            {
                await _client.SendPost("add_case/" + sectionId, testCase);
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding TestRail Test Cases {testCase.Title} to Section Id '{sectionId}'.", ex);
                throw;
            }
        }

        private static async Task AddTestResults(IEnumerable<TestResult> testResultsToAdd, int testRunId)
        {
            var testResults = new TestResults
            {
                Results = testResultsToAdd.ToList()
            };

            try
            {
                await _client.SendPost("add_results_for_cases/" + testRunId, testResults);
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding {testResults.Results.Count()} TestRail test results. Test Run ID: {testRunId}", ex);
                throw;
            }
        }

        private static async Task<int> AddTestRun(string testRunName, int projectId)
        {
            var testRun = new TestRun { Name = testRunName };

            try
            {
                var response = await _client.SendPost("add_run/" + projectId, testRun);
                var testRunId = (int)((JObject)response)["id"];

                Log.Info($"Added Test Run: {testRunId}");

                return testRunId;
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding TestRail Test Run '{testRunName}' for Project '{projectId}'", ex);
                throw;
            }

        }
    }
}
