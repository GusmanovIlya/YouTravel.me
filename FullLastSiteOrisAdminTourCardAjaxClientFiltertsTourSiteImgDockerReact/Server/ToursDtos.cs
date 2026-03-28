namespace HttpListenerServer;

public class TourCardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Country { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public double? Discount { get; set; }
    public int Days { get; set; }
    public int Nights { get; set; }
    public string ImagePath { get; set; } = "";
    public int ComfortLevel { get; set; }
    public int ActivityLevel { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
}

public class ToursListResponse
{
    public List<TourCardDto> Items { get; set; } = new();
    public int Total { get; set; }
}

public class TourDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public double? Discount { get; set; }
    public int Days { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public string MainImage { get; set; } = "";
    public List<string> SmallImages { get; set; } = new();

    public string OrganizerName { get; set; } = "";
    public string OrganizerRating { get; set; } = "";
    public string OrganizerReviewsCount { get; set; } = "";
    public string OrganizerToursCount { get; set; } = "";
    public string OrganizerJoined { get; set; } = "";
    public string OrganizerDescription { get; set; } = "";

    public List<string> DayDescriptions { get; set; } = new();
}