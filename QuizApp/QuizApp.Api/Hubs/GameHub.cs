using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Api.Hubs
{
    public class GameHub : Hub
    {
        private readonly QuizAppContext _context;

        public GameHub(QuizAppContext context)
        {
            _context = context;
        }

        public async Task JoinGame(string gameCode, string playerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
            
            // Get player details and notify others
            if (!string.IsNullOrEmpty(playerId) && int.TryParse(playerId, out int playerIdInt))
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == playerIdInt);
                
                if (player != null)
                {
                    await Clients.OthersInGroup(gameCode).SendAsync("PlayerJoined", player);
                }
            }
        }

        public async Task LeaveGame(string gameCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
        }

        public async Task HostGame(string gameCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
            
            // Send existing players to the host
            var gameSession = await _context.GameSessions
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Code == gameCode);

            if (gameSession != null)
            {
                foreach (var player in gameSession.Players)
                {
                    await Clients.Caller.SendAsync("PlayerJoined", player);
                }
            }
        }
    }
} 