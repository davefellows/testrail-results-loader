using System;
using System.Text;

namespace TestRail.ResultsImporter
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendWhen(this StringBuilder sb, bool condition, string value) =>
            condition ? sb.Append(value) : sb;

    }
}
