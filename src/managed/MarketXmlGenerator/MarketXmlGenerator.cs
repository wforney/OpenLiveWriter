// <copyright file="MarketXmlGenerator.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace MarketXmlGenerator
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// The market XML generator class.
    /// </summary>
    internal class MarketXmlGenerator
    {
        private const string FEATURES = "features";
        private static bool succeed;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            // 1. input: path to where all the files are located, output file?
            string inputFilePath;
            string outputFile;
            if (args.Length != 2)
            {
                Console.WriteLine("incorrect number of arguments. Correct usage: xxx {inputFilePath} {outputFile}");
                Environment.Exit(1);
            }

            inputFilePath = args[0];
            outputFile = args[1];

            // inputFilePath = @"..\..\..\..\..\..\..\SD\working.client.writer\client\writer\intl\markets";
            // outputFile = @"c:\temp\output.xml";

            // 2. generate blank xml root
            var marketsXml = new XmlDocument();
            var declaration = marketsXml.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            var entryNode = marketsXml.CreateElement(FEATURES);
            marketsXml.AppendChild(entryNode);
            marketsXml.InsertBefore(declaration, entryNode);

            // 3. get all folders
            var dir = new DirectoryInfo(inputFilePath);
            var marketDirs = dir.GetDirectories();

            // 4. open each folder and get the xml file from inside
            foreach (var marketDir in marketDirs)
            {
                var files = marketDir.GetFiles("market.xml");

                // case 1: no market xml
                if (files.Length == 0)
                {
                    continue;
                }

                var xmlFile = files[0]; // defaulting to taking the first here...shouldn't be more than one tho!

                // 5. scan xml file for correctness. if correct, move on, else log error
                //    5a. is actual XML document
                //    5b. XML structure: contains feature root, one market element, 0-many feature elements, each with 0 - many params
                //    5c. features have name and enabled attributes. parameters have name and value attributes.
                if (!ValidateXml(inputFilePath, xmlFile.FullName))
                {
                    Console.WriteLine($"Validation Failed for file {xmlFile.FullName}");
                    Environment.Exit(1);
                }

                // 6. take the market portion and copy into new xml document
                using (var xmlStream = xmlFile.OpenRead())
                {
                    var marketDocument = new XmlDocument();
                    marketDocument.Load(xmlStream);
                    var marketNode = marketDocument.SelectSingleNode("//features/market");
                    if (marketNode == null)
                    {
                        Console.WriteLine("Invalid marketizationXml.xml file detected");
                        Environment.Exit(1);
                    }

                    var marketName = marketNode.Attributes["name"].InnerText;
                    if (marketName.ToLower() != marketName)
                    {
                        Console.WriteLine("market name must be lower case");
                        Environment.Exit(1);
                    }

                    entryNode.AppendChild(marketsXml.ImportNode(marketNode, true));
                }
            }

            // 7. output the xml into document (check for existence, create. Do not append)
            marketsXml.Save(outputFile);
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="xmlFile">The XML file.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool ValidateXml(string inputFilePath, string xmlFile)
        {
            succeed = true;

            //// TODO:OLW
            ////XmlSchemaCollection sc = new XmlSchemaCollection();
            ////sc.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandle);

            ////sc.Add(null, Path.Combine(inputFilePath, "validator.xsd"));
            ////XmlTextReader tr = new XmlTextReader(xmlFile);
            ////XmlValidatingReader rdr = new XmlValidatingReader(tr);

            ////rdr.ValidationType = ValidationType.Schema;
            ////rdr.Schemas.Add(sc);
            ////rdr.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandle);
            ////while (rdr.Read());

            return succeed;
        }

        /// <summary>
        /// Validations the event handle.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ValidationEventArgs"/> instance containing the event data.</param>
        private static void ValidationEventHandle(object sender, ValidationEventArgs args)
        {
            Console.WriteLine($"Validation error: {args.Message}");
            Console.WriteLine($"Line {args.Exception.LineNumber} Pos {args.Exception.LinePosition}");
            succeed = false;
        }
    }
}
