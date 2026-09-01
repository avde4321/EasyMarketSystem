using System.Globalization;
using System.Text;

namespace TestDeIa.Api.Reporting;

public sealed class SimpleRidePdfRenderer
{
    public byte[] Render(string title, IReadOnlyCollection<string> lines)
    {
        var sanitizedLines = lines
            .Select(Sanitize)
            .Chunk(48)
            .FirstOrDefault() ?? Array.Empty<string>();

        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 18 Tf");
        content.AppendLine("50 790 Td");
        content.AppendLine($"({Sanitize(title)}) Tj");
        content.AppendLine("/F1 10 Tf");

        var yOffset = -28;
        foreach (var line in sanitizedLines)
        {
            content.AppendLine($"0 {yOffset.ToString(CultureInfo.InvariantCulture)} Td");
            content.AppendLine($"({line}) Tj");
            yOffset = -16;
        }

        content.AppendLine("ET");
        var contentBytes = Encoding.ASCII.GetBytes(content.ToString());

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream"
        };

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.WriteLine("%PDF-1.4");

        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            writer.Flush();
            offsets.Add(stream.Position);
            writer.WriteLine($"{index + 1} 0 obj");
            writer.WriteLine(objects[index]);
            writer.WriteLine("endobj");
        }

        writer.Flush();
        var xrefOffset = stream.Position;
        writer.WriteLine("xref");
        writer.WriteLine($"0 {objects.Count + 1}");
        writer.WriteLine("0000000000 65535 f ");
        foreach (var offset in offsets.Skip(1))
        {
            writer.WriteLine($"{offset:0000000000} 00000 n ");
        }

        writer.WriteLine("trailer");
        writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine(xrefOffset);
        writer.WriteLine("%%EOF");
        writer.Flush();

        return stream.ToArray();
    }

    private static string Sanitize(string? value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
    }
}
