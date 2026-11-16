using Pos.Domain;

namespace Pos.Application;

public class PosFacade
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubric;
    private readonly IEvaluationRepository _evaluations;
    private readonly EvaluationFactory _factory;
    private readonly FinalGradeCalculator _calculator;
    private readonly IReportExporter _exporter;
    private readonly List<IFinalGradePolicy> _policies;

    public PosFacade(
        IStudentRepository students,
        IRubricRepository rubric,
        IEvaluationRepository evaluations,
        EvaluationFactory factory,
        FinalGradeCalculator calculator,
        IReportExporter exporter,
        IEnumerable<IFinalGradePolicy> policies)
    {
        _students = students;
        _rubric = rubric;
        _evaluations = evaluations;
        _factory = factory;
        _calculator = calculator;
        _exporter = exporter;
        _policies = policies.ToList();
        CurrentPolicy = _policies[0];
    }

    public IFinalGradePolicy CurrentPolicy { get; private set; }

    public IReadOnlyList<IFinalGradePolicy> Policies => _policies;

    public void SelectPolicy(IFinalGradePolicy policy) => CurrentPolicy = policy;

    public List<Student> GetStudents() => _students.GetAll().OrderBy(s => s.Name).ToList();

    public Student AddStudent(string name, string? group)
    {
        var student = new Student(Guid.NewGuid(), name, group);
        _students.Add(student);
        return student;
    }

    public void RemoveStudent(Guid studentId)
    {
        var student = GetStudent(studentId);
        foreach (var evaluation in GetEvaluations(student.Id))
            _evaluations.Remove(evaluation.Id);
        _students.Remove(student.Id);
    }

    public IReadOnlyList<Criterion> GetCriteria() => _rubric.Get().Criteria;

    public Criterion AddCriterion(string title, decimal maxScore, decimal weight)
    {
        var rubric = _rubric.Get();
        var criterion = rubric.AddCriterion(title, new MaxScore(maxScore), new Weight(weight));
        _rubric.Save(rubric);
        return criterion;
    }

    public void RemoveCriterion(Guid criterionId)
    {
        var rubric = _rubric.Get();
        var hasEvaluations = _evaluations.GetAll().Any(e => e.CriterionId == criterionId);
        rubric.RemoveCriterion(criterionId, hasEvaluations);
        _rubric.Save(rubric);
    }

    public List<Evaluation> GetEvaluations(Guid? studentId = null) =>
        _evaluations.GetAll().Where(e => studentId == null || e.StudentId == studentId).ToList();

    public Evaluation Grade(Guid studentId, Guid criterionId, decimal score, string? comment)
    {
        var student = GetStudent(studentId);
        if (GetEvaluations(studentId).Any(e => e.CriterionId == criterionId))
            throw new DomainException("Оценка по этому критерию уже стоит, её можно только исправить.");

        var evaluation = _factory.Create(student, _rubric.Get(), criterionId, new Score(score), Today(), comment);
        _evaluations.Save(evaluation);
        return evaluation;
    }

    public void CorrectEvaluation(Guid evaluationId, decimal score, string? comment)
    {
        var evaluation = GetEvaluation(evaluationId);
        var criterion = _rubric.Get().Get(evaluation.CriterionId);
        evaluation.Correct(criterion, new Score(score), Today(), comment);
        _evaluations.Save(evaluation);
    }

    public void CancelEvaluation(Guid evaluationId) => _evaluations.Remove(GetEvaluation(evaluationId).Id);

    public FinalGrade GetFinalGrade(Guid studentId) =>
        _calculator.Calculate(GetStudent(studentId), _rubric.Get(), _evaluations.GetAll(), CurrentPolicy);

    public List<FinalGradeRow> GetFinalGrades()
    {
        var rubric = _rubric.Get();
        var evaluations = _evaluations.GetAll();
        return GetStudents()
            .Select(s =>
            {
                var grade = _calculator.Calculate(s, rubric, evaluations, CurrentPolicy);
                return new FinalGradeRow(s.Name, s.Group, grade.Value, grade.Rounded, grade.GradedCriteria, grade.TotalCriteria);
            })
            .ToList();
    }

    public int ExportFinalGrades(string path)
    {
        var rows = GetFinalGrades();
        _exporter.Export(rows, path);
        return rows.Count;
    }

    private Student GetStudent(Guid id) =>
        _students.Find(id) ?? throw new DomainException("Студент не найден.");

    private Evaluation GetEvaluation(Guid id) =>
        _evaluations.Find(id) ?? throw new DomainException("Оценка не найдена.");

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);
}
