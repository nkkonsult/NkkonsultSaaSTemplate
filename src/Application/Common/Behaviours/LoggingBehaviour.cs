using System.Reflection;
using Nkkonsult.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Nkkonsult.Application.Common.Behaviours;

public class LoggingBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TMessage> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TMessage).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("Nkkonsult Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, BuildLogPayload(message));

        return await next(message, cancellationToken);
    }

    private static object BuildLogPayload<T>(T message) where T : notnull
    {
        if (message is not ISensitiveRequest sensitive)
            return message;

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return props.ToDictionary(
            p => p.Name,
            p => sensitive.SensitiveProperties.Contains(p.Name) ? (object)"***" : p.GetValue(message)
        );
    }
}
