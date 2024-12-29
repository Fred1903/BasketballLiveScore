using Basketball_LiveScore.Server.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Basketball_LiveScore.Server.Hubs
{
    public class MatchHub : Hub
    {
        public async Task NotifyFoulEvent(int matchId, int playerId, string foulType, int quarter, TimeSpan time)
        {
            await Clients.Group(matchId.ToString()).SendAsync("FoulEventOccurred", new
            {
                PlayerId = playerId,
                FoulType = foulType,
                Quarter = quarter,
                Time = time
            });
        }

        public async Task NotifyBasketEvent(BasketEvent basketEvent)
        {
            Console.WriteLine($"Broadcasting BasketEvent for Match: {basketEvent.MatchId}, Player: {basketEvent.PlayerId}, Points: {basketEvent.Points}");
            await Clients.Group(basketEvent.MatchId.ToString()).SendAsync("BasketEventOccurred", basketEvent);
        }

        public async Task NotifySubstitutionEvent(int matchId, int playerInId, int playerOutId, int quarter, TimeSpan time)
        {
            await Clients.Group(matchId.ToString()).SendAsync("SubstitutionEventOccurred", new
            {
                PlayerInId = playerInId,
                PlayerOutId = playerOutId,
                Quarter = quarter,
                Time = time
            });
        }

        public async Task NotifyTimeoutEvent(int matchId, string team, int quarter, TimeSpan time)
        {
            await Clients.Group(matchId.ToString()).SendAsync("TimeoutEventOccurred", new
            {
                Team = team,
                Quarter = quarter,
                Time = time
            });
        }

        public async Task NotifyChronoEvent(int matchId, bool isRunning, int quarter, TimeSpan time)
        {
            await Clients.Group(matchId.ToString()).SendAsync("ChronoEventOccurred", new
            {
                IsRunning = isRunning,
                Quarter = quarter,
                Time = time
            });
        }

        public async Task NotifyQuarterChangeEvent(int matchId, int quarter)
        {
            await Clients.Group(matchId.ToString()).SendAsync("QuarterChangeEventOccurred", new
            {
                Quarter = quarter
            });
        }
        //Ci-dessous update dans le quarter, au dessus changement de quarter
        public async Task NotifyQuarterUpdate(int matchId, int currentQuarter)
        {
            await Clients.Group(matchId.ToString()).SendAsync("QuarterUpdate", new
            {
                MatchId = matchId,
                CurrentQuarter = currentQuarter
            });
        }


        public async Task NotifyMatchStatusChange(int matchId, string matchStatus)
        {
            await Clients.Group(matchId.ToString()).SendAsync("MatchStatusChanged", new
            {
                MatchId = matchId,
                MatchStatus = matchStatus
            });
        }

        public async Task JoinMatchGroup(string matchId)
        {
            Console.WriteLine($"Client {Context.ConnectionId} joining Match Group: {matchId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
            await Clients.Group(matchId).SendAsync("Notify", $"Client {Context.ConnectionId} joined match {matchId}.");
        }

    }
}
