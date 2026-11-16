namespace Pos.Domain;

public class Rubric
{
    private readonly List<Criterion> _criteria = new();

    public IReadOnlyList<Criterion> Criteria => _criteria;

    public Rubric()
    {
    }

    public Rubric(IEnumerable<Criterion> criteria)
    {
        foreach (var criterion in criteria)
            Add(criterion);
    }

    public Criterion AddCriterion(string title, MaxScore maxScore, Weight weight)
    {
        var criterion = new Criterion(Guid.NewGuid(), title, maxScore, weight);
        Add(criterion);
        return criterion;
    }

    public Criterion? Find(Guid id) => _criteria.FirstOrDefault(c => c.Id == id);

    public Criterion Get(Guid id) =>
        Find(id) ?? throw new DomainException("Такого критерия нет в рубрикаторе.");

    public void RemoveCriterion(Guid id, bool hasEvaluations)
    {
        var criterion = Get(id);
        if (hasEvaluations)
            throw new DomainException($"По критерию '{criterion.Title}' уже есть оценки, сначала отмените их.");

        _criteria.Remove(criterion);
    }

    private void Add(Criterion criterion)
    {
        if (_criteria.Any(c => c.Title.Equals(criterion.Title, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Критерий '{criterion.Title}' уже есть в рубрикаторе.");

        _criteria.Add(criterion);
    }
}
