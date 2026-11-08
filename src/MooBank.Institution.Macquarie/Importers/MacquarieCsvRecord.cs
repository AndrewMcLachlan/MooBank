using CsvHelper;

namespace Asm.MooBank.Institution.Macquarie.Importers;

[CsvRecord]
internal partial class MacquarieCsvRecord
{
    [CsvColumn("Transaction Date")]
    public string TransactionDate { get; set; } = String.Empty;

    [CsvColumn("Details")]
    public string Details { get; set; } = String.Empty;

    [CsvColumn("Account")]
    public string Account { get; set; } = String.Empty;

    [CsvColumn("Category")]
    public string Category { get; set; } = String.Empty;

    [CsvColumn("Subcategory")]
    public string Subcategory { get; set; } = String.Empty;

    [CsvColumn("Tags")]
    public string Tags { get; set; } = String.Empty;

    [CsvColumn("Notes")]
    public string Notes { get; set; } = String.Empty;

    [CsvColumn("Debit")]
    public string Debit { get; set; } = String.Empty;

    [CsvColumn("Credit")]
    public string Credit { get; set; } = String.Empty;

    [CsvColumn("Balance")]
    public string Balance { get; set; } = String.Empty;

    [CsvColumn("Original Description")]
    public string OriginalDescription { get; set; } = String.Empty;
}
