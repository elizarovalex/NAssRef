using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssRef.Models.Implementation
{
	public class ConflictDetecter
	{
		private Dictionary<string, string> _args = new Dictionary<string, string>
			{
				{ "console", null },
				{ "path", null },
				{ "filter", null },
				{ "subdir", null }
			};

		public bool Detect(string[] args)
		{
			ParseArgs(args);

			var showError = "1" == _args["console"];
			var isSubdir = "1" == _args["subdir"];
			var directoryPath = _args["path"];
			var filter = _args["filter"];

			var fileSource = new FileSource();
			var assList = fileSource.GetAssRefList(directoryPath, isSubdir);

			Dictionary<string, HashSet<string>> errorIndex;
			string basePath;
			new Indexer().CreateIndex(assList, directoryPath, out errorIndex, out basePath);
			var basePathCount = (basePath ?? "").Trim().Length;

			var regex = string.IsNullOrWhiteSpace(filter)
				? null
				: new Regex(filter);

			var conflictItems = assList.Where(item =>
				(1 < errorIndex[item.AssemblyName].Count)
				&& ((null == regex) || (regex.IsMatch(item.AssemblyName)))
				).ToList();

			var res = 0 < conflictItems.Count;
			if (res && showError)
			{
				Console.WriteLine();
				foreach (var g in conflictItems.GroupBy(i => i.AssemblyName))
				{
					Console.WriteLine("* " + g.Key);
					foreach (var item in g.OrderBy(i => i.AssemblyVersion))
					{
						Console.WriteLine("{0} -> {1}"
							, item.FileName.Substring(basePathCount)
							, item.AssemblyVersion);
					}
					Console.WriteLine();
				}

			}
			return res;
		}

		private void ParseArgs(string[] args)
		{
			string nowKey = null;
			foreach (var s in args)
			{
				bool foundKey = false;
				foreach (var key in _args.Keys)
				{
					if (s.StartsWith(key + "="))
					{
						nowKey = key;
						_args[nowKey] = s.Substring(nowKey.Length + 1);
						foundKey = true;
						break;
					}
				}
				if (!foundKey && (null != nowKey))
				{
					_args[nowKey] += " " + s;
				}
			}
		}
	}
}