using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Nibras.BuildingBlocks.Web;

/// <summary>
/// Runs the registered FluentValidation validator of every endpoint argument before the handler, and answers
/// <c>_VALIDATION_FAILED</c> with one field entry per failure (REQ-SEC-017). An argument without a validator
/// costs nothing: the decision is made once, when the endpoint is built.
/// </summary>
internal static class ValidationFilter
{
    public static EndpointFilterDelegate Create(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        var registry = context.ApplicationServices.GetService<IServiceProviderIsService>();
        var validated = context.MethodInfo.GetParameters()
            .Select((parameter, index) => (Index: index, Validator: typeof(IValidator<>).MakeGenericType(parameter.ParameterType)))
            .Where(p => registry?.IsService(p.Validator) == true)
            .ToArray();
        if (validated.Length == 0)
        {
            return next;
        }

        return async invocation =>
        {
            var errors = new List<FieldError>();
            foreach (var (index, validatorType) in validated)
            {
                if (invocation.Arguments[index] is not { } argument
                    || invocation.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                {
                    continue;
                }

                var result = await validator.ValidateAsync(new ValidationContext<object>(argument), invocation.HttpContext.RequestAborted)
                    .ConfigureAwait(false);
                errors.AddRange(result.Errors.Select(ToFieldError));
            }

            if (errors.Count == 0)
            {
                return await next(invocation).ConfigureAwait(false);
            }

            var catalog = invocation.HttpContext.RequestServices.GetRequiredService<ErrorCatalog>();
            return new NibrasProblemResult(catalog.Prefix + ErrorCatalog.ValidationFailed, "One or more fields are not valid.", errors: errors);
        };
    }

    internal static FieldError ToFieldError(ValidationFailure failure) =>
        new(FieldPath(failure.PropertyName), CodeOf(failure.ErrorCode), ParamsOf(failure.FormattedMessagePlaceholderValues));

    /// <summary><c>Items[2].Amount.Currency</c> becomes <c>items[2].amount.currency</c>, the path the client sent.</summary>
    internal static string FieldPath(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return "$";
        }

        var path = new StringBuilder(propertyName.Length);
        foreach (var segment in propertyName.Split('.'))
        {
            if (path.Length > 0)
            {
                path.Append('.');
            }

            var bracket = segment.IndexOf('[', StringComparison.Ordinal);
            var name = bracket < 0 ? segment : segment[..bracket];
            path.Append(JsonNamingPolicy.CamelCase.ConvertName(name));
            if (bracket >= 0)
            {
                path.Append(segment[bracket..]);
            }
        }

        return path.ToString();
    }

    /// <summary>
    /// A rule's own code (set with <c>WithErrorCode("dateInFuture")</c>) is kept; a built-in validator's is
    /// translated to the shared validation catalog of document 22 §8.
    /// </summary>
    internal static string CodeOf(string errorCode) => errorCode switch
    {
        null or "" => "invalid",
        "NotEmptyValidator" or "NotNullValidator" => "required",
        "MaximumLengthValidator" => "maxLength",
        "MinimumLengthValidator" => "minLength",
        "LengthValidator" or "ExactLengthValidator" => "length",
        "EmailValidator" => "email",
        "GreaterThanValidator" or "GreaterThanOrEqualValidator" or "LessThanValidator" or "LessThanOrEqualValidator"
            or "InclusiveBetweenValidator" or "ExclusiveBetweenValidator" => "outOfRange",
        "RegularExpressionValidator" => "pattern",
        "EnumValidator" or "StringEnumValidator" => "unknownEnumValue",
        "ScalePrecisionValidator" or "PrecisionScaleValidator" => "precision",
        _ when errorCode.EndsWith("Validator", StringComparison.Ordinal) => "invalid",
        _ => errorCode,
    };

    /// <summary>Only the limits a message template needs; never the submitted value, which may be personal data.</summary>
    private static readonly Dictionary<string, string> ParamNames = new(StringComparer.Ordinal)
    {
        ["MaxLength"] = "maxLength",
        ["MinLength"] = "minLength",
        ["ComparisonValue"] = "limit",
        ["From"] = "min",
        ["To"] = "max",
        ["ExpectedPrecision"] = "precision",
        ["ExpectedScale"] = "scale",
    };

    private static JsonObject? ParamsOf(Dictionary<string, object>? placeholders)
    {
        if (placeholders is null)
        {
            return null;
        }

        JsonObject? result = null;
        foreach (var (key, value) in placeholders)
        {
            if (!ParamNames.TryGetValue(key, out var name) || value is null || value is int and < 0)
            {
                continue;
            }

            result ??= [];
            result[name] = value switch
            {
                int i => JsonValue.Create(i),
                long l => JsonValue.Create(l),
                decimal d => JsonValue.Create(d.ToString(CultureInfo.InvariantCulture)),
                DateOnly date => JsonValue.Create(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                DateTimeOffset at => JsonValue.Create(at.UtcDateTime.ToString(NibrasJson.TimestampFormat, CultureInfo.InvariantCulture)),
                IFormattable formattable => JsonValue.Create(formattable.ToString(null, CultureInfo.InvariantCulture)),
                _ => JsonValue.Create(value.ToString()),
            };
        }

        return result;
    }
}
