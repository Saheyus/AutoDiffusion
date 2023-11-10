using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Notifications;
using MediatR;

namespace Endpoints.Services
{
    public sealed class NotificationsHandler : INotificationHandler<ImageGenerationJobNotification>
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly NotificationsWebHookRepository _notificationsWebHookRepository;

        public NotificationsHandler(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _notificationsWebHookRepository = new NotificationsWebHookRepository();
            _jsonSerializerOptions = new JsonSerializerOptions
            { 
                AllowTrailingCommas = true, 
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
        public async Task Handle(ImageGenerationJobNotification notification, CancellationToken cancellationToken)
        {
            var clients = await _notificationsWebHookRepository.GetClientsAsync(notification.NotificationType);

            if (!clients.Any())
                return;

            const string callBackUri = "CallBacks/ReceiveNotification";
            var postTasks = clients.Select(async clientName =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var client = _clientFactory.CreateClient(clientName);
                using var content = CreateContent(notification);
                await client.PostAsync(callBackUri, content, cancellationToken);
            });

            await Task.WhenAll(postTasks);
        }

        private StringContent CreateContent(ImageGenerationJobNotification notification)
        {
            var content = JsonSerializer.Serialize(notification, _jsonSerializerOptions);
            return new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }
}
