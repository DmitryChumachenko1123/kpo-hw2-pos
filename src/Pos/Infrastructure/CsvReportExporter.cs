using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public class CsvReportExporter : IReportExporter
{
    public void Export(IReadOnlyList<FinalGradeRow> rows, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ФИО;Группа;Итог;Округлённый итог;Выставлено критериев;Всего критериев");
        foreach (var row in rows)
        {
            var value = row.Value.ToString("0.00", CultureInfo.InvariantCulture);
            sb.AppendLine($"{Clean(row.Student)};{Clean(row.Group)};{value};{row.Rounded};{row.GradedCriteria};{row.TotalCriteria}");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (directory != null)
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    private static string Clean(string? value) => value?.Replace(';', ',') ?? "";
}
