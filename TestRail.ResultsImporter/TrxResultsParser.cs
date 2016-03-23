using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    internal class TrxResultsParser : ResultsParser
    {
        public TrxResultsParser(string filename) : base(filename)
        {
        }

        private TestRunType _testRunType;

        public override string TestName => _testRunType.name;

        public override IEnumerable<TestCase> GetMissingTests(IEnumerable<TestCase> existingTestCases)
        {
            var resultsFromReport = GetResultsItems(_testRunType).ToList();

            // Get only those tests that don't already exist in TestRail
            return from testResult in resultsFromReport
                    where !(
                        from testcase in existingTestCases
                        select testcase.Title
                        ).Contains(testResult.testName)
                    select new TestCase {Title = testResult.testName};

        }

        public override IEnumerable<TestResult> GetTestResultsWithCaseIds(IEnumerable<TestCase> existingTestCases)
        {
            var resultsFromReport = GetResultsItems(_testRunType).ToList();

            return 
                                from testResult in resultsFromReport
                                join testcase in existingTestCases
                                on testResult.testName equals testcase.Title
                                select new TestResult
                                {
                                    CaseId = testcase.Id,
                                    StatusId = testResult.outcome == TestOutcome.Passed.ToString() ? 1 : 5,
                                    Elapsed = FormatDuration(testResult.duration)
                                };

        }

        protected override void Load(string filename)
        {
            _testRunType = LoadFile<TestRunType>(filename);
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
