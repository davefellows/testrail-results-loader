using System;
using System.Threading.Tasks;

namespace TestRail.ResultsImporter
{
    using System.IO;

    class Program
    {
        private static string _testResultsPath = ".";
        private const int ProjectId = 1;
        private const int SectionId = 2;

        static int Main(string[] args)
        {

            if (args.Length > 0)
            {
                _testResultsPath = args[0];
                if (!Directory.Exists(_testResultsPath))
                {
                    Log.Error($"Supplied directory doesn't exist or is invalid: {_testResultsPath}");
                    return 1;
                }
                Log.Info($"Processing file: {_testResultsPath}");
            }
            else
            {
                Log.Warn($"No file argument passed. Using test path: {_testResultsPath}");
            }

            try
            {
                Task.Run(async () =>
                {
                    await new ImportManager(_testResultsPath, ProjectId, SectionId).Run(_testResultsPath);
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
