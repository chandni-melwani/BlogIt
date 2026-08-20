using System.Timers;
using Timer = System.Timers.Timer;

namespace BlogApp.Services;

public class SearchStateService : IDisposable
{
    private string _query = "";
    private readonly Timer _debounceTimer;

    // Fired when the debounce timer elapses
    public event Action<string>? OnSearchExecuted;

    // Fired immediately when the query changes (e.g. to show "Searching..." UI)
    public event Action<string>? OnQueryChanged;

    public string Query
    {
        get => _query;
        set
        {
            if (_query == value) return;

            _query = value;
            OnQueryChanged?.Invoke(_query);

            // Restart the timer
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }

    public SearchStateService()
    {
        _debounceTimer = new Timer(350);
        _debounceTimer.AutoReset = false; // Only fire once per keystroke pause
        _debounceTimer.Elapsed += (sender, args) =>
        {
            OnSearchExecuted?.Invoke(_query);
        };
    }

    public void Reset()
    {
        _debounceTimer.Stop();
        if (_query != "")
        {
            _query = "";
            OnQueryChanged?.Invoke(_query);
            OnSearchExecuted?.Invoke(_query);
        }
    }

    public void Dispose()
    {
        _debounceTimer.Dispose();
    }
}
