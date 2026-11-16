namespace Pos.Domain;

public class Criterion
{
    public Guid Id { get; }
    public string Title { get; }
    public MaxScore MaxScore { get; }
    public Weight Weight { get; }

    public Criterion(Guid id, string title, MaxScore maxScore, Weight weight)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Название критерия не может быть пустым.");

        Id = id;
        Title = title.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }

    public void CheckScore(Score score)
    {
        if (score.Value > MaxScore.Value)
            throw new DomainException($"Балл {score} больше максимума по критерию '{Title}' ({MaxScore}).");
    }
}
