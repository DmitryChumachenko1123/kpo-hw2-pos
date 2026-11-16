namespace Pos.Domain;

public class EvaluationFactory
{
    public Evaluation Create(Student student, Rubric rubric, Guid criterionId, Score score, DateOnly date, string? comment)
    {
        var criterion = rubric.Get(criterionId);
        return Evaluation.Create(student.Id, criterion, score, date, comment);
    }
}
