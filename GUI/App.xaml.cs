using GUI.Services;
using GUI.ViewModels;
using GUI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GUI;

public partial class App : Application
{
	private IServiceProvider? _serviceProvider;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		var services = new ServiceCollection();

		services.AddSingleton<IFileService, FileService>();
		services.AddSingleton<IFileDialogService, FileDialogService>();
		services.AddSingleton<IDocumentService, DocumentService>();
		services.AddSingleton<IWindowService, WindowService>();

		services.AddSingleton<MainViewModel>();

		services.AddTransient<MainWindow>();
		services.AddTransient<AboutWindow>();

		_serviceProvider = services.BuildServiceProvider();

		var mainWindow = _serviceProvider.GetService<MainWindow>();
		mainWindow?.Show();
	}
}
