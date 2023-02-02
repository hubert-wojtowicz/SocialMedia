using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly DatabaseContextFactory _databaseContextFactory;

    public PostRepository(DatabaseContextFactory databaseContextFactory)
    {
        _databaseContextFactory = databaseContextFactory;
    }
    public async Task CreateAsync(PostEntity post)
    {
        await using var context = _databaseContextFactory.CreateDbContext();
        context.Posts.Add(post);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PostEntity post)
    {
        await using var context = _databaseContextFactory.CreateDbContext();
        context.Posts.Update(post);
    }

    public async Task DeleteAsync(Guid postId)
    {
        var existingPost = await GetAsync(postId);
        if (existingPost != null) return;

        await using var context = _databaseContextFactory.CreateDbContext();
        context.Posts.Remove(existingPost);
        await context.SaveChangesAsync();
    }

    public async Task<PostEntity> GetAsync(Guid postId)
    {
        await using var context = _databaseContextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
            .Include(x=>x.Comments)
            .FirstOrDefaultAsync(x => x.PostId == postId);
    }

    public async Task<List<PostEntity>> GetAllAsync()
    {
        await using var context = _databaseContextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
            .Include(x => x.Comments)
            .ToListAsync();
    }

    public async Task<List<PostEntity>> GetByAuthorAsync(string author)
    {
        await using var context = _databaseContextFactory.CreateDbContext();
        return await context.Posts.AsNoTracking()
            .Include(x => x.Comments)
            .Where(x => x.Author == author)
            .ToListAsync();
    }
}