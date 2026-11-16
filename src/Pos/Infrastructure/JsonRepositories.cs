using Pos.Domain;

namespace Pos.Infrastructure;

public record StudentDto(Guid Id, string Name, string? Group);

public record CriterionDto(Guid Id, string Title, decimal MaxScore, decimal Weight);

public record EvaluationDto(Guid Id, Guid StudentId, Guid CriterionId, decimal Score, DateOnly Date, string? Comment);

public class JsonStudentRepository : IStudentRepository
{
    private readonly JsonFile<List<StudentDto>> _file;
    private readonly List<Student> _students;

    public JsonStudentRepository(StorageSettings settings)
    {
        _file = new JsonFile<List<StudentDto>>(settings.DataDirectory, "students.json");
        _students = _file.Load().Select(d => new Student(d.Id, d.Name, d.Group)).ToList();
    }

    public List<Student> GetAll() => _students.ToList();

    public Student? Find(Guid id) => _students.FirstOrDefault(s => s.Id == id);

    public void Add(Student student)
    {
        _students.Add(student);
        Flush();
    }

    public void Remove(Guid id)
    {
        _students.RemoveAll(s => s.Id == id);
        Flush();
    }

    private void Flush() => _file.Save(_students.Select(s => new StudentDto(s.Id, s.Name, s.Group)).ToList());
}

public class JsonRubricRepository : IRubricRepository
{
    private readonly JsonFile<List<CriterionDto>> _file;
    private Rubric _rubric;

    public JsonRubricRepository(StorageSettings settings)
    {
        _file = new JsonFile<List<CriterionDto>>(settings.DataDirectory, "rubric.json");
        _rubric = new Rubric(_file.Load().Select(d =>
            new Criterion(d.Id, d.Title, new MaxScore(d.MaxScore), new Weight(d.Weight))));
    }

    public Rubric Get() => _rubric;

    public void Save(Rubric rubric)
    {
        _rubric = rubric;
        _file.Save(rubric.Criteria
            .Select(c => new CriterionDto(c.Id, c.Title, c.MaxScore.Value, c.Weight.Value))
            .ToList());
    }
}

public class JsonEvaluationRepository : IEvaluationRepository
{
    private readonly JsonFile<List<EvaluationDto>> _file;
    private readonly List<Evaluation> _evaluations;

    public JsonEvaluationRepository(StorageSettings settings)
    {
        _file = new JsonFile<List<EvaluationDto>>(settings.DataDirectory, "evaluations.json");
        _evaluations = _file.Load()
            .Select(d => Evaluation.Restore(d.Id, d.StudentId, d.CriterionId, new Score(d.Score), d.Date, d.Comment))
            .ToList();
    }

    public List<Evaluation> GetAll() => _evaluations.ToList();

    public Evaluation? Find(Guid id) => _evaluations.FirstOrDefault(e => e.Id == id);

    public void Save(Evaluation evaluation)
    {
        if (!_evaluations.Contains(evaluation))
            _evaluations.Add(evaluation);
        Flush();
    }

    public void Remove(Guid id)
    {
        _evaluations.RemoveAll(e => e.Id == id);
        Flush();
    }

    private void Flush() =>
        _file.Save(_evaluations
            .Select(e => new EvaluationDto(e.Id, e.StudentId, e.CriterionId, e.Score.Value, e.Date, e.Comment))
            .ToList());
}
