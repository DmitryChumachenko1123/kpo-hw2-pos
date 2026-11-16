namespace Pos.Domain;

public interface IFinalGradePolicy
{
    string Name { get; }

    decimal Calculate(IReadOnlyList<(Criterion Criterion, Score? Score)> rows);
}

public class WeightedTotalPolicy : IFinalGradePolicy
{
    public string Name => "Накопленная (нет оценки = 0)";

    public decimal Calculate(IReadOnlyList<(Criterion Criterion, Score? Score)> rows) => WeightedAverage(rows);

    internal static decimal WeightedAverage(IEnumerable<(Criterion Criterion, Score? Score)> rows)
    {
        decimal sum = 0, weights = 0;
        foreach (var (criterion, score) in rows)
        {
            var ratio = score is null ? 0 : score.Value.Value / criterion.MaxScore.Value;
            sum += criterion.Weight.Value * ratio;
            weights += criterion.Weight.Value;
        }
        return weights == 0 ? 0 : 10 * sum / weights;
    }
}

public class CurrentProgressPolicy : IFinalGradePolicy
{
    public string Name => "Текущая (только выставленные)";

    public decimal Calculate(IReadOnlyList<(Criterion Criterion, Score? Score)> rows) =>
        WeightedTotalPolicy.WeightedAverage(rows.Where(r => r.Score is not null));
}
