namespace Pos.Domain;

public interface IStudentRepository
{
    List<Student> GetAll();
    Student? Find(Guid id);
    void Add(Student student);
    void Remove(Guid id);
}

public interface IRubricRepository
{
    Rubric Get();
    void Save(Rubric rubric);
}

public interface IEvaluationRepository
{
    List<Evaluation> GetAll();
    Evaluation? Find(Guid id);
    void Save(Evaluation evaluation);
    void Remove(Guid id);
}
