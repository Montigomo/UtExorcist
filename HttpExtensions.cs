using System;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace utexorcist
{
	public static class HttpExtensions
	{

		#region Enums
		public enum Method
		{
			GET,
			HEAD,
			POST,
			PUT,
			DELETE,
			TRACE,
			OPTIONS
		}

		#endregion

		#region UserAgent

		private static Random _uaRnd = new Random(~unchecked((int)DateTime.Now.Ticks));

		private static string[] _userAgents = {
			// ******* Internet Explorer *******
			@"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)",
			@"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)",
			@"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322)",
			@"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)",
			@"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)",
			@"Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.6.30 Version/10.61",
			// ******* Opera *******
			@"Mozilla/5.0 (Windows NT 5.1; U; en) Opera 8.50",
			@"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en) Opera 8.50",
			// ******* Chrome *******
			@"Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.215 Safari/534.10"
		};

		public static string UserAgentRandom
		{
			get
			{
				return _userAgents[_uaRnd.Next(0, _userAgents.Length)];
			}
		}

		#endregion

		#region Http verbs

		public static string Get(this HttpWebRequest request)
		{
			return request.GetResponseString(Method.GET);
		}

		public static string Post(this HttpWebRequest request, string requestParams = null)
		{
			return request.GetResponseString(Method.POST, requestParams);
		}

		#endregion

		#region GetResponseString  && Common Request

		public static HttpWebRequest CreateCommonRequest(Uri uri)
		{
			HttpWebRequest htc = (HttpWebRequest)HttpWebRequest.Create(uri);
			htc.KeepAlive = true;
			htc.Timeout = 60000;
			htc.PreAuthenticate = true;
			htc.KeepAlive = true;
			htc.AllowAutoRedirect = true;
			htc.ContentType = "application/x-www-form-urlencoded";
			htc.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
			htc.Accept = "text/html, application/xhtml+xml, */*";
			htc.Headers.Add(new NameValueCollection() {
								{ @"Accept-Encoding", @"gzip, deflate" },
								{ @"Accept-Language", @"en-US,en;q=0.7,ru;q=0.3" }});
			htc.Credentials = CredentialCache.DefaultCredentials;
			return htc;
		}


		public static string GetResponseString(this HttpWebRequest request, Method rt, string requestParams = null, Encoding encoding = null)
		{
			string responseData = string.Empty;
			request.Method = rt.ToString().Trim();

			HttpWebResponse RawResponse = null;

			if (rt == Method.POST && requestParams != null)
			{
				encoding = encoding ?? Encoding.Default;

				byte[] bytes = encoding.GetBytes(requestParams.Trim());
				request.ContentLength = bytes.Length;
				using (Stream outputStream = request.GetRequestStream())
					outputStream.Write(bytes, 0, bytes.Length);
			}

			RawResponse = (HttpWebResponse)request.GetResponse();

			encoding = Encoding.UTF8;// encoding ?? (String.IsNullOrWhiteSpace(RawResponse.CharacterSet) ? Encoding.Default : Encoding.GetEncoding(RawResponse.CharacterSet));

			if (RawResponse.ContentEncoding == @"gzip")
				using (GZipStream _stream = new GZipStream(RawResponse.GetResponseStream(), CompressionMode.Decompress))
				{
					using (StreamReader _sr = new StreamReader(_stream, encoding))
						responseData = _sr.ReadToEnd();
				}
			else
				using (StreamReader _sr = new StreamReader(RawResponse.GetResponseStream()))
				{
					responseData = _sr.ReadToEnd();
				}
			return responseData;
		}

		#endregion

	}
}
