using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
namespace Virtusales.Biblio.API
{
	public abstract class BiblioAPIBase
	{
		public bool DebugToConsole { get; set; }

		public void Debug(string Text)
		{
			if (DebugToConsole)
				Console.WriteLine(Text);
		}
	}
}