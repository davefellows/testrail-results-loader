using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TestRail.ResultsImporter.TestRailModel;

namespace TestRail.ResultsImporter
{
    /// <summary>
    /// Base class for parsing test results files
    /// </summary>
    internal abstract class ResultsParser
    {
        public abstract string TestName { get; }

        public abstract string FileName { get; }

        public abstract DateTime StartTime { get; }

        /// <summary>
        /// Returns a collection of tests that don't exist in TestRail (matching on test Title/Name).
        /// </summary>
        public abstract IEnumerable<TestCase> GetMissingTests(IEnumerable<TestCase> existingTestCases);

        /// <summary>
        /// Returns a collection of test results with the TestRail case Id added to the results.
        /// </summary>
        public abstract IEnumerable<TestResult> GetTestResultsWithCaseIds(IEnumerable<TestCase> existingTestCases);


        protected static T DeserializeFile<T>(string xmlPath)
        {
            string xmlString = File.ReadAllText(xmlPath);
            return Deserialize<T>(xmlString);
        }

        protected static T Deserialize<T>(string inputString)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(inputString))
            {
                // Read the object as XML string.
                using (var rd = XmlReader.Create(stringReader))
                {
                    return (T)serializer.Deserialize(rd);
                }
            }
        }

        /// <summary>
        /// Format the duration to TestRail's expected format: {hours}h {minutes}m {seconds}s
        /// </summary>
        protected static string FormatDuration(string duration)
        {
            var t = TimeSpan.Parse(duration);

            // Time resolution in TestRail is down to seconds so if less than
            // 1 second then show 1 second rather than blank.
            if (t.TotalSeconds < 1)
                return "1s";

            return new StringBuilder()
                .AppendWhen(t.Hours > 0, $"{t.Hours}h ")
                .AppendWhen(t.Minutes > 0, $"{t.Minutes}m ")
                .AppendWhen(t.Seconds > 0, $"{t.Seconds}s")
                .ToString();
            
        }

    }
}
