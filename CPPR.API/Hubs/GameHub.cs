using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace CPPR.API.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinGame(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task RollDice(string groupName, int bet, int number)
        {
            var random = new Random();
            var dice1 = random.Next(1, 7);
            var dice2 = random.Next(1, 7);
            var dice3 = random.Next(1, 7);

            var matches = 0;
            if (dice1 == number) matches++;
            if (dice2 == number) matches++;
            if (dice3 == number) matches++;

            var winnings = 0;
            if (matches > 0)
            {
                winnings = bet * (matches + 1); // Payout: 1:1, 2:1, 3:1 plus original bet
            }
            else
            {
                winnings = -bet;
            }

            await Clients.Group(groupName).SendAsync("GameResult", dice1, dice2, dice3, winnings);
        }
    }
}
