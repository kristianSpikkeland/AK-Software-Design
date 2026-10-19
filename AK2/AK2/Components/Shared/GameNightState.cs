using AK2.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;

namespace AK2.Components.Shared
{
    public class GameNightState(ProtectedLocalStorage storage)
    {
        public int Count => games.Count;


        private readonly List<BoardGame> games = new();
        public IReadOnlyList<BoardGame> Games => games;
        public int TotalPlayTime => games.Sum(g => g.PlayTimeMinutes);


        // The bell
        public event Action? OnChange;

        public bool Contains(int gameId) => games.Any(g => g.Id == gameId);

        public async Task<bool> AddAsync(BoardGame game)
        {
            // Early return if BoradGame allready exists
            if (Contains(game.Id)) return false;

            games.Add(game);
            await SaveAsync();

            // Ring the bell
            OnChange?.Invoke();
            
            return true;            
        }

        public async Task RemoveAsync(int gameId)
        {
            games.RemoveAll(g => g.Id == gameId);
            await SaveAsync();
            OnChange?.Invoke();
        }

        // Resets the game night
        public async Task ClearAsync()
        {
            games.Clear();
            await SaveAsync();
            OnChange?.Invoke();
        }

        #region Browser storage (ProtectedLocalStorage)

        private const string StorageKey = "gameNight";
        private bool loaded;

        // Loads the game night form the browser once per circuit.
        // Called from MainLayout after the first render.

        public async Task LoadAsync()
        {
            if (loaded) return;
            loaded = true;

            try
            {
                var result = await storage.GetAsync<List<BoardGame>>(StorageKey);

                if (result.Success && result.Value is not null)
                {
                    games.Clear();
                    games.AddRange(result.Value);
                    OnChange?.Invoke();
                }
            }

            catch (CryptographicException)
            {
                // Stored data can no longer be decrypted, start fresh
                await storage.DeleteAsync(StorageKey);
            }
        }

        // Saves the current list, or removes the key when the list is empty
        private async Task SaveAsync()
        {
            if (games.Count == 0)
            {
                await storage.DeleteAsync(StorageKey);
            }

            else
            {
                await storage.SetAsync(StorageKey, games);
            }
        }

        #endregion


    }
}
