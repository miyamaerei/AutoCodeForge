using AutoCodeForge.Core.Entities.Base;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class BaseRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;

    public BaseRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.baserepo.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(TestUserOwnedEntity));
    }

    [Fact]
    public async Task CreateAsync_WhenNtIdMissing_StampsAuditFieldsAndCurrentUser()
    {
        var repository = new BaseRepository<TestUserOwnedEntity>(_db, new TestCurrentUser("owner.a"));
        var entity = new TestUserOwnedEntity
        {
            Id = Guid.NewGuid(),
            Name = "Created",
        };

        var created = await repository.CreateAsync(entity);

        Assert.Equal("owner.a", created.NtId);
        Assert.False(created.IsDeleted);
        Assert.True(created.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1));
        Assert.True(created.UpdatedAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task GetAllAsync_ShouldRespectNtIdAndSoftDeleteFilters()
    {
        var ownerARepository = new BaseRepository<TestUserOwnedEntity>(_db, new TestCurrentUser("owner.a"));
        var ownerBRepository = new BaseRepository<TestUserOwnedEntity>(_db, new TestCurrentUser("owner.b"));

        var ownerAEntity = await ownerARepository.CreateAsync(new TestUserOwnedEntity
        {
            Id = Guid.NewGuid(),
            Name = "owner-a-visible",
        });

        _ = await ownerBRepository.CreateAsync(new TestUserOwnedEntity
        {
            Id = Guid.NewGuid(),
            Name = "owner-b-visible",
        });

        var deletedEntity = await ownerARepository.CreateAsync(new TestUserOwnedEntity
        {
            Id = Guid.NewGuid(),
            Name = "owner-a-deleted",
        });
        await ownerARepository.SoftDeleteAsync(deletedEntity.Id);

        var visibleToOwnerA = await ownerARepository.GetAllAsync();
        var visibleToAll = await ownerARepository.GetAllAsync(includeAllUsers: true);

        Assert.Single(visibleToOwnerA);
        Assert.Equal(ownerAEntity.Id, visibleToOwnerA[0].Id);
        Assert.Equal(2, visibleToAll.Count);
        Assert.DoesNotContain(visibleToAll, item => item.Name == "owner-a-deleted");
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldHideEntityFromDefaultQueries()
    {
        var repository = new BaseRepository<TestUserOwnedEntity>(_db, new TestCurrentUser("owner.a"));
        var entity = await repository.CreateAsync(new TestUserOwnedEntity
        {
            Id = Guid.NewGuid(),
            Name = "delete-me",
        });

        await repository.SoftDeleteAsync(entity.Id);

        var hidden = await repository.GetByIdAsync(entity.Id);
        var visibleWithBypass = await repository.GetByIdAsync(entity.Id, includeAllUsers: true);

        Assert.Null(hidden);
        Assert.Null(visibleWithBypass);

        var raw = await _db.Queryable<TestUserOwnedEntity>().FirstAsync(item => item.Id == entity.Id);
        Assert.NotNull(raw);
        Assert.True(raw!.IsDeleted);
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
        }
    }

    [SugarTable("BaseRepositoryTestEntities")]
    private sealed class TestUserOwnedEntity : UserOwnedEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        public new Guid Id { get; set; }

        [SugarColumn(Length = 120, IsNullable = false)]
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        private readonly string? _ntId;

        public TestCurrentUser(string? ntId)
        {
            _ntId = ntId;
        }

        public string? GetCurrentNtId()
        {
            return _ntId;
        }

        public bool IsAdmin()
        {
            return false;
        }
    }
}