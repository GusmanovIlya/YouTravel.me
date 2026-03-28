namespace HttpListenerServer;

public class PageRenderer
{
    private readonly TourCardRenderer _cardRenderer;
    private readonly TourDetailsService _detailsService;

    public PageRenderer(string connectionString, string staticFolder)
    {
        _cardRenderer = new TourCardRenderer(connectionString, staticFolder);
        _detailsService = new TourDetailsService(connectionString);
    }

    public Task<ToursListResponse> GetToursAsync(Dictionary<string, string> filters)
        => _cardRenderer.GetToursAsync(filters);

    public Task<TourDetailDto?> GetTourDetailAsync(int id)
        => _detailsService.GetTourAsync(id);
}