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
