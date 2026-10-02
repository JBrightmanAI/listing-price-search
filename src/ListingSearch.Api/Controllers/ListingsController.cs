using ListingSearch.Api.Contracts;
using ListingSearch.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController(IListingSearchService searchService) : ControllerBase
{
    /// <summary>Searches listings, ranked by relevance, one page at a time.</summary>
    /// <response code="200">A page of ranked results. An empty page is a valid answer, not an error.</response>
    /// <response code="400">One or more query parameters are invalid; errors are keyed by parameter name.</response>
    /// <response code="429">Too many requests from this client.</response>
    [HttpGet("search")]
    [ProducesResponseType<SearchListingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SearchListingsResponse>> Search(
        [FromQuery] SearchListingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await searchService.SearchAsync(request.ToQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            foreach (var (field, messages) in result.Errors.ToDictionary())
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(field, message);
                }
            }

            // Same ValidationProblemDetails shape as model-binding failures, so clients handle one format.
            return ValidationProblem(ModelState);
        }

        return Ok(SearchListingsResponse.From(result.Value));
    }
}
