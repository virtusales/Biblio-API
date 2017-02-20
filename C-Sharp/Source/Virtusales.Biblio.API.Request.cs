using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace Virtusales.Biblio.API
{
	public abstract class RequestBase : BiblioAPIBase
	{
		private string ApplicationName;
		private string URL;
		private string Page;
		private string Path;
		private FormattingOption ResponseFormat;
		private Session _Session;

		internal Session Session {
			get { return _Session; }
			set { _Session = value; }
		}

		internal RequestBase(string applicationName, string url, string page, string executionPath = "", FormattingOption responseFormat = FormattingOption.JSON)
		{
			this.URL = url;
			this.Page = page;
			this.Path = executionPath;
			this.ResponseFormat = responseFormat;

			if (page.EndsWith(".aspx"))
				page = page.Substring(0, page.Length - 5);
		}

		private string ConstructQueryString()
		{
			return "?fwpath=" + string.IsNullOrEmpty(Path) ? "default" : Path;
		}

		private string ConstructURL()
		{
			return URL + !URL.EndsWith("\\") ? "\\" : "" + Page + ".aspx" + ConstructQueryString();
		}

		private string GetRequestMIMEFormat()
		{
			switch (ResponseFormat) {
				case FormattingOption.JSON:
					return "application/json";
				case FormattingOption.XML:
					return "application/xml";
				case FormattingOption.CSV:
					return "text/csv";
				case FormattingOption.TSV:
					return "text/tsv";
				case FormattingOption.Excel:
					return "application/vnd.ms-excel";
				default:
					throw new NotSupportedException("Unsupported Formatting Option: " + ResponseFormat.ToString());
			}
		}

		protected bool PostRequestInternal(ref ResponseBasic Response, string postData = null)
		{
			try {
				Debug("----------------------- REQUEST -----------------------");

				string FullURL = ConstructURL();

				HTTPWebRequest WRequest = WebRequest.Create(FullURL);
				WRequest.ContentType = "application/json";
				WRequest.UserAgent = "API:" + ApplicationName;
				WRequest.Accept = GetRequestMIMEFormat();

				if (Session != null) {
					WRequest.Headers("Authorization") = Session.Token;
				}

				Debug("URL: " + FullURL);

				if (postData == null) {
					Debug("GET Request");
				} else {
					Debug("POST Request - JSON Encoded Body: " + postData);
					WRequest.Method = WebRequestMethods.Http.Post;
					using (Stream DataStream = WRequest.GetRequestStream()) {
						using (StreamWriter Writer = new StreamWriter(DataStream)) {
							Writer.Write(postData);
						}
					}
				}

				using (HTTPWebResponse WResponse = WRequest.GetResponse()) {
					using (Stream DataStream = WResponse.GetResponseStream()) {
						using (StreamReader Reader = new StreamReader(DataStream)) {
							Response.RawText = Reader.ReadToEnd();
						}
					}
					Response.StatusCode = WResponse.StatusCode;
				}


				Debug("----------------------- /REQUEST -----------------------");
				Debug("----------------------- RESPONSE -----------------------");
				Debug(Response.RawText);
				Debug("----------------------- /RESPONSE -----------------------");
			} catch (WebException E) {
				using (HTTPWebResponse WResponse = E.Response()) {
					using (Stream DataStream = WResponse.GetResponseStream()) {
						using (StreamReader Reader = new StreamReader(DataStream)) {
							Response.RawText = Reader.ReadToEnd();
						}
					}
					Response.StatusCode = WResponse.StatusCode;
					Response.ErrorMessage = "A WebException was thrown: " + E.ToString();
				}
				return true;
			} catch (Exception E) {
				Response.Success = false;
				Response.ErrorMessage = "Unable to send HTTP request!" + Constants.vbCrLf + E.ToString();
				return false;
			}

			return true;
		}
	}

	public class RequestBasic : RequestBase
	{
		internal RequestBasic(string applicationName, string url, string page, string executionPath = "", FormattingOption responseFormat = FormattingOption.JSON) : base(applicationName, url, page, executionPath: executionPath, responseFormat: responseFormat)
		{
		}

		public ResponseBasic PostRequest(string postData)
		{
			ResponseBasic Response = new ResponseBasic();
			PostRequestInternal(ref Response, postData: postData);
			return Response;
		}

	}

	public class RequestObject<TRequest, TResponse> : RequestBase
	{
		internal RequestObject(string applicationName, string url, string page, string executionPath = "") : base(applicationName, url, page, executionPath: executionPath, responseFormat: FormattingOption.JSON)
		{
		}

		public ResponseObject<TResponse> PostRequest(TRequest PostObject)
		{
			ResponseObject<TResponse> Response = new ResponseObject<TResponse>();
			string EncodedPostData = SerialiseRequestObject(PostObject);
			if (PostRequestInternal(ref Response, postData: EncodedPostData)) {
				DeSerialiseResponseObject(ref Response);
			}
			return Response;
		}

		public ResponseObject<TResponse> GetRequest()
		{
			ResponseObject<TResponse> Response = new ResponseObject<TResponse>();
			if (PostRequestInternal(Response)) {
				DeSerialiseResponseObject(ref Response);
			}
			return Response;
		}

		private bool DeSerialiseResponseObject(ref ResponseObject<TResponse> Response)
		{
			Response.Value = DeserializeResponse(Response.RawText);
			if (Response.Value != null) {
				Response.Success = true;
			} else {
				Response.Success = false;
				JavaScriptSerializer JSSerial = new JavaScriptSerializer();
				Dictionary<string, object> ResponseParams = default(Dictionary<string, object>);
				try {
					ResponseParams = JSSerial.DeserializeObject(Response.RawText);

					if (!(ResponseParams.TryGetValue("errormessage", Response.ErrorMessage) || ResponseParams.TryGetValue("error", Response.ErrorMessage))) {
						Response.ErrorMessage = "Unexpected Result";
					}
				} catch {
					Response.ErrorMessage = "Could not deserialise object of type [" + typeof(TResponse).ToString() + "] from server response";
				}
			}
			return Response.Success;
		}

		private TResponse DeserializeResponse(string JSON)
		{
			DataContractJsonSerializerSettings Settings = new DataContractJsonSerializerSettings();
			Settings.UseSimpleDictionaryFormat = true;
			DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(TResponse), Settings);
			MemoryStream MemStream = new MemoryStream(Encoding.Unicode.GetBytes(JSON));

			try {
				return Serializer.ReadObject(MemStream);
			} catch {
				return null;
			}
		}

		private string SerialiseRequestObject(TRequest O)
		{
			MemoryStream MS = new MemoryStream();
			DataContractJsonSerializer Ser = new DataContractJsonSerializer(typeof(TRequest));
			Ser.WriteObject(MS, O);
			MS.Position = 0;
			StreamReader SR = new StreamReader(MS);
			return SR.ReadToEnd();
		}

	}

	public class ResponseBasic
	{
		public bool Success;
		public HttpStatusCode StatusCode;
		public string RawText;
		public string ErrorMessage = "";
	}

	public class ResponseObject<T> : ResponseBasic
	{
		public T Value = null;
	}
}