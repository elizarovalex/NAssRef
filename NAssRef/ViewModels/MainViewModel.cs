using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
		private readonly SimpleDelayer _refreshDelay;
		private readonly Dispatcher _dispatcher;
		private Dictionary<string, HashSet<string>> _errorIndex = new Dictionary<string, HashSet<string>>();
		private GroupTypes _groupType;
		private ObservableCollection<string> _directoryHistory = new ObservableCollection<string>();

		private readonly List<HistoryFilterItem> _historyFilterList = new List<HistoryFilterItem>();
		private int _historyFilterIndex;
		private int _historyFilterCount;
		private string _title;
		private int _basePathCount;
		private bool _isUseSubdir;

		public MainViewModel()
		{
			_dispatcher = Application.Current.Dispatcher;

			_fileSource = new FileSource();
			_refreshDelay = new SimpleDelayer();
			_refreshDelay.Execute += OnRefreshDelayExecute;

			ChangeTitle();
			ResetHistoryFilter();

			ChangeDirectoryPathCommand = new DelegateCommand(ChangeDirectoryPathCommandHandler);
			BackwardHistoryFilterCommand = new DelegateCommand(BackwardHistoryFilterCommandHandler, CanBackwardHistoryFilterCommandHandler);
			ForwardHistoryFilterCommand = new DelegateCommand(ForwardHistoryFilterCommandHandler, CanForwardHistoryFilterCommandHandler);

			LoadDirectoryHistory();
		}

		private void ResetHistoryFilter()
		{
			_historyFilterIndex = -1;
			_historyFilterCount = 0;
		}

		private bool CanForwardHistoryFilterCommandHandler(object obj)
		{
			return _historyFilterIndex < _historyFilterCount - 1;
		}

		private void ForwardHistoryFilterCommandHandler(object obj)
		{
			_historyFilterIndex++;
			ApplyCurrentHistoryFilterItem();
		}

		private bool CanBackwardHistoryFilterCommandHandler(object obj)
		{
			return _historyFilterIndex > 0;
		}

		private void BackwardHistoryFilterCommandHandler(object obj)
		{
			_historyFilterIndex--;
			ApplyCurrentHistoryFilterItem();
		}

		private void ApplyCurrentHistoryFilterItem()
		{
			var filterItem = _historyFilterList[_historyFilterIndex];

			_filterText = filterItem.Filter;
			OnPropertyChanged(() => FilterText);

			_groupType = filterItem.GroupType;
			OnPropertyChanged(() => GroupType);

			RefreshList(false);
		}

		private void OnRefreshDelayExecute()
		{
			_dispatcher.Invoke(new Action(() => RefreshList(true)));
		}

		private void ChangeDirectoryPathCommandHandler(object obj)
		{
			_assRefModels = _fileSource.GetAssRefList(DirectoryPath, IsUseSubdir);

			ResetHistoryFilter();

			RefreshIndex();

			RefreshList(true);

			SaveDirectoryHistory();

			ChangeTitle();
		}

		private void ChangeTitle()
		{
			Title = "NAssRef v0.1 - " + DirectoryPath;
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
						var s = line.Trim();
						if (Directory.Exists(s))
						{
							strs.Add(s);
						}
					}
				}
				if (5 < strs.Count)
				{
					strs = new HashSet<string>(strs.Skip(strs.Count - 5));
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

		private void RefreshIndex()
		{
			Dictionary<string, HashSet<string>> errorIndex;
			string basePath;
			new Indexer().CreateIndex(_assRefModels, DirectoryPath, out errorIndex, out basePath);

			_basePathCount = (basePath ?? "").Trim().Length;
			_errorIndex = errorIndex;
		}

		private void RefreshList(bool inHistory)
		{
			if ((null == _assRefModels) || (0 == _assRefModels.Length))
			{
				AssRefList = null;
				return;
			}
			Func<AssRefItem, string, bool> filter1;
			if (string.IsNullOrWhiteSpace(FilterText))
			{
				filter1 = (m, fs) => true;
			}
			else
			{
				Regex regex;
				try
				{
					regex = new Regex(FilterText);
				}
				catch
				{
					return;
				}
				filter1 = (m, fs) => regex.IsMatch(m.AssemblyName);
			}
			var filter2 = GroupTypes.FileName == GroupType
				? (Func<AssRefItem, bool>)(m => true)
				: (Func<AssRefItem, bool>)(m => 1 < _errorIndex[m.AssemblyName].Count);
			var fStr = FilterText;
			AssRefList = new ObservableCollection<AssRefItemViewModel>(
				_assRefModels.Where(m => filter1(m, fStr) && filter2(m)).Select(m => new AssRefItemViewModel
				{
					FileName = m.FileName.Substring(_basePathCount),
					AssemblyName = m.AssemblyName,
					AssemblyVersion = m.AssemblyVersion,
					AssemblyPublicKey = m.AssemblyPublicKey,
					IsErrorVersion = 1 < _errorIndex[m.AssemblyName].Count,
					GroupType = GroupType
				}));

			if (inHistory)
			{
				_historyFilterIndex++;
				var item = new HistoryFilterItem { Filter = FilterText, GroupType = GroupType };
				if (_historyFilterIndex >= _historyFilterList.Count)
				{
					_historyFilterList.Add(item);
				}
				else
				{
					_historyFilterList[_historyFilterIndex] = item;
				}
				_historyFilterCount = _historyFilterIndex + 1;
			}
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
				RefreshList(true);
			}
		}

		public string Title
		{
			get { return _title; }
			set
			{
				if (value == _title)
				{
					return;
				}
				_title = value;
				OnPropertyChanged(() => Title);
			}
		}

		public bool IsUseSubdir
		{
			get { return _isUseSubdir; }
			set
			{
				if (value == _isUseSubdir)
				{
					return;
				}
				_isUseSubdir = value;
				OnPropertyChanged(() => IsUseSubdir);
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
		public ICommand ForwardHistoryFilterCommand { get; private set; }
		public ICommand BackwardHistoryFilterCommand { get; private set; }

		private class SimpleDelayer
		{
			private readonly Thread _thread;
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

		private class HistoryFilterItem
		{
			public string Filter { get; set; }

			public GroupTypes GroupType { get; set; }
		}
	}
}