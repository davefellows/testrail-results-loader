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
        /// <summary>
        /// Name/Title for the Test run
        /// </summary>
        public abstract string TestName { get; }

        public abstract DateTime StartTime { get; }

        /// <summary>
        /// Returns a collection of tests that don't exist in TestRail (matching on test Title/Name).
        /// </summary>
        /// <param name="existingTestCases">Existing tests in TestRail</param>
        public abstract IEnumerable<TestCase> GetMissingTests(IEnumerable<TestCase> existingTestCases);

        /// <summary>
        /// Returns a collection of test results with the TestRail case Id added to the results.
        /// </summary>
        /// <param name="existingTestCases">Existing tests in TestRail</param>
        public abstract IEnumerable<TestResult> GetTestResultsWithCaseIds(IEnumerable<TestCase> existingTestCases);

        protected static T LoadFile<T>(string xmlPath, XmlReaderSettings settings = null)
        {
            string xmlString = File.ReadAllText(xmlPath);
            return DeSerialize<T>(xmlString, settings);
        }

        protected static T DeSerialize<T>(string inputString, XmlReaderSettings settings = null)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(inputString))
            {
                // Read the object as XML string.
                using (var rd = XmlReader.Create(stringReader, settings))
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

            var elapsed = String.Empty;

            // Time resolution in TestRail is down to seconds so if less than
            // 1 second then show 1 second rather than blank.
            if (t.TotalSeconds < 1) elapsed = "1s";
            else
            {
                if (t.Hours > 0) elapsed += $"{t.Hours}h ";
                if (t.Minutes > 0) elapsed += $"{t.Minutes}m ";
                if (t.Seconds > 0) elapsed += $"{t.Seconds}s";
            }

            return elapsed;
        }
    }
}
