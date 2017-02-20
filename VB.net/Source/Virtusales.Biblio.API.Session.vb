Imports System.Net
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json

Namespace Virtusales.Biblio.API
    Friend Class Session : Inherits BiblioAPIBase
        Private LoginRequest As RequestObject(Of LoginRequestData, LoginResponseData)
        Private Username As String
        Private Password As String
        
        Public ErrorMessage As String = ""
        
        Private _Token As String
        Public Property Token As String
            Get
                Return _Token
            End Get
            Private Set(Value As String)
                _Token = Value
            End Set
        End Property
    
        Public Sub New(applicationName As String, url As String, username As String, password As String)
            Me.Username = username
            Me.Password = password
            Me.LoginRequest = New RequestObject(Of LoginRequestData, LoginResponseData)(applicationName, url, "APILogin")
            Me.LoginRequest.DebugToConsole = Me.DebugToConsole
        End Sub  
    
        Public Function Login(ByRef Optional errorMessage As String = "") As Boolean
            Dim RequestData As New LoginRequestData With { .Username = Username, .Password = Password }
            Dim Response As New ResponseObject(Of LoginResponseData)
            
            Token = ""
            
            Response = LoginRequest.PostRequest(RequestData)
            
            If Response.Success Then
                If Response.Value.Success Then
                    Token = Response.Value.SessionToken
                Else
                    ErrorMessage = "Reason: " & Response.Value.Detail
                    Return False
                End If
            Else If Response.StatusCode = HttpStatusCode.Unauthorized Then
                errorMessage = "Unauthorised: " & Response.ErrorMessage
                Return False
            Else
                errorMessage =  Response.ErrorMessage & vbCrLf & _
                                "Response HTTP State Code was: " & CInt(Response.StatusCode) & " " & Response.StatusCode.ToString() & vbCrLf & _
                                "Response was: " & Response.RawText
                Return False
            End If
            
            Return True
        End Function
    
        <DataContract>
        Private Class LoginRequestData
            <DataMember(Name := "username", IsRequired := True)> Public Username As String
            <DataMember(Name := "password", IsRequired := True)> Public Password As String
        End Class    
        
        <DataContract>
        Private Class LoginResponseData
            <DataMember(Name := "success", IsRequired := True)> Public Success As Boolean
            <DataMember(Name := "detail", IsRequired := True)> Public Detail As String
            <DataMember(Name := "sessiontoken", IsRequired := True)> Public SessionToken As String
        End Class  
    End Class
End Namespace