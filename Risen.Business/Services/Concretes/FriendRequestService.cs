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
    public class FriendRequestService : IFriendRequestService
    {
        private readonly AppDbContext _appDbContext;

        public FriendRequestService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(FriendRequest friendRequest)
        {
            await _appDbContext.FriendRequests.AddAsync(friendRequest);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(FriendRequest friendRequest)
        {
            _appDbContext.FriendRequests.Remove(friendRequest);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<FriendRequest>> GetAllAsync()
        {
            var friendRequests = await _appDbContext.FriendRequests.ToListAsync();
            return friendRequests;
        }

        public async Task<FriendRequest> GetByIdAsync(int id)
        {
            var friendRequest = await _appDbContext.FriendRequests.FirstOrDefaultAsync(x => x.Id == id);
            return friendRequest;
        }

        public async Task UpdateAsync(FriendRequest friendRequest)
        {
            _appDbContext.FriendRequests.Update(friendRequest);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
