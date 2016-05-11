using Microsoft.VisualBasic;
using Virtusales.Biblio.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Virtusales.Biblio.API
{
	public class Session
	{
		public string URL;
		public string Username;
		public string Password;
		public string Detail;
		public string AuthenticationString;
		public string SessionID;

		public bool DebugToConsole;
		public Session(string URL, string Username, string Password)
		{
			this.URL = URL;
			this.Username = Username;
			this.Password = Password;
		}

		[DataContract()]
		private class APILoginResponse
		{
			[DataMember()]
			public bool success = true;
			[DataMember()]
			public string detail = "";
			[DataMember()]
			public string sessionid = "";
		}

		[DataContract()]
		private class APILoginRequest
		{
			[DataMember()]
			public string username;
			[DataMember()]
			public string password;
		}

		public bool Login()
		{
			APILoginRequest RequestData = new APILoginRequest {
				username = Username,
				password = Password
			};
			Virtusales.Biblio.API.Request Req = NewRequest("apilogin", "DoLogin");
			APILoginResponse Resp = Req.SendObject<APILoginRequest, APILoginResponse>(RequestData);
			Detail = Resp.detail;
			SessionID = Resp.sessionid;
			AuthenticationString = "&sessioncookie=" + Resp.sessionid + "&usercookie=" + Username;
			return Resp.success;
		}

		public Virtusales.Biblio.API.Request NewRequest(string Page, string Path)
		{
			Virtusales.Biblio.API.Request R = new Virtusales.Biblio.API.Request();
			R.URL = URL;
			R.AuthenticationString = AuthenticationString;
			R.Page = Page;
			R.Path = Path;
			R.DebugToConsole = DebugToConsole;
			return R;
		}

	}
}
