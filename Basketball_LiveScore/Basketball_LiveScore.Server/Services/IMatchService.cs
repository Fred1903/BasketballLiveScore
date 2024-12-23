using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Hubs;
using Basketball_LiveScore.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Basketball_LiveScore.Server.Services
{
    public interface IMatchService
    {
        public List<int> GetNumberOfQuartersOptions();
        public List<int> GetQuarterDurationOptions();
        public List<int> GetTimeOutDurationOptions();

        public Dictionary<string, int> GetDefaultSettings();
        public Match CreateMatch(CreateMatchDTO matchDTO);
        //public MatchEvent AddMatchEvent(MatchEventDTO matchEventDto);
        public List<MatchEvent> GetMatchEvents(int matchId);
        public MatchDTO GetMatchDetails(int matchId);

        public MatchEvent AddFoulEvent(int matchId, FoulEventDTO foulEventDTO);
        public Task<MatchEvent> AddBasketEvent(int matchId, BasketEventDTO basketEventDTO);
        public MatchEvent AddSubstitutionEvent(int matchId, SubstitutionEventDTO substitutionEventDTO);
        public MatchEvent AddTimeoutEvent(int matchId, TimeoutEventDTO timeoutEventDTO);
        public MatchEvent AddChronoEvent(int matchId, ChronoEventDTO chronoEventDTO);
        MatchEvent AddQuarterChangeEvent(int matchId, QuarterChangeEventDTO quarterChangeEventDTO);
        //public void UpdateScore(int matchId, int teamId, int points);
        //public void AddFoul(int matchId, int playerId, int quarter, string foulType, TimeSpan time);

    }
}
