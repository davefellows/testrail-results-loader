using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Gurock.TestRail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    class Program
    {
        private static APIClient _client;
        private const int ProjectId = 1;
        private const int SectionId = 2;
        private const string TestResultsFile = "TestResultsFile.trx";

        static void Main(string[] args)
        {

            _client = new APIClient(ConfigurationManager.AppSettings["testrail-endpoint"])
            {
                User = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"]
            };

            
            ResultsParser resultsParser = new TrxResultsParser(TestResultsFile);

            // Add a TestRail test run for this instantiation
            var testRunId = AddTestRun(resultsParser.TestName);

            // Retrieve all existing "unit test" test cases from TestRail (for Azure Batch project)
            var testCases = GetTestCases(SectionId);

            // Get only those tests that don't already exist in TestRail
            var missingTests = resultsParser.GetMissingTests(testCases).ToList();

            if (missingTests.Any())
            {
                Task.Run(async () =>
                {
                    // Add the missing test cases
                    await AddMissingTestCases(missingTests);
                }).GetAwaiter().GetResult();


                // Refresh list of test cases in TestRail
                testCases = GetTestCases(SectionId);
            }

            // Combine the Test report results with the Test Case id from TestRail
            var testResultsWithCaseIds = resultsParser.GetTestResultsWithCaseIds(testCases);


            // Add the test results to TestRail
            AddTestResults(testResultsWithCaseIds, testRunId);
        }


        private static IEnumerable<TestCase> GetTestCases(int sectionId)
        {
            try
            {
                // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
                var testCasesResponse = (JArray)_client.SendGet("get_cases/1&section_id=" + SectionId);

                var testCasesList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testCasesResponse.ToString());

                return testCasesList.Select(
                    test => new TestCase
                    {
                        Title = test["title"].ToString(),
                        Id = int.Parse(test["id"].ToString())
                    }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting TestRail Test Cases for Section Id '{SectionId}'.", ex);
                throw;
            }
        }

        private static async Task AddMissingTestCases(IEnumerable<TestCase> missingTests)
        {
            var tasks = missingTests.Select(AddTestCase);
            await Task.WhenAll(tasks);
        }

        private static async Task AddTestCase(TestCase testCase)
        {
            try
            {
                _client.SendPost("add_case/" + SectionId, testCase);
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding TestRail Test Cases {testCase.Title} to Section Id '{SectionId}'.", ex);
                throw;
            }
        }

        private static void AddTestResults(IEnumerable<TestResult> testResultsToAdd, int testRunId)
        {
            var testResults = new TestResults
            {
                Results = testResultsToAdd.ToList()
            };

            try
            {
                _client.SendPost("add_results_for_cases/" + testRunId, testResults);
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding {testResults.Results.Count()} TestRail test results. Test Run ID: {testRunId}", ex);
                throw;
            }
        }

        private static int AddTestRun(string testRunName)
        {
            var testRun = new TestRun {Name = testRunName};

            try
            {
                var response = (JObject)_client.SendPost("add_run/" + ProjectId, testRun);
                return (int)response["id"];
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding TestRail Test Run '{testRunName}' for Project '{ProjectId}'", ex);
                throw;
            }

        }

    }
}


//var testResults = new Dictionary<string, List<Dictionary<string, object>>>
//            {
//                {
//                    "results",
//                    new List<Dictionary<string, object>>
//                    {
//                        new Dictionary<string, object>
//                        {
//                            {"test_id", 1},
//                            {"status_id", 2},
//                            {"comment", "This test worked fine!"},
//                            {"elapsed", "2s"}
//                        }
//                    }
//                }
//            };

//var results = new JObject
//{
//    ["results"] = JsonConvert.SerializeObject(new List<Dictionary<string, object>>
//                    {
//                        new Dictionary<string, object>
//                        {
//                            {"test_id", 1},
//                            {"status_id", 1},
//                            {"comment", "This test worked fine!"},
//                            {"elapsed", "2s"}
//                        },
//                        new Dictionary<string, object>
//                        {
//                            {"test_id", 2},
//                            {"status_id", 1},
//                            {"comment", "This test worked fine!"},
//                            {"elapsed", "15s"}
//                        }

//                    })
//};

//var testResult = new Dictionary<string, object>
//            {
//                {"status_id", 1},
//                {"comment", "This test worked fine!"},
//                {"elapsed", "2s"}
//            };