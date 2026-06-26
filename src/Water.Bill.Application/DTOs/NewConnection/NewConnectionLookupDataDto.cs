namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionLookupDataDto
{
    public IReadOnlyList<NewConnectionLookupOptionDto> Sectors { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> PipeSizes { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> ConnectionCategories { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> ConnectionTypes { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> ConnectionSubTypes { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> DocumentTypes { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> Villages { get; set; } = [];
}

public class NewConnectionSectorContextDto
{
    public int? DevType { get; set; }

    public string DivisionDisplay { get; set; } = string.Empty;

    public IReadOnlyList<NewConnectionLookupOptionDto> Blocks { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> ConnectionCategories { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> PipeSizes { get; set; } = [];

    public IReadOnlyList<NewConnectionLookupOptionDto> Villages { get; set; } = [];
}

public class NewConnectionLookupOptionDto
{
    public string Value { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string? ParentValue { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsRequired { get; set; }
}
