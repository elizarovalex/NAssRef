using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssRef.Models.Implementation
{
	public class FileSource : MarshalByRefObject, IFileSource
	{
		public AssRefItem[] GetAssRefList(string directoryPath, bool isUseSubdir)
		{
			if (!Directory.Exists(directoryPath))
			{
				return new AssRefItem[0];
			}
			var searchOption = isUseSubdir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var files = Directory.GetFiles(directoryPath, "*.dll", searchOption).ToList();
			files.AddRange(Directory.GetFiles(directoryPath, "*.exe", searchOption));

			var localAppD = AppDomain.CreateDomain("ExploreAssemblies");
			var localInnerFs = (FileSource)localAppD.CreateInstanceAndUnwrap(typeof(FileSource).Assembly.FullName, typeof(FileSource).FullName);

			var getAppD = isUseSubdir
				? (Func<AppDomain>)(() => AppDomain.CreateDomain("ExploreAssemblies"))
				: (Func<AppDomain>)(() => localAppD ?? (localAppD = AppDomain.CreateDomain("ExploreAssemblies")));
			var getInnerFs = isUseSubdir
				? (Func<AppDomain, FileSource>)(d => (FileSource)d.CreateInstanceAndUnwrap(typeof(FileSource).Assembly.FullName, typeof(FileSource).FullName))
				: (Func<AppDomain, FileSource>)(d => localInnerFs ?? (localInnerFs = (FileSource)d.CreateInstanceAndUnwrap(typeof(FileSource).Assembly.FullName, typeof(FileSource).FullName)));
			var freeAppD = isUseSubdir
				? (Action<AppDomain>)(AppDomain.Unload)
				: (Action<AppDomain>)(d => { });

			var res = new List<AssRefItem>();

			foreach (var file in files)
			{
				var appD = getAppD();
				var innerFs = getInnerFs(appD);
				string[] refs;
				string[] vers;
				string[] keys;
				Exception ex;
				innerFs.GetRefs(file, out refs, out vers, out keys, out ex);
				res.AddRange(refs.Select((t, i) => new AssRefItem
					{
						FileName = file,
						AssemblyName = t,
						AssemblyVersion = vers[i],
						AssemblyPublicKey = keys[i]
					}));
				freeAppD(appD);
			}
			if (null != localAppD)
			{
				AppDomain.Unload(localAppD);
			}

			return res.ToArray();
		}

		private void GetRefs(string file, out string[] refs, out string[] vers, out string[] keys, out Exception exception)
		{
			var refList = new List<string>();
			var verList = new List<string>();
			var keyList = new List<string>();
			exception = null;
			try
			{
				var a = Assembly.ReflectionOnlyLoadFrom(file);
				var refA = a.GetReferencedAssemblies();
				foreach (var name in refA)
				{
					var chars = name.GetPublicKeyToken().Select(b => b.ToString("X").PadLeft(2, '0')).ToArray();
					var key = 0 == chars.Length ? "" : chars.Aggregate((s1, s2) => s1 + s2);
					refList.Add(name.Name);
					verList.Add(name.Version.ToString());
					keyList.Add(key);
				}
			}
			catch (Exception e)
			{
				exception = e;
			}
			refs = refList.ToArray();
			vers = verList.ToArray();
			keys = keyList.ToArray();
		}
	}
}