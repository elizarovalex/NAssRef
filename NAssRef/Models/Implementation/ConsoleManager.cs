﻿using System;
using System.Diagnostics;
using System.IO;
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
		[DllImport(Kernel32DllName)]
		private static extern bool AllocConsole();

		[DllImport(Kernel32DllName)]
		private static extern bool FreeConsole();

		[DllImport(Kernel32DllName)]
		private static extern IntPtr GetConsoleWindow();

		public static bool HasConsole
		{
			get { return GetConsoleWindow() != IntPtr.Zero; }
		}

		/// <summary>
		/// Creates a new console instance if the process is not attached to a console already.
		/// </summary>
		public static void Show()
		{
			//#if DEBUG
			if (!HasConsole)
			{
				AllocConsole();
				InvalidateOutAndError();
			}
			//#endif
		}

		/// <summary>
		/// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
		/// </summary>
		public static void Hide()
		{
			//#if DEBUG
			if (HasConsole)
			{
				SetOutAndErrorNull();
				FreeConsole();
			}
			//#endif
		}

		public static void Toggle()
		{
			if (HasConsole)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}

		static void InvalidateOutAndError()
		{
			Type type = typeof(Console);

			System.Reflection.FieldInfo _out = type.GetField("_out",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			System.Reflection.FieldInfo error = type.GetField("_error",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			System.Reflection.MethodInfo initializeStdOutError = type.GetMethod("InitializeStdOutError",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			Debug.Assert(_out != null);
			Debug.Assert(error != null);

			Debug.Assert(initializeStdOutError != null);

			_out.SetValue(null, null);
			error.SetValue(null, null);

			initializeStdOutError.Invoke(null, new object[] { true });
		}

		static void SetOutAndErrorNull()
		{
			Console.SetOut(TextWriter.Null);
			Console.SetError(TextWriter.Null);
		}
	}
}