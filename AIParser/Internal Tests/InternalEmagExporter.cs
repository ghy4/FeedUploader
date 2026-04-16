using ClosedXML.Excel;
using FeedUploader.Data.Models;
using System.Reflection;

public static class InternalEmagExporter
{
    public static void ExportOnTemplate(
        List<Product> products,
        string pathTemplate,
        string pathOut,
        Dictionary<string, string> mapari,
        Dictionary<string, string> valoriImplicite)
    {
    // Example mapari usage:
    // {"part_number","PartNumber"}, {"vendor_ext_id","Id"}, {"name","Name"}, ...
        using var wb = new XLWorkbook(pathTemplate);
        var ws = wb.Worksheet("Template");

        // Header row (r1 = titluri, r2 = coduri eMAG)
        int codeRow = 3;
        int lastCol = ws.LastColumnUsed().ColumnNumber();

        // Citim codurile de coloană din rândul 2
        var emagCodes = new List<string>();
        for (int c = 1; c <= lastCol; c++)
            emagCodes.Add(Normalize(ws.Cell(codeRow, c).GetString()));
        
        // Prima linie liberă după antet
        int startRow = ws.LastRowUsed().RowNumber() + 1;
        Console.WriteLine($"Emag codes: {string.Join(',', emagCodes)}");
        foreach (var product in products)
        {
            var props = typeof(Product).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int c = 0; c < emagCodes.Count; c++)
            {
                string emagCode = emagCodes[c];
                string? val = null;

                // 1) Dacă există mapare explicită la o proprietate Product
                if (mapari.TryGetValue(emagCode, out var propName))
                {
                    var prop = props.FirstOrDefault(p => p.Name == propName);
                    if (prop != null)
                    {
                        var objVal = prop.GetValue(product);
                        if (objVal != null)
                            val = objVal.ToString();
                    }
                }

                // 2) Dacă e atribut specific categoriei
                if (val == null)
                {
                    // Очищаем входной emagCode от лишних пробелов
                    var searchKey = emagCode.Trim();

                    var attr = product.Attributes.FirstOrDefault(a =>
                        a.Attribute != null && !string.IsNullOrWhiteSpace(a.Attribute.Name) &&
                        (
                            string.Equals(a.Attribute.Name, searchKey, StringComparison.OrdinalIgnoreCase) ||

  
                            string.Equals($"[{a.Attribute.Code}]", searchKey, StringComparison.OrdinalIgnoreCase) ||

                            string.Equals($"{a.Attribute.Name}: [{a.Attribute.Code}]", searchKey, StringComparison.OrdinalIgnoreCase)
                        ));

                    if (attr != null)
                        val = attr.Value;
                }

                // 3) Dacă avem valoare implicită
                if (val == null && valoriImplicite.TryGetValue(emagCode, out var defaultVal))
                {
                    val = defaultVal;
                }

                ws.Cell(startRow, c + 1).Value = val ?? "";
            }

            startRow++;
        }

        wb.SaveAs(pathOut);
        return;
    }
    static string Normalize(string s) =>
    s?.Trim().ToLowerInvariant();
}
