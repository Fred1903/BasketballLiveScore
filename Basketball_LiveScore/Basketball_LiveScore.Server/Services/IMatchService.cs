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

        public List<int> GetTimeOutAmountOptions();

        public Dictionary<string, int> GetDefaultSettings();
        public Match CreateMatch(CreateMatchDTO matchDTO);
        //public MatchEvent AddMatchEvent(MatchEventDTO matchEventDto);
        public List<MatchEvent> GetMatchEvents(int matchId);
        public MatchDTO GetMatchDetails(int matchId);

        public Task<MatchEvent> AddFoulEvent(int matchId, FoulEventDTO foulEventDTO);
        public Task<MatchEvent> AddBasketEvent(int matchId, BasketEventDTO basketEventDTO);
        public Task<MatchEvent> AddSubstitutionEvent(int matchId, SubstitutionEventDTO substitutionEventDTO);
        public Task<MatchEvent> AddTimeoutEvent(int matchId, TimeoutEventDTO timeoutEventDTO);
        public Task<MatchEvent> AddChronoEvent(int matchId, ChronoEventDTO chronoEventDTO);
        Task<MatchEvent> AddQuarterChangeEvent(int matchId, QuarterChangeEventDTO quarterChangeEventDTO);

        List<MatchWithStatusDTO> GetAllMatchesWithStatus();
        public void StartMatch(int matchId);
        public void FinishMatch(int matchId);
    }
}
