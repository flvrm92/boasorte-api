using BoaSorte.Api.Contracts;
using BoaSorte.Api.Http;
using BoaSorte.Application.Draws.CreateDraw;
using BoaSorte.Application.Draws.GetDraw;
using Microsoft.AspNetCore.Mvc;

namespace BoaSorte.Api.Endpoints;

internal static class DrawEndpoints
{
    // 10,000 entries × 200 characters as JSON is ~2 MB (contract: 3 MB limit).
    private const long MaxCreateBodyBytes = 3 * 1024 * 1024;

    public static IEndpointRouteBuilder MapDrawEndpoints(this IEndpointRouteBuilder app)
    {
        var draws = app.MapGroup("/api/v1/draws").WithTags("Draws");

        draws.MapPost("/", CreateAsync)
            .RequireRateLimiting(RateLimiting.CreateDrawPolicy)
            .WithMetadata(new RequestSizeLimitAttribute(MaxCreateBodyBytes))
            .WithName("CreateDraw")
            .Produces<DrawResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        draws.MapGet("/{publicId}", GetAsync)
            .WithName("GetDraw")
            .Produces<DrawResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone);

        return app;
    }

    private static async Task<IResult> CreateAsync(CreateDrawRequest request, CreateDrawHandler handler, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        if (command.IsFailure)
        {
            return command.Errors.ToProblem();
        }

        var result = await handler.HandleAsync(command.Value, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/draws/{result.Value.PublicId}", DrawResponse.From(result.Value))
            : result.Errors.ToProblem();
    }

    private static async Task<IResult> GetAsync(string publicId, GetDrawHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetDrawQuery(publicId), cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? TypedResults.Ok(DrawResponse.From(result.Value))
            : result.Errors.ToProblem();
    }
}
