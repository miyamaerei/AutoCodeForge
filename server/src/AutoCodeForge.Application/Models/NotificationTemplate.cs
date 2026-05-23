using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Application.Models;

public class NotificationTemplate
{
    public string TemplateId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Subject { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public NotificationChannel Channel { get; set; }

    public string ResolveSubject(Dictionary<string, string> variables)
    {
        return ReplaceVariables(Subject, variables);
    }

    public string ResolveContent(Dictionary<string, string> variables)
    {
        return ReplaceVariables(Content, variables);
    }

    private string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }
}

public class NotificationTemplateSettings
{
    public List<NotificationTemplate> Templates { get; set; } = new();

    public NotificationTemplate? GetTemplate(string templateId)
    {
        return Templates.FirstOrDefault(t => t.TemplateId == templateId);
    }
}