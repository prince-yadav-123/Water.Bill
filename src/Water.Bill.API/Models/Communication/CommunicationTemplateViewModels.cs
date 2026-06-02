using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Communication;

public class CommunicationTemplateListViewModel
{
    public string? Search { get; set; }
    public string? PurposeKey { get; set; }
    public string? Channel { get; set; }
    public string? ActiveStatus { get; set; }
    public List<SelectListItem> Purposes { get; set; } = [];
    public List<SelectListItem> Channels { get; set; } = [];
    public IReadOnlyList<Water.Bill.Infrastructure.Data.Entities.CommunicationTemplate> Templates { get; set; } = [];
}

public class CommunicationTemplateFormViewModel
{
    public int? Id { get; set; }
    public int PurposeId { get; set; }
    public string PurposeKey { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? ExternalTemplateId { get; set; }
    public string? Language { get; set; }
    public bool IsDefault { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public List<SelectListItem> Purposes { get; set; } = [];
    public List<SelectListItem> Channels { get; set; } = [];
    public IReadOnlyList<string> AllowedPlaceholders { get; set; } = [];
}

public class CommunicationTemplatePreviewViewModel
{
    public int Id { get; set; }
    public string PurposeKey { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string PreviewSubject { get; set; } = string.Empty;
    public string PreviewBody { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> SampleValues { get; set; } = new Dictionary<string, string>();
}
