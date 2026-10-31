namespace TestDeIa.Shared.Responses.Common;

public sealed class PagedResultResponse<TItem>
{
    public IReadOnlyCollection<TItem> Items { get; set; } = Array.Empty<TItem>();

    public int TotalCount { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; }
}
