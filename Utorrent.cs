using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace utexorcist
{
	internal class Utorrent
	{

		#region Values

		public static ObservableCollection<UteItem> Values
		{
			get
			{
				return UteSettings.Instance.Values;
			}
		}

		public static ObservableCollection<UteItem> InitValues = new ObservableCollection<UteItem>
		{
			//new UteItem() {Name = "gui.pro_installed", Value = true },
			new UteItem() {Name = "offers.left_rail_offer_enabled",  Value = false },
			new UteItem() {Name = "offers.sponsored_torrent_offer_enabled",  Value = false },
			new UteItem() {Name = "show_bundles_tab", Value =  false },
			new UteItem() {Name = "offers.featured_content_badge_enabled",  Value = false },
			new UteItem() {Name = "gui.show_plus_upsell_nodes",  Value = false },
			new UteItem() {Name = "gui.show_gate_notify",  Value = false },
			new UteItem() {Name = "gui.show_plus_upsell",  Value = false },
			new UteItem() {Name = "gui.show_plus_av_upsell",  Value = false },
			new UteItem() {Name = "offers.content_offer_autoexec",  Value = false },
			new UteItem() {Name = "offers.featured_content_notifications_enabled",  Value = false },
			new UteItem() {Name = "offers.featured_content_rss_enabled",  Value = false }
		};

		#endregion

		private static Uri _uri = new Uri("http://localhost");
		private static int _port = 10000;
		//private static string _callback = "Callback";
		private static string _appName = "UtExorsist";
		private static string _hostName = "google.com";

		internal static UtVersion Version { get; set; }
		internal static bool IsConnected { get; set; }

		static Utorrent()
		{
			Initialize();
		}

		internal static void Initialize()
		{
			try
			{
				Version = GetVersion();
				IsConnected = true;
			}
			catch (System.Net.WebException e)
			{
				//"No connection could be made because the target machine actively refused it 127.0.0.1:10000"
				var _msg = "Unable to connect to the remote server";
				if (e.Message.Contains(_msg))
					IsConnected = false;
			}
		}
		internal static Task InitializeTask()
		{
			return Task.Run(() =>
			{
				Initialize();
			});
		}

		internal static UtVersion GetVersion()
		{
			var ser = new JavaScriptSerializer();
			string hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Version)).Get();
			return ser.Deserialize<UtVersion>(hr);
		}

		private static UteConnect Connect()
		{
			UteConnect _session = new UteConnect();
			var ser = new JavaScriptSerializer();
			string hr = String.Empty;

			hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Pair,
				new Dictionary<string, string> {
					{ "name", _appName },
					//{ "callback", _callback }
				})).Get();

			//hr = RemoveCallback(hr);
			//var pkObject = ser.Deserialize<UtPairing>(hr);

			_session.AuthToken = hr; // pkObject.pairing_key;

			hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api, new Dictionary<string, string> {
				{ "pairing", _session.AuthToken },
				{ "type", "state" },
				{ "queries", "[[\"btapp\"]]" },
				{ "hostname",  _hostName },
				//{ "callback", _callback }
			})).Get();

			var sessionObject = ser.Deserialize<UtSession>(hr);
			_session.SessionKey = sessionObject.session;

			hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api, new Dictionary<string, string> {
				{ "pairing", _session.AuthToken },
				{ "session", _session.SessionKey },
				{ "type", "update" },
				{ "hostname",  _hostName },
				//{ "callback", _callback }
			})).Get();

			return _session;
		}

		internal static Task WriteValues()
		{
			return Task.Run(() =>
			{
				string hr = String.Empty;
				UteConnect _connect = Connect();
				foreach (var item in Values)
				{
					hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api,
						new Dictionary<string, string> {
						{ "pairing", _connect.AuthToken },
						{ "session", _connect.SessionKey },
						{ "type", "function" },
						{ "path", "[\"btapp\",\"settings\",\"set\"]"},
						{ "args", "[\"" + item.Name + "\"," + item.Value.ToString().ToLower() + "]"},
						{ "hostname",  _hostName },
							//{ "callback", _callback }
						})).Get();
				}
			});
		}

		internal static Task ReadValues()
		{
			return Task.Run(() =>
			{
				string hr = String.Empty;
				UteConnect _connect = Connect();
				var ser = new JavaScriptSerializer();

				foreach (var item in Values)
				{
					hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api,
						new Dictionary<string, string> {
						{ "pairing", _connect.AuthToken },
						{ "session", _connect.SessionKey },
						{ "type", "function" },
						{ "path", "[\"btapp\",\"settings\",\"get\"]"},
						{ "args", "[\"" + item.Name + "\"," + item.Value.ToString().ToLower() + "]"},
						{ "hostname",  _hostName },
							//{ "callback", _callback }
						})).Get();
					var btvalue = ser.Deserialize<UtResponseReadValue>(hr);
					var value = false;
					if (bool.TryParse(btvalue.btapp.settings.get, out value))
					{
						Console.WriteLine(item.Name + "  " + value);
						item.Value = value;
					}
				}
			});
		}

		#region  Helper methods

		public static Dictionary<Endpoin, string> Endpoints = new Dictionary<Endpoin, string>{
						{ Endpoin.Pair, "http://localhost:%s/gui/pair"},
						{ Endpoin.Version, "http://localhost:%s/version/" },
						{ Endpoin.Ping, "http://localhost:%s/gui/pingimg" },
						{ Endpoin.Api, "http://localhost:%s/btapp/" }
				};

		private static Uri UriBuilder(Endpoin epoint, Dictionary<string, string> values = null)
		{
			epoint = Enum.IsDefined(typeof(Endpoin), epoint) ? epoint : Endpoin.Version;
			var suri = Endpoints[epoint];
			suri = Regex.Replace(suri, @"\%s", _port.ToString());
			if (values != null)
				suri += "?" + string.Join("&", values.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value)).ToArray());
			return new Uri(suri);
		}

		private static string RemoveCallback(string st)
		{
			string _result = st;
			Regex _rx = new Regex(@"\(([^)]*)\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
			if (_rx.IsMatch(st))
				_result = Regex.Match(st, @"\(([^)]*)\)").Groups[1].Value;
			return _result;
		}

		#endregion

		#region Helper classes

		public enum Endpoin
		{
			Pair,
			Version,
			Ping,
			Api
		}
		#pragma warning disable 0649
		// { "name": "uTorrent", "version": { "device_id": "client", "engine_version": 40760, "features": 
		// { "device_pairing": { "jsonp": 1, "supported_types": [ 0, 1, 2 ] }, "remote": 1, "settings_set": 1 }, 
		// "major_version": 3, "minor_version": 4, "name": "uTorrent", "peer_id": "UT3430", "product_code": "PRODUCT_CODE", "tiny_version": 3,
		// "ui_version": 40760, "user_agent": "uTorrent\/3430(40760)(USER_AGENT_PRODUCT_CODE)", "version_date": "2012-12-04 23:04:37 -0800" } } 
		internal class UtVersion
		{
			public string name;
			public UtVersionVersion Version;

			public class UtVersionVersion
			{
				public int major_version;
				public int minor_version;
				public string name;
				public string peer_id;
				public string product_code;
				public int tiny_version;
				public int ui_version;
				public string user_agent;
				public string version_date;
			}
		}
		internal class UtPairing
		{
			public string pairing_key;
			public int pairing_type;
			public int code;
		}

		internal class UtSession
		{
			public string session;
		}

		//{ 
		// "btapp":
		// { 
		//   "settings":
		//   {
		//    "get": true
		//   }
		//  },
		// "session": "9124"
		//}

		internal class UtResponseReadValue
		{
			public UtResponseSettings btapp;
			public string session;
		}

		internal class UtResponseSettings
		{
			public UtResponseSettingsValuePair settings;
		}

		internal class UtResponseSettingsValuePair
		{
			public string get;
		}

		#pragma warning restore 0649
		#endregion

	}

	#region Classes

	public class UteItem : INotifyPropertyChanged
	{
		private string _name;
		[XmlAttribute("name")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool _value;
		[XmlAttribute("value")]
		public bool Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					_value = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool _isValid;
		[XmlIgnore]
		public bool IsValid
		{
			get
			{
				return _isValid;
			}
			set
			{
				if (_isValid != value)
				{
					_isValid = value;
					NotifyPropertyChanged();
				}
			}
		}

		#region PropertyChanges

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}

	internal class UteConnect
	{
		public string AuthToken { get; set; }
		public string SessionKey { get; set; }
	}

	#endregion

}

