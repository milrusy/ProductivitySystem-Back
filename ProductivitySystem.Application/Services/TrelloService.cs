using Microsoft.Extensions.Configuration;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using System.Net.Http.Json;

namespace ProductivitySystem.Application.Services;

public class TrelloService : ITrelloService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public TrelloService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<List<TrelloCardDto>> GetCardsAsync()
    {
        var boardId = _config["Trello:BoardId"];
        var key = _config["Trello:ApiKey"];
        var token = _config["Trello:Token"];

        var url =
            $"https://api.trello.com/1/boards/{boardId}/cards" +
            $"?members=true" +
            $"&member_fields=fullName,username" +
            $"&fields=id,name,idMembers,due,idList" +
            $"&key={key}&token={token}";

        var cards = await _http.GetFromJsonAsync<List<TrelloCardDto>>(url);

        return cards ?? new();
    }
}
