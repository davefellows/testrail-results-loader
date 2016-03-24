using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    internal class TrxResultsParser : ResultsParser
    {
        private TestRunType _testRunType;
        private IEnumerable<UnitTestResultType> _resultsFromReport;

        public TrxResultsParser(string filename)
        {
            _testRunType = LoadFile<TestRunType>(filename);
            _resultsFromReport = GetResultsItems();
        }

        public override string TestName => _testRunType.name;

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
                        Comment = GetLogAndError(testResult)
                    };

        }

        private static string GetLogAndError(UnitTestResultType resultItem)
        {
            
            if (resultItem.Items == null) return string.Empty;
            string error, stdout;

            try
            {
                error = (((OutputType)resultItem.Items[0]).ErrorInfo == null
                    ? string.Empty
                    : ((XmlNode[])((OutputType)resultItem.Items[0]).ErrorInfo.Message)[0].Value) + "\n\n";
            }
            catch (Exception ex)
            {
                Log.Error($"Error parsing ErrorInfo from test result: {resultItem.testName}", ex);
                error = string.Empty;
            }

            try
            {
                stdout = ((XmlNode[])((OutputType)resultItem.Items[0]).StdOut)[0].Value;
            }
            catch (Exception ex)
            {
                Log.Error($"Error parsing StdOut from test result: {resultItem.testName}", ex);
                stdout = string.Empty;
            }

            return error + stdout;
        }


        private IEnumerable<UnitTestResultType> GetResultsItems()
        {
            ResultsType returnValue = new ResultsType();

            foreach (var item in _testRunType.Items)
            {
                TypeSwitch.Switch(
                    item,
                    TypeSwitch.Case<ResultsType>((results) => returnValue = results));
            }

            return returnValue.Items.Select(item => (UnitTestResultType)item);
        }



    }

}
