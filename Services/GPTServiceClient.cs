using Grpc.Net.Client;

public class GPTServiceClient
{
    private readonly GPTService.GPTServiceClient _client;

    public GPTServiceClient()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:50051");
        _client = new GPTService.GPTServiceClient(channel);
    }

    public async Task<ProcessReply> ProcessTextAsync(ProcessTextRequest request)
    {
        return await _client.ProcessTextAsync(request);
    }
}

