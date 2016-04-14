using System;
using System.Threading.Tasks;

namespace TestRail.ResultsImporter
{
    using System.IO;

    class Program
    {
        //TODO: retrieve project id from testrail
        private const int ProjectId = 2; // test project
        //TODO: create and retrieve section ids for each group of tests
        //private const int SectionId = 2;
        private const int SectionId = 5;

        static int Main(string[] args)
        {

            if (args.Length < 2)
            {
                Log.Error("Expected args missing. Expect folder path to the results files and the test run name (e.g. branch + build #).");
            }


            string testResultsPath = args[0];

            if (!Directory.Exists(testResultsPath))
            {
                Log.Error($"Supplied directory doesn't exist or is invalid: {testResultsPath}");
                return 1;
            }
            Log.Info($"Processing file: {testResultsPath}");


            string branchAndBuildLabel = args[1];

            try
            {
                Task.Run(async () =>
                {
                    await new ImportManager(ProjectId, SectionId).Run(testResultsPath, branchAndBuildLabel);
                }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error("An error has occured during processing:", ex);
                return 1;
            }
            return 0;
        }

    }
}
