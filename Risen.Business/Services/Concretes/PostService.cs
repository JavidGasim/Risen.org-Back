using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _db;

        public PostService(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Post post)
        {
            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Post>> GetAllAsync()
        {
            var posts = await _db.Posts
        .Include(x => x.Sender)
        .Include(x => x.Comments)
            .ThenInclude(c => c.Sender)
        .ToListAsync();
            return posts;
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            var post = await _db.Posts
        .Include(x => x.Sender)
        .Include(x => x.Comments)
            .ThenInclude(c => c.Sender)
        .FirstOrDefaultAsync(x => x.Id == id);
            return post;
        }

        public async Task UpdateAsync(Post post)
        {
            _db.Posts.Update(post);
            await _db.SaveChangesAsync();
        }
    }
}
