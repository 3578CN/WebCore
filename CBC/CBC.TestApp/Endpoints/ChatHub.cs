using Microsoft.AspNetCore.SignalR;

namespace CBC.TestApp.Endpoints
{
    public class ChatHub : Hub
    {
        // 发送消息给所有客户端的方法。
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}