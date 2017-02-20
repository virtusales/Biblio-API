Namespace Virtusales.Biblio.API
    Public Class [BiblioService] : Inherits BiblioAPIBase
        Private ApplicationName As String
        Private Session As Session
        Private URL As String
        
        Sub New(applicationName As String, url As String)
            Me.ApplicationName = applicationName
            Me.URL = url
        End Sub
        
        Public Function Login(username As String, password As String, ByRef Optional errorMessage As String = "") As Boolean
            Session = New Session(ApplicationName, URL, username, password)
            If Not Session.Login(ErrorMessage) Then
                Return False
            End If
            Return True
        End Function
        
        Public Function CreateRequestBasic(page As String, Optional executionPath As String = "", Optional responseFormat As FormattingOption = FormattingOption.[JSON])
            Dim Request = New RequestBasic(ApplicationName, URL, page, executionPath := executionPath, responseFormat := responseFormat)
            Request.Session = Session
            Return Request
        End Function
        
        Public Function CreateRequestObject(Of TRequest, TResponse)(page As String, Optional executionPath As String = "") As RequestObject(Of TRequest, TResponse)
            Dim Request As New RequestObject(Of TRequest, TResponse)(ApplicationName, URL, page, executionPath := executionPath)
            Request.Session = Session
            Return Request
        End Function
    End Class
End Namespace