using Npgsql;

namespace HttpListenerServer;

public class TourDetailsService
{
    private readonly string _connString;

    public TourDetailsService(string connString)
    {
        _connString = connString;
    }

    public async Task<TourDetailDto?> GetTourAsync(int tourId)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        string title = "";
        string description = "";
        decimal price = 0;
        double? discount = null;
        int days = 7;
        double rating = 4.9;
        int reviews = 0;
        int? organizerId = null;

        await using (var cmd = new NpgsqlCommand(@"
            SELECT id, title, description, price, discount, days, rating, reviews_count, organizer_id
            FROM tours WHERE id = $1", conn))
        {
            cmd.Parameters.AddWithValue(tourId);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
                return null;

            title = r.GetString(1);
            description = r.IsDBNull(2) ? "" : r.GetString(2);
            price = r.GetDecimal(3);
            discount = r.IsDBNull(4) ? null : r.GetDouble(4);
            days = r.GetInt32(5);
            rating = r.GetDouble(6);
            reviews = r.GetInt32(7);
            organizerId = r.IsDBNull(8) ? null : r.GetInt32(8);
        }

        string organizerName = "Организатор";
        string organizerRating = "4.9";
        string organizerReviewsCount = "156";
        string organizerToursCount = "0";
        string organizerJoined = "01.01.2024";
        string organizerDescription = "Описание отсутствует.";

        if (organizerId.HasValue)
        {
            await using (var cmd = new NpgsqlCommand(@"
                SELECT name, rating, reviews_count, TO_CHAR(joined_at, 'DD.MM.YYYY'),
                       COALESCE(self_description, '')
                FROM organizers WHERE id = $1", conn))
            {
                cmd.Parameters.AddWithValue(organizerId.Value);
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    organizerName = r.GetString(0);
                    organizerRating = r.GetDouble(1).ToString("F1");
                    organizerReviewsCount = r.GetInt32(2).ToString();
                    organizerJoined = r.GetString(3);
                    organizerDescription = r.GetString(4);
                }
            }

            await using (var cmdCount = new NpgsqlCommand(
                "SELECT COUNT(*) FROM tours WHERE organizer_id = $1", conn))
            {
                cmdCount.Parameters.AddWithValue(organizerId.Value);
                organizerToursCount = (await cmdCount.ExecuteScalarAsync())?.ToString() ?? "0";
            }
        }

        string mainImage = "/static/images/default.jpg";
        var smallImages = new List<string>();

        await using (var cmd = new NpgsqlCommand(@"
            SELECT image_path FROM tour_images 
            WHERE tour_id = $1 ORDER BY sort_order LIMIT 5", conn))
        {
            cmd.Parameters.AddWithValue(tourId);
            await using var r = await cmd.ExecuteReaderAsync();
            int count = 0;
            while (await r.ReadAsync())
            {
                string path = "/" + r.GetString(0).Replace("\\", "/").TrimStart('/');
                if (count == 0) mainImage = path;
                else smallImages.Add(path);
                count++;
            }
        }

        var dayDescriptions = new List<string>();

        try
        {
            await using var cmd = new NpgsqlCommand(@"
                SELECT day1_description, day2_description, day3_description,
                       day4_description, day5_description, day6_description, day7_description
                FROM day_descriptions WHERE tour_id = $1", conn);

            cmd.Parameters.AddWithValue(tourId);

            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                for (int i = 0; i < 7; i++)
                {
                    dayDescriptions.Add(r.IsDBNull(i) ? "" : r.GetString(i));
                }
            }
        }
        catch
        {
            for (int i = 0; i < 7; i++)
                dayDescriptions.Add("");
        }

        while (dayDescriptions.Count < 7)
            dayDescriptions.Add("");

        return new TourDetailDto
        {
            Id = tourId,
            Title = title,
            Description = description,
            Price = price,
            Discount = discount,
            Days = days,
            Rating = rating,
            ReviewsCount = reviews,
            MainImage = mainImage,
            SmallImages = smallImages,
            OrganizerName = organizerName,
            OrganizerRating = organizerRating,
            OrganizerReviewsCount = organizerReviewsCount,
            OrganizerToursCount = organizerToursCount,
            OrganizerJoined = organizerJoined,
            OrganizerDescription = organizerDescription,
            DayDescriptions = dayDescriptions
        };
    }
}