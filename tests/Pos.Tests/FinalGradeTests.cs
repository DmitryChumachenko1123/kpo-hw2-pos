using Pos.Domain;

namespace Pos.Tests;

public class FinalGradeTests
{
    private readonly DateOnly _today = new(2026, 11, 11);
    private readonly EvaluationFactory _factory = new();
    private readonly FinalGradeCalculator _calculator = new();
    private readonly Student _student = new(Guid.NewGuid(), "Петров Борис", null);
    private readonly Rubric _rubric = new();
    private readonly Criterion _hw;
    private readonly Criterion _test;
    private readonly Criterion _exam;

    public FinalGradeTests()
    {
        _hw = _rubric.AddCriterion("ДЗ", new MaxScore(10), new Weight(4));
        _test = _rubric.AddCriterion("Контрольная", new MaxScore(20), new Weight(3));
        _exam = _rubric.AddCriterion("Экзамен", new MaxScore(10), new Weight(3));
    }

    [Fact]
    public void Weighted_total()
    {
        var evaluations = new[] { Grade(_hw, 9), Grade(_test, 17), Grade(_exam, 8) };

        var grade = _calculator.Calculate(_student, _rubric, evaluations, new WeightedTotalPolicy());

        Assert.Equal(8.55m, grade.Value);
        Assert.Equal(9, grade.Rounded);
    }

    [Fact]
    public void Missing_evaluation_counts_as_zero()
    {
        var evaluations = new[] { Grade(_hw, 6.5m), Grade(_test, 12) };

        var grade = _calculator.Calculate(_student, _rubric, evaluations, new WeightedTotalPolicy());

        Assert.Equal(4.40m, grade.Value);
        Assert.Equal(2, grade.GradedCriteria);
    }

    [Fact]
    public void Current_progress_ignores_missing_evaluations()
    {
        var evaluations = new[] { Grade(_hw, 6.5m), Grade(_test, 12) };

        var grade = _calculator.Calculate(_student, _rubric, evaluations, new CurrentProgressPolicy());

        Assert.Equal(6.29m, grade.Value);
    }

    [Fact]
    public void Evaluation_for_unknown_criterion_is_rejected()
    {
        var stale = Evaluation.Restore(Guid.NewGuid(), _student.Id, Guid.NewGuid(), new Score(5), _today, null);

        Assert.Throws<DomainException>(() =>
            _calculator.Calculate(_student, _rubric, new[] { stale }, new WeightedTotalPolicy()));
    }

    [Theory]
    [InlineData(7.49, 7)]
    [InlineData(7.5, 8)]
    public void Final_grade_is_rounded_half_up(decimal value, int expected)
    {
        Assert.Equal(expected, new FinalGrade(_student.Id, value, 1, 1).Rounded);
    }

    private Evaluation Grade(Criterion criterion, decimal score) =>
        _factory.Create(_student, _rubric, criterion.Id, new Score(score), _today, null);
}
