namespace Pos.Domain;

public class Evaluation
{
    public Guid Id { get; }
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public Score Score { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Comment { get; private set; }

    private Evaluation(Guid id, Guid studentId, Guid criterionId, Score score, DateOnly date, string? comment)
    {
        Id = id;
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
        Date = date;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    public static Evaluation Create(Guid studentId, Criterion criterion, Score score, DateOnly date, string? comment)
    {
        criterion.CheckScore(score);
        return new Evaluation(Guid.NewGuid(), studentId, criterion.Id, score, date, comment);
    }

    public static Evaluation Restore(Guid id, Guid studentId, Guid criterionId, Score score, DateOnly date, string? comment) =>
        new(id, studentId, criterionId, score, date, comment);

    public void Correct(Criterion criterion, Score score, DateOnly date, string? comment)
    {
        if (criterion.Id != CriterionId)
            throw new DomainException("Оценку можно исправить только по тому же критерию.");

        criterion.CheckScore(score);
        Score = score;
        Date = date;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
