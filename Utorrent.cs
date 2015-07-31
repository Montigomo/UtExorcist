using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace utexorcist
{
	public class UtItem
	{
		public string Name { get; set; }

		public bool Value { get; set; }
	}

	public class Utorrent
	{
		#region Values

		public static ObservableCollection<UtItem> Values = new ObservableCollection<UtItem>
		{
			new UtItem() {Name = "gui.pro_installed", Value = true },
			new UtItem() {Name = "offers.left_rail_offer_enabled",  Value = false },
			new UtItem() {Name = "offers.sponsored_torrent_offer_enabled",  Value = false },
			new UtItem() {Name = "show_bundles_tab", Value =  false },
			new UtItem() {Name = "offers.featured_content_badge_enabled",  Value = false },
			new UtItem() {Name = "gui.show_plus_upsell_nodes",  Value = false },
			new UtItem() {Name = "gui.show_gate_notify",  Value = false },
			new UtItem() {Name = "gui.show_plus_upsell",  Value = false },
			new UtItem() {Name = "gui.show_plus_av_upsell",  Value = false },
			new UtItem() {Name = "offers.content_offer_autoexec",  Value = false },
			new UtItem() {Name = "offers.featured_content_notifications_enabled",  Value = false },
			new UtItem() {Name = "offers.featured_content_rss_enabled",  Value = false }
		};

		#endregion

		private static Uri _uri = new Uri("http://localhost");
		private static int _port = 10000;

		private static string _sessionKey;
		private static string _authToken;

		private static string _callback = "Callback";
		private static string _appName = "UtExorsist";
		private static string _hostName = "google.com";

		public static void Connect()
		{
			var ser = new JavaScriptSerializer();

			// get version
			string hr = String.Empty;
			try
			{
				hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Version)).Get();
			}
			catch
			{
				return;
			}

			var objVersion = ser.Deserialize<UtVersion>(hr);

			hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Pair,
				new Dictionary<string, string> {
					{ "name", _appName },
					{ "callback", _callback }
				})).Get();

			hr = RemoveCallback(hr);

			var pkObject = ser.Deserialize<UtPairing>(hr);

			_authToken = pkObject.pairing_key;

			hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api, new Dictionary<string, string> {
				{ "pairing", _authToken },
				{ "type", "state" },
				{ "queries", "[[\"btapp\"]]" },
				{ "hostname",  _hostName },
				//{ "callback", _callback }
			})).Get();

			var sessionObject = ser.Deserialize<UtSession>(hr);
			_sessionKey = sessionObject.session;

			hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api, new Dictionary<string, string> {
				{ "pairing", _authToken },
				{ "session", _sessionKey },
				{ "type", "update" },
				{ "hostname",  _hostName },
				//{ "callback", _callback }
			})).Get();


			foreach (var item in Values)
			{
				hr = HttpExtensions.CreateCommonRequest(UriBuilder(Endpoin.Api,
					new Dictionary<string, string> {
						{ "pairing", _authToken },
						{ "session", _sessionKey },
						{ "type", "function" },
						{ "path", "[\"btapp\",\"settings\",\"set\"]"},
						{ "args", "[\"" + item.Name + "\"," + item.Value.ToString().ToLower() + "]"},
						{ "hostname",  _hostName },
						//{ "callback", _callback }
				})).Get();
			}
		}

		public enum Endpoin
		{
			Pair,
			Version,
			Ping,
			Api
		}

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

		#region Helper classes

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

		#endregion

	}
}

