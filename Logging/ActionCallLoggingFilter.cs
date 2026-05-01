using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace student_app.Logging
{
    public class ActionCallLoggingFilter : IAsyncActionFilter
    {
        private static readonly JsonSerializerOptions PayloadJsonOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private readonly RollingTextFileLogger _fileLogger;
        private readonly ActionCallFileLoggingOptions _options;

        public ActionCallLoggingFilter(
            RollingTextFileLogger fileLogger,
            IOptions<ActionCallFileLoggingOptions> options)
        {
            _fileLogger = fileLogger;
            _options = options.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var functionName = descriptor == null
                ? context.ActionDescriptor.DisplayName ?? "Unknown action"
                : $"{descriptor.ControllerName}Controller.{descriptor.ActionName}";

            var request = context.HttpContext.Request;
            var startedAt = DateTimeOffset.Now;
            var stopwatch = Stopwatch.StartNew();
            var payload = FormatPayload(context.ActionArguments);

            await _fileLogger.WriteLineAsync(
                $"[{FormatTime(startedAt)}] START {functionName} | {request.Method} {request.Path}{request.QueryString} | Payload: {payload}");

            var executedContext = await next();
            stopwatch.Stop();

            var completedAt = DateTimeOffset.Now;
            var statusCode = context.HttpContext.Response.StatusCode;
            var result = executedContext.Exception == null ? "SUCCESS" : "ERROR";
            var errorText = executedContext.Exception == null
                ? string.Empty
                : $" | Error: {executedContext.Exception.GetType().Name} - {executedContext.Exception.Message}";

            await _fileLogger.WriteLineAsync(
                $"[{FormatTime(completedAt)}] END   {functionName} | {result} | Status: {statusCode} | Duration: {stopwatch.ElapsedMilliseconds} ms{errorText}");
        }

        private static string FormatTime(DateTimeOffset value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        }

        private string FormatPayload(IDictionary<string, object?> actionArguments)
        {
            if (actionArguments.Count == 0)
            {
                return "No payload";
            }

            try
            {
                var payload = JsonSerializer.Serialize(actionArguments, PayloadJsonOptions);
                var maxPayloadLength = Math.Max(1, _options.MaxPayloadLength);

                if (payload.Length <= maxPayloadLength)
                {
                    return payload;
                }

                return payload[..maxPayloadLength] + "... [Payload truncated]";
            }
            catch (Exception ex)
            {
                return $"Unable to serialize payload: {ex.Message}";
            }
        }
    }
}
