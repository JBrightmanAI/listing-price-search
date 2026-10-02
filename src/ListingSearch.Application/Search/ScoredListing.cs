using ListingSearch.Domain;

namespace ListingSearch.Application.Search;

public sealed record ScoredListing(Listing Listing, double Score);
