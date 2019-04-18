/* Syntax highlighter - by Uriel Guy
 * Original version 2005
 * This version 2019 - Jason James Newland
 */
using System;
using System.Runtime.InteropServices;

namespace ircScript.Controls.SyntaxHightlight.Helpers
{
	public class Win32
	{
		public const int WmUser = 0x400;
		public const int WmPaint = 0xF;
		public const int WmKeydown = 0x100;
		public const int WmKeyup = 0x101;
		public const int WmChar = 0x102;

	    public const int EmGetscrollpos = (WmUser + 221);
	    public const int EmSetscrollpos = (WmUser + 222);

		public const int VkControl = 0x11;
		public const int VkUp = 0x26;
		public const int VkDown = 0x28;
		public const int VkNumlock = 0x90;

		public const short KsOn = 0x01;
		public const short KsKeydown = 0x80;

		[StructLayout(LayoutKind.Sequential)]
		public struct Point 
		{
			public int x;
			public int y;
		}

		[DllImport("user32")] public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);
		[DllImport("user32")] public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
		[DllImport("user32")] public static extern short GetKeyState(int nVirtKey);
		[DllImport("user32")] public static extern int LockWindowUpdate(IntPtr hwnd);
	}
}
