namespace Asm.MooBank.Domain.Entities.Reports;

public class TagAverage
{
    public int TagId { get; set; }

    public required string Name { get; set; }

    public decimal Average { get; set; }

}
