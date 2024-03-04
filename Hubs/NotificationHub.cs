using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ecommerce.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task Notify(string userId, string title, string message, string type)
        {
            await Clients.Group(userId).SendAsync(title, message, type);
        }

        public override async Task OnConnectedAsync()
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (userEmail != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userEmail);
            }

            await base.OnConnectedAsync();
        }
    }

    public class NotificationModel
    {
        public string Message { get; set; }
        public string Type { get; set; }
    }
}
