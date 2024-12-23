
using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Hubs;
using Basketball_LiveScore.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Basketball_LiveScore.Server.Services
{
    public class MatchService : IMatchService
    {
        private readonly BasketballDBContext basketballDBContext;
        private readonly IHubContext<MatchHub> hubContext;

        public MatchService(BasketballDBContext basketballDBContext, IHubContext<MatchHub> hubContext)
        {
            this.basketballDBContext = basketballDBContext;
            this.hubContext = hubContext;
        }
        public Match CreateMatch(CreateMatchDTO matchDTO)
        {
            //On check d'abord si 5 joueurs...
            if (matchDTO.PlayersTeam1 == null || matchDTO.PlayersTeam1.Count < 5 ||
                matchDTO.PlayersTeam2 == null || matchDTO.PlayersTeam2.Count < 5)
            {
                throw new System.ArgumentException("Each team must have at least 5 players.");
            }

            if (matchDTO.StartersTeam1.Count != 5 || matchDTO.StartersTeam2.Count != 5)
            {
                throw new System.ArgumentException("Each team must have exactly 5 starters.");
            }
            var playersTeam1 = basketballDBContext.Players
               .Where(p => matchDTO.PlayersTeam1.Contains(p.Id))
               .ToList();

            var playersTeam2 = basketballDBContext.Players
                .Where(p => matchDTO.PlayersTeam2.Contains(p.Id))
                .ToList();
            if (playersTeam1.Count != matchDTO.PlayersTeam1.Count || playersTeam2.Count != matchDTO.PlayersTeam2.Count)
            {
                throw new Exception("One or more players could not be found in the database.");
            }
            var match = new Match
            {
                HomeTeam = basketballDBContext.Teams.Find(matchDTO.Team1),
                AwayTeam = basketballDBContext.Teams.Find(matchDTO.Team2),
                Quarters = Enumerable.Range(1, matchDTO.NumberOfQuarters).Select(q => new Quarter
                {
                    Number = (NumberOfQuarters)q,
                    Duration = (QuarterDuration)matchDTO.QuarterDuration
                }).ToList(),
                Timeouts = new List<Models.Timeout>(),//Timeouts peuvent encore être ajoutés plus tard
                HomeTeamStartingFiveIds = matchDTO.StartersTeam1,
                AwayTeamStartingFiveIds = matchDTO.StartersTeam2,
                EncoderSettingsId = matchDTO.EncoderSettingsId,
                EncoderRealTimeId = matchDTO.EncoderRealTimeId
            };

            // Ajout du match à la db
            basketballDBContext.Matches.Add(match);
            basketballDBContext.SaveChanges();

            // Ajouter les joueurs au match
            var matchPlayers = playersTeam1.Select(p => new MatchPlayer
            {
                MatchId = match.Id,
                PlayerId = p.Id,
                IsStarter = matchDTO.StartersTeam1.Contains(p.Id),
                IsHomeTeam = true
            }).Concat(playersTeam2.Select(p => new MatchPlayer
            {
                MatchId = match.Id,
                PlayerId = p.Id,
                IsStarter = matchDTO.StartersTeam2.Contains(p.Id),
                IsHomeTeam = false
            })).ToList();
            
            basketballDBContext.MatchPlayers.AddRange(matchPlayers);
            basketballDBContext.SaveChanges();

            return match;
        }

        public Dictionary<string, int> GetDefaultSettings()
        {
            return new Dictionary<string, int>
            {
                { "NumberOfQuarters", (int)NumberOfQuarters.Four },
                { "QuarterDuration", (int)QuarterDuration.TenMinutes },
                { "TimeoutDuration", (int)TimeOutDuration.SixtySeconds }
            };
        }

        public List<int> GetNumberOfQuartersOptions()
        {
            return Enum.GetValues(typeof(NumberOfQuarters)).Cast<int>().ToList();
        }

        public List<int> GetQuarterDurationOptions()
        {
            return Enum.GetValues(typeof(QuarterDuration)).Cast<int>().ToList();
        }

        public List<int> GetTimeOutDurationOptions()
        {
            return Enum.GetValues(typeof(TimeOutDuration)).Cast<int>().ToList();
        }

        public List<MatchEvent> GetMatchEvents(int matchId)
        {
            var events = basketballDBContext.MatchEvents
                .Where(e => e.MatchId == matchId)
                .OrderBy(e => e.Quarter)
                .ThenBy(e => e.Time)
                .ToList();

            if (!events.Any())
            {
                return new List<MatchEvent>();//Retourne liste vide si pas encore d event
            }

            return events;
        }

        public MatchDTO GetMatchDetails(int matchId)
        {
            try
            {
                //On prend le match depuis la db
                var match = basketballDBContext.Matches
                    .Include(m => m.Quarters)
                    .Include(m => m.Timeouts)
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .Include(m => m.MatchPlayers)
                        .ThenInclude(mp => mp.Player)
                    .FirstOrDefault(m => m.Id == matchId);

                // Si le match n'existe pas
                if (match == null) throw new Exception($"Match with ID {matchId} not found.");

                return new MatchDTO
                {
                    MatchId = match.Id,
                    MatchDate = match.matchDate,
                    HomeTeam = new TeamMatchDTO
                    {
                        TeamId = match.HomeTeam.Id,
                        Name = match.HomeTeam.Name,
                        Coach = match.HomeTeam.Coach
                    },
                    AwayTeam = new TeamMatchDTO
                    {
                        TeamId = match.AwayTeam.Id,
                        Name = match.AwayTeam.Name,
                        Coach = match.AwayTeam.Coach
                    },
                    QuarterDuration = (int)match.Quarters.FirstOrDefault()?.Duration,
                    NumberOfQuarters = match.Quarters.Count,
                    Timeouts = match.Timeouts.Count,
                    ScoreHome = match.ScoreHome,
                    ScoreAway = match.ScoreAway,
                    Players = match.MatchPlayers.Select(mp => new MatchPlayerDTO
                    {//Ces valeurs sont dans MatchPlayerDTO
                        PlayerId = mp.PlayerId,
                        PlayerName = mp.Player.FirstName+" "+mp.Player.LastName,
                        PlayerNumber = mp.Player.Number,
                        IsStarter = mp.IsStarter,
                        IsHomeTeam = mp.IsHomeTeam
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving match details for Match ID {matchId}: {ex.Message}", ex);
            }
        }

        //Ci dessous les events de match : 
        public MatchEvent AddFoulEvent(int matchId, FoulEventDTO foulEventDTO)
        {
            try
            {
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                var player = basketballDBContext.Players.Find(foulEventDTO.PlayerId);
                if (player == null)
                    throw new Exception($"Player with ID {foulEventDTO.PlayerId} not found.");

                var foulEvent = new FoulEvent
                {
                    MatchId = matchId,
                    PlayerId = foulEventDTO.PlayerId,
                    FoulType = foulEventDTO.FoulType,
                    Quarter = foulEventDTO.Quarter,
                    Time = foulEventDTO.Time
                };

                basketballDBContext.MatchEvents.Add(foulEvent);
                basketballDBContext.SaveChanges();

                // Diffuser via SignalR
                hubContext.Clients.Group(matchId.ToString()).SendAsync("FoulEventOccurred", foulEvent);

                return foulEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding foul event: {ex.Message}");
            }
        }

        public async Task<MatchEvent> AddBasketEvent(int matchId, BasketEventDTO basketEventDTO)
        {
            try
            {
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                var player = basketballDBContext.Players.Find(basketEventDTO.PlayerId);
                if (player == null)
                    throw new Exception($"Player with ID {basketEventDTO.PlayerId} not found.");

                var basketEvent = new BasketEvent
                {
                    MatchId = matchId,
                    PlayerId = basketEventDTO.PlayerId,
                    Points = basketEventDTO.Points,
                    Quarter = basketEventDTO.Quarter,
                    Time = basketEventDTO.Time
                };

                // Mettre à jour le score de l'équipe
                if (basketEventDTO.Points > 0)
                {
                    if (match.HomeTeamStartingFiveIds.Contains(basketEventDTO.PlayerId))
                        match.ScoreHome += basketEventDTO.Points;
                    else
                        match.ScoreAway += basketEventDTO.Points;
                }

                basketballDBContext.MatchEvents.Add(basketEvent);
                basketballDBContext.SaveChanges();

                // Diffuser via SignalR
                try
                {
                    Console.WriteLine($"Notifying BasketEvent for Match: {basketEvent.MatchId}, Player: {basketEvent.PlayerId}, Points: {basketEvent.Points}");
                    await hubContext.Clients.Group(basketEvent.MatchId.ToString()).SendAsync("BasketEventOccurred", basketEvent);
                    Console.WriteLine("BasketEvent notification sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying BasketEvent: {ex.Message}");
                }
                return basketEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding basket event: {ex.Message}");
            }
        }


        public MatchEvent AddSubstitutionEvent(int matchId, SubstitutionEventDTO substitutionEventDTO)
        {
            try
            {
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                var playerIn = basketballDBContext.Players.Find(substitutionEventDTO.PlayerInId);
                var playerOut = basketballDBContext.Players.Find(substitutionEventDTO.PlayerOutId);

                if (playerIn == null || playerOut == null)
                    throw new Exception("Player(s) not found for substitution.");

                var substitutionEvent = new SubstitutionEvent
                {
                    MatchId = matchId,
                    PlayerInId = substitutionEventDTO.PlayerInId,
                    PlayerOutId = substitutionEventDTO.PlayerOutId,
                    Quarter = substitutionEventDTO.Quarter,
                    Time = substitutionEventDTO.Time
                };

                basketballDBContext.MatchEvents.Add(substitutionEvent);
                basketballDBContext.SaveChanges();

                // Diffuser via SignalR
                hubContext.Clients.Group(matchId.ToString()).SendAsync("SubstitutionEventOccurred", substitutionEvent);

                return substitutionEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding substitution event: {ex.Message}");
            }
        }

        public MatchEvent AddTimeoutEvent(int matchId, TimeoutEventDTO timeoutEventDTO)
        {
            try
            {
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                var timeoutEvent = new TimeoutEvent
                {
                    MatchId = matchId,
                    Team = timeoutEventDTO.Team,
                    Quarter = timeoutEventDTO.Quarter,
                    Time = timeoutEventDTO.Time
                };

                basketballDBContext.MatchEvents.Add(timeoutEvent);
                basketballDBContext.SaveChanges();

                // Diffuser via SignalR
                hubContext.Clients.Group(matchId.ToString()).SendAsync("TimeoutEventOccurred", timeoutEvent);

                return timeoutEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding timeout event: {ex.Message}");
            }
        }

        public MatchEvent AddQuarterChangeEvent(int matchId, QuarterChangeEventDTO quarterChangeEventDTO)
        {
            try
            {
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                var quarterChangeEvent = new QuarterChangeEvent
                {
                    MatchId = matchId,
                    Quarter = quarterChangeEventDTO.Quarter
                };

                basketballDBContext.MatchEvents.Add(quarterChangeEvent);
                basketballDBContext.SaveChanges();

                // Diffuser via SignalR
                hubContext.Clients.Group(matchId.ToString()).SendAsync("QuarterChangeEventOccurred", quarterChangeEvent);

                return quarterChangeEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding quarter change event: {ex.Message}");
            }
        }

        public MatchEvent AddChronoEvent(int matchId, ChronoEventDTO chronoEventDTO)
        {
            try
            {
                // Vérifier si le match existe
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                // Créer l'événement Chrono
                var chronoEvent = new ChronoEvent
                {
                    MatchId = matchId,
                    IsRunning = chronoEventDTO.IsRunning,
                    Quarter = chronoEventDTO.Quarter,
                    Time = chronoEventDTO.Time
                };

                basketballDBContext.MatchEvents.Add(chronoEvent);
                basketballDBContext.SaveChanges();

                // Diffuser via SignalR
                hubContext.Clients.Group(matchId.ToString()).SendAsync("ChronoEventOccurred", chronoEvent);

                return chronoEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding chrono event: {ex.Message}");
            }
        }

    }
}
