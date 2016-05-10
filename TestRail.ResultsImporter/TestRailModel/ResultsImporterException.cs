using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRail.ResultsImporter.TestRailModel
{
    public class ResultsImporterException : Exception
    {
        public ResultsImporterException(string message) : base(message)
        {
        }

        public ResultsImporterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
