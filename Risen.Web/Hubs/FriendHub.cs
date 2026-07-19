using Microsoft.AspNetCore.SignalR;

namespace Risen.Web.Hubs
{
    public class FriendHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("FriendHub Connected");
            Console.WriteLine($"UserIdentifier: {Context.UserIdentifier}");

            await base.OnConnectedAsync();
        }
    }
}
