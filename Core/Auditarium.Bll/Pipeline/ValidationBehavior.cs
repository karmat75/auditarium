// SPDX-License-Identifier: MIT
using Auditarium.Common.Results;
using FluentValidation;
using Mediator;

namespace Auditarium.Bll.Pipeline;

public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : IAppResult
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TMessage>(message);
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(validation => validation.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new AppError(failure.ErrorCode, ErrorType.Validation, failure.PropertyName))
            .ToArray();

        return failures.Length == 0 ? await next(message, cancellationToken) : ResultFactory.Failure<TResponse>(failures);
    }
}
