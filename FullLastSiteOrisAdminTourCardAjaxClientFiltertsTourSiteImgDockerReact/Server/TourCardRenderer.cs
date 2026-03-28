using Npgsql;

namespace HttpListenerServer;

public class TourCardRenderer
{
    private readonly string _connString;
    private readonly string _templatePath;

    public TourCardRenderer(string connString, string staticFolder)
    {
        _connString = connString;
        _templatePath = Path.Combine(staticFolder, "templates", "card_template.html");
    }

    public async Task<ToursListResponse> GetToursAsync(Dictionary<string, string> filters)
    {
        var items = new List<TourCardDto>();

        var sql = new System.Text.StringBuilder();
        sql.Append("SELECT id, title, country, description, price, discount, days, nights, image_path, comfort_level, activity_level, rating, reviews_count ");
        sql.Append("FROM tours WHERE 1=1");

        var parameters = new List<NpgsqlParameter>();
        int paramIndex = 1;

        if (!string.IsNullOrWhiteSpace(filters.GetValueOrDefault("country")))
        {
            sql.Append($" AND (title ILIKE ${paramIndex} OR country ILIKE ${paramIndex})");
            parameters.Add(new NpgsqlParameter { Value = "%" + filters["country"].Trim() + "%" });
            paramIndex++;
        }

        if (int.TryParse(filters.GetValueOrDefault("price_min"), out int priceMin) && priceMin > 0)
        {
            sql.Append($" AND price >= ${paramIndex}");
            parameters.Add(new NpgsqlParameter { Value = priceMin });
            paramIndex++;
        }

        if (int.TryParse(filters.GetValueOrDefault("price_max"), out int priceMax) && priceMax > 0)
        {
            sql.Append($" AND price <= ${paramIndex}");
            parameters.Add(new NpgsqlParameter { Value = priceMax });
            paramIndex++;
        }

        if (!string.IsNullOrEmpty(filters.GetValueOrDefault("duration")))
        {
            var dur = filters["duration"];
            if (dur == "7-10")
                sql.Append(" AND days BETWEEN 7 AND 10");
            else if (dur == "11-14")
                sql.Append(" AND days BETWEEN 11 AND 14");
            else if (dur == "15+")
                sql.Append(" AND days >= 15");
        }

        if (int.TryParse(filters.GetValueOrDefault("comfort"), out int comfort) && comfort > 0)
        {
            sql.Append($" AND comfort_level = ${paramIndex}");
            parameters.Add(new NpgsqlParameter { Value = comfort });
            paramIndex++;
        }

        if (int.TryParse(filters.GetValueOrDefault("activity"), out int activity) && activity > 0)
        {
            sql.Append($" AND activity_level = ${paramIndex}");
            parameters.Add(new NpgsqlParameter { Value = activity });
            paramIndex++;
        }

        if (filters.GetValueOrDefault("discount") == "1")
            sql.Append(" AND discount IS NOT NULL AND discount > 0");

        var sort = filters.GetValueOrDefault("sort") ?? "popularity";
        sql.Append(sort switch
        {
            "price_asc" => " ORDER BY price ASC",
            "price_desc" => " ORDER BY price DESC",
            "rating_desc" => " ORDER BY rating DESC",
            "duration_asc" => " ORDER BY days ASC",
            _ => " ORDER BY rating DESC, reviews_count DESC"
        });

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        foreach (var param in parameters)
            cmd.Parameters.Add(param);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new TourCardDto
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Country = reader.GetString(2),
                Description = reader.GetString(3),
                Price = reader.GetDecimal(4),
                Discount = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                Days = reader.GetInt32(6),
                Nights = reader.GetInt32(7),
                ImagePath = "/" + reader.GetString(8).TrimStart('/'),
                ComfortLevel = reader.GetInt32(9),
                ActivityLevel = reader.GetInt32(10),
                Rating = reader.GetDouble(11),
                ReviewsCount = reader.GetInt32(12)
            });
        }

        return new ToursListResponse
        {
            Items = items,
            Total = items.Count
        };
    }
}