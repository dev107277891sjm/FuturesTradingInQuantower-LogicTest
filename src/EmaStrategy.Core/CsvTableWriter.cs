using System.Globalization;

namespace EmaStrategy.Core;

public sealed class CsvTableWriter : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly IReadOnlyList<string> _headers;
    private readonly object _lock = new();

    public CsvTableWriter(string filePath, IReadOnlyList<string> headers)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));

        _headers = headers ?? throw new ArgumentNullException(nameof(headers));

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        _writer = new StreamWriter(filePath, append: false);
        _writer.WriteLine(string.Join(",", _headers.Select(Escape)));
    }

    public void WriteRow(IReadOnlyList<object?> values)
    {
        // Be tolerant to upstream mistakes; during strategy development it's useful
        // to keep the sim running and inspect the exported CSV.
        if (values.Count < _headers.Count)
        {
            var padded = new object?[_headers.Count];
            for (var i = 0; i < values.Count; i++)
                padded[i] = values[i];
            for (var i = values.Count; i < _headers.Count; i++)
                padded[i] = "";
            values = padded;
        }
        else if (values.Count > _headers.Count)
        {
            values = values.Take(_headers.Count).ToArray();
        }

        lock (_lock)
        {
            var line = string.Join(",", values.Select(v => Escape(FormatValue(v))));
            _writer.WriteLine(line);
        }
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
            return "";

        return value switch
        {
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double x => x.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            bool b => b ? "True" : "False",
            _ => value.ToString() ?? ""
        };
    }

    private static string Escape(string input)
    {
        if (input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r'))
            return "\"" + input.Replace("\"", "\"\"") + "\"";
        return input;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}

