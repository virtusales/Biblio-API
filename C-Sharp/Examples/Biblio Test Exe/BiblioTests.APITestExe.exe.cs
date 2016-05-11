using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Xml;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Virtusales.Biblio.API;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace BiblioTests
{
	public class exe
	{
		//This is the Biblio username and password to use for API access
		private static string Username = "yourusername";
		private static string Password = "yourpassword";

		//This is the WorkID to use for the tests below
		private static string WorkID = "2666";

		//This is the root URL to the site. End in /
		private static string URL = "http://bibliourl/";

		public static void Main()
		{
			Console.Clear();
			dbg("-----------------------------------------------");
			dbg("This is the Example Text EXE for the Biblio API");
			dbg("-----------------------------------------------");

			dbg("URL is: " + URL);
			dbg("Logging In....");
			Virtusales.Biblio.API.Session Session = new Virtusales.Biblio.API.Session(URL, Username, Password);
			if ( !Session.Login() ) {
				dbg("Login Failed: " + Session.Detail);
				return;
			}
			Session.DebugToConsole = true;

			dbg("Login succeeded. " + Session.Detail);
			dbg("Your session ID is " + Session.SessionID);

			Fetch_JSON_String(Session);
			Fetch_XML_String(Session);
			Fetch_CSV_String(Session);
			Fetch_Object(Session);
		}

		public static void Fetch_JSON_String(Virtusales.Biblio.API.Session Session)
		{

			dbg("Requesting some Data as a simple JSON string...");
			Virtusales.Biblio.API.Request Req = Session.NewRequest(Page: "work-editions", Path: "default");
			Req.RequestProtocol = "json";
			Req.Post = "{\"id\": \"" + WorkID + "\"}";
			Req.FetchString();
			dbg(Req.Response);

		}

		public static void Fetch_XML_String(Virtusales.Biblio.API.Session Session)
		{

			dbg("Requesting some Data as a simple XML string...");
			Virtusales.Biblio.API.Request Req = Session.NewRequest(Page: "work-editions", Path: "default");
			Req.View = "xml";
			Req.RequestProtocol = "text";
			Req.Post = "id=" + WorkID;
			Req.FetchString();
			dbg(Req.Response);

		}

		public static void Fetch_CSV_String(Virtusales.Biblio.API.Session Session)
		{

			dbg("Requesting some Data as a simple CSV string...");
			dynamic Req = Session.NewRequest(Page: "work-editions", Path: "default");
			Req.View = "csv";
			Req.RequestProtocol = "text";
			Req.Post = "id=" + WorkID;
			Req.FetchString();
			dbg(Req.Response);

		}

		public static void Fetch_Object(Virtusales.Biblio.API.Session Session)
		{

			dbg("Requesting some Data as an object...");
			dynamic Req = Session.NewRequest(Page: "work-editions", Path: "default");
			RequestObject ReqObject = new RequestObject();
			ReqObject.id = WorkID;
			ReturnObject Results = Req.SendObject<RequestObject, ReturnObject>(ReqObject);
			dbg(Req.Response);
			dbg("Got " + Results.editions.Count + " editions and " + Results.products.Count + " products...");
			foreach (KeyValuePair<string,Edition> kvp_loopVariable in Results.editions) {
				KeyValuePair<string,Edition> kvp = kvp_loopVariable;
				Edition edition = kvp.Value;
				dbg(kvp.Key + "...");
				dbg("    " + edition.edition_id + " ... " + edition.edition_ean);
			}

		}

		public static void dbg(string Txt)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(Txt);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		[DataContract()]
		public class RequestObject
		{
			[DataMember()]
			public string id;
		}

		[DataContract()]
		public class ReturnObject
		{
			[DataMember()]
			public Dictionary<string, Edition> editions = new Dictionary<string, Edition>();
			[DataMember()]
			public Dictionary<string, Edition> products = new Dictionary<string, Edition>();
		}

		[DataContract()]
		public class Edition
		{
			[DataMember()]
			public string edition_id;
			[DataMember()]
			public string edition_ean;
		}

	}
}