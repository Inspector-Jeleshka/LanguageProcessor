namespace GUI.Services;

public class WindowService : IWindowService
{
	private AboutWindow? _openedAboutWindow;

	public void ShowAboutWindow()
	{
		if (_openedAboutWindow is null)
		{
			_openedAboutWindow = new AboutWindow();
			_openedAboutWindow.Closed += (_, _) => _openedAboutWindow = null;
			_openedAboutWindow.Show();
		}
		else
			_ = _openedAboutWindow.Focus();
	}
}
