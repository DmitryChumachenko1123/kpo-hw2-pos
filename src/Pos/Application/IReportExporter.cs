namespace Pos.Application;

public record FinalGradeRow(string Student, string? Group, decimal Value, int Rounded, int GradedCriteria, int TotalCriteria);

public interface IReportExporter
{
    void Export(IReadOnlyList<FinalGradeRow> rows, string path);
}
