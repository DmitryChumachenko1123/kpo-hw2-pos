namespace Pos.Domain;

public record FinalGrade(Guid StudentId, decimal Value, int GradedCriteria, int TotalCriteria)
{
    public int Rounded => (int)Math.Round(Value, MidpointRounding.AwayFromZero);
}
