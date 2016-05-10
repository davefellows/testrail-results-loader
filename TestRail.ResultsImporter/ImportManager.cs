using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
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

        private static IApiClient _client;
        private readonly int _projectId;


        public ImportManager(IApiClient client, int projectId)
        {
            _projectId = projectId;
            _client = client;
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
                Log.Info($"Importing file: {resultsFile}");

                ResultsParser resultsParser = new TrxResultsParser(resultsFile);

                if (testRunId == 0)
                {
                    var testRunName = $"{branchAndBuildLabel} - {resultsParser.StartTime.ToString(new CultureInfo("en-US"))}";
                    // Add a TestRail test run for this instantiation
                    testRunId = AddTestRun(testRunName, _projectId).Result;
                }
                
                // Get the TestRail section based on the test dll name
                int sectionId = await GetOrCreateTestSection(resultsParser.FileName);

                // Retrieve all existing test cases from TestRail for the relevant section
                var testCases = (await GetTestCases(sectionId)).ToList();

                // Get only those tests that don't already exist in TestRail. 
                var missingTests = resultsParser.GetMissingTests(testCases).ToList();

                Log.Info($"Missing Test Cases: {missingTests.Count}");

                if (missingTests.Any())
                {
                    // Add the missing test cases
                    await AddMissingTestCases(missingTests, sectionId);

                    Log.Info($"Added {missingTests.Count} missing tests");

                    // Refresh list of test cases in TestRail
                    testCases = (await GetTestCases(sectionId)).ToList();
                }

                // Combine the Test report results with the Test Case id from TestRail
                var testResultsWithCaseIds = resultsParser.GetTestResultsWithCaseIds(testCases);

                // Add the test results to TestRail
                await AddTestResults(testResultsWithCaseIds, testRunId);

                Log.Info($"Finished adding results for file: {resultsFile}");

            }
        }

        private async Task<int> GetOrCreateTestSection(string testProjectName)
        {
            return await GetTestSection(testProjectName) ?? await CreateTestSection(testProjectName);
        }

        private async Task<int> CreateTestSection(string testProjectName)
        {
            try
            {
                var sectionId =
                    (int)
                    ((JObject)
                     await _client.SendPost("add_section/" + _projectId, new Section { Name = testProjectName }))["id"];

                Log.Info($"Added Section: '{testProjectName}' id: '{sectionId}'");

                return sectionId;
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding TestRail Section '{testProjectName}' for Project '{_projectId}'", ex);
                throw;
            }
        }
        private async Task<int?> GetTestSection(string testProjectName)
        {
            try
            {
                // Retrieve all existing test Sections for this project
                var sectionsResponse = (JArray)await _client.SendGet("get_sections/" + _projectId);

                var sectionsList =
                    JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(sectionsResponse.ToString());

                if (!sectionsList.Exists(section => (string)section["name"] == testProjectName)) return null;

                //TODO: Project onto Sections collection and cache
                return
                    sectionsList.Where(section => (string)section["name"] == testProjectName)
                        .Select(section => int.Parse(section["id"].ToString())).FirstOrDefault();
            }
            catch (AggregateException ex)
            {

                var message = new StringBuilder();

                foreach (var iex in ex.InnerExceptions)
                {
                    var bcex = iex as WebException;
                    message.AppendLine(bcex?.Status.ToString() ?? iex.ToString());
                }
                Log.Error($"Error getting TestRail Section: '{testProjectName}' for Project Id: '{_projectId}'.", ex);
                throw;
            }
        }

        private async Task<IEnumerable<TestCase>> GetTestCases(int sectionId)
        {
            try
            {
                // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
                var testCasesResponse = (JArray) await _client.SendGet($"get_cases/{_projectId}&section_id={sectionId}");

                var testCasesList =
                    JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testCasesResponse.ToString());

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

        private async Task AddMissingTestCases(IEnumerable<TestCase> missingTests, int sectionId)
        {
            var tasks = missingTests.Select(test => AddTestCase(test, sectionId));
            await Task.WhenAll(tasks);
        }

        internal async Task AddTestCase(TestCase testCase, int sectionId)
        {
            try
            {
                //TODO: Add retry if transient error
                await _client.SendPost("add_case/" + sectionId, testCase);
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding TestRail Test Case '{testCase.Title}' to Section Id '{sectionId}'.\n\tErrorMessage: {ex.Message}");
                //TODO: Determine how to handle failures here. Or do we just truncate for now?
                //throw;
            }
        }

        private async Task AddTestResults(IEnumerable<TestResult> testResultsToAdd, int testRunId)
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

        private async Task<int> AddTestRun(string testRunName, int projectId)
        {
            var testRun = new TestRun { Name = testRunName };

            try
            {
                var testRunId = (int)((JObject)await _client.SendPost("add_run/" + projectId, testRun))["id"];

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
