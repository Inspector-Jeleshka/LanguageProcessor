using System.Windows;
using GUI.ViewModels;

namespace GUI.Views;

public partial class MainWindow : Window
{
	private readonly MainViewModel _viewModel;

	public MainWindow(MainViewModel viewModel)
	{
		InitializeComponent();

		_viewModel = viewModel;
		DataContext = _viewModel;

		rtb.Document = _viewModel.Document;

		_viewModel.NavigateToPostionRequested += newCaretPosition =>
		{
			rtb.CaretPosition = newCaretPosition;
			rtb.Focus();
		};
	}

	protected override void OnClosed(EventArgs e)
	{
		base.OnClosed(e);
		Application.Current.Shutdown();
	}
}
