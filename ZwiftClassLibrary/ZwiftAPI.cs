using Google.Protobuf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ZwiftClassLibrary;

public class ZwiftAPI
{
    private string _host = "us-or-rly101.zwift.com";
    private string _scheme = "https";
    private string _username;
    private string _authToken;
    private PlayerProfile _profile;
    private dynamic _exclusions;
    private const int CadenceMax = 240 * 1000000 / 60;
    private const double halfCircle = 1000000 * Math.PI;

    private string Host { get => _host; set => _host = value; }
    private string Scheme { get => _scheme; set => _scheme = value; }
    private string Username { get => _username; set => _username = value; }
    private string AuthToken { get => _authToken; set => _authToken = value; }
    private PlayerProfile Profile { get => _profile; set => _profile = value; }
    private dynamic Exclusions { get => _exclusions; set => _exclusions = value; }

    private Dictionary<int, string> _idHashes { get; } = new Dictionary<int, string>();
    private  Dictionary<string, double> _idHashTimestamps { get; } = new Dictionary<string, double>();
    private int _idHashUse { get; set; } = 0;
    private Timer _nextRefresh { get; set; } 
    private string _refreshToken { get; set; }




    public ZwiftAPI(Dictionary<string, object> options = null)
    {
        if (options == null)
            options = new Dictionary<string, object>();

        if (options.ContainsKey("exclusions"))
            Exclusions = (HashSet<string>)options["exclusions"];
        else
            Exclusions = new HashSet<string>();
    }

    public async Task Authenticate(string username, string password)
    {
        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{"https://secure.zwift.com"}/auth/realms/zwift/protocol/openid-connect/token");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "Zwift Game Client"),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("username", username)
            });
            request.Content = content;

            HttpResponseMessage response = await client.SendAsync(request);
            string resp = await response.Content.ReadAsStringAsync();            
            JsonReader toDeserialize = new JsonTextReader(new StringReader(resp));

            var jsonSerializer = new JsonSerializer();
            Dictionary<string, object> authResponse = jsonSerializer.Deserialize<Dictionary<string, object>>(toDeserialize);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception(authResponse.ContainsKey("error_description") ? authResponse["error_description"].ToString() : "Login failed");

            this.Username = username;
            AuthToken = authResponse["access_token"].ToString();

            Console.WriteLine("Zwift auth token acquired");

            
            if (authResponse.ContainsKey("expires_in") && int.TryParse(authResponse["expires_in"].ToString(), out int expires_in))
            {
                int refreshNumber = expires_in * 1000 / 2;
                _schedRefresh(refreshNumber);

                Profile = await GetProfileAsync("me");
            }
        }
    }

    public async Task LoopPlayerStateRetrieval(int zwiftId, Action<int> DraftReceiver)
    {
        Thread.Sleep(2000);
        while (true)
        {
            var ps = await GetPlayerState(zwiftId);
            if (ps != null)
                DraftReceiver(ps.Draft);

            Thread.Sleep(2000);
        }
    }

    public async Task<PlayerState> GetPlayerState() => 
        await GetPlayerState(Profile.Id.Value);

    public async Task<PlayerState> GetPlayerState(long id)
    {
        PlayerState pb;
        try
        {
            pb = await FetchPB($"/relay/worlds/1/players/{id}", new Dictionary<string, string> { { "protobuf", "PlayerState" } });
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e?.Message);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e?.Message);
            return null;
        }
        
        return pb;
    }

    public async Task<PlayerState> FetchPB(string urn, Dictionary<string, string> options, Dictionary<string, string> headers = null) 
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Scheme}://{Host}{urn}");
        if (headers != null)
            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

        if (!string.IsNullOrEmpty(_authToken))
            request.Headers.Add("Authorization", $"Bearer {_authToken}");

        var _httpClient = new HttpClient();
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync();
        if (options != null && options.ContainsKey("debug") && options["debug"] == "true")
            Console.WriteLine($"PB API DEBUG {urn} {BitConverter.ToString(data).Replace("-", "")}");
        
        var parser = new MessageParser<PlayerState>(() => new PlayerState());
        return parser.ParseFrom(data);
    }

    public async Task<PlayerProfile> GetProfileAsync(string id) => await FetchJSONAsync<PlayerProfile>($"/api/profiles/{id}");

    public async Task<T> FetchJSONAsync<T>(string urn)
    {
        HttpResponseMessage response = await FetchAsync(urn, new Dictionary<string, object> { { "accept", "json" } });
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default(T);
        }
        var rawResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(rawResponse);
    }

    public async Task<HttpResponseMessage> FetchAsync(string urn, Dictionary<string, object> options = null, Dictionary<string, string> headers = null)
    {
        options = options ?? new Dictionary<string, object>();
        headers = headers ?? new Dictionary<string, string>();

        if (!options.ContainsKey("noAuth") || !(bool)options["noAuth"])
        {
            if (!IsAuthenticated())
                throw new InvalidOperationException("Auth token not set");
            
            headers["Authorization"] = $"Bearer {AuthToken}";
        }

        if (options.ContainsKey("json"))
        {
            options["body"] = JsonConvert.SerializeObject(options["json"]);
            headers["Content-Type"] = "application/json";
        }

        if (options.ContainsKey("pb"))
        {
            options["body"] = ((IMessage)options["pb"]).ToByteArray();
            headers["Content-Type"] = "application/x-protobuf-lite; version=2.0";
        }

        if (options.ContainsKey("accept"))
            headers["Accept"] = options["accept"].ToString() == "json" ? "application/json" : "application/x-protobuf-lite";

        if (options.ContainsKey("apiVersion"))
            headers["Zwift-Api-Version"] = options["apiVersion"].ToString();

        var defHeaders = new Dictionary<string, string>
        {
            { "Platform", "OSX" },
            { "Source", "Game Client" },
            { "User-Agent", "CNL/3.44.0 (Darwin Kernel 23.2.0) zwift/1.0.122968 game/1.54.0 curl/8.4.0" }
        };

        var query = options.ContainsKey("query") ? options["query"] as Dictionary<string, string> : null;
        var q = query != null ? $"?{string.Join("&", query.Select(kv => $"{kv.Key}={kv.Value}"))}" : string.Empty;

        var uri = $"{Scheme}://{Host}/{urn.TrimStart('/')}{q}";

        if (!options.ContainsKey("silent") || !(bool)options["silent"])
            Console.WriteLine($"Fetch: {(options.ContainsKey("method") ? options["method"] : "GET")} {uri}");

        var timeout = options.ContainsKey("timeout") ? (int)options["timeout"] : 30000;

        using (var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) })
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = new HttpMethod(options.ContainsKey("method") ? options["method"].ToString() : "GET")
            };

            foreach (var header in defHeaders)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            foreach (var header in headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (options.ContainsKey("body"))
                request.Content = new StringContent(options["body"].ToString());

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode && (!options.ContainsKey("ok") || !((List<int>)options["ok"]).Contains((int)response.StatusCode)))
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Zwift HTTP Error: [{(int)response.StatusCode}]: {msg}");
            }

            return response;
        }
    }

    private bool IsAuthenticated() => !string.IsNullOrEmpty(AuthToken);




    private void _schedRefresh(int delay)
    {
        _nextRefresh?.Dispose();
        Console.WriteLine($"Refresh Zwift token in: {fmtTime(delay)}");
        _nextRefresh = new Timer(async _ => await RefreshToken(), null, Math.Min(0x7fffffff, delay), Timeout.Infinite);
    }

    private async Task RefreshToken()
    {
        if (string.IsNullOrEmpty(this.AuthToken))
        {
            Console.WriteLine("No auth token to refresh");
            return;
        }

        var client = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "Zwift Game Client"),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", this._refreshToken)
        });

        var response = await client.PostAsync($"{Scheme}://{Host}/auth/realms/zwift/protocol/openid-connect/token", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

        this.AuthToken = resp["access_token"];
        this._refreshToken = resp["refresh_token"];
        Console.WriteLine("Zwift auth token refreshed");
        _schedRefresh(int.Parse(resp["expires_in"]) * 1000 / 2);
    }

    private string fmtTime(int ms)
    {
        if (double.IsNaN(ms))
            return ms.ToString();

        var sign = ms < 0 ? "-" : "";
        ms = Math.Abs(ms);

        if (ms > 60000)
            return $"{sign}{ms / 60000}m, {Math.Round(ms % 60000 / 1000.0)}s";
        else if (ms > 1000)
            return $"{sign}{(ms % 60000 / 1000.0):F1}s";
        else
            return $"{sign}{Math.Round((double)ms)}ms";
    }
}
