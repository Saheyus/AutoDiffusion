using Domain.Events;

namespace Endpoints.Services
{
    internal sealed class NotificationsWebHookRepository
    {
        private readonly Dictionary<string, IEnumerable<string>> _webHooksDictionary = new()
        {
            {   nameof (ImageGenerationJobFailedEvent), new [] {"AutoDiffusion"} },
            {   nameof (ImageGenerationJobFinishedEvent), new [] {"AutoDiffusion"} },
            {   nameof (ImageGenerationJobStartedEvent), new [] {"AutoDiffusion"} }
        };

        public Task<IEnumerable<string>> GetClientsAsync(string notificationType)
        {
            return !_webHooksDictionary.TryGetValue(notificationType, out var clients) ?
                Task.FromResult(Enumerable.Empty<string>()) : 
                Task.FromResult(clients);
        }
    }
}
