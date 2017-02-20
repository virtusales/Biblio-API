using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
namespace Virtusales.Biblio.API
{
	public class BiblioService : BiblioAPIBase
	{
		private string ApplicationName;
		private Session Session;
		private string URL;

		public BiblioService(string applicationName, string url)
		{
			this.ApplicationName = applicationName;
			this.URL = url;
		}

		public bool Login(string username, string password, ref string errorMessage = "")
		{
			Session = new Session(ApplicationName, URL, username, password);
			if (!Session.Login(errorMessage)) {
				return false;
			}
			return true;
		}

		public object CreateRequestBasic(string page, string executionPath = "", FormattingOption responseFormat = FormattingOption.JSON)
		{
			dynamic Request = new RequestBasic(ApplicationName, URL, page, executionPath: executionPath, responseFormat: responseFormat);
			Request.Session = Session;
			return Request;
		}

		public RequestObject<TRequest, TResponse> CreateRequestObject<TRequest, TResponse>(string page, string executionPath = "")
		{
			RequestObject<TRequest, TResponse> Request = new RequestObject<TRequest, TResponse>(ApplicationName, URL, page, executionPath: executionPath);
			Request.Session = Session;
			return Request;
		}
	}
}