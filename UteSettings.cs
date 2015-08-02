using System;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace utexorcist
{
	public class UteSettings
	{

		#region Inctance

		private static bool _subcribeOnExit = true;

		private static XmlSerializer _serializer;

		public static readonly string ExePath = Assembly.GetExecutingAssembly().Location;

		private static string _xmlFileName = @"UteSettings.xml";

		private static string _xmlFilePath = System.IO.Path.Combine((System.IO.Path.GetDirectoryName(ExePath)), _xmlFileName);

		private static readonly Lazy<UteSettings> _instance = new Lazy<UteSettings>(Load);

		private UteSettings()
		{
			if (_subcribeOnExit)
			{
				Application.Current.Exit += App_Exit;
				_subcribeOnExit = false;
			}
		}

		internal static UteSettings Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public string FilePath
		{
			get
			{
				return _xmlFilePath;
			}
			set
			{
				_xmlFilePath = value;
			}
		}

		private static UteSettings Load()
		{
			UteSettings sws = new UteSettings();
			try
			{
				using (FileStream fs = new FileStream(_xmlFilePath, FileMode.Open))
					sws = (UteSettings)Serializer.Deserialize(fs);
			}
			catch { }
			return sws;
		}

		private static XmlSerializer Serializer
		{
			get
			{
				if (_serializer == null)
					_serializer = new XmlSerializer(typeof(UteSettings));
				return _serializer;
			}
		}

		public void Save()
		{
			using (TextWriter twriter = new StreamWriter(_xmlFilePath))
			{
				Serializer.Serialize(twriter, UteSettings.Instance);
				twriter.Close();
			}
		}

		private static void App_Exit(object sender, ExitEventArgs e)
		{
			if (_instance != null)
				UteSettings.Instance.Save();
		}

		#endregion

		//#region PropertyChanges

		//public event PropertyChangedEventHandler PropertyChanged;

		//private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		//{
		//	if (PropertyChanged != null)
		//	{
		//		PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		//	}
		//}

		//#endregion

		#region Properties

		public ObservableCollection<UteItem> Values = new ObservableCollection<UteItem>();

		#endregion
	}
}
