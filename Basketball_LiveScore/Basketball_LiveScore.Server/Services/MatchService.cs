
using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Basketball_LiveScore.Server.Services
{
    public class MatchService : IMatchService
    {
        private readonly BasketballDBContext basketballDBContext;

        public MatchService(BasketballDBContext basketballDBContext)
        {
            this.basketballDBContext = basketballDBContext;
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


        /*public MatchEvent AddMatchEvent(MatchEventDTO matchEventDto)
        {
            MatchEvent matchEvent;

            // Vérifiez le type réel du DTO et créez l'instance appropriée
            switch (matchEventDto)
            {
                case BasketEventDTO basketEventDto:
                    matchEvent = new BasketEvent
                    {
                        MatchId = basketEventDto.MatchId,
                        PlayerId = basketEventDto.PlayerId,
                        Points = basketEventDto.Points,
                        Quarter = basketEventDto.Quarter,
                        Time = basketEventDto.Time
                    };
                    break;

                case FoulEventDTO foulEventDto:
                    matchEvent = new FoulEvent
                    {
                        MatchId = foulEventDto.MatchId,
                        PlayerId = foulEventDto.PlayerId,
                        FoulType = foulEventDto.FoulType,
                        Quarter = foulEventDto.Quarter,
                        Time = foulEventDto.Time
                    };
                    break;

                case SubstitutionEventDTO substitutionEventDto:
                    matchEvent = new SubstitutionEvent
                    {
                        MatchId = substitutionEventDto.MatchId,
                        PlayerInId = substitutionEventDto.PlayerInId,
                        PlayerOutId = substitutionEventDto.PlayerOutId,
                        Quarter = substitutionEventDto.Quarter,
                        Time = substitutionEventDto.Time
                    };
                    break;

                case TimeoutEventDTO timeoutEventDto:
                    matchEvent = new TimeoutEvent
                    {
                        MatchId = timeoutEventDto.MatchId,
                        Team = timeoutEventDto.Team,
                        Quarter = timeoutEventDto.Quarter,
                        Time = timeoutEventDto.Time
                    };
                    break;

                case ChronoEventDTO chronoEventDto:
                    matchEvent = new ChronoEvent
                    {
                        MatchId = chronoEventDto.MatchId,
                        IsRunning = chronoEventDto.IsRunning,
                        Quarter = chronoEventDto.Quarter,
                        Time = chronoEventDto.Time
                    };
                    break;

                default:
                    throw new Exception("Unsupported event type.");
            }

            //Ajouter event a la db
            basketballDBContext.MatchEvents.Add(matchEvent);
            basketballDBContext.SaveChanges();

            return matchEvent;
        }*/


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
                // Charger le match avec les relations nécessaires
                var match = basketballDBContext.Matches
                    .Include(m => m.Quarters)
                    .Include(m => m.Timeouts)
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .FirstOrDefault(m => m.Id == matchId);

                // Si le match n'existe pas
                if (match == null)return null;

                // Construire le DTO
                return new MatchDTO
                {
                    MatchId = match.Id,
                    MatchDate = match.matchDate,
                    HomeTeam = new TeamMatchDTO
                    {
                        TeamId = match.HomeTeam.Id,
                        Name = match.HomeTeam.Name,
                        Coach = match.HomeTeam.Coach,
                        StartingFive = match.HomeTeamStartingFiveIds
                    },
                    AwayTeam = new TeamMatchDTO
                    {
                        TeamId = match.AwayTeam.Id,
                        Name = match.AwayTeam.Name,
                        Coach = match.AwayTeam.Coach,
                        StartingFive = match.AwayTeamStartingFiveIds
                    },
                    QuarterDuration = (int)match.Quarters.FirstOrDefault()?.Duration,
                    NumberOfQuarters = match.Quarters.Count,
                    Timeouts = match.Timeouts.Count,
                    ScoreHome = match.ScoreHome,
                    ScoreAway = match.ScoreAway
                };
            }
            catch (Exception ex)
            {
                // Gérer les erreurs inattendues
                throw new Exception($"Error retrieving match details for Match ID {matchId}: {ex.Message}", ex);
            }
        }

        /*public void UpdateScore(int matchId, int teamId, int points)
        {
            var match = basketballDBContext.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefault(m => m.Id == matchId);

            if (match == null)
                throw new Exception($"Match with ID {matchId} not found.");

            if (match.HomeTeam.Id == teamId)
            {
                match.ScoreHome += points;
            }
            else if (match.AwayTeam.Id == teamId)
            {
                match.ScoreAway += points;
            }
            else
            {
                throw new Exception($"Team with ID {teamId} does not belong to the match.");
            }
            basketballDBContext.SaveChanges();
        }

        public void AddFoul(int matchId, int playerId, int quarter, string foulType, TimeSpan time)
        {
            //verif si match existe
            var match = basketballDBContext.Matches.Find(matchId);
            if (match == null)
                throw new Exception($"Match with ID {matchId} not found.");

            //verif si joueur existe
            var player = basketballDBContext.Players.Find(playerId);
            if (player == null)
                throw new Exception($"Player with ID {playerId} not found.");

            var foulEvent = new FoulEvent
            {
                MatchId = matchId,
                PlayerId = playerId,
                Quarter = quarter,
                Time = time,
                FoulType = foulType // Type de faute (P0, P1, P2, P3)
            };

            basketballDBContext.MatchEvents.Add(foulEvent);
            basketballDBContext.SaveChanges();
        }*/

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
                return foulEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding foul event: {ex.Message}");
            }
        }

        public MatchEvent AddBasketEvent(int matchId, BasketEventDTO basketEventDTO)
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

                basketballDBContext.MatchEvents.Add(basketEvent);
                basketballDBContext.SaveChanges();
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
                return timeoutEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding timeout event: {ex.Message}");
            }
        }

        public MatchEvent AddChronoEvent(int matchId, ChronoEventDTO chronoEventDTO)
        {
            try
            {
                var match = basketballDBContext.Matches.Find(matchId);
                if (match == null)
                    throw new Exception($"Match with ID {matchId} not found.");

                var chronoEvent = new ChronoEvent
                {
                    MatchId = matchId,
                    IsRunning = chronoEventDTO.IsRunning,
                    Quarter = chronoEventDTO.Quarter,
                    Time = chronoEventDTO.Time
                };

                basketballDBContext.MatchEvents.Add(chronoEvent);
                basketballDBContext.SaveChanges();
                return chronoEvent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding chrono event: {ex.Message}");
            }
        }

    }
}
