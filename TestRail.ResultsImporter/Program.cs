using System;
using System.Configuration;
using System.Threading.Tasks;
using Gurock.TestRail;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    using System.IO;

    class Program
    {
        //TODO: retrieve project id from testrail
        private const int ProjectId = 2; // test project

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Log.Error("Expected args missing. Expect folder path to the results files, and the test run name (e.g. branch + build #).");
            }

            try
            {
                var resultsPath = ResultsPath(args);
                var branchAndBuildLabel = args[1];

                Log.Info($"Processing path: '{resultsPath}' for branch/build: '{branchAndBuildLabel}'");


                var client = new APIClient(ConfigurationManager.AppSettings["testrail-endpoint"])
                {
                    User = ConfigurationManager.AppSettings["username"],
                    Password = ConfigurationManager.AppSettings["password"]
                };

                Task.Run(async () =>
                {
                    await new ImportManager(client, ProjectId)
                                    .Run(resultsPath, branchAndBuildLabel);

                }).GetAwaiter().GetResult();
            }
            catch (ResultsImporterException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (Exception ex)
            {
                Log.Error("An error has occured during processing:", ex);
                return 2;
            }
            return 0;
        }

        private static string ResultsPath(string[] args)
        {
            var testResultsPath = args[0];

            if (!Directory.Exists(testResultsPath))
            {
                throw new ResultsImporterException($"Supplied directory doesn't exist or is invalid: {testResultsPath}");
            }

            return testResultsPath;
        }

    }
}
