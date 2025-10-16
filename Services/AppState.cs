namespace MDConverter.Services;

public class AppState
{
    public event Action? OnAboutModalRequested;

    public void ShowAboutModal()
    {
        OnAboutModalRequested?.Invoke();
    }
}
