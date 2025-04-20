using Microsoft.AspNetCore.SignalR;

public class BoardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var boardId = Context.GetHttpContext()?.Request.Query["boardId"];
        if (!string.IsNullOrEmpty(boardId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var boardId = Context.GetHttpContext()?.Request.Query["boardId"];
        if (!string.IsNullOrEmpty(boardId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}