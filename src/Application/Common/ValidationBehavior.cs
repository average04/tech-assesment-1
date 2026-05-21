using System.Reflection;
using FluentValidation;
using MediatR;

namespace CustomerOnboarding.Application.Common;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var messages = failures.Select(f => f.ErrorMessage).ToArray();

        // TResponse must be Result or Result<T>. Build the failure via reflection.
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(
                nameof(Result<object>.Failure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                new[] { typeof(ErrorType), typeof(string[]) })!;
            return (TResponse)failureMethod.Invoke(null, new object[] { ErrorType.Validation, messages })!;
        }

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(ErrorType.Validation, messages);
        }

        // Validators registered for a request whose response isn't a Result — let it through.
        return await next();
    }
}
