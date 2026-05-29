using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface ITrelloService
{
    Task<List<TrelloCardDto>> GetCardsAsync();
}
