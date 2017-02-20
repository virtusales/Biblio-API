using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using Virtusales.Biblio.API;

namespace BiblioAPISamples
{
	public class exe
	{
		//This is the root URL to the site. End in /
		private const string BiblioURL = "https://www.bibliolive.com/customername/";

		private static BiblioService BiblioService;

		//# This is the Biblio Username and Password to use for API access
		private static string Username = "Username";
		private static string Password = "Password";

		//# This is the Work ID to use for the test
		private static int WorkID = 1001;


		public static void Main()
		{
			Console.Clear();
			Console.WriteLine("-----------------------------------------------");
			Console.WriteLine("This is the Example Text EXE for the Biblio API");
			Console.WriteLine("-----------------------------------------------");
			Console.WriteLine("URL is: " + BiblioURL);

			BiblioService = new BiblioService("API Test", BiblioURL);
			BiblioService.DebugToConsole = true;

			Console.WriteLine("Logging In....");
			string LoginError = "";
			if (!BiblioService.Login(Username, Password, LoginError)) {
				HandleError("Login Failed! " + LoginError);
			}
			Console.WriteLine("Login succeeded!");

			Fetch_JSON_String();
			Fetch_XML_String();
			Fetch_CSV_String();
			Fetch_Object();
		}

		public static void Fetch_JSON_String()
		{
			Console.WriteLine("Requesting some Data As a simple JSON string...");

			dynamic APIRequest = BiblioService.CreateRequestBasic(Page: "work-editions");
			dynamic APIResponse = APIRequest.PostRequest("{\"id\": \"" + WorkID + "\"}");

			if (APIResponse.Success) {
				Console.WriteLine(APIResponse.RawText);
			} else {
				HandleError(APIResponse.ErrorMessage);
			}
		}

		public static void Fetch_XML_String()
		{
			Console.WriteLine("Requesting some Data As a simple XML string...");

			dynamic APIRequest = BiblioService.CreateRequestBasic(Page: "work-editions", responseFormat: FormattingOption.XML);
			dynamic APIResponse = APIRequest.PostRequest("{\"id\": \"" + WorkID + "\"}");

			if (APIResponse.Success) {
				Console.WriteLine(APIResponse.RawText);
			} else {
				HandleError(APIResponse.ErrorMessage);
			}
		}

		public static void Fetch_CSV_String()
		{
			Console.WriteLine("Requesting some Data As a simple CSV string...");

			dynamic APIRequest = BiblioService.CreateRequestBasic(Page: "work-editions", responseFormat: FormattingOption.CSV);
			dynamic APIResponse = APIRequest.PostRequest("{\"id\": \"" + WorkID + "\"}");

			if (APIResponse.Success) {
				Console.WriteLine(APIResponse.RawText);
			} else {
				HandleError(APIResponse.ErrorMessage);
			}
		}

		public static void Fetch_Object()
		{
			Console.WriteLine("Requesting some Data As an object...");

			dynamic APIRequest = BiblioService.CreateRequestObject<ModelRequest, ModelResponse>(Page: "work-editions");

			ModelRequest RequestM = new ModelRequest();
			RequestM.Id = WorkID;

			dynamic APIResponse = APIRequest.PostRequest(RequestM);

			if (APIResponse.Success) {
				Console.WriteLine(APIResponse.RawText);
				Console.WriteLine("Got " + APIResponse.Value.Editions.Count + " editions and " + APIResponse.Value.Products.Count + " products...");

				foreach (void KVP_loopVariable in APIResponse.Value.Editions) {
					KVP = KVP_loopVariable;
					ModelEdition Edition = KVP.Value;
					Console.WriteLine(KVP.Key + "...");
					Console.WriteLine("    " + Edition.Id + " ... " + Edition.EAN);
				}
			} else {
				HandleError(APIResponse.ErrorMessage);
			}
		}

		private static void HandleError(string ErrorMessage)
		{
			Console.WriteLine("ERROR: " + ErrorMessage);
			Environment.Exit(0);
		}

		[DataContract()]
		public class ModelRequest
		{
			[DataMember(IsRequired = true)]
			public string Id;
		}

		[DataContract()]
		public class ModelResponse
		{
			[DataMember(Name = "editions", IsRequired = true)]
			public Dictionary<string, ModelEdition> Editions = new Dictionary<string, ModelEdition>();
			[DataMember(Name = "products", IsRequired = true)]
			public Dictionary<string, ModelEdition> Products = new Dictionary<string, ModelEdition>();
		}

		[DataContract()]
		public class ModelEdition
		{
			[DataMember(Name = "edition_id", IsRequired = true)]
			public string Id;
			[DataMember(Name = "edition_ean")]
			public string EAN;
		}
	}
}