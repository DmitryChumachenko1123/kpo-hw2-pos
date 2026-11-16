namespace Pos.Domain;

public class EvaluationFactory
{
    public Evaluation Create(Student student, Rubric rubric, Guid criterionId, Score score, DateOnly date, string? comment)
    {
        var criterion = rubric.Get(criterionId);
        criterion.CheckScore(score);
        return new Evaluation(Guid.NewGuid(), student.Id, criterion.Id, score, date, comment);
    }
}
