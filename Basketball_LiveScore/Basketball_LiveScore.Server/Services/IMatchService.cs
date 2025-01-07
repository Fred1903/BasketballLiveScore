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
        public Task<Match> CreateMatch(CreateMatchDTO matchDTO);
        public Task<List<GetMatchEventDTO>> GetMatchEvents(int matchId);
        public Task<MatchDTO> GetMatchDetails(int matchId);

        public Task<MatchEvent> AddFoulEvent(int matchId, FoulEventDTO foulEventDTO);
        public Task<MatchEvent> AddBasketEvent(int matchId, BasketEventDTO basketEventDTO);
        public Task<MatchEvent> AddSubstitutionEvent(int matchId, SubstitutionEventDTO substitutionEventDTO);
        public Task<MatchEvent> AddTimeoutEvent(int matchId, TimeoutEventDTO timeoutEventDTO);
        public Task<MatchEvent> AddChronoEvent(int matchId, ChronoEventDTO chronoEventDTO);
        Task<MatchEvent> AddQuarterChangeEvent(int matchId, QuarterChangeEventDTO quarterChangeEventDTO);

        Task<List<MatchWithStatusDTO>> GetAllMatchesWithStatus();
        public Task StartMatch(int matchId);
        public Task FinishMatch(int matchId);
    }
}
