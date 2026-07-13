using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Diagnostics.Metrics;

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


        public FriendController(UserManager<CustomIdentityUser> userManager, IFriendService friendService, IFriendRequestService friendRequestService, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _friendService = friendService;
            _friendRequestService = friendRequestService;
            _db = appDbContext;
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.UniversityId,
                u.Stats
            }).ToList();
            return Ok(userDtos);
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
        {
            var users = await _userManager.Users
                .Where(u => u.FullName.Contains(searchTerm))
                .ToListAsync();
            var userDtos = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.UniversityId,
                u.Stats
            }).ToList();
            return Ok(userDtos);
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
            var myRequests = requests.Where(r => r.SenderId == user?.Id.ToString());
            var friends = await _friendService.GetAllAsync();
            var myFriends = friends.Where(f => f.OwnId == user?.Id.ToString() || f.YourFriendId == user?.Id.ToString());

            var friendUsers = datas
            .Where(u => myFriends.Any(f => f.OwnId == u.Id.ToString() || f.YourFriendId == u.Id.ToString()) && u.Id != user?.Id)
            .Select(u => new CustomIdentityUser
            {
                Id = u.Id,
                FullName = u.FullName,
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
                    ((x.SenderId == senderId &&
                    x.ReceiverId == receiverId
                    ) || (x.SenderId == receiverId && x.ReceiverId == senderId)) && (x.Status == "Pending" || x.Status == "Accepted"));


            if (exists)
                return BadRequest("Already sent");


            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = "Pending"
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

            await _friendService.AddAsync(new Friend
            {
                OwnId = request.SenderId,
                YourFriendId = request.ReceiverId
            });

            request.Status = "Accepted";
            await _friendRequestService.UpdateAsync(request);
            await _db.SaveChangesAsync();

            return Ok("You are friends now");
        }

        [Authorize]
        [HttpPost("reject/{requestId}")]
        public async Task<IActionResult> DeclineRequest(int requestId)
        {
            var userId = _userManager.GetUserId(User);

            var request = await _db.FriendRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId &&
                    x.ReceiverId == userId);

            if (request == null)
                return NotFound();

            request.Status = "Rejected";
            await _friendRequestService.UpdateAsync(request);
            await _db.SaveChangesAsync();

            return Ok("Request declined");
        }

        [Authorize]
        [HttpGet("sent-requests")]
        public async Task<IActionResult> GetSentRequests()
        {
            int counter = 0;
            var userId = _userManager.GetUserId(User);

            var requests = await _db.FriendRequests
                .Where(r => r.SenderId == userId)
                .Select(r => new
                {
                    r.Id,
                    Receiver = _db.Users
                        .Where(u => u.Id.ToString() == r.ReceiverId)
                        .Select(u => new
                        {
                            u.Id,
                            u.FullName,
                            u.Email,
                            u.UniversityId,
                            u.Stats
                        })
                        .FirstOrDefault(),
                    r.Status
                })
                .ToListAsync();

            foreach (var request in requests)
            {
                if (request.Status == "Pending" || request.Status == "pending" || request.Status == "PENDING")
                {
                    counter++;
                }
            }

            if (counter == 0)
                return Ok(new { message = "No pending requests" });
            else
                return Ok(requests);
        }

        [Authorize]
        [HttpGet("received-requests")]
        public async Task<IActionResult> GetReceivedRequests()
        {
            int counter = 0;
            var userId = _userManager.GetUserId(User);

            var requests = await _db.FriendRequests
                .Where(r => r.ReceiverId == userId)
                .Select(r => new
                {
                    r.Id,
                    Sender = _db.Users
                        .Where(u => u.Id.ToString() == r.SenderId)
                        .Select(u => new
                        {
                            u.Id,
                            u.FullName,
                            u.Email,
                            u.UniversityId,
                            u.Stats
                        })
                        .FirstOrDefault(),
                    r.Status
                })
                .ToListAsync();

            foreach (var request in requests)
            {
                if (request.Status == "Pending" || request.Status == "pending" || request.Status == "PENDING")
                {
                    counter++;
                }
            }

            if (counter == 0)
                return Ok(new { message = "No pending requests" });
            else
                return Ok(requests);
        }

        [Authorize]
        [HttpPost("remove-friend/{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = _userManager.GetUserId(User);

            var friend = await _db.Friends
                .FirstOrDefaultAsync(f =>
                    (f.OwnId == userId &&
                    f.YourFriendId == friendId.ToString()) || (f.OwnId == friendId.ToString() && f.YourFriendId == userId.ToString()));

            if (friend == null)
                return NotFound();

            var friendRequest = await _db.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    (fr.SenderId == userId && fr.ReceiverId == friendId.ToString()) || (fr.SenderId == friendId.ToString() && fr.ReceiverId == userId.ToString()));

            if (friendRequest == null)
                return NotFound();

            _db.FriendRequests.Remove(friendRequest);
            _db.Friends.Remove(friend);
            await _db.SaveChangesAsync();

            return Ok("Friend removed");
        }
    }
}
