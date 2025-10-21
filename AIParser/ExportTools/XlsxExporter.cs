using ClosedXML.Excel;

namespace AIParser.ExportTools
{
    public class XlsxExporter
    {
        public static void Export(string filePath, List<List<string>> rows)// to refactor using templates, example - see InternalEmagExporter
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Template");

                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    for (int j = 0; j < row.Count; j++)
                    {
                        worksheet.Cell(i + 1, j + 1).Value = row[j];
                    }
                }

                workbook.SaveAs(filePath);
            }
        }
    }
}
