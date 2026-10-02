namespace ListingSearch.Domain;

/// <summary>
/// Identity of a listing. Feed ids are only unique within a source, so the source is part of the key.
/// </summary>
public readonly record struct ListingKey(string Source, string Id)
{
    public override string ToString() => $"{Source}:{Id}";
}
