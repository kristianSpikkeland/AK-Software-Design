using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.ComponentModel.DataAnnotations;

namespace AK2.Models
{
    public class BoardGame
    {
        public int Id { get; set; }
        public DateTime? StartedAt { get; set; }
        public string Name { get; set; } = "";
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string Category { get; set; } = "";

        [Range(1, 6, ErrorMessage = "Terningkastet må være fra 1 til 6")]
        public int Rating { get; set; } // dice roll, 1-6
        public int RatingCounter { get; set; }
        public string Description { get; set; } = "";

        public BoardGame(string name, int minPlayers, int maxPlayers, string category)
        {
            Name = name;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            Category = category;

        }
    }
}
