using Microsoft.EntityFrameworkCore;

namespace Post.Query.Infrastructure.DataAccess;

public class DatabaseContextFactory
{
    private readonly Action<DbContextOptionsBuilder> _configurationDbContext;

    public DatabaseContextFactory(Action<DbContextOptionsBuilder> configurationDbContext)
    {
        _configurationDbContext = configurationDbContext;
    }

    public DatabaseContext CreateDbContext()
    {
        DbContextOptionsBuilder<DatabaseContext> builder = new();
        _configurationDbContext(builder);
        return new DatabaseContext(builder.Options);
    }
}
