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
		private int _exitCode;

		[STAThread]
		public static int Main(string[] args)
		{
			if ((null == args) || (0 == args.Length))
			{
				ConsoleManager.Hide();
			}

			var app = new App();
			app.InitializeComponent();
			return app.Run();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (0 < e.Args.Length)
			{
				var isConflict = new ConflictDetecter().Detect(e.Args);
				_exitCode = isConflict ? 1 : 0;
				Shutdown(_exitCode);
				return;
			}

			ShutdownMode = ShutdownMode.OnMainWindowClose;
			MainWindow = new MainWindow();
			MainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			e.ApplicationExitCode = _exitCode;

			base.OnExit(e);
		}
	}
}
