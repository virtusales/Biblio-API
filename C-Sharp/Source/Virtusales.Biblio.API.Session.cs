using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Virtusales.Biblio.API
{
	internal class Session : BiblioAPIBase
	{
		private RequestObject<LoginRequestData, LoginResponseData> LoginRequest;
		private string Username;
		private string Password;

		public string ErrorMessage = "";

		private string _Token;
		public string Token {
			get { return _Token; }
			private set { _Token = value; }
		}

		public Session(string applicationName, string url, string username, string password)
		{
			this.Username = username;
			this.Password = password;
			this.LoginRequest = new RequestObject<LoginRequestData, LoginResponseData>(applicationName, url, "APILogin");
			this.LoginRequest.DebugToConsole = this.DebugToConsole;
		}

		public bool Login(ref string errorMessage = "")
		{
			LoginRequestData RequestData = new LoginRequestData {
				Username = Username,
				Password = Password
			};
			ResponseObject<LoginResponseData> Response = new ResponseObject<LoginResponseData>();

			Token = "";

			Response = LoginRequest.PostRequest(RequestData);

			if (Response.Success) {
				if (Response.Value.Success) {
					Token = Response.Value.SessionToken;
				} else {
					errorMessage = "Reason: " + Response.Value.Detail;
					return false;
				}
			} else if (Response.StatusCode == HttpStatusCode.Unauthorized) {
				errorMessage = "Unauthorised: " + Response.ErrorMessage;
				return false;
			} else {
				errorMessage = Response.ErrorMessage + Constants.vbCrLf + "Response HTTP State Code was: " + Convert.ToInt32(Response.StatusCode) + " " + Response.StatusCode.ToString() + Constants.vbCrLf + "Response was: " + Response.RawText;
				return false;
			}

			return true;
		}

		[DataContract()]
		private class LoginRequestData
		{
			[DataMember(Name = "username", IsRequired = true)]
			public string Username;
			[DataMember(Name = "password", IsRequired = true)]
			public string Password;
		}

		[DataContract()]
		private class LoginResponseData
		{
			[DataMember(Name = "success", IsRequired = true)]
			public bool Success;
			[DataMember(Name = "detail", IsRequired = true)]
			public string Detail;
			[DataMember(Name = "sessiontoken", IsRequired = true)]
			public string SessionToken;
		}
	}
}