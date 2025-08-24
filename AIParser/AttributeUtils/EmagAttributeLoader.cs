using System.ComponentModel;
using OfficeOpenXml;
using FeedUploader.Data.Models;

namespace AIParser.AttributeUtils
{
    public static class EmagAttributeLoader
    {
        public static List<Attribute> LoadFromEmagTemplate(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));

            var rulesSheet = package.Workbook.Worksheets["Reguli"];
            var valuesSheet = package.Workbook.Worksheets["Valori caracteristici"];

            var attributes = LoadAttributesFromRules(rulesSheet);
            LoadAllowedValues(valuesSheet, attributes);

            return attributes;
        }

        private static List<Attribute> LoadAttributesFromRules(ExcelWorksheet rulesSheet)
        {
            var attributes = new List<Attribute>();
            int row = 2;

            while (!string.IsNullOrEmpty(rulesSheet.Cells[row, 1].Text))
            {
                attributes.Add(new Attribute
                {
                    Name = rulesSheet.Cells[row, 1].Text.Trim(),
                    Code = rulesSheet.Cells[row, 2].Text.Trim(),
                    IsRequired = rulesSheet.Cells[row, 3].Text == "DA",
                    IsRestricted = rulesSheet.Cells[row, 4].Text == "DA",
                    Unit = rulesSheet.Cells[row, 5].Text.Trim()
                });
                row++;
            }

            return attributes;
        }

        private static void LoadAllowedValues(ExcelWorksheet valuesSheet, List<Attribute> attributes)
        {
            int row = 2;
            while (!string.IsNullOrEmpty(valuesSheet.Cells[row, 1].Text))
            {
                var attrName = valuesSheet.Cells[row, 1].Text.Trim();
                var value = valuesSheet.Cells[row, 2].Text.Trim();

                var attr = attributes.FirstOrDefault(a => a.Name == attrName);
                if (attr != null)
                {
                    if (attr.AllowedValues == null)
                        attr.AllowedValues = new List<string>();

                    attr.AllowedValues.Add(value);
                }

                row++;
            }
        }
    }
}