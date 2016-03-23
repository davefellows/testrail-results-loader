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
        private const string TestResultsFile = "TestResultsFile-small.trx";

        static void Main(string[] args)
        {

            _client = new APIClient(ConfigurationManager.AppSettings["testrail-endpoint"])
            {
                User = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"]
            };


            TestRunType testRunType = DeserializeTestReportFile(TestResultsFile);

            
            var testRunId = AddTestRun(testRunType.name);

            // Individual Test Results
            var resultsFromReport = GetResultsItems(testRunType).ToList();

            // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
            var testCases = GetTestCases(SectionId);


            // Get only those tests that don't already exist in TestRail
            var missingTests = (from testResult in resultsFromReport
                                where !(
                                    from testcase in testCases
                                    select testcase["title"]
                                    ).Contains(testResult.testName)
                                select testResult).ToList();

            if (missingTests.Any())
            {
                Task.Run(async () =>
                {
                    await AddMissingTests(missingTests);
                }).GetAwaiter().GetResult();


                // Refresh list of test cases in TestRail
                testCases = GetTestCases(SectionId);
            }

            var testResultsWithCaseIds =
                                from testResult in resultsFromReport
                                join testcase in testCases
                                on testResult.testName equals testcase["title"]
                                select new TestResult
                                {
                                    CaseId = int.Parse(testcase["id"].ToString()),
                                    StatusId = testResult.outcome == TestOutcome.Passed.ToString() ? 1 : 5,
                                    Elapsed = FormatDuration(testResult.duration)
                                };


            AddTestResults(testResultsWithCaseIds, testRunId);
            
        }

        /// <summary>
        /// Format the duration to TestRail's expected format: {hours}h {minutes}m {seconds}s
        /// </summary>
        private static string FormatDuration(string duration)
        {
            var t = TimeSpan.Parse(duration);
            
            var elapsed = string.Empty;

            // Time resolution in TestRail is down to seconds so if less than
            // 1 second then show 1 second rather than blank.
            if (t.TotalSeconds < 1) elapsed = "1s";
            else
            {
                if (t.Hours > 0) elapsed += $"{t.Hours}h ";
                if (t.Minutes > 0) elapsed += $"{t.Minutes}m ";
                if (t.Seconds > 0) elapsed += $"{t.Seconds}s";
            }

            return elapsed;
        }

        private static List<Dictionary<string, object>> GetTestCases(int sectionId)
        {
            // Retrieve all existing unit test cases from TestRail (for Azure Batch project)
            var testCasesResponse = (JArray) _client.SendGet("get_cases/1&section_id=" + SectionId);
            
            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testCasesResponse.ToString());
        }

        private static async Task AddMissingTests(IEnumerable<UnitTestResultType> missingTests)
        {
            var tasks = missingTests.Select(AddTestCase);
            await Task.WhenAll(tasks);
        }

        private static async Task AddTestCase(UnitTestResultType missingTest)
        {
            var testCase = new TestCase
            {
                Title = missingTest.testName
            };
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