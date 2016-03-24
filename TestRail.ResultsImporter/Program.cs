using System.Threading.Tasks;

namespace TestRail.ResultsImporter
{
    class Program
    {
        private const string TestResultsFile = "TestResultsFile.trx";
        private const int ProjectId = 1;
        private const int SectionId = 2;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await new ImportManager(TestResultsFile, ProjectId, SectionId).Run();
            }).GetAwaiter().GetResult();
        }

    }
}
