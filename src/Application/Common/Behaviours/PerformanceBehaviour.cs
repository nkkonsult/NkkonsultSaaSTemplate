using System.Diagnostics;
using System.Reflection;
using Hoplo.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hoplo.Application.Common.Behaviours;

public class PerformanceBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TMessage> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public PerformanceBehaviour(
        ILogger<TMessage> logger,
        IUser user,
        IIdentityService identityService)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next(message, cancellationToken);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TMessage).Name;
            var userId = _user.Id ?? string.Empty;
            var userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await _identityService.GetUserNameAsync(userId);
            }

            _logger.LogWarning("Hoplo Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, RedactSensitive(message));
        }

        return response;
    }

    private static object RedactSensitive(TMessage message)
    {
        if (message is not ISensitiveRequest sensitive)
            return message;

        var props = typeof(TMessage).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return props.ToDictionary(
            p => p.Name,
            p => sensitive.SensitiveProperties.Contains(p.Name) ? (object)"[REDACTED]" : p.GetValue(message)
        );
    }
}
