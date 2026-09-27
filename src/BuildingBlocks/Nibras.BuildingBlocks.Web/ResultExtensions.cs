using Microsoft.AspNetCore.Http;
using Nibras.BuildingBlocks.Domain;

namespace Nibras.BuildingBlocks.Web;

/// <summary>Turns a handler's <see cref="Result"/> into the HTTP response of document 22: the value on success, Problem Details on failure.</summary>
public static class ResultExtensions
{
    /// <summary>The Problem Details of a failed result; the status comes from the catalog row of its code.</summary>
    public static IResult ToProblem(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var error = result.Error ?? throw new InvalidOperationException("A successful result has no problem.");
        return new NibrasProblemResult(error.Code, error.Message);
    }

    /// <summary>204 on success, Problem Details on failure.</summary>
    public static IResult ToHttpResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    /// <summary>200 with the value, or the result <paramref name="onSuccess"/> builds (201, a versioned read), or Problem Details.</summary>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return onSuccess is null ? TypedResults.Ok(result.Value) : onSuccess(result.Value);
    }
}
