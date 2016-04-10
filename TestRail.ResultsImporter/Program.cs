using System;
using System.Threading.Tasks;

namespace TestRail.ResultsImporter
{
    class Program
    {
        private static string _testResultsFile = "TestResultsFile.trx";
        private const int ProjectId = 1;
        private const int SectionId = 2;

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                _testResultsFile = args[0];
            }
            else
            {
                Log.Info("No file argument passed.");
            }

            Task.Run(async () =>
            {
                await new ImportManager(_testResultsFile, ProjectId, SectionId).Run();
            }).GetAwaiter().GetResult();
        }

    }
}
