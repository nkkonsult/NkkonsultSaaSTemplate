using System.Reflection;
using Ardalis.GuardClauses;
using Hoplo.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hoplo.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger<TMessage> _logger;

    public UnhandledExceptionBehaviour(ILogger<TMessage> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (NotFoundException)
        {
            // NotFoundException est une exception domaine attendue (mapped to HTTP 404) — pas à logger comme erreur
            throw;
        }
        catch (Exception ex)
        {
            var requestName = typeof(TMessage).Name;

            _logger.LogError(ex, "Hoplo Request: Unhandled Exception for Request {Name} {@Request}", requestName, RedactSensitive(message));

            throw;
        }
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
