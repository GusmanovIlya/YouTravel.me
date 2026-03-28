using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HttpListenerServer;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _staticFolder;
    private readonly string _connString;
    private readonly PageRenderer _pageRenderer;
    private readonly AdminPanel _adminPanel;
    private readonly Dictionary<string, DateTime> _sessions = new();

    public HttpServer(string prefix, string staticFolder, string connectionString)
    {
        _staticFolder = Path.GetFullPath(staticFolder);
        _connString = connectionString;
        _pageRenderer = new PageRenderer(_connString, _staticFolder);
        _adminPanel = new AdminPanel(_connString);
        _listener.Prefixes.Add(prefix);
    }

    public async Task RunAsync()
    {
        _listener.Start();
        Console.WriteLine("Сервер запущен → http://localhost:1234");
        Console.WriteLine("Админка → http://localhost:1234/admin");
        Console.WriteLine("Для остановки введите: quit");

        var cts = new CancellationTokenSource();

        _ = Task.Run(() =>
        {
            while (true)
            {
                var line = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (line is "quit" or "exit" or "q")
                {
                    cts.Cancel();
                    break;
                }
            }
        });

        while (!cts.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(ctx), cts.Token);
            }
            catch when (cts.IsCancellationRequested)
            {
                break;
            }
            catch
            {
            }
        }

        _listener.Stop();
        _listener.Close();
    }

    private async void HandleRequest(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var resp = ctx.Response;
        string path = req.Url?.AbsolutePath ?? "/";

        try
        {
            // =========================
            // ADMIN
            // =========================
            if (path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                bool isAuth = IsAuthenticated(req);

                if (!isAuth)
                {
                    if (req.HttpMethod == "POST")
                        await HandleLogin(ctx);
                    else
                        await ServeLoginPage(resp);

                    return;
                }

                if (path == "/admin/logout")
                {
                    Logout(ctx);
                    resp.Redirect("/admin");
                    return;
                }

                if (path == "/admin" || path == "/admin/")
                {
                    if (req.HttpMethod == "POST")
                        await _adminPanel.HandleCrud(ctx);
                    else
                        await _adminPanel.ServeDashboard(ctx);

                    return;
                }

                var editMatch = Regex.Match(path, @"^/admin/edit/(\d+)$", RegexOptions.IgnoreCase);
                if (editMatch.Success)
                {
                    int id = int.Parse(editMatch.Groups[1].Value);
                    await _adminPanel.ServeEditPage(ctx, id);
                    return;
                }

                if (req.HttpMethod == "POST" &&
                    (path == "/admin/add" || path == "/admin/delete" || path == "/admin/save"))
                {
                    await _adminPanel.HandleCrud(ctx);
                    return;
                }
            }

            // =========================
            // API: TOURS LIST
            // =========================
            if (path == "/api/tours")
            {
                var filters = new Dictionary<string, string>();
                var q = req.QueryString;

                filters["country"] = q["country"] ?? "";
                filters["price_min"] = q["price_min"] ?? "";
                filters["price_max"] = q["price_max"] ?? "";
                filters["duration"] = q["duration"] ?? "";
                filters["activity"] = q["activity"] ?? "";
                filters["comfort"] = q["comfort"] ?? "";
                filters["discount"] = q["discount"] ?? "0";
                filters["sort"] = q["sort"] ?? "popularity";

                var result = await _pageRenderer.GetToursAsync(filters);
                await SendJson(resp, result);
                return;
            }

            // =========================
            // API: TOUR DETAILS
            // =========================
            var apiTourMatch = Regex.Match(path, @"^/api/tours/(\d+)$", RegexOptions.IgnoreCase);
            if (apiTourMatch.Success)
            {
                int tourId = int.Parse(apiTourMatch.Groups[1].Value);
                var result = await _pageRenderer.GetTourDetailAsync(tourId);

                if (result == null)
                {
                    resp.StatusCode = 404;
                    await SendJson(resp, new { message = "Тур не найден" });
                    return;
                }

                await SendJson(resp, result);
                return;
            }

            // =========================
            // STATIC FILES
            // =========================
            if (IsStaticFileRequest(path))
            {
                string localPath = Path.Combine(_staticFolder, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                await ServeFile(resp, localPath);
                return;
            }

            // =========================
            // REACT SPA ROUTES
            // =========================
            if (path == "/" ||
                path == "/index.html" ||
                Regex.IsMatch(path, @"^/tour/\d+$", RegexOptions.IgnoreCase))
            {
                await ServeFile(resp, Path.Combine(_staticFolder, "index.html"));
                return;
            }

            // =========================
            // 404
            // =========================
            string error404 = Path.Combine(_staticFolder, "errors", "404.html");
            if (File.Exists(error404))
            {
                resp.StatusCode = 404;
                await ServeFile(resp, error404);
            }
            else
            {
                resp.StatusCode = 404;
                SendHtml(resp, "<h1>404 — Страница не найдена</h1>");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Ошибка] {ex.Message}\n{ex.StackTrace}");

            if (!resp.OutputStream.CanWrite)
                return;

            resp.StatusCode = 500;
            SendHtml(resp, "<h1>500 — Ошибка сервера</h1>");
        }
        finally
        {
            try
            {
                resp.Close();
            }
            catch
            {
            }
        }
    }

    private static bool IsStaticFileRequest(string path)
    {
        return path.Contains('.') &&
               !path.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsAuthenticated(HttpListenerRequest req)
    {
        var sid = req.Cookies["session"]?.Value;
        return sid != null && _sessions.TryGetValue(sid, out var exp) && exp > DateTime.UtcNow;
    }

    private void CreateSession(HttpListenerResponse resp)
    {
        var sid = Guid.NewGuid().ToString();
        _sessions[sid] = DateTime.UtcNow.AddHours(3);
        resp.Cookies.Add(new Cookie("session", sid, "/") { HttpOnly = true });
    }

    private void Logout(HttpListenerContext ctx)
    {
        if (ctx.Request.Cookies["session"]?.Value is string old)
            _sessions.Remove(old);

        ctx.Response.Cookies.Add(new Cookie("session", "", "/")
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        });
    }

    private async Task HandleLogin(HttpListenerContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.InputStream);
        var body = await reader.ReadToEndAsync();
        var data = HttpUtility.ParseQueryString(body);

        if (data["login"] == "admin" && data["password"] == "12345")
        {
            CreateSession(ctx.Response);
            ctx.Response.Redirect("/admin");
        }
        else
        {
            SendHtml(ctx.Response, "<script>alert('Неверный логин или пароль'); history.back();</script>");
        }
    }

    private async Task ServeLoginPage(HttpListenerResponse resp)
    {
        const string html = @"
<!DOCTYPE html><html lang='ru'><head><meta charset='UTF-8'><title>Вход</title>
<style>
    body{background:linear-gradient(135deg,#667eea,#764ba2);display:flex;justify-content:center;align-items:center;height:100vh;margin:0;font-family:Arial}
    .box{background:#fff;padding:50px;border-radius:16px;box-shadow:0 15px 35px rgba(0,0,0,0.2);width:380px;text-align:center}
    input{padding:14px;margin:10px 0;width:100%;border-radius:8px;border:1px solid #ddd;font-size:16px}
    button{padding:14px;background:#5e35b1;color:#fff;border:none;border-radius:8px;cursor:pointer;width:100%;font-size:16px}
</style></head>
<body><div class='box'><h2>Админ-панель</h2>
<form method='post'>
    <input name='login' placeholder='Логин' value='admin' required><br>
    <input type='password' name='password' placeholder='Пароль' value='12345' required><br>
    <button type='submit'>Войти</button>
</form></div></body></html>";
        SendHtml(resp, html);
        await Task.CompletedTask;
    }

    private async Task SendJson(HttpListenerResponse resp, object data)
    {
        string json = JsonSerializer.Serialize(data);
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        resp.ContentType = "application/json; charset=utf-8";
        resp.ContentLength64 = buffer.Length;
        await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

    private void SendHtml(HttpListenerResponse resp, string html)
    {
        var buffer = Encoding.UTF8.GetBytes(html);
        resp.ContentType = "text/html; charset=utf-8";
        resp.ContentLength64 = buffer.Length;
        resp.OutputStream.Write(buffer, 0, buffer.Length);
    }

    private async Task ServeFile(HttpListenerResponse resp, string filePath)
    {
        if (!File.Exists(filePath))
        {
            resp.StatusCode = 404;
            SendHtml(resp, "<h1>404 — Файл не найден</h1>");
            return;
        }

        var buffer = await File.ReadAllBytesAsync(filePath);

        resp.ContentType = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".map" => "application/json; charset=utf-8",
            ".html" => "text/html; charset=utf-8",
            _ => "application/octet-stream"
        };

        resp.ContentLength64 = buffer.Length;
        await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }
}