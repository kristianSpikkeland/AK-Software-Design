using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace AK2.Models
{
    public class BoardGame
    {
        private static int _nextId = 1;
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayTimeMinutes { get; set; }
        public string Category { get; set; } = "";
        public int Rating { get; set; } // dice roll, 1-6
        public string Description { get; set; } = "";

        public BoardGame(string name, int minPlayers, int maxPlayers, string category)
        {
            Id = _nextId++;
            Name = name;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            Category = category;

        }
    }
}
