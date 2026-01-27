// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
#if FEATURE_ASPNET
using Microsoft.AspNetCore.Http;
#endif

namespace Samples.MyTaggedPackage;

public record Error : IResult
{
    public bool IsSuccess => false;

    public string? Message { get; set; }

#if FEATURE_DEBUG
    public string? File { get; }
    public int Line { get; }
    public string? Method { get; }

    public Error()
    {
        var trace = new StackTrace(1, true);
        if (trace.FrameCount > 0)
        {
            var frame = trace.GetFrame(0)!;
            File = frame.GetFileName();
            Line = frame.GetFileLineNumber();
            var method = DiagnosticMethodInfo.Create(frame);
            if (method is not null)
                Method = method.DeclaringTypeName + '.' + method.Name;
        }
    }
#endif

#if FEATURE_ASPNET
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 500;
        if (Message is not null)
        {
            await httpContext.Response.WriteAsync(Message);
        }
    }
#endif
}
