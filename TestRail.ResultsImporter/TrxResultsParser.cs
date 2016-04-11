using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    internal class TrxResultsParser : ResultsParser
    {
        private readonly TestRunType _testRunType;
        private readonly IEnumerable<UnitTestResultType> _resultsFromReport;
        private TestRunTypeTimes _testRunTimes;

        public TrxResultsParser(string filename)
        {
            // Deserialize
            _testRunType = DeserializeFile<TestRunType>(filename);

            // Parse Results collection and Times element from the report
            var resultsType = new ResultsType();

            foreach (var item in _testRunType.Items)
            {
                TypeSwitch.Switch(item,
                    TypeSwitch.Case<ResultsType>((results) => resultsType = results),
                    TypeSwitch.Case<TestRunTypeTimes>((results) => _testRunTimes = results));
            }

            _resultsFromReport = resultsType.Items.Select(item => (UnitTestResultType)item);
        }

        public override string TestName => _testRunType.name;

        public override DateTime StartTime
        {
            get
            {
                DateTime start;
                if (!DateTime.TryParse(_testRunTimes.start, out start))
                {
                    Log.Warn($"Failed to parse start datetime From Test Report. Value: {_testRunTimes.start}");
                    return DateTime.MinValue;
                }
                return start;
            }
        } 

        public override IEnumerable<TestCase> GetMissingTests(IEnumerable<TestCase> existingTestCases)
        {
            // Get only those tests that don't already exist in TestRail
            return from testResult in _resultsFromReport
                   where !(
                        from testcase in existingTestCases
                        select testcase.Title
                        ).Contains(testResult.testName)
                    select new TestCase {Title = testResult.testName};

        }

        public override IEnumerable<TestResult> GetTestResultsWithCaseIds(IEnumerable<TestCase> existingTestCases)
        {
            // Combine the Test report results with the Test Case id from TestRail
            return from testResult in _resultsFromReport
                    join testcase in existingTestCases
                        on testResult.testName equals testcase.Title
                    select new TestResult
                    {
                        CaseId = testcase.Id,
                        StatusId = testResult.outcome == TestOutcome.Passed.ToString() ? 1 : 5,
                        Elapsed = FormatDuration(testResult.duration),
                        Comment = ExtractDatesAndStdoutError(testResult)
                    };

        }

        private static string ExtractDatesAndStdoutError(UnitTestResultType resultItem)
        {
            if (resultItem == null) throw new ArgumentNullException(nameof(resultItem));

            string error = string.Empty, stdout = string.Empty;

            // Some items don't contain either Log or Error
            if (resultItem.Items != null)
            {
                // extract error if available
                try
                {
                    error = (((OutputType) resultItem.Items[0]).ErrorInfo == null
                        ? string.Empty
                        : ((XmlNode[]) ((OutputType) resultItem.Items[0]).ErrorInfo.Message)[0].Value);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error parsing ErrorInfo from test result: {resultItem.testName}", ex);
                    error = string.Empty;
                }

                // extract StdOut if available
                try
                {
                    stdout = ((XmlNode[]) ((OutputType) resultItem.Items[0]).StdOut)[0].Value;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error parsing Stdout from test result: {resultItem.testName}", ex);
                    stdout = string.Empty;
                }
            }

            return $"Start:\t{resultItem.startTime}\n" +
                   $"End:\t{resultItem.endTime}\n\n" +
                   $"Error:\n{error}\n\n" +
                   $"Log:\n{stdout}";
        }


    }

}
