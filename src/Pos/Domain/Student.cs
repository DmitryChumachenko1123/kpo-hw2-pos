namespace Pos.Domain;

public class Student
{
    public Guid Id { get; }
    public string Name { get; }
    public string? Group { get; }

    public Student(Guid id, string name, string? group)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("ФИО студента не может быть пустым.");

        Id = id;
        Name = name.Trim();
        Group = string.IsNullOrWhiteSpace(group) ? null : group.Trim();
    }
}
