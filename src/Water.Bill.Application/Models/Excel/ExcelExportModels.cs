namespace Water.Bill.Application.Models.Excel;

public sealed class ExcelColumnDefinition<T>
{
    public required string Header { get; init; }
    public required Func<T, object?> ValueFactory { get; init; }
    public string? NumberFormat { get; init; }
    public double? Width { get; init; }
}

public sealed class ExcelExportRequest<T>
{
    public required string SheetName { get; init; }
    public required IReadOnlyList<ExcelColumnDefinition<T>> Columns { get; init; }
    public required IReadOnlyList<T> Rows { get; init; }
}
