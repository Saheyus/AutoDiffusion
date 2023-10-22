using Endpoints.Dtos;

namespace Endpoints.Ports
{
    public interface IPythonScriptInvoker
    {
        Task<PythonScriptResponse> InvokeAsync(string script, string[] arguments, CancellationToken cancellationToken = default);
    }
}
