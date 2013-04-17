using System.Runtime.InteropServices;
using System.Security;

namespace AssRef.Models.Implementation
{
	[SuppressUnmanagedCodeSecurity]
	public static class ConsoleManager
	{
		private const string Kernel32DllName = "kernel32.dll";

		[DllImport(Kernel32DllName, SetLastError = true)]
		private static extern bool AttachConsole(uint dwProcessId);

		private const uint AttachParentProcess = 0x0ffffffff;  // default value if not specifing a process ID

		public static void AttachParentConsole()
		{
			AttachConsole(AttachParentProcess);
		}
	}
}