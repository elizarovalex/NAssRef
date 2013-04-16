using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AssRef.Models;
using AssRef.Models.Implementation;

namespace AssRef.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ObservableCollection<AssRefItemViewModel> _assRefList;
		private string _directoryPath;
		private readonly IFileSource _fileSource;
		private AssRefItem[] _assRefModels;
		private string _filterText;
		private SimpleDelayer _refreshDelay;
		private Dispatcher _dispatcher;
		private Dictionary<string, HashSet<string>> _errorIndex = new Dictionary<string, HashSet<string>>();
		private GroupTypes _groupType;
		private ObservableCollection<string> _directoryHistory = new ObservableCollection<string>();

		public MainViewModel()
		{
			_dispatcher = Application.Current.Dispatcher;

			_fileSource = new FileSource();
			_refreshDelay = new SimpleDelayer();
			_refreshDelay.Execute += OnRefreshDelayExecute;
			ChangeDirectoryPathCommand = new DelegateCommand(ChangeDirectoryPathCommandHandler);

			LoadDirectoryHistory();
		}

		private void OnRefreshDelayExecute()
		{
			_dispatcher.Invoke(new Action(RefreshList));
		}

		private void ChangeDirectoryPathCommandHandler(object obj)
		{
			_assRefModels = _fileSource.GetAssRefList(DirectoryPath);

			RefreshErrorIndex();

			RefreshList();

			SaveDirectoryHistory();
		}

		private void LoadDirectoryHistory()
		{
			var fn = GetHistoryFileName();
			if (File.Exists(fn))
			{
				var strs = new HashSet<string>();
				using (var stream = File.OpenRead(fn))
				using (var tr = new StreamReader(stream))
				{
					while (true)
					{
						var line = tr.ReadLine();
						if (string.IsNullOrWhiteSpace(line))
						{
							break;
						}
						strs.Add(line.Trim());
					}
				}
				DirectoryHistory = new ObservableCollection<string>(strs);
			}
			else
			{
				DirectoryHistory = new ObservableCollection<string>();
			}
		}

		private static string GetHistoryFileName()
		{
			var fn = Path.GetFileName(Assembly.GetEntryAssembly().Location);
			var dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			fn = Path.Combine(dir, fn + ".history.txt");
			return fn;
		}

		private void SaveDirectoryHistory()
		{
			var strs = new HashSet<string>(DirectoryHistory) { DirectoryPath };

			var fn = GetHistoryFileName();
			using (var stream = File.OpenWrite(fn))
			using (var tr = new StreamWriter(stream))
			{
				foreach (var str in strs)
				{
					tr.WriteLine(str);
				}
			}
			DirectoryHistory = new ObservableCollection<string>(strs);
		}

		private void RefreshErrorIndex()
		{
			var errorIndex = new Dictionary<string, HashSet<string>>();
			if ((null != _assRefModels) && (0 < _assRefModels.Length))
			{
				foreach (var item in _assRefModels)
				{
					HashSet<string> ver;
					if (!errorIndex.TryGetValue(item.AssemblyName, out ver))
					{
						ver = new HashSet<string>();
						errorIndex.Add(item.AssemblyName, ver);
					}
					ver.Add(item.AssemblyVersion);
				}
			}
			_errorIndex = errorIndex;
		}

		private void RefreshList()
		{
			if ((null == _assRefModels) || (0 == _assRefModels.Length))
			{
				AssRefList = null;
				return;
			}
			var filter1 = string.IsNullOrWhiteSpace(FilterText)
				? (Func<AssRefItem, string, bool>)((m, fs) => true)
				: (Func<AssRefItem, string, bool>)((m, fs) => m.AssemblyName.StartsWith(fs));
			var filter2 = GroupTypes.FileName == GroupType
				? (Func<AssRefItem, bool>)(m => true)
				: (Func<AssRefItem, bool>)(m => 1 < _errorIndex[m.AssemblyName].Count);
			var fStr = FilterText;
			AssRefList = new ObservableCollection<AssRefItemViewModel>(
				_assRefModels.Where(m => filter1(m, fStr) && filter2(m)).Select(m => new AssRefItemViewModel
				{
					FileName = m.FileName,
					AssemblyName = m.AssemblyName,
					AssemblyVersion = m.AssemblyVersion,
					AssemblyPublicKey = m.AssemblyPublicKey,
					IsErrorVersion = 1 < _errorIndex[m.AssemblyName].Count,
					GroupType = GroupType
				}));
		}

		public string FilterText
		{
			get { return _filterText; }
			set
			{
				if (value == _filterText)
				{
					return;
				}
				_filterText = value;
				OnPropertyChanged(() => FilterText);
				_refreshDelay.SetTime();
			}
		}

		public GroupTypes GroupType
		{
			get { return _groupType; }
			set
			{
				if (value == _groupType)
				{
					return;
				}
				_groupType = value;
				OnPropertyChanged(() => GroupType);
				RefreshList();
			}
		}

		public ObservableCollection<AssRefItemViewModel> AssRefList
		{
			get { return _assRefList; }
			set
			{
				if (value == _assRefList)
				{
					return;
				}
				_assRefList = value;
				OnPropertyChanged(() => AssRefList);
			}
		}

		public string DirectoryPath
		{
			get { return _directoryPath; }
			set
			{
				if (value == _directoryPath)
				{
					return;
				}
				_directoryPath = value;
				OnPropertyChanged(() => DirectoryPath);
			}
		}

		public ObservableCollection<string> DirectoryHistory
		{
			get { return _directoryHistory; }
			set
			{
				if (value == _directoryHistory)
				{
					return;
				}
				_directoryHistory = value;
				OnPropertyChanged(() => DirectoryHistory);
			}
		}

		public ICommand ChangeDirectoryPathCommand { get; private set; }

		private class SimpleDelayer
		{
			private Thread _thread;
			private DateTime _time;
			private bool _isSet;

			public SimpleDelayer()
			{
				_thread = new Thread(CheckTime) { IsBackground = true, Name = "SimpleDelayer" };
				_thread.Start();
			}

			public void SetTime()
			{
				_time = DateTime.Now.AddMilliseconds(500);
				_isSet = true;
			}

			private void CheckTime()
			{
				while (true)
				{
					if (_isSet)
					{
						if (DateTime.Now >= _time)
						{
							_isSet = false;
							OnExecute();
						}
					}
					Thread.Sleep(500);
				}
			}

			private void OnExecute()
			{
				var handler = Execute;
				if (null != handler)
				{
					handler();
				}
			}

			public event Action Execute;
		}
	}
}