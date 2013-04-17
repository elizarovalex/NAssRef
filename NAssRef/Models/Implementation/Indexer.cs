using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AssRef.Models.Implementation
{
	public class Indexer
	{
		public void CreateIndex(AssRefItem[] items, string path, out Dictionary<string, HashSet<string>> errorIndex, out string basePath)
		{
			errorIndex = new Dictionary<string, HashSet<string>>();
			basePath = null;
			if ((null != items) && (0 < items.Length))
			{
				basePath = path;
				foreach (var item in items)
				{
					while (!string.IsNullOrEmpty(basePath))
					{
						if (!item.FileName.StartsWith(basePath))
						{
							basePath = Path.GetDirectoryName(basePath);
						}
						else
						{
							break;
						}
					}
					HashSet<string> ver;
					if (!errorIndex.TryGetValue(item.AssemblyName, out ver))
					{
						ver = new HashSet<string>();
						errorIndex.Add(item.AssemblyName, ver);
					}
					ver.Add(item.AssemblyVersion);
				}

				if (!string.IsNullOrWhiteSpace(basePath) && !basePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
				{
					basePath += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
				}
			}
		}
	}
}