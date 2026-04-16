using FeedUploader.Data.Models;
using System.Text.Json;
using Attribute = FeedUploader.Data.Models.Attribute;


namespace AIParser.DataUtils
{
    public class ProductDeserializationResult
    {
        public Product? Product { get; set; }
        public bool IsMatchOk { get; set; } = true;
        public string? ErrorReason { get; set; }
        public int? RowIndex { get; set; }
        public List<string> ValidationErrors { get; } = new();
    }


    public static class ProductDeserializer
        {

            public static ProductDeserializationResult DeserializeFromJson(string json, List<Attribute> availableAttributes)
            {
                var result = new ProductDeserializationResult();

                if (string.IsNullOrWhiteSpace(json))
                {
                    result.IsMatchOk = false;
                    result.ErrorReason = "Empty JSON";
                    return result;
                }

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    JsonElement productElement;
                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        if (root.GetArrayLength() == 0)
                        {
                            result.IsMatchOk = false;
                            result.ErrorReason = "JSON array is empty";
                            return result;
                        }

                        productElement = root[0];
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        productElement = root;
                    }
                    else
                    {
                        result.IsMatchOk = false;
                        result.ErrorReason = "Unexpected JSON root (not object/array)";
                        return result;
                    }

                    // RowIndex (опционально)
                    if (productElement.TryGetProperty("RowIndex", out var rowIdxProp) && rowIdxProp.ValueKind == JsonValueKind.Number)
                        result.RowIndex = rowIdxProp.GetInt32();

                    // MatchStatus / ErrorReason
                    string? matchStatus = null;
                    if (productElement.TryGetProperty("MatchStatus", out var matchProp) && matchProp.ValueKind == JsonValueKind.String)
                        matchStatus = matchProp.GetString()?.Trim().ToLowerInvariant();

                    if (matchStatus == "error")
                    {
                        result.IsMatchOk = false;
                        if (productElement.TryGetProperty("ErrorReason", out var errProp) && errProp.ValueKind == JsonValueKind.String)
                            result.ErrorReason = errProp.GetString();
                        else
                            result.ErrorReason = "AI reported MatchStatus:error";

                        return result; // продукт ошибочный — возвращаем причину, Product = null
                    }

                    // Идёт маппинг полей в Product
                    var product = new Product();

                    // Helper для чтения строк
                    string GetString(string propName)
                    {
                        if (productElement.TryGetProperty(propName, out var p) && p.ValueKind == JsonValueKind.String)
                            return p.GetString() ?? string.Empty;
                        return string.Empty;
                    }

                    // Basic fields
                    product.Name = GetString("Name");
                    product.Description = GetString("Description");
                    product.Model = GetString("Model");
                    product.PartNumber = GetString("Cod produs");
                    if (string.IsNullOrWhiteSpace(product.PartNumber))
                        product.PartNumber = GetString("Code");
                    product.Manufacturer = GetString("Manufacturer");
                    product.Category = GetString("Category");

                    // Price / SalePrice (numbers)
                    product.Price = GetDecimalSafe(productElement, "Price", defaultValue: 0m);
                    product.SalePrice = GetDecimalSafe(productElement, "SalePrice", defaultValue: 0m);

                    // Currency (default RON)
                    var currency = GetString("Currency");
                    product.Currency = string.IsNullOrWhiteSpace(currency) ? "RON" : currency;

                    product.Quantity = GetIntSafe(productElement, "Quantity", defaultValue: 0);
                    product.Warranty = GetNullableIntSafe(productElement, "Warranty");
                    product.MainImage = GetString("MainImage");
                    product.AdditionalImage1 = GetString("AdditionalImage1");
                    product.AdditionalImage2 = GetString("AdditionalImage2");
                    product.AdditionalImage3 = GetString("AdditionalImage3");
                    product.AdditionalImage4 = GetString("AdditionalImage4");
                    product.Type = string.IsNullOrWhiteSpace(GetString("Type")) ? "new" : GetString("Type");

                    // Attributes: ожидаем массив объектов { "Name": "...", "Value": "...", optional "Unit": "..." }
                    var extractedAttrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (productElement.TryGetProperty("Attributes", out var attrsProp) && attrsProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var a in attrsProp.EnumerateArray())
                        {
                            if (a.ValueKind != JsonValueKind.Object) continue;

                            string name = a.TryGetProperty("Name", out var nameP) && nameP.ValueKind == JsonValueKind.String
                                ? nameP.GetString() ?? string.Empty
                                : string.Empty;

                            string value = a.TryGetProperty("Value", out var valueP) && valueP.ValueKind == JsonValueKind.String
                                ? valueP.GetString() ?? string.Empty
                                : (a.TryGetProperty("Value", out var v2) && v2.ValueKind == JsonValueKind.Number ? v2.ToString() : string.Empty);

                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                // если уже есть — не перезаписываем, но можно решить иначе
                                if (!extractedAttrs.ContainsKey(name))
                                    extractedAttrs[name] = value;
                            }
                        }
                    }
                    else if (productElement.TryGetProperty("ExtractedAttributes", out var dictAttrs) && dictAttrs.ValueKind == JsonValueKind.Object)
                    {
                        // fallback: старый формат словаря
                        foreach (var prop in dictAttrs.EnumerateObject())
                            extractedAttrs[prop.Name] = prop.Value.GetString() ?? string.Empty;
                    }

                    var extractedAttrsByCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    // Создаём ProductAttribute для каждого извлечённого атрибута.
                    foreach (var kvp in extractedAttrs)
                    {
                        var attrName = kvp.Key;
                        var attrValue = kvp.Value ?? string.Empty;

                        var found = FindAttributeByName(availableAttributes, attrName);

                        var pa = new ProductAttribute
                        {
                            Value = attrValue,
                            IsExtractedByAI = true
                        };

                        if (found != null)
                        {
                            // если нашли запись в справочнике — заполняем Attribute ссылкой (или AttributeId)
                            pa.Attribute = found;
                            pa.AttributeId = found.Id;
                            if (!string.IsNullOrWhiteSpace(found.Code) && !extractedAttrsByCode.ContainsKey(found.Code))
                                extractedAttrsByCode[found.Code] = attrValue;
                        }
                        else
                        {
                            // временный объект-описание, чтобы не терять имя — окончательный маппинг будет в FeedExtractor
                            pa.Attribute = new Attribute { Name = attrName, Code = attrName.ToLowerInvariant() };
                            if (!extractedAttrsByCode.ContainsKey(attrName))
                                extractedAttrsByCode[attrName] = attrValue;
                        }

                        product.Attributes.Add(pa);
                    }

                    product.ExtractedAttributes = extractedAttrsByCode;

                    // Валидация: проверяем обязательные атрибуты из availableAttributes (IsRequired == true)
                    var requiredMissing = new List<string>();
                    var requiredFromConfig = availableAttributes.Where(a => a.IsRequired).ToList();
                    foreach (var req in requiredFromConfig)
                    {
                        // присутствие проверяем по имени (case-insensitive) в extractedAttrs
                        if (!extractedAttrs.Keys.Any(k => string.Equals(k, req.Name, StringComparison.OrdinalIgnoreCase)))
                            requiredMissing.Add(req.Name);
                    }

                    if (requiredMissing.Count > 0)
                    {
                        result.IsMatchOk = false;
                        result.ValidationErrors.AddRange(requiredMissing.Select(n => $"Missing required attribute: {n}"));
                        result.ErrorReason = string.Join("; ", result.ValidationErrors);
                        // Возвращаем продукт всё ещё — обработчик решит, сохранять его или нет
                        result.Product = product;
                        return result;
                    }

                    // Всё ок
                    result.IsMatchOk = true;
                    result.Product = product;
                    return result;
                }
                catch (JsonException jex)
                {
                    result.IsMatchOk = false;
                    result.ErrorReason = $"JSON parsing failed: {jex.Message}";
                    return result;
                }
                catch (Exception ex)
                {
                    result.IsMatchOk = false;
                    result.ErrorReason = $"Unexpected error: {ex.Message}";
                    return result;
                }

                // локальные вспомогательные функции
                static decimal GetDecimalSafe(JsonElement el, string propName, decimal defaultValue)
                {
                    if (el.TryGetProperty(propName, out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number)
                        {
                            if (p.TryGetDecimal(out var d)) return d;
                            if (p.TryGetDouble(out var dd)) return Convert.ToDecimal(dd);
                            // as string?
                            if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var parsed)) return parsed;
                        }
                        else if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var parsed2))
                        {
                            return parsed2;
                        }
                    }
                    return defaultValue;
                }

                static int GetIntSafe(JsonElement el, string propName, int defaultValue)
                {
                    if (el.TryGetProperty(propName, out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                        if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var parsed)) return parsed;
                    }
                    return defaultValue;
                }

                static int? GetNullableIntSafe(JsonElement el, string propName)
                {
                    if (el.TryGetProperty(propName, out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                        if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var parsed)) return parsed;
                        if (p.ValueKind == JsonValueKind.Null) return null;
                    }
                    return null;
                }
            }

            private static Attribute? FindAttributeByName(List<Attribute> attributes, string name)
            {
                if (attributes == null) return null;
                return attributes.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            }

                public static ProductDeserializationResult DeserializeFromJsonElement(JsonElement rootElement, List<Attribute> availableAttributes)
                {
                    var result = new ProductDeserializationResult();

                    try
                    {
                        JsonElement productElement;

                        if (rootElement.ValueKind == JsonValueKind.Array)
                        {
                            if (rootElement.GetArrayLength() == 0)
                            {
                                result.IsMatchOk = false;
                                result.ErrorReason = "JSON array is empty";
                                return result;
                            }
                            productElement = rootElement[0];
                        }
                        else if (rootElement.ValueKind == JsonValueKind.Object)
                        {
                            productElement = rootElement;
                        }
                        else
                        {
                            result.IsMatchOk = false;
                            result.ErrorReason = "Unexpected JSON root (not object/array)";
                            return result;
                        }

                        // RowIndex (опционально)
                        if (productElement.TryGetProperty("RowIndex", out var rowIdxProp) && rowIdxProp.ValueKind == JsonValueKind.Number)
                            result.RowIndex = rowIdxProp.GetInt32();

                        // MatchStatus
                        if (productElement.TryGetProperty("MatchStatus", out var matchProp) && matchProp.ValueKind == JsonValueKind.String)
                        {
                            var ms = matchProp.GetString()?.Trim().ToLowerInvariant();
                            if (ms == "error")
                            {
                                result.IsMatchOk = false;
                                result.ErrorReason = productElement.TryGetProperty("ErrorReason", out var errProp) && errProp.ValueKind == JsonValueKind.String
                                    ? errProp.GetString()
                                    : "AI reported MatchStatus:error";
                                return result;
                            }
                        }

                        // Создаём Product и маппим поля
                        var product = new Product();

                        string GetString(string propName)
                        {
                            if (productElement.TryGetProperty(propName, out var p) && p.ValueKind == JsonValueKind.String)
                                return p.GetString() ?? string.Empty;
                            return string.Empty;
                        }
                        product.PartNumber = GetString("PartNumber");
                        product.Name = GetString("Name");
                        product.Description = GetString("Description");
                        product.Model = GetString("Model");  
                        product.Manufacturer = GetString("Manufacturer");
                        product.Category = GetString("Category");
                        product.Price = GetDecimalSafe(productElement, "Price", 0m);
                        product.SalePrice = GetDecimalSafe(productElement, "SalePrice", 0m);
                        var currency = GetString("Currency");
                        product.Currency = string.IsNullOrWhiteSpace(currency) ? "RON" : currency;
                        product.Quantity = GetIntSafe(productElement, "Quantity", 0);
                        product.Warranty = GetNullableIntSafe(productElement, "Warranty");
                        product.MainImage = GetString("MainImage");
                        product.AdditionalImage1 = GetString("AdditionalImage1");
                        product.AdditionalImage2 = GetString("AdditionalImage2");
                        product.AdditionalImage3 = GetString("AdditionalImage3");
                        product.AdditionalImage4 = GetString("AdditionalImage4");
                        product.Type = string.IsNullOrWhiteSpace(GetString("Type")) ? "new" : GetString("Type");

                        // Собираем извлечённые атрибуты (ожидаем массив объектов {Name, Value, optional Unit})
                        var extractedAttrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        if (productElement.TryGetProperty("Attributes", out var attrsProp) && attrsProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var a in attrsProp.EnumerateArray())
                            {
                                if (a.ValueKind != JsonValueKind.Object) continue;

                                string name = a.TryGetProperty("Name", out var nameP) && nameP.ValueKind == JsonValueKind.String
                                    ? nameP.GetString() ?? string.Empty
                                    : string.Empty;

                                string value = string.Empty;
                                if (a.TryGetProperty("Value", out var valueP))
                                {
                                    if (valueP.ValueKind == JsonValueKind.String)
                                        value = valueP.GetString() ?? string.Empty;
                                    else if (valueP.ValueKind == JsonValueKind.Number)
                                        value = valueP.ToString();
                                    else
                                        value = valueP.GetRawText();
                                }

                                if (!string.IsNullOrWhiteSpace(name) && !extractedAttrs.ContainsKey(name))
                                    extractedAttrs[name] = value;
                            }
                        }
                        else if (productElement.TryGetProperty("ExtractedAttributes", out var dictAttrs) && dictAttrs.ValueKind == JsonValueKind.Object)
                        {
                            // fallback: старый словарь
                            foreach (var prop in dictAttrs.EnumerateObject())
                                extractedAttrs[prop.Name] = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() ?? string.Empty : prop.Value.ToString();
                        }

                        var extractedAttrsByCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        // Создаём ProductAttribute только для известных атрибутов (из availableAttributes)
                        foreach (var kvp in extractedAttrs)
                        {
                            var attrName = kvp.Key;
                            var attrValue = kvp.Value ?? string.Empty;

                            var found = FindAttributeByName(availableAttributes, attrName);

                            if (found != null)
                            {
                                var pa = new ProductAttribute
                                {
                                    Value = attrValue,
                                    IsExtractedByAI = true,
                                    Attribute = found,
                                    AttributeId = found.Id
                                };
                                product.Attributes.Add(pa);
                                if (!string.IsNullOrWhiteSpace(found.Code) && !extractedAttrsByCode.ContainsKey(found.Code))
                                    extractedAttrsByCode[found.Code] = attrValue;
                            }
                            else
                            {
                                // НЕ добавляем неизвестные атрибуты.
                                // Записываем в валидационные замечания, чтобы тест/лог мог увидеть проблему.
                                result.ValidationErrors.Add($"Unknown attribute (skipped): {attrName}");
                                if (!extractedAttrsByCode.ContainsKey(attrName))
                                    extractedAttrsByCode[attrName] = attrValue;
                            }
                        }

                        product.ExtractedAttributes = extractedAttrsByCode;

                        // Проверка обязательных атрибутов (IsRequired == true)
                        var requiredMissing = new List<string>();
                        foreach (var req in availableAttributes.Where(a => a.IsRequired))
                        {
                            if (!product.Attributes.Any(pa => string.Equals(pa.Attribute?.Name, req.Name, StringComparison.OrdinalIgnoreCase)))
                                requiredMissing.Add(req.Name);
                        }

                        if (requiredMissing.Count > 0)
                        {
                            result.IsMatchOk = false;
                            result.ValidationErrors.AddRange(requiredMissing.Select(n => $"Missing required attribute: {n}"));
                            result.ErrorReason = string.Join("; ", result.ValidationErrors);
                            result.Product = product; // возращаем продукт с теми атрибутами, которые удалось взять
                            return result;
                        }

                        // успех
                        result.IsMatchOk = true;
                        result.Product = product;
                        return result;
                    }
                    catch (JsonException jex)
                    {
                        return new ProductDeserializationResult
                        {
                            IsMatchOk = false,
                            ErrorReason = $"JSON parsing failed: {jex.Message}"
                        };
                    }
                    catch (Exception ex)
                    {
                        return new ProductDeserializationResult
                        {
                            IsMatchOk = false,
                            ErrorReason = $"Unexpected error: {ex.Message}"
                        };
                    }
                }
                static decimal GetDecimalSafe(JsonElement el, string propName, decimal defaultValue)
                {
                    if (el.TryGetProperty(propName, out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
                        if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var dd)) return Convert.ToDecimal(dd);
                        if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var parsed)) return parsed;
                    }
                    return defaultValue;
                }

                static int GetIntSafe(JsonElement el, string propName, int defaultValue)
                {
                    if (el.TryGetProperty(propName, out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                        if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var parsed)) return parsed;
                    }
                    return defaultValue;
                }

                static int? GetNullableIntSafe(JsonElement el, string propName)
                {
                    if (el.TryGetProperty(propName, out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                        if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var parsed)) return parsed;
                        if (p.ValueKind == JsonValueKind.Null) return null;
                    }
                    return null;
                }
        }


    }


