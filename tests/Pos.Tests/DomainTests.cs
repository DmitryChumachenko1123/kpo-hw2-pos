using Pos.Domain;

namespace Pos.Tests;

public class DomainTests
{
    private readonly DateOnly _today = new(2026, 11, 11);
    private readonly EvaluationFactory _factory = new();
    private readonly Student _student = new(Guid.NewGuid(), "Иванова Анна", null);
    private readonly Rubric _rubric = new();

    [Fact]
    public void Score_above_max_is_rejected()
    {
        var hw = _rubric.AddCriterion("ДЗ", new MaxScore(10), new Weight(1));

        Assert.Throws<DomainException>(() => _factory.Create(_student, _rubric, hw.Id, new Score(12), _today, null));
    }

    [Fact]
    public void Score_equal_to_max_is_accepted()
    {
        var hw = _rubric.AddCriterion("ДЗ", new MaxScore(10), new Weight(1));

        var evaluation = _factory.Create(_student, _rubric, hw.Id, new Score(10), _today, null);

        Assert.Equal(10, evaluation.Score.Value);
    }

    [Fact]
    public void Negative_score_is_rejected()
    {
        Assert.Throws<DomainException>(() => new Score(-1));
    }

    [Fact]
    public void Criterion_must_be_in_rubric()
    {
        Assert.Throws<DomainException>(() => _factory.Create(_student, _rubric, Guid.NewGuid(), new Score(5), _today, null));
    }

    [Fact]
    public void Correction_checks_max_score()
    {
        var hw = _rubric.AddCriterion("ДЗ", new MaxScore(10), new Weight(1));
        var evaluation = _factory.Create(_student, _rubric, hw.Id, new Score(5), _today, null);

        Assert.Throws<DomainException>(() => evaluation.Correct(hw, new Score(11), _today, null));
        Assert.Equal(5, evaluation.Score.Value);
    }

    [Fact]
    public void Criterion_titles_must_be_unique()
    {
        _rubric.AddCriterion("Экзамен", new MaxScore(10), new Weight(1));

        Assert.Throws<DomainException>(() => _rubric.AddCriterion("экзамен", new MaxScore(10), new Weight(1)));
    }

    [Fact]
    public void Criterion_with_evaluations_cannot_be_removed()
    {
        var exam = _rubric.AddCriterion("Экзамен", new MaxScore(10), new Weight(1));

        Assert.Throws<DomainException>(() => _rubric.RemoveCriterion(exam.Id, hasEvaluations: true));
        Assert.Single(_rubric.Criteria);
    }
}
