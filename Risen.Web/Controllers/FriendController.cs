using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly IFriendService _friendService;
        private readonly IFriendRequestService _friendRequestService;
        private readonly AppDbContext _db;


        public FriendController(UserManager<CustomIdentityUser> userManager, IFriendService friendService, IFriendRequestService friendRequestService)
        {
            _userManager = userManager;
            _friendService = friendService;
            _friendRequestService = friendRequestService;
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFriends()
        {
            var q = _db.Users.OrderBy(u => u.FullName);
            var items = q.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.UniversityId,
                u.Stats,
                IsAdmin = _db.UserRoles.Any(ur => ur.UserId == u.Id && _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin"))
            }).ToList();

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var requests = await _friendRequestService.GetAllAsync();
            var datas = items;
            var myRequests = requests.Where(r => r.SenderId == user.Id.ToString());
            var friends = await _friendService.GetAllAsync();
            var myFriends = friends.Where(f => f.OwnId == user.Id.ToString() || f.YourFriendId == user.Id.ToString());

            var friendUsers = datas
            .Where(u => myFriends.Any(f => f.OwnId == u.Id.ToString() || f.YourFriendId == u.Id.ToString()) && u.Id != user.Id)
            .Select(u => new CustomIdentityUser
            {
                Id = u.Id,
                UserName = u.Email,
                Email = u.Email
            })
            .ToList();

            return Ok(friendUsers);
        }

        [Authorize]
        [HttpPost("send-request/{receiverId}")]
        public async Task<IActionResult> SendRequest(string receiverId)
        {
            var senderId = _userManager.GetUserId(User);


            if (senderId == receiverId)
                return BadRequest("Cannot send request yourself");


            var exists = await _db.FriendRequests
                .AnyAsync(x =>
                    x.SenderId == senderId &&
                    x.ReceiverId == receiverId);


            if (exists)
                return BadRequest("Already sent");


            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId
            };


            await _friendRequestService.AddAsync(request);
            return Ok("Request sent");
        }

        [Authorize]
        [HttpPost("accept/{requestId}")]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {

            var userId = _userManager.GetUserId(User);


            var request = await _db.FriendRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId &&
                    x.ReceiverId == userId);


            if (request == null)
                return NotFound();

            return Ok("You are friends now");
        }

    }
}
