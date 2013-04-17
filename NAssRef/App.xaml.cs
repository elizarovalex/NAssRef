using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using AssRef.Models.Implementation;
using AssRef.Views;

namespace AssRef
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (0 < e.Args.Length)
			{
				ConsoleManager.AttachParentConsole();
				var isConflict = new ConflictDetecter().Detect(e.Args);
				Shutdown(isConflict ? 1 : 0);
				return;
			}

			ShutdownMode = ShutdownMode.OnMainWindowClose;
			MainWindow = new MainWindow();
			MainWindow.Show();
		}
	}
}
