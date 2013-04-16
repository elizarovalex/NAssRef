using System.Windows;
using System.Windows.Input;
using AssRef.ViewModels;

namespace AssRef.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnDirectoryPathKeyDown(object sender, KeyEventArgs e)
		{
			if (Key.Return == e.Key)
			{
				var vm = (MainViewModel)DataContext;
				vm.ChangeDirectoryPathCommand.Execute(null);
			}
		}
	}
}
