using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityFloatingImage
{
	static class Util
	{
		[DllImport("user32.dll")]
		static extern bool GetWindowRect(IntPtr hwnd, ref RectData rectangle);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[StructLayout(LayoutKind.Sequential)]
		struct RectData
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		public static RectInt GetCurrentWindowRect()
		{
			var handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			RectData rect = new();
			if (!GetWindowRect(handle, ref rect))
			{
				throw new Exception("GetWindowRect failed");
			}
			return new RectInt(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}
	}

}