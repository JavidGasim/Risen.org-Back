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
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _appDbContext;

        public FriendService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(Friend friend)
        {
            await _appDbContext.Friends.AddAsync(friend);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Friend friend)
        {
            _appDbContext.Friends.Remove(friend);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<Friend>> GetAllAsync()
        {
            var friends = await _appDbContext.Friends.ToListAsync();
            return friends;
        }

        public async Task<Friend> GetByIdAsync(int id)
        {
            var friend = await _appDbContext.Friends.FirstOrDefaultAsync(x => x.Id == id);
            return friend;
        }

        public async Task UpdateAsync(Friend friend)
        {
            _appDbContext.Friends.Update(friend);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
