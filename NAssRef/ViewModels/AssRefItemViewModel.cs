namespace AssRef.ViewModels
{
	public class AssRefItemViewModel : ViewModelBase
	{
		private string _fileName;
		private string _assemblyName;
		private string _assemblyVersion;
		private string _assemblyPublicKey;
		private string _groupName;
		private bool _isErrorVersion;

		public string FileName
		{
			get { return _fileName; }
			set
			{
				if (value == _fileName)
				{
					return;
				}
				_fileName = value;
				OnPropertyChanged(() => FileName);
			}
		}

		public string AssemblyName
		{
			get { return _assemblyName; }
			set
			{
				if (value == _assemblyName)
				{
					return;
				}
				_assemblyName = value;
				OnPropertyChanged(() => AssemblyName);
			}
		}

		public string AssemblyVersion
		{
			get { return _assemblyVersion; }
			set
			{
				if (value == _assemblyVersion)
				{
					return;
				}
				_assemblyVersion = value;
				OnPropertyChanged(() => AssemblyVersion);
			}
		}

		public string AssemblyPublicKey
		{
			get { return _assemblyPublicKey; }
			set
			{
				if (value == _assemblyPublicKey)
				{
					return;
				}
				_assemblyPublicKey = value;
				OnPropertyChanged(() => AssemblyPublicKey);
			}
		}

		public string GroupName
		{
			get
			{
				return GroupTypes.FileName == GroupType
					? FileName
					: ((GroupTypes.AssemblyError == GroupType) && IsErrorVersion ? AssemblyName : "");
			}
		}

		public bool IsErrorVersion
		{
			get { return _isErrorVersion; }
			set
			{
				if (value == _isErrorVersion)
				{
					return;
				}
				_isErrorVersion = value;
				OnPropertyChanged(() => IsErrorVersion);
			}
		}

		public GroupTypes GroupType { get; set; }
	}
}