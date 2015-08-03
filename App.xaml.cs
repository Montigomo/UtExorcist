using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Resources;

namespace utexorcist
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static public readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
		static public readonly string AppTitle = "UtExorcist  - " + Version;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
		}
	}
}
