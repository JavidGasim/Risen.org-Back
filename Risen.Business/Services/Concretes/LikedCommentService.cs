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
    public class LikedCommentService : ILikedCommentService
    {
        private readonly AppDbContext _db;

        public LikedCommentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(LikedComment value)
        {
            await _db.LikedComments.AddAsync(value);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(LikedComment value)
        {
            _db.LikedComments.Remove(value);
            await _db.SaveChangesAsync();
        }

        public async Task<List<LikedComment>> GetAllAsync()
        {
            var likedComments = await _db.LikedComments.Include(nameof(LikedComment.User)).Include(nameof(LikedComment.Comment)).ToListAsync();
            return likedComments;
        }

        public async Task<LikedComment> GetByIdAsync(int id)
        {
            var likedComment = await _db.LikedComments.Include(nameof(LikedComment.User)).Include(nameof(LikedComment.Comment)).FirstOrDefaultAsync(x => x.Id == id);
            return likedComment;
        }

        public async Task UpdateAsync(LikedComment value)
        {
            _db.LikedComments.Update(value);
            await _db.SaveChangesAsync();
        }

        public async Task<List<LikedComment>> GetByUserIdAsync(string userId)
        {
            return await _db.LikedComments
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
