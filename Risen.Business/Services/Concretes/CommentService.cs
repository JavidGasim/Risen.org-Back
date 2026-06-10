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
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;

        public CommentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Comment comment)
        {
            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Comment comment)
        {
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetAllAsync()
        {
            var chats = await _db.Comments.Include(nameof(Comment.Sender)).Include(nameof(Comment.Post)).ToListAsync();
            return chats;
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            var chat = await _db.Comments.Include(nameof(Comment.Sender)).Include(nameof(Comment.Post)).FirstOrDefaultAsync(x => x.Id == id);
            return chat;
        }

        public async Task UpdateAsync(Comment comment)
        {
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync();
        }
    }
}
