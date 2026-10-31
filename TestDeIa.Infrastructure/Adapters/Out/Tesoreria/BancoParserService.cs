using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Domain.Modules.Tesoreria.Entities;
using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Infrastructure.Adapters.Out.Tesoreria;

public sealed class BancoParserService : IBancoParserService
{
    private static readonly CultureInfo EcCulture = new("es-EC");
    private static readonly Regex SpacesRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex ReferenceRegex = new(@"(?:TRX|TRANSF|TRANSFERENCIA|DEP|DEPOSITO|PAPELETA|LOTE|REF|REFERENCIA|DOC|COMPROBANTE)[\s:\-#]*(?<ref>[A-Z0-9\-]{4,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<IReadOnlyCollection<ExtractoBancarioDetalle>> ParsearExtractoBancarioAsync(
        Stream fileStream,
        string extension,
        string bancoNombre,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        extension = NormalizeExtension(extension);
        if (extension is ".csv" or ".txt")
        {
            return await ParseCsvAsync(fileStream, cancellationToken);
        }

        if (extension is ".xlsx")
        {
            return await ParseXlsxAsync(fileStream, cancellationToken);
        }

        if (extension is ".xls")
        {
            throw new InvalidOperationException("El formato XLS binario no está soportado. Exporta el extracto como CSV o XLSX.");
        }

        throw new InvalidOperationException($"El formato '{extension}' no está soportado para extractos bancarios.");
    }

    private static async Task<IReadOnlyCollection<ExtractoBancarioDetalle>> ParseCsvAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        if (lines.Count == 0)
        {
            return [];
        }

        var delimiter = DetectDelimiter(lines[0]);
        var rows = lines
            .Select(line => SplitCsvLine(line, delimiter))
            .Where(row => row.Count > 0)
            .ToList();

        return ParseRows(rows);
    }

    private static async Task<IReadOnlyCollection<ExtractoBancarioDetalle>> ParseXlsxAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        using var archive = new ZipArchive(memory, ZipArchiveMode.Read, leaveOpen: false);
        var sharedStrings = ReadSharedStrings(archive);
        var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml")
            ?? archive.Entries.FirstOrDefault(entry => entry.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase));

        if (sheetEntry is null)
        {
            return [];
        }

        await using var sheetStream = sheetEntry.Open();
        var document = await XDocument.LoadAsync(sheetStream, LoadOptions.None, cancellationToken);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        var rows = document
            .Descendants(ns + "row")
            .Select(row => row.Elements(ns + "c").Select(cell => ReadCellValue(cell, sharedStrings, ns)).ToList())
            .Where(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
            .ToList();

        return ParseRows(rows);
    }

    private static IReadOnlyCollection<ExtractoBancarioDetalle> ParseRows(IReadOnlyCollection<IReadOnlyList<string>> rows)
    {
        if (rows.Count == 0)
        {
            return [];
        }

        var first = rows.First();
        var hasHeader = first.Any(IsHeaderToken);
        var map = hasHeader ? BuildHeaderMap(first) : ColumnMap.Default;
        var dataRows = hasHeader ? rows.Skip(1) : rows;
        var result = new List<ExtractoBancarioDetalle>();

        foreach (var row in dataRows)
        {
            var fechaText = GetValue(row, map.FechaIndex);
            if (!TryParseDate(fechaText, out var fecha))
            {
                continue;
            }

            var descripcion = NormalizeText(GetValue(row, map.DescripcionIndex));
            var referencia = NormalizeReference(GetValue(row, map.DocumentoIndex), descripcion);
            var (tipo, monto) = ResolveMovement(row, map, descripcion);
            if (monto <= 0)
            {
                continue;
            }

            result.Add(new ExtractoBancarioDetalle
            {
                Id = Guid.NewGuid(),
                FechaTransaccion = fecha,
                NumeroDocumentoRef = referencia,
                ConceptoDescripcion = descripcion,
                TipoMovimiento = tipo,
                Monto = decimal.Round(monto, 4, MidpointRounding.AwayFromZero),
                Conciliado = false
            });
        }

        return result;
    }

    private static (TipoMovimientoBancario Tipo, decimal Monto) ResolveMovement(IReadOnlyList<string> row, ColumnMap map, string descripcion)
    {
        var credito = TryParseAmount(GetValue(row, map.CreditoIndex), out var creditoValue) ? Math.Abs(creditoValue) : 0m;
        var debito = TryParseAmount(GetValue(row, map.DebitoIndex), out var debitoValue) ? Math.Abs(debitoValue) : 0m;

        if (credito > 0)
        {
            return (TipoMovimientoBancario.DepositoCredito, credito);
        }

        if (debito > 0)
        {
            return (TipoMovimientoBancario.RetiroDebito, debito);
        }

        if (TryParseAmount(GetValue(row, map.MontoIndex), out var monto))
        {
            var tipoText = NormalizeText(GetValue(row, map.TipoIndex));
            var isDebit = monto < 0 || tipoText.Contains("DEB", StringComparison.OrdinalIgnoreCase) ||
                tipoText.Contains("RET", StringComparison.OrdinalIgnoreCase) ||
                descripcion.Contains("COMISION", StringComparison.OrdinalIgnoreCase) ||
                descripcion.Contains("NOTA DE DEBITO", StringComparison.OrdinalIgnoreCase);

            return (isDebit ? TipoMovimientoBancario.RetiroDebito : TipoMovimientoBancario.DepositoCredito, Math.Abs(monto));
        }

        return (TipoMovimientoBancario.DepositoCredito, 0m);
    }

    private static ColumnMap BuildHeaderMap(IReadOnlyList<string> headers)
    {
        var normalized = headers.Select(NormalizeKey).ToArray();
        return new ColumnMap(
            FindIndex(normalized, "fecha", "ftransaccion", "fechatransaccion", "date"),
            FindIndex(normalized, "documento", "referencia", "numdocumento", "nrodocumento", "comprobante", "lote", "secuencial"),
            FindIndex(normalized, "descripcion", "concepto", "detalle", "observacion", "glosa"),
            FindIndex(normalized, "tipo", "tipomovimiento", "movimiento"),
            FindIndex(normalized, "monto", "valor", "importe"),
            FindIndex(normalized, "credito", "deposito", "haber", "entrada"),
            FindIndex(normalized, "debito", "retiro", "debe", "salida"));
    }

    private static int FindIndex(IReadOnlyList<string> values, params string[] candidates)
    {
        for (var i = 0; i < values.Count; i++)
        {
            if (candidates.Any(candidate => values[i].Contains(candidate, StringComparison.OrdinalIgnoreCase)))
            {
                return i;
            }
        }

        return -1;
    }

    private static string GetValue(IReadOnlyList<string> row, int index)
    {
        return index >= 0 && index < row.Count ? row[index].Trim() : string.Empty;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        extension = extension.Trim().ToLowerInvariant();
        return extension.StartsWith('.') ? extension : $".{extension}";
    }

    private static char DetectDelimiter(string line)
    {
        var candidates = new[] { ';', ',', '\t', '|' };
        return candidates
            .Select(candidate => new { candidate, count = line.Count(current => current == candidate) })
            .OrderByDescending(current => current.count)
            .First().candidate;
    }

    private static IReadOnlyList<string> SplitCsvLine(string line, char delimiter)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var character in line)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (character == delimiter && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString().Trim());
        return values;
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return document.Descendants(ns + "si")
            .Select(item => string.Concat(item.Descendants(ns + "t").Select(text => text.Value)))
            .ToArray();
    }

    private static string ReadCellValue(XElement cell, IReadOnlyList<string> sharedStrings, XNamespace ns)
    {
        var value = cell.Element(ns + "v")?.Value ?? cell.Element(ns + "is")?.Element(ns + "t")?.Value ?? string.Empty;
        var type = cell.Attribute("t")?.Value;
        if (type == "s" && int.TryParse(value, out var index) && index >= 0 && index < sharedStrings.Count)
        {
            return sharedStrings[index];
        }

        return value;
    }

    private static bool TryParseDate(string value, out DateTime date)
    {
        value = value.Trim();
        var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy", "MM/dd/yyyy", "M/d/yyyy" };
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var serial) && serial is > 20 and < 60000)
        {
            date = DateTime.FromOADate(serial).Date;
            return true;
        }

        return DateTime.TryParse(value, EcCulture, DateTimeStyles.None, out date);
    }

    private static bool TryParseAmount(string value, out decimal amount)
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim()
            .Replace("$", string.Empty, StringComparison.Ordinal)
            .Replace("USD", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        var isNegative = value.StartsWith('(') && value.EndsWith(')');
        value = value.Trim('(', ')');

        if (value.Contains(',') && value.Contains('.'))
        {
            value = value.LastIndexOf(',') > value.LastIndexOf('.')
                ? value.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.')
                : value.Replace(",", string.Empty, StringComparison.Ordinal);
        }
        else if (value.Count(current => current == ',') == 1 && !value.Contains('.'))
        {
            value = value.Replace(',', '.');
        }
        else if (value.Count(current => current == '.') > 1)
        {
            var last = value.LastIndexOf('.');
            value = value[..last].Replace(".", string.Empty, StringComparison.Ordinal) + value[last..];
        }

        if (!decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount))
        {
            return false;
        }

        if (isNegative)
        {
            amount *= -1;
        }

        return true;
    }

    private static string NormalizeText(string value)
    {
        return SpacesRegex.Replace(value.Trim(), " ");
    }

    private static string NormalizeKey(string value)
    {
        var normalized = value.Trim().ToLowerInvariant()
            .Replace("á", "a", StringComparison.Ordinal)
            .Replace("é", "e", StringComparison.Ordinal)
            .Replace("í", "i", StringComparison.Ordinal)
            .Replace("ó", "o", StringComparison.Ordinal)
            .Replace("ú", "u", StringComparison.Ordinal)
            .Replace("ñ", "n", StringComparison.Ordinal);

        return Regex.Replace(normalized, @"[^a-z0-9]", string.Empty);
    }

    private static bool IsHeaderToken(string value)
    {
        var key = NormalizeKey(value);
        return key.Contains("fecha", StringComparison.Ordinal) ||
            key.Contains("concepto", StringComparison.Ordinal) ||
            key.Contains("descripcion", StringComparison.Ordinal) ||
            key.Contains("monto", StringComparison.Ordinal) ||
            key.Contains("debito", StringComparison.Ordinal) ||
            key.Contains("credito", StringComparison.Ordinal);
    }

    private static string NormalizeReference(string reference, string description)
    {
        reference = NormalizeText(reference);
        if (!string.IsNullOrWhiteSpace(reference))
        {
            return reference.Length > 50 ? reference[..50] : reference;
        }

        var match = ReferenceRegex.Match(description);
        if (match.Success)
        {
            var value = match.Groups["ref"].Value.Trim();
            return value.Length > 50 ? value[..50] : value;
        }

        return string.Empty;
    }

    private sealed record ColumnMap(
        int FechaIndex,
        int DocumentoIndex,
        int DescripcionIndex,
        int TipoIndex,
        int MontoIndex,
        int CreditoIndex,
        int DebitoIndex)
    {
        public static ColumnMap Default { get; } = new(0, 1, 2, 3, 4, 4, 3);
    }
}
