
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MyPong
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			using (var window = new GameWindow()) {
				window.Show();
				while (window.Created) {
					Application.DoEvents();
				}
			}
		}
		
	}
}
