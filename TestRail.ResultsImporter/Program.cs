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

            // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
            var testCases = GetTestCases(SectionId);


            // Get only those tests that don't already exist in TestRail
            var missingTests = resultsParser.GetMissingTests(testCases).ToList();

            if (missingTests.Any())
            {
                Task.Run(async () =>
                {
                    await AddMissingTests(missingTests);
                }).GetAwaiter().GetResult();


                // Refresh list of test cases in TestRail
                testCases = GetTestCases(SectionId);
            }

            var testResultsWithCaseIds = resultsParser.GetTestResultsWithCaseIds(testCases);


            AddTestResults(testResultsWithCaseIds, testRunId);
            
        }


        private static IEnumerable<TestCase> GetTestCases(int sectionId)
        {
            // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
            var testCasesResponse = (JArray) _client.SendGet("get_cases/1&section_id=" + SectionId);
            
            var testCasesList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testCasesResponse.ToString());

            return testCasesList.Select(
                test => new TestCase
                {
                    Title = test["title"].ToString(),
                    Id = int.Parse(test["id"].ToString())
                }).ToList();
        }

        private static async Task AddMissingTests(IEnumerable<TestCase> missingTests)
        {
            var tasks = missingTests.Select(AddTestCase);
            await Task.WhenAll(tasks);
        }

        private static async Task AddTestCase(TestCase testCase)
        {
            var response = (JObject)_client.SendPost("add_case/" + SectionId, testCase);
        }

        private static void AddTestResults(IEnumerable<TestResult> testResultsToAdd, int testRunId)
        {
            var testResults = new TestResults
            {
                Results = testResultsToAdd.ToList()
            };
            var response = (JArray)_client.SendPost("add_results_for_cases/" + testRunId, testResults);
            
            //TODO: Check response.
        }

        private static int AddTestRun(string testRunName)
        {
            var testRun = new TestRun {Name = testRunName};
            var response = (JObject)_client.SendPost("add_run/" + ProjectId, testRun);

             return (int)response["id"];
        }


        private static TestRunType DeserializeTestReportFile(string reportFile)
        {
            // MSTest is known to not properly escape characters in its reports
            var settings = new XmlReaderSettings { CheckCharacters = false };
            return XmlStringConverter.Load<TestRunType>(reportFile, settings);
        }

        private static IEnumerable<UnitTestResultType> GetResultsItems(TestRunType testRun)
        {
            ResultsType returnValue = new ResultsType();

            foreach (var item in testRun.Items)
            {
                TypeSwitch.Switch(
                    item,
                    TypeSwitch.Case<ResultsType>((results) => returnValue = results));
            }

            return returnValue.Items.Select(item => (UnitTestResultType)item);
            //return (UnitTestResultType[])returnValue.Items;
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