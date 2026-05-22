using AutoCodeForge.Core.Entities;
using SqlSugar;

namespace AutoCodeForge.Application.Services;

public class ChatDefaultsProvisioningService
{
    private const string DefaultAgentName = "default-worker";
    private const string DefaultModelName = "gpt-4o-mini";
    private const string DefaultModelEndpoint = "https://example.local/azure-openai";
    private const string DefaultModelApiKey = "encrypted-placeholder";

    private readonly ISqlSugarClient _db;

    public ChatDefaultsProvisioningService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task EnsureDefaultsForNtIdAsync(string ntId, CancellationToken cancellationToken = default)
    {
        var normalizedNtId = ntId.Trim();
        if (string.IsNullOrWhiteSpace(normalizedNtId))
        {
            return;
        }

        var model = await EnsureDefaultModelConfigAsync(normalizedNtId, cancellationToken);
        await EnsureDefaultWorkerAsync(normalizedNtId, model?.Id, cancellationToken);
    }

    private async Task<LLMModelConfigEntity?> EnsureDefaultModelConfigAsync(string ntId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var existing = await _db.Queryable<LLMModelConfigEntity>()
            .Where(config => !config.IsDeleted && config.NtId == ntId)
            .OrderBy(config => config.CreatedAtUtc)
            .FirstAsync();

        if (existing is not null)
        {
            return existing;
        }

        var template = await _db.Queryable<LLMModelConfigEntity>()
            .Where(config => !config.IsDeleted)
            .OrderBy(config => config.CreatedAtUtc)
            .FirstAsync();

        var utcNow = DateTime.UtcNow;
        var created = template is null
            ? new LLMModelConfigEntity
            {
                Id = Guid.NewGuid(),
                NtId = ntId,
                Provider = LLMProvider.AzureOpenAI,
                ModelName = DefaultModelName,
                Endpoint = DefaultModelEndpoint,
                ApiKey = DefaultModelApiKey,
                IsDeleted = false,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow,
            }
            : new LLMModelConfigEntity
            {
                Id = Guid.NewGuid(),
                NtId = ntId,
                Provider = template.Provider,
                ModelName = template.ModelName,
                Endpoint = template.Endpoint,
                ApiKey = template.ApiKey,
                CliExecutable = template.CliExecutable,
                Organization = template.Organization,
                AuthMode = template.AuthMode,
                PatEnvVar = template.PatEnvVar,
                IsDeleted = false,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow,
            };

        await _db.Insertable(created).ExecuteCommandAsync();
        return created;
    }

    private async Task EnsureDefaultWorkerAsync(string ntId, Guid? llmModelConfigId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var existing = await _db.Queryable<AgentEntity>()
            .Where(agent => !agent.IsDeleted && agent.NtId == ntId && agent.Name == DefaultAgentName)
            .FirstAsync();

        if (existing is not null)
        {
            if (!existing.LlmModelConfigId.HasValue && llmModelConfigId.HasValue)
            {
                existing.LlmModelConfigId = llmModelConfigId;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await _db.Updateable(existing).ExecuteCommandAsync();
            }

            return;
        }

        var utcNow = DateTime.UtcNow;
        await _db.Insertable(new AgentEntity
        {
            Id = Guid.NewGuid(),
            NtId = ntId,
            Name = DefaultAgentName,
            Description = "Default worker agent profile",
            LlmModelConfigId = llmModelConfigId,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        }).ExecuteCommandAsync();
    }
}