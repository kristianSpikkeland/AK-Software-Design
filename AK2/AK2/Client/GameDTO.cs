namespace AK2.Client
{
    public sealed record GameDTO(
        int Id,
        string Name,
        string Category,
        int MinPlayers,
        int MaxPlayers,
        int Rating,
        string Description
        );

    
}
