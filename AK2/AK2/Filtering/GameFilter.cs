using AK2.Models;

namespace AK2.Filtering
{
    public static class GameFilter
    {
        public static IEnumerable<BoardGame> FilterGames(string nameFilter, IEnumerable<BoardGame> boardGames)
        {
            if (string.IsNullOrWhiteSpace(nameFilter))
                return boardGames;

            var filter = nameFilter.Trim();

            if (int.TryParse(filter, out int players))
            {
                return boardGames.Where(g =>
                    g.MinPlayers <= players && players <= g.MaxPlayers);
            }

            return boardGames.Where(g =>
                g.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
        }

        public static BoardGame? GetRandomGame(IReadOnlyList<BoardGame> boardGames)
        {
            if (boardGames.Count == 0)
                return null;
            int index = Random.Shared.Next(boardGames.Count);
            return boardGames[index];
        }
    }
}
