using Application.Dtos;

namespace Application.Ports
{
    public interface IPythonScriptInvoker
    {
        Task<PythonScriptResponse> InvokeAsync(string[] arguments, CancellationToken cancellationToken = default);
    }
}
