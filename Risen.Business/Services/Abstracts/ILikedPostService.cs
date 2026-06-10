using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface ILikedPostService
    {
        Task<List<LikedPost>> GetAllAsync();
        Task<LikedPost> GetByIdAsync(int id);
        Task AddAsync(LikedPost value);
        Task UpdateAsync(LikedPost value);
        Task DeleteAsync(LikedPost value);
    }
}
