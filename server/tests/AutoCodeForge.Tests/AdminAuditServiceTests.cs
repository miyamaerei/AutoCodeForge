using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class AdminAuditServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly MutableCurrentUser _currentUser;
    private readonly AdminAuditService _service;

    public AdminAuditServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.adminaudit.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(AdminAuditLogEntity), typeof(GlobalConfigEntity));

        _currentUser = new MutableCurrentUser();
        var auditRepository = new AdminAuditLogRepository(_db, _currentUser);
        var globalConfigRepository = new GlobalConfigRepository(_db, _currentUser);
        _service = new AdminAuditService(auditRepository, globalConfigRepository, _currentUser);
    }

    [Fact]
    public async Task AuthorizeCrossTenantAsync_NonAdmin_ShouldDenyAndWriteAudit()
    {
        _currentUser.Set("normal.user", isAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.AuthorizeCrossTenantAsync("GET", "/api/v1/admin/audit-logs", "AdminAuditLogs", "tenant.b"));

        var logs = await _service.GetAuditLogsAsync(1, 20);
        Assert.Single(logs.Items);
        var entry = logs.Items[0];
        Assert.Equal("normal.user", entry.AdminNtId);
        Assert.Equal("tenant.b", entry.TargetNtId);
        Assert.Equal("AdminAuditLogs", entry.ResourceType);
        Assert.Equal("deny", entry.AccessDecision);
        Assert.Equal("not-admin", entry.DecisionReason);
    }

    [Fact]
    public async Task AuthorizeCrossTenantAsync_AdminWithoutWhitelist_ShouldDenyAndWriteAudit()
    {
        _currentUser.Set("admin.a", isAdmin: true);
        await _service.SetWhitelistAsync("admin.other|AdminAuditLogs|tenant.b");

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.AuthorizeCrossTenantAsync("GET", "/api/v1/admin/audit-logs", "AdminAuditLogs", "tenant.b"));

        var logs = await _service.GetAuditLogsAsync(1, 20);
        Assert.Single(logs.Items);
        Assert.Equal("deny", logs.Items[0].AccessDecision);
        Assert.Equal("not-whitelisted", logs.Items[0].DecisionReason);
    }

    [Fact]
    public async Task AuthorizeCrossTenantAsync_ScopedWhitelistMatch_ShouldAllowAndWriteAudit()
    {
        _currentUser.Set("admin.a", isAdmin: true);
        await _service.SetWhitelistAsync("admin.a|AdminAuditLogs|tenant.b");

        await _service.AuthorizeCrossTenantAsync(
            "GET",
            "/api/v1/admin/audit-logs",
            "AdminAuditLogs",
            "tenant.b");

        var logs = await _service.GetAuditLogsAsync(1, 20);
        Assert.Single(logs.Items);
        Assert.Equal("allow", logs.Items[0].AccessDecision);
        Assert.Equal("whitelist-match", logs.Items[0].DecisionReason);
    }

    [Fact]
    public async Task AuthorizeCrossTenantAsync_EmptyWhitelistBootstrap_ShouldAllowForWhitelistWrite()
    {
        _currentUser.Set("admin.bootstrap", isAdmin: true);
        await _service.SetWhitelistAsync(string.Empty);

        await _service.AuthorizeCrossTenantAsync(
            "PUT",
            "/api/v1/admin/whitelist",
            "AdminWhitelistWrite",
            allowBootstrapWhenWhitelistEmpty: true);

        var logs = await _service.GetAuditLogsAsync(1, 20);
        Assert.Single(logs.Items);
        Assert.Equal("allow", logs.Items[0].AccessDecision);
        Assert.Equal("bootstrap-empty-whitelist", logs.Items[0].DecisionReason);
    }

    [Fact]
    public async Task SetWhitelistAsync_ShouldNormalizeLegacyAndScopedRules()
    {
        await _service.SetWhitelistAsync("admin.a|*|*,admin.a,admin.a|AdminAuditLogs|tenant.b,admin.a|AdminAuditLogs|tenant.b");

        var whitelist = await _service.GetWhitelistAsync();
        Assert.Equal("admin.a,admin.a|AdminAuditLogs|tenant.b", whitelist);
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (!File.Exists(_dbPath))
        {
            return;
        }

        try
        {
            File.Delete(_dbPath);
        }
        catch (IOException)
        {
            // The OS can still hold a short-lived sqlite lock at teardown.
        }
    }

    private sealed class MutableCurrentUser : ICurrentUser
    {
        private string? _ntId;
        private bool _isAdmin;

        public void Set(string? ntId, bool isAdmin)
        {
            _ntId = ntId;
            _isAdmin = isAdmin;
        }

        public string? GetCurrentNtId()
        {
            return _ntId;
        }

        public bool IsAdmin()
        {
            return _isAdmin;
        }
    }
}
