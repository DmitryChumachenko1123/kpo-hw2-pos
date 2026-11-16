namespace Pos.Domain;

public class FinalGradeCalculator
{
    public FinalGrade Calculate(Student student, Rubric rubric, IEnumerable<Evaluation> evaluations, IFinalGradePolicy policy)
    {
        if (rubric.Criteria.Count == 0)
            throw new DomainException("Рубрикатор пуст, итог посчитать нельзя.");

        var scores = new Dictionary<Guid, Score>();
        foreach (var evaluation in evaluations.Where(e => e.StudentId == student.Id))
        {
            var criterion = rubric.Find(evaluation.CriterionId)
                ?? throw new DomainException("Есть оценка по критерию, которого нет в рубрикаторе.");
            criterion.CheckScore(evaluation.Score);

            if (!scores.TryAdd(criterion.Id, evaluation.Score))
                throw new DomainException($"У студента две оценки по критерию '{criterion.Title}'.");
        }

        var rows = rubric.Criteria
            .Select(c => (c, scores.TryGetValue(c.Id, out var s) ? s : (Score?)null))
            .ToList();

        var value = Math.Round(policy.Calculate(rows), 2, MidpointRounding.AwayFromZero);
        return new FinalGrade(student.Id, value, scores.Count, rubric.Criteria.Count);
    }
}
