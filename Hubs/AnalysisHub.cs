using AssessmentPlatform.Backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace AssessmentPlatform.Backend.Hubs
{
    public class AnalysisHub : Hub
    {
        public async Task SendQuizResult(List<Ranking> rankings)
        {
            await Clients.All.SendAsync("ReceiveQuizResult", rankings);
        }
    }
}