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
    public class LikedPostService : ILikedPostService
    {
        private readonly AppDbContext _appDbContext;

        public LikedPostService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(LikedPost value)
        {
            await _appDbContext.LikedPosts.AddAsync(value);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(LikedPost value)
        {
            _appDbContext.LikedPosts.Remove(value);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<LikedPost>> GetAllAsync()
        {
            var likedPosts = await _appDbContext.LikedPosts.Include(nameof(LikedPost.User)).Include(nameof(LikedPost.Post)).ToListAsync();
            return likedPosts;
        }

        public async Task<LikedPost> GetByIdAsync(int id)
        {
            var likedPosts = await _appDbContext.LikedPosts.Include(nameof(LikedPost.User)).Include(nameof(LikedPost.Post)).FirstOrDefaultAsync(x => x.Id == id);
            return likedPosts;
        }

        public async Task UpdateAsync(LikedPost value)
        {
            _appDbContext.LikedPosts.Update(value);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<LikedPost>> GetByUserIdAsync(string userId)
        {
            return await _appDbContext.LikedPosts
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
