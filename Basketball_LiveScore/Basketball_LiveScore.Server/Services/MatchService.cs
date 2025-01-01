
using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Hubs;
using Basketball_LiveScore.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Basketball_LiveScore.Server.Services
{
    public class MatchService : IMatchService
    {
        private readonly BasketballDBContext basketballDBContext;
        private readonly IHubContext<MatchHub> hubContext;
        private static readonly ConcurrentDictionary<int, CancellationTokenSource> matchTimers =
            new ConcurrentDictionary<int, CancellationTokenSource>();

        public MatchService(BasketballDBContext basketballDBContext, IHubContext<MatchHub> hubContext)
        {
            this.basketballDBContext = basketballDBContext;
            this.hubContext = hubContext;
        }
        public Match CreateMatch(CreateMatchDTO matchDTO)
        {
            if (!Enum.IsDefined(typeof(TimeOutAmount), matchDTO.TimeoutAmount))
            {
                throw new ArgumentException("Invalid TimeoutAmount value.");
            }

            if (!Enum.IsDefined(typeof(TimeOutDuration), matchDTO.TimeoutDuration))
            {
                throw new ArgumentException("Invalid TimeoutDuration value.");
            }

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
            if(matchDTO.PlayersTeam1.Count>13 || matchDTO.PlayersTeam2.Count > 13)
            {
                throw new System.ArgumentException("You can maximum select 13 players for the game");
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
                    Duration = (QuarterDuration)matchDTO.QuarterDuration,
                    RemainingTime = TimeSpan.FromMinutes((int)(QuarterDuration)matchDTO.QuarterDuration)
                }).ToList(),
                Timeouts = Enumerable.Range(1,matchDTO.TimeoutAmount).Select(t => new Models.Timeout
                {
                    Amount = (TimeOutAmount)t,
                    Duration=(TimeOutDuration)matchDTO.TimeoutDuration,
                    Team = t % 2 == 0 ? "Team1" : "Team2", //On met pr les 2 equipes le mm nombre de timeout
                }).ToList(),
                AwayTeamRemainingTimeouts = matchDTO.TimeoutAmount,
                HomeTeamRemainingTimeouts = matchDTO.TimeoutAmount,
                HomeTeamStartingFiveIds = matchDTO.StartersTeam1,
                AwayTeamStartingFiveIds = matchDTO.StartersTeam2,
                HomeTeamOnFieldIds = new List<int>(matchDTO.StartersTeam1),//au debut du match forcement les mm
                AwayTeamOnFieldIds = new List<int>(matchDTO.StartersTeam2),
                CurrentQuarter = 1, //On commence tjrs par le premier quarter
                EncoderSettingsId = matchDTO.EncoderSettingsId,
                EncoderRealTimeId = matchDTO.EncoderRealTimeId,
                MatchDate = matchDTO.MatchDate,
                Status = MatchStatus.NotStarted
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
                { "TimeoutDuration", (int)TimeOutDuration.SixtySeconds },
                { "TimeoutAmount", (int)TimeOutAmount.Seven }
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

        public List<int> GetTimeOutAmountOptions()
        {
            return Enum.GetValues(typeof (TimeOutAmount)).Cast<int>().ToList();
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

                var currentQuarter = match.CurrentQuarter;

                // Déterminer le temps restant du quart en secondes
                var currentTime = match.Quarters
                    .Where(q => (int)q.Number == currentQuarter)
                    .Select(q => (int)q.RemainingTime.TotalSeconds)
                    .FirstOrDefault();
                var lastChronoEvent = basketballDBContext.MatchEvents
                    .OfType<ChronoEvent>()
                    .Where(e => e.MatchId == matchId)
                    .OrderByDescending(e => e.Id) //Trier par ID pour obtenir le dernier événement
                    .FirstOrDefault();

                return new MatchDTO
                {
                    MatchId = match.Id,
                    MatchDate = match.MatchDate,
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
                    TimeoutDuration = (int)match.Timeouts.FirstOrDefault()?.Duration,
                    HomeTeamRemainingTimeouts = match.HomeTeamRemainingTimeouts,
                    AwayTeamRemainingTimeouts=match.AwayTeamRemainingTimeouts,
                    ScoreHome = match.ScoreHome,
                    ScoreAway = match.ScoreAway,
                    Players = match.MatchPlayers.Select(mp => new MatchPlayerDTO
                    {//Ces valeurs sont dans MatchPlayerDTO
                        PlayerId = mp.PlayerId,
                        PlayerName = mp.Player.FirstName + " " + mp.Player.LastName,
                        PlayerNumber = mp.Player.Number,
                        IsStarter = mp.IsStarter,
                        IsHomeTeam = mp.IsHomeTeam,
                        Fouls = basketballDBContext.MatchEvents.OfType<FoulEvent>()
                            .Count(fe => fe.PlayerId == mp.PlayerId && fe.MatchId == match.Id),
                        Points = basketballDBContext.MatchEvents.OfType<BasketEvent>()
                            .Where(be => be.PlayerId == mp.PlayerId && be.MatchId == match.Id)
                            .Sum(be => be.Points),
                        OnField = match.HomeTeamOnFieldIds.Contains(mp.PlayerId) ||
                            match.AwayTeamOnFieldIds.Contains(mp.PlayerId)
                    }).ToList(),
                    CurrentQuarter = currentQuarter,
                    CurrentTime = (int)currentTime,//si isRunning est null, par défaut on va mettre false!!!!!!!!!!
                    IsRunning = lastChronoEvent?.IsRunning ?? false,
                    MatchStatus = match.Status
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving match details for Match ID {matchId}: {ex.Message}", ex);
            }
        }

        public async Task<MatchEvent> AddBasketEvent(int matchId, BasketEventDTO dto)
        {
            var match = await basketballDBContext.Matches.Include(m => m.Quarters).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) throw new Exception($"Match {matchId} not found");

            var basketEvent = new BasketEvent
            {
                MatchId = matchId,
                PlayerId = dto.PlayerId,
                Points = dto.Points,
                Quarter = dto.Quarter,
                Time = dto.Time
            };

            // Update score
            if (match.HomeTeamOnFieldIds.Contains(dto.PlayerId))
                match.ScoreHome += dto.Points;
            else if (match.AwayTeamOnFieldIds.Contains(dto.PlayerId))
                match.ScoreAway += dto.Points;

            // Update quarter time
            var quarterEntity = match.Quarters.FirstOrDefault(q => (int)q.Number == dto.Quarter);
            if (quarterEntity != null)
                quarterEntity.RemainingTime = dto.Time;

            basketballDBContext.MatchEvents.Add(basketEvent);
            await basketballDBContext.SaveChangesAsync();

            await hubContext.Clients.Group(matchId.ToString()).SendAsync("BasketEventOccurred", dto);
            return basketEvent;
        }

        public async Task<MatchEvent> AddFoulEvent(int matchId, FoulEventDTO dto)
        {
            var match = await basketballDBContext.Matches.Include(m => m.Quarters).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) throw new Exception($"Match {matchId} not found");

            var foulEvent = new FoulEvent
            {
                MatchId = matchId,
                PlayerId = dto.PlayerId,
                FoulType = dto.FoulType,
                Quarter = dto.Quarter,
                Time = dto.Time
            };

            var quarterEntity = match.Quarters.FirstOrDefault(q => (int)q.Number == dto.Quarter);
            if (quarterEntity != null)
                quarterEntity.RemainingTime = dto.Time;

            basketballDBContext.MatchEvents.Add(foulEvent);
            await basketballDBContext.SaveChangesAsync();

            await hubContext.Clients.Group(matchId.ToString()).SendAsync("FoulEventOccurred", dto);
            return foulEvent;
        }

        public async Task<MatchEvent> AddSubstitutionEvent(int matchId, SubstitutionEventDTO dto)
        {
            var match = await basketballDBContext.Matches.Include(m => m.Quarters).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) throw new Exception($"Match {matchId} not found");

            var subEvent = new SubstitutionEvent
            {
                MatchId = matchId,
                PlayerInId = dto.PlayerInId,
                PlayerOutId = dto.PlayerOutId,
                Quarter = dto.Quarter,
                Time = dto.Time
            };

            // Update players on court
            if (match.HomeTeamOnFieldIds.Contains(dto.PlayerOutId))
            {
                match.HomeTeamOnFieldIds.Remove(dto.PlayerOutId);
                match.HomeTeamOnFieldIds.Add(dto.PlayerInId);
            }
            else if (match.AwayTeamOnFieldIds.Contains(dto.PlayerOutId))
            {
                match.AwayTeamOnFieldIds.Remove(dto.PlayerOutId);
                match.AwayTeamOnFieldIds.Add(dto.PlayerInId);
            }

            var quarterEntity = match.Quarters.FirstOrDefault(q => (int)q.Number == dto.Quarter);
            if (quarterEntity != null)
                quarterEntity.RemainingTime = dto.Time;

            basketballDBContext.MatchEvents.Add(subEvent);
            await basketballDBContext.SaveChangesAsync();

            await hubContext.Clients.Group(matchId.ToString()).SendAsync("SubstitutionEventOccurred", dto);
            return subEvent;
        }

        public async Task<MatchEvent> AddTimeoutEvent(int matchId, TimeoutEventDTO dto)
        {
            var match = await basketballDBContext.Matches.Include(m => m.Quarters).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) throw new Exception($"Match {matchId} not found");

            if (dto.Team == "Home" && match.HomeTeamRemainingTimeouts <= 0)
                throw new Exception("No timeouts remaining for home team");
            if (dto.Team == "Away" && match.AwayTeamRemainingTimeouts <= 0)
                throw new Exception("No timeouts remaining for away team");

            var timeoutEvent = new TimeoutEvent
            {
                MatchId = matchId,
                Team = dto.Team,
                Quarter = dto.Quarter,
                Time = dto.Time
            };

            // Update timeouts
            if (dto.Team == "Home")
                match.HomeTeamRemainingTimeouts--;
            else
                match.AwayTeamRemainingTimeouts--;

            var quarterEntity = match.Quarters.FirstOrDefault(q => (int)q.Number == dto.Quarter);
            if (quarterEntity != null)
                quarterEntity.RemainingTime = dto.Time;

            basketballDBContext.MatchEvents.Add(timeoutEvent);
            await basketballDBContext.SaveChangesAsync();

            await hubContext.Clients.Group(matchId.ToString()).SendAsync("TimeoutEventOccurred", dto);
            return timeoutEvent;
        }

        public async Task<MatchEvent> AddQuarterChangeEvent(int matchId, QuarterChangeEventDTO dto)
        {
            var match = await basketballDBContext.Matches.Include(m => m.Quarters).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) throw new Exception($"Match {matchId} not found");

            if (dto.Quarter + 1 > match.Quarters.Count())
                throw new Exception("Cannot exceed number of quarters");

            match.CurrentQuarter = dto.Quarter + 1;

            var quarterEvent = new QuarterChangeEvent
            {
                MatchId = matchId,
                Quarter = match.CurrentQuarter
            };

            basketballDBContext.MatchEvents.Add(quarterEvent);
            await basketballDBContext.SaveChangesAsync();

            await hubContext.Clients.Group(matchId.ToString()).SendAsync("QuarterChangeEventOccurred", dto);
            return quarterEvent;
        }

        public async Task<MatchEvent> AddChronoEvent(int matchId, ChronoEventDTO dto)
        {
            var match = await basketballDBContext.Matches.Include(m => m.Quarters).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) throw new Exception($"Match {matchId} not found");

            var chronoEvent = new ChronoEvent
            {
                MatchId = matchId,
                Quarter = dto.Quarter,
                Time = dto.Time,
                IsRunning = dto.IsRunning
            };

            var quarterEntity = match.Quarters.FirstOrDefault(q => (int)q.Number == dto.Quarter);
            if (quarterEntity != null)
                quarterEntity.RemainingTime = dto.Time;

            basketballDBContext.MatchEvents.Add(chronoEvent);
            await basketballDBContext.SaveChangesAsync();

            await hubContext.Clients.Group(matchId.ToString()).SendAsync("ChronoEventOccurred", dto);
            return chronoEvent;
        }


        public void StartMatch(int matchId)
        {
            var match = basketballDBContext.Matches.Find(matchId);
            if (match == null)
            {
                throw new Exception("Match not found.");
            }
            if (match.Status != MatchStatus.NotStarted)
            {
                throw new Exception("Match has already started or is finished.");
            }

            match.Status = MatchStatus.Live;
            basketballDBContext.SaveChanges();
            hubContext.Clients.Group(matchId.ToString()).SendAsync("MatchStatusChanged", new { matchId, matchStatus = MatchStatus.Live.ToString() });
        }

        public void FinishMatch(int matchId)
        {
            var match = basketballDBContext.Matches.Find(matchId);
            if (match == null)
            {
                throw new Exception("Match not found.");
            }
            if (match.Status == MatchStatus.NotStarted)
            {
                throw new Exception("Match has not started");
            }
            match.Status = MatchStatus.Finished;
            basketballDBContext.SaveChanges();
            hubContext.Clients.Group(matchId.ToString()).SendAsync("MatchStatusChanged", MatchStatus.Finished.ToString());
        }
            
        public List<MatchWithStatusDTO> GetAllMatchesWithStatus()
        {
            var currentTime = DateTime.UtcNow;

            var matches = basketballDBContext.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Select(m => new MatchWithStatusDTO
                {
                    MatchId = m.Id,
                    MatchDate = m.MatchDate,
                    HomeTeam = new TeamMatchDTO
                    {
                        TeamId = m.HomeTeam.Id,
                        Name = m.HomeTeam.Name,
                        Coach = m.HomeTeam.Coach
                    },
                    AwayTeam = new TeamMatchDTO
                    {
                        TeamId = m.AwayTeam.Id,
                        Name = m.AwayTeam.Name,
                        Coach = m.AwayTeam.Coach
                    },
                    ScoreHome = m.ScoreHome,
                    ScoreAway = m.ScoreAway,
                    Status = m.MatchDate > currentTime
                        ? MatchStatus.NotStarted
                        : m.Quarters.Any(q => q.RemainingTime > TimeSpan.Zero)
                            ? MatchStatus.Live
                            : MatchStatus.Finished,
                    CurrentQuarter = m.CurrentQuarter,
                    RemainingTime = m.Quarters
                        .Where(q => q.RemainingTime > TimeSpan.Zero)
                        .OrderBy(q => q.Number)
                        .Select(q => (int)q.RemainingTime.TotalSeconds)
                        .FirstOrDefault(),
                    EncoderRealTimeId = m.EncoderRealTimeId
                })
                .ToList();
            return matches;
        }

        public List<GetMatchEventDTO> GetMatchEvents(int matchId)
        {
            // Retrieve all events for the match
            var events = basketballDBContext.MatchEvents
                .Where(e => e.MatchId == matchId)
                .OrderByDescending(e => e.Id)
                .ToList();

            // Map events to their respective DTOs
            return events.Select<MatchEvent, GetMatchEventDTO>(e =>
            {
                if (e is BasketEvent basketEvent)
                {
                    return new GetMatchEventDTO
                    {
                        PlayerId = basketEvent.PlayerId,
                        Points = basketEvent.Points,
                        Quarter = basketEvent.Quarter,
                        Time = basketEvent.Time,
                        EventType = "Basket"
                    };
                }
                else if (e is FoulEvent foulEvent)
                {
                    return new GetMatchEventDTO
                    {
                        PlayerId = foulEvent.PlayerId,
                        FoulType = foulEvent.FoulType,
                        Quarter = foulEvent.Quarter,
                        Time = foulEvent.Time,
                        EventType = "Foul"
                    };
                }
                else if (e is SubstitutionEvent subEvent)
                {
                    return new GetMatchEventDTO
                    {
                        PlayerInId = subEvent.PlayerInId,
                        PlayerOutId = subEvent.PlayerOutId,
                        Quarter = subEvent.Quarter,
                        Time = subEvent.Time,
                        EventType = "Substitution"
                    };
                }
                else if (e is TimeoutEvent timeoutEvent)
                {
                    return new GetMatchEventDTO
                    {
                        Team = timeoutEvent.Team,
                        Quarter = timeoutEvent.Quarter,
                        Time = timeoutEvent.Time,
                        EventType = "Timeout"
                    };
                }
                else if (e is QuarterChangeEvent quarterEvent)
                {
                    return new GetMatchEventDTO
                    {
                        Quarter = quarterEvent.Quarter,
                        Time = quarterEvent.Time,
                        EventType = "QuarterChange"
                    };
                }
                else if (e is ChronoEvent chronoEvent)
                {
                    return new GetMatchEventDTO
                    {
                        IsRunning = chronoEvent.IsRunning,
                        Quarter = chronoEvent.Quarter,
                        Time = chronoEvent.Time,
                        EventType = "Chrono"
                    };
                }
                return null; // Handle unexpected types gracefully
            }).Where(e => e != null).ToList();
        }
    }
}