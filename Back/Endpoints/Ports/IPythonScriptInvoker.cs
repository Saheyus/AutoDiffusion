﻿using Endpoints.Dtos;

namespace Endpoints.Ports
{
    public interface IPythonScriptInvoker
    {
        Task<PythonScriptResponse> InvokeAsync(string[] arguments, CancellationToken cancellationToken = default);
    }
}
