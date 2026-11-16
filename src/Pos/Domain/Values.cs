namespace Pos.Domain;

public readonly record struct Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        if (value < 0)
            throw new DomainException("Балл не может быть отрицательным.");
        Value = value;
    }

    public override string ToString() => Value.ToString("0.##");
}

public readonly record struct MaxScore
{
    public decimal Value { get; }

    public MaxScore(decimal value)
    {
        if (value <= 0)
            throw new DomainException("Максимальный балл должен быть больше нуля.");
        Value = value;
    }

    public override string ToString() => Value.ToString("0.##");
}

public readonly record struct Weight
{
    public decimal Value { get; }

    public Weight(decimal value)
    {
        if (value <= 0)
            throw new DomainException("Вес критерия должен быть больше нуля.");
        Value = value;
    }

    public override string ToString() => Value.ToString("0.##");
}
