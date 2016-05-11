using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace Virtusales.Biblio.API
{
	public class Request
	{
		public string URL;
		public string Page;
		public string Path;
		public string View = "json";
		public string AuthenticationString;
		public string RequestProtocol = "json";
		public string Post;
		public string Response;
		public bool DebugToConsole;

		public string MakeQueryString()
		{
			return "fwpath=" + Path + "&fwview=" + View + AuthenticationString;
		}

		public string MakeURL()
		{
			return URL + Page + ".aspx?" + MakeQueryString();
		}

		public string FetchString()
		{
			Dbg("----------------------- REQUEST -----------------------");
			WebClient WC = new WebClient();
			WC.Headers.Add("User-Agent", "API:APITests");
			//Ensure your string starts API: and then add a string describing your application.

			string WholeURL = MakeURL();

			Dbg("URL: " + WholeURL);

			if (string.IsNullOrEmpty(Post)) {
				Response = WC.DownloadString(WholeURL);
			} else {
				switch (RequestProtocol.ToLower()) {
					case "text":
					case "form":
					case "":
						WC.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
						Dbg("Set the request protocol headers as Encoded Text Form");
						break;
					case "json":
						WC.Headers.Add("Content-Type", "application/json");
						Dbg("Set the request protocol headers as JSON");
						break;
					case "xml":
						WC.Headers.Add("Content-Type", "application/xml");
						Dbg("Set the request protocol headers as XML");
						break;
					default:
						throw new System.Exception("Invalid Request Protocol");
				}
				Dbg("Post: " + Post);
				Response = WC.UploadString(WholeURL, Post);
			}
			Dbg("----------------------- /REQUEST -----------------------");
			Dbg("----------------------- RESPONSE -----------------------");
			Dbg(Response);
			Dbg("----------------------- /RESPONSE -----------------------");

			return Response;
		}

		public ResponseObject SendObject<RequestObject, ResponseObject>(RequestObject O)
		{
			Post = Serialise(O);
			View = "json";
			RequestProtocol = "json";
			return DeSerialise<ResponseObject>(FetchString());
		}

		public ResponseObject FetchObject<ResponseObject>()
		{
			View = "json";
			return DeSerialise<ResponseObject>(FetchString());
		}

		public ObjType DeSerialise<ObjType>(string JSON)
		{
			DataContractJsonSerializerSettings Settings = new DataContractJsonSerializerSettings();
			Settings.UseSimpleDictionaryFormat = true;
			DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(ObjType), Settings);
			MemoryStream MemStream = new MemoryStream(Encoding.Unicode.GetBytes(JSON));
			ObjType RetObject = default(ObjType);
			try {
				RetObject = (ObjType)Serializer.ReadObject(MemStream);
			} catch(System.Exception e) {
				throw new System.Exception("Could not deserialise object of type [" + typeof(ObjType).ToString() + "] from JSON " + JSON + " " + e.ToString());
			}
			return RetObject;
		}

		public string Serialise<ObjType>(ObjType O)
		{
			MemoryStream MS = new MemoryStream();
			DataContractJsonSerializer Ser = new DataContractJsonSerializer(O.GetType());
			Ser.WriteObject(MS, O);
			MS.Position = 0;
			StreamReader SR = new StreamReader(MS);
			return SR.ReadToEnd();
		}

		public void Dbg(string Str)
		{
			if (DebugToConsole)
				 System.Console.WriteLine(Str);
		}

	}
}
