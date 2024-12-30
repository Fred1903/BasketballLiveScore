
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
                    CurrentTime = (int)currentTime,//si isRunning est null, par défaut on va mettre true
                    IsRunning = lastChronoEvent?.IsRunning ?? true,
                    MatchStatus = match.Status
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving match details for Match ID {matchId}: {ex.Message}", ex);
            }
        }

        /*private async Task<MatchEvent> AddMatchEvent<TEvent>(int matchId,
            Func<Match, TEvent> createEvent, // Fonction pour créer l'événement
            string signalRMethod, // Nom de la méthode SignalR
            Action<Match> postProcess = null // Action optionnelle pour effectuer des mises à jour spécifiques
        ) where TEvent : MatchEvent
        {
            var match = basketballDBContext.Matches
                .Include(m => m.Quarters)
                .FirstOrDefault(m => m.Id == matchId);
            if (match == null)
                throw new Exception($"Match with ID {matchId} not found.");

            var currentQuarter = match.CurrentQuarter;
            if (currentQuarter == null)
                throw new Exception($"No active quarter found for match {matchId}.");
            if (currentQuarter == 0)currentQuarter = 1;

            var matchEvent = createEvent(match);

            //On ne verif pas si time>0 car sinon va pas prendre en compte la sauvegarde de qd c a 0sec
            var quarterEntity = match.Quarters.FirstOrDefault(q => (int)q.Number == currentQuarter);
            if (quarterEntity != null)
            {
                quarterEntity.RemainingTime = matchEvent.Time;
            }

            //logique spécifique si besoin
            postProcess?.Invoke(match);

            basketballDBContext.MatchEvents.Add(matchEvent);
            await basketballDBContext.SaveChangesAsync();

            // Diffuser via SignalR
            await hubContext.Clients.Group(matchId.ToString()).SendAsync(signalRMethod, matchEvent);

            return matchEvent;
        }
        public async Task<MatchEvent> AddBasketEvent(int matchId, BasketEventDTO basketEventDTO)
        {
            Console.WriteLine($"Adding basket event for match {matchId}");
            return await AddMatchEvent(
                matchId,
                match => new BasketEvent
                {
                    MatchId = matchId,
                    PlayerId = basketEventDTO.PlayerId,
                    Points = basketEventDTO.Points,
                    Quarter = basketEventDTO.Quarter,
                    Time = basketEventDTO.Time
                },
                "BasketEventOccurred",
                match => //On va aussi ajouter les points dans le match 
                {
                    //Verif equipe du joueur
                    if (match.HomeTeamOnFieldIds.Contains(basketEventDTO.PlayerId))
                    {
                        match.ScoreHome += basketEventDTO.Points;
                    }
                    else if (match.AwayTeamOnFieldIds.Contains(basketEventDTO.PlayerId))
                    {
                        match.ScoreAway += basketEventDTO.Points;
                    }
                }
            );
        }

        public async Task<MatchEvent> AddFoulEvent(int matchId, FoulEventDTO foulEventDTO)
        {
            return await AddMatchEvent(
                matchId,
                match => new FoulEvent
                {
                    MatchId = matchId,
                    PlayerId = foulEventDTO.PlayerId,
                    FoulType = foulEventDTO.FoulType,
                    Quarter = foulEventDTO.Quarter,
                    Time = foulEventDTO.Time
                },
                "FoulEventOccurred"
            );
        }

        public async Task<MatchEvent> AddSubstitutionEvent(int matchId, SubstitutionEventDTO substitutionEventDTO)
        {
            return await AddMatchEvent(
                matchId,
                match => new SubstitutionEvent
                {
                    MatchId = matchId,
                    PlayerInId = substitutionEventDTO.PlayerInId,
                    PlayerOutId = substitutionEventDTO.PlayerOutId,
                    Quarter = substitutionEventDTO.Quarter,
                    Time = substitutionEventDTO.Time
                },
                "SubstitutionEventOccurred",
                match =>
                {
                    //Màj des joueurs sur terrain dans classe 'Match'
                    if (match.HomeTeamOnFieldIds.Contains(substitutionEventDTO.PlayerOutId))
                    {
                        match.HomeTeamOnFieldIds.Remove(substitutionEventDTO.PlayerOutId);
                        match.HomeTeamOnFieldIds.Add(substitutionEventDTO.PlayerInId);
                    }
                    else if (match.AwayTeamOnFieldIds.Contains(substitutionEventDTO.PlayerOutId))
                    {
                        match.AwayTeamOnFieldIds.Remove(substitutionEventDTO.PlayerOutId);
                        match.AwayTeamOnFieldIds.Add(substitutionEventDTO.PlayerInId);
                    }
                }
            );
        }

        public async Task<MatchEvent> AddTimeoutEvent(int matchId, TimeoutEventDTO timeoutEventDTO)
        {
            return await AddMatchEvent(
                matchId,
                match =>
                {
                    // Vérifier l'équipe et décrémenter le nombre de timeouts restants
                    if (timeoutEventDTO.Team == "Home")
                    {
                        if (match.HomeTeamRemainingTimeouts > 0)
                        {
                            match.HomeTeamRemainingTimeouts--; 
                        }
                        else
                        {
                            throw new Exception("No timeouts remaining for the home team.");
                        }
                    }
                    else if (timeoutEventDTO.Team == "Away")
                    {
                        if (match.AwayTeamRemainingTimeouts > 0)
                        {
                            match.AwayTeamRemainingTimeouts--;
                        }
                        else
                        {
                            throw new Exception("No timeouts remaining for the away team.");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid team specified for timeout.");
                    }

                    // Retourner un nouvel événement de timeout
                    return new TimeoutEvent
                    {
                        MatchId = matchId,
                        Team = timeoutEventDTO.Team,
                        Quarter = timeoutEventDTO.Quarter,
                        Time = timeoutEventDTO.Time
                    };
                },
                "TimeoutEventOccurred"
            );
        }

        public async Task<MatchEvent> AddQuarterChangeEvent(int matchId, QuarterChangeEventDTO quarterChangeEventDTO)
        {
            return await AddMatchEvent(
                matchId,
                match => //new QuarterChangeEvent
                {
                    if(quarterChangeEventDTO.Quarter + 1 > match.Quarters.Count())
                         throw new InvalidOperationException("Cannot exceed the number of quarters in the match.");

                    match.CurrentQuarter = quarterChangeEventDTO.Quarter + 1;
                    return new QuarterChangeEvent
                    {
                        MatchId = matchId,
                        Quarter = match.CurrentQuarter
                    };
                },
                "QuarterChangeEventOccurred",
                match =>
                {
                    //Mise à jour du match en db
                    basketballDBContext.Matches.Update(match);
                }
            );
        }

        public async Task<MatchEvent> AddChronoEvent(int matchId, ChronoEventDTO chronoEventDTO)
        {
            return await AddMatchEvent(
                matchId,
                match => new ChronoEvent
                {
                    MatchId = matchId,
                    Quarter = chronoEventDTO.Quarter,
                    Time = chronoEventDTO.Time,
                    IsRunning = chronoEventDTO.IsRunning
                },
                "ChronoEventOccurred"
            );
        }*/


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
            //StartMatchTimer(matchId);//on start le timer du back 
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
            //StopMatchTimer(matchId);//on close le timer du back
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
    }
}
