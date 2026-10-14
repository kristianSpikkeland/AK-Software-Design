namespace AK2.Components.Shared
{
    public class GameNightState
    {
        public int Count { get; private set; }

        // The bell
        public event Action? OnChange;
        
        public void Add()
        {
            Count++;

            // Ring the bell
            OnChange?.Invoke();
        }
    }
}
