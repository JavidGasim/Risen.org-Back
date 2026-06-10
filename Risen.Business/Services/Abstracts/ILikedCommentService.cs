using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface ILikedCommentService
    {
        Task<List<LikedComment>> GetAllAsync();
        Task<LikedComment> GetByIdAsync(int id);
        Task AddAsync(LikedComment value);
        Task UpdateAsync(LikedComment value);
        Task DeleteAsync(LikedComment value);
    }
}
