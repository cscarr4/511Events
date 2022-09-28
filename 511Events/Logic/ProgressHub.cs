using Microsoft.AspNetCore.SignalR;

namespace _511Events.Logic
{
    public class ProgressHub : Hub
    {
        public async Task UpdateFill(int pctComplete)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateProgressBarFill", pctComplete);
        }

        public async Task UpdateText(string text)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateProgressBarText", text);
        }
    }
}
