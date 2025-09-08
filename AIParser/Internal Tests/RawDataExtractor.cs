using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;
namespace AIParser.DataUtils
{


    public static class RawDataExtractor
    {
        public static RawFeedData ExtractFromCsv(string path, char separator = ',')
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            var headers = lines[0].Split(separator).ToList();
            var rows = lines.Skip(1)
                            .Select(line => line.Split(separator).ToList())
                            .ToList();

            return new RawFeedData
            {
                Headers = headers,
                Rows = rows
            };
        }

        public static RawFeedData ExtractFromXml(string path, string productNodeName = "product")
        {
            var doc = new XmlDocument();
            doc.Load(path);
            var products = doc.GetElementsByTagName(productNodeName);

            var headers = new HashSet<string>();
            var rows = new List<List<string>>();

            // Prima trecere: adună toate cheile posibile (toate nodurile copil din <product>)
            foreach (XmlNode product in products)
            {
                foreach (XmlNode child in product.ChildNodes)
                {
                    headers.Add(child.Name);
                }
            }

            var headerList = headers.ToList();

            // A doua trecere: construiește fiecare rând în ordinea headerelor
            foreach (XmlNode product in products)
            {
                var row = new List<string>();
                foreach (var header in headerList)
                {
                    var node = product[header];
                    row.Add(node?.InnerText ?? "");
                }
                rows.Add(row);
            }

            return new RawFeedData
            {
                Headers = headerList,
                Rows = rows
            };
        }

        public static RawFeedData ExtractFromXlsx(string path)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets.First();

            var headers = new List<string>();
            var rows = new List<List<string>>();

            int colCount = worksheet.Dimension.End.Column;
            int rowCount = worksheet.Dimension.End.Row;

            for (int col = 1; col <= colCount; col++)
            {
                headers.Add(worksheet.Cells[1, col].Text);
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    rowData.Add(worksheet.Cells[row, col].Text);
                }
                rows.Add(rowData);
            }

            return new RawFeedData
            {
                Headers = headers,
                Rows = rows
            };
        }
    }

}
