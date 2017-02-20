Namespace Virtusales.Biblio.API
    Public MustInherit Class [BiblioAPIBase]
        Public Property DebugToConsole as Boolean
        
        Sub Debug(Text as String)
            If DebugToConsole Then Console.WriteLine(Text)
        End Sub
    End Class
End Namespace