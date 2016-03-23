// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlStringConverter.cs" company="Microsoft">
//   Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
//   Convert between type and serialized Xml string
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace TestRail.ResultsImporter
{
    /// <summary>
    /// Convert between type and serialized Xml string
    /// </summary>
    public static class XmlStringConverter
    {
        /// <summary>
        /// The read timeout in milliseconds.
        /// </summary>
        private const int ReadTimeoutInMilliseconds = 5000;

        /// <summary>
        /// Serial a type into XML string
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object
        /// </typeparam>
        /// <param name="thisObject">
        /// input object
        /// </param>
        /// <returns>
        /// String representation
        /// </returns>
        public static string SerializeToXml<T>(this T thisObject)
        {
            using (var memoryStream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                var settings = new System.Xml.XmlWriterSettings();

                settings.Indent = true;
                settings.Encoding = new UTF8Encoding(false);
                settings.NewLineOnAttributes = false;

                using (var writer = XmlWriter.Create(memoryStream, settings))
                {
                    serializer.Serialize(writer, thisObject);
                }

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Serial a type into XML file
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object
        /// </typeparam>
        /// <param name="thisObject">
        /// input object
        /// </param>
        /// <param name="xmlPath">
        /// the path to write a xml file to
        /// </param>
        public static void Save<T>(this T thisObject, string xmlPath)
        {
            string xmlString = SerializeToXml<T>(thisObject);
            File.WriteAllText(xmlPath, xmlString);
        }

        /// <summary>
        /// deserialize an object.
        /// </summary>
        /// <param name="inputString">
        /// the string representation
        /// </param>
        /// <param name="settings">
        /// Optional. XmlReaderSettings to use during deserialization
        /// </param>
        /// <typeparam name="T">
        /// Object type to deserialize string to
        /// </typeparam>
        /// <returns>
        /// new object
        /// </returns>
        public static T DeSerialize<T>(string inputString, XmlReaderSettings settings = null)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new System.IO.StringReader(inputString))
            {
                // Read the object as XML string.
                using (var rd = System.Xml.XmlReader.Create(stringReader, settings))
                {
                    return (T)serializer.Deserialize(rd);
                }
            }
        }

        /// <summary>
        /// Deserialize an XML file.
        /// </summary>
        /// <param name="xmlPath">
        /// The file path of the XML file to be deserialized
        /// </param>
        /// <param name="settings">
        /// Optional. XmlReaderSettings to use during deserialization
        /// </param>
        /// <typeparam name="T">
        /// Object type to deserialize string to
        /// </typeparam>
        /// <returns>
        /// New object
        /// </returns>
        public static T Load<T>(string xmlPath, XmlReaderSettings settings = null)
        {
            string xmlString = File.ReadAllText(xmlPath);
            return DeSerialize<T>(xmlString, settings);
        }


        /// <summary>
        /// Utility function to santizie string to be serialized into xml
        /// </summary>
        /// <param name="input">
        /// string to sanitize
        /// </param>
        /// <returns>
        /// input string with invalid characters removed
        /// </returns>
        public static string SanitizeXMLString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // From xml spec valid chars:
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(input, re, string.Empty);
        }
    }
}
