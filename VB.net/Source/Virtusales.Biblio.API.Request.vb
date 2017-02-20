Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Web
Imports System.Web.Script.Serialization

Namespace Virtusales.Biblio.API
Public MustInherit Class RequestBase : Inherits BiblioAPIBase
    Private ApplicationName As String
    Private URL As String
    Private Page As String
    Private Path As String
    Private ResponseFormat As FormattingOption
    Private _Session As Session
    
    Friend Property Session As Session
        Get
            Return _Session
        End Get
        Set(Session As Session)
            _Session = Session
        End Set
    End Property
    
    Friend Sub New(     applicationName As String, url As String, page As String _
                        , Optional executionPath As String = "", Optional responseFormat As FormattingOption = FormattingOption.[JSON]      )
        Me.URL = url
        Me.Page = page
        Me.Path = executionPath
        Me.ResponseFormat = responseFormat
        
        If Page.EndsWith(".aspx") Then Page = Page.Substring(0, Page.Length - 5)
    End Sub
    
    Private Function ConstructQueryString() As String
        Return "?fwpath=" & If(Path = "", "default", Path)
    End Function
    
    Private Function ConstructURL() As String
        Return URL & If(Not URL.EndsWith("\"), "\", "") & Page & ".aspx" & ConstructQueryString()
    End Function
    
    Private Function GetRequestMIMEFormat() As String
        Select Case ResponseFormat
            Case FormattingOption.[JSON]
                Return "application/json"
            Case FormattingOption.[XML] 
                Return "application/xml"
            Case FormattingOption.[CSV]
                Return "text/csv"
            Case FormattingOption.[TSV]
                Return "text/tsv"
            Case FormattingOption.[Excel]
                Return "application/vnd.ms-excel"
            Case Else
                Throw New NotSupportedException("Unsupported Formatting Option: " & ResponseFormat.ToString())
        End Select
    End Function

    Protected Function PostRequestInternal(ByRef Response As ResponseBasic, Optional postData As String = Nothing) As Boolean
        Try
            Debug("----------------------- REQUEST -----------------------")
            
            Dim FullURL As string = ConstructURL()
            
            Dim WRequest As HTTPWebRequest = WebRequest.Create(FullURL)
            WRequest.ContentType = "application/json"
            WRequest.UserAgent = "API:" & ApplicationName
            WRequest.Accept = GetRequestMIMEFormat()
            
            If Session IsNot Nothing Then
                WRequest.Headers("Authorization") = Session.Token
            End If
            
            Debug("URL: " & FullURL)
            
            If postData Is Nothing Then
                Debug("GET Request")
            Else
                Debug("POST Request - JSON Encoded Body: " & postData)
                WRequest.Method = WebRequestMethods.Http.Post
                Using DataStream As Stream = WRequest.GetRequestStream()
                    Using Writer As New StreamWriter(DataStream)
                        Writer.Write(postData)
                    End Using
                End Using
            End If
            
            Using WResponse As HTTPWebResponse = WRequest.GetResponse()
                Using DataStream As Stream = WResponse.GetResponseStream()
                    Using Reader As New StreamReader(DataStream)
                        Response.RawText = Reader.ReadToEnd()
                    End Using
                End Using
                Response.StatusCode = WResponse.StatusCode
            End Using
            
            
            Debug("----------------------- /REQUEST -----------------------")
            Debug("----------------------- RESPONSE -----------------------")
            Debug(Response.RawText)
            Debug("----------------------- /RESPONSE -----------------------")
        Catch E as WebException
            Using WResponse As HTTPWebResponse = E.Response()
                Using DataStream As Stream = WResponse.GetResponseStream()
                    Using Reader As New StreamReader(DataStream)
                        Response.RawText = Reader.ReadToEnd()
                    End Using
                End Using
                Response.StatusCode = WResponse.StatusCode
                Response.ErrorMessage = "A WebException was thrown: " & E.ToString()
            End Using
            Return True
        Catch E As Exception
            Response.Success = False
            Response.ErrorMessage = "Unable to send HTTP request!" & vbCrLf & E.ToString()
            Return False
        End Try
        
        Return True
    End Function
End Class

Public Class RequestBasic : Inherits RequestBase
    Friend Sub New(     applicationName As String, url As String, page As String _
                        , Optional executionPath As String = "", Optional responseFormat As FormattingOption = FormattingOption.[JSON]      )
        MyBase.New(applicationName, url, page, executionPath := executionPath, responseFormat := responseFormat)
    End Sub
    
    Public Function PostRequest(postData As String) As [ResponseBasic]
        Dim Response As New ResponseBasic
        PostRequestInternal(Response, postData := postData)
        Return Response
    End Function
    
End Class

Public Class RequestObject(Of TRequest, TResponse) : Inherits RequestBase
    Friend Sub New(     applicationName As String, url As String, page As String _
                        , Optional executionPath As String = ""     )
        MyBase.New(applicationName, url, page, executionPath := executionPath, responseFormat := FormattingOption.[JSON])
    End Sub
    
    Public Function PostRequest(PostObject As TRequest) As ResponseObject(Of TResponse)
        Dim Response As New ResponseObject(Of TResponse)
        Dim EncodedPostData As String = SerialiseRequestObject(PostObject)
        If PostRequestInternal(Response, postData := EncodedPostData) Then
            DeSerialiseResponseObject(Response)
        End If
        Return Response
    End Function
    
    Public Function GetRequest() As ResponseObject(Of TResponse)
        Dim Response As New ResponseObject(Of TResponse)
        If PostRequestInternal(Response) Then
            DeSerialiseResponseObject(Response)
        End If
        Return Response
    End Function    
    
    Private Function DeSerialiseResponseObject(ByRef Response As ResponseObject(Of TResponse)) As Boolean
        Response.Value = DeserializeResponse(Response.RawText)
        If Response.Value IsNot Nothing Then
            Response.Success = True
        Else
            Response.Success = False
            Dim JSSerial As New JavaScriptSerializer()
            Dim ResponseParams As Dictionary(Of String, Object)
            Try
                ResponseParams = JSSerial.DeserializeObject(Response.RawText)
                
                If Not (    ResponseParams.TryGetValue("errormessage", Response.ErrorMessage) OrElse _
                            ResponseParams.TryGetValue("error", Response.ErrorMessage)    ) Then
                    Response.ErrorMessage = "Unexpected Result"
                End If
            Catch
                Response.ErrorMessage = "Could not deserialise object of type [" & GetType(TResponse).ToString() & "] from server response"
            End Try
        End If
        Return Response.Success
    End Function
    
    Private Function DeserializeResponse(JSON as String) As TResponse
        Dim Settings As New DataContractJsonSerializerSettings()
        Settings.UseSimpleDictionaryFormat = True
        Dim Serializer As New DataContractJsonSerializer(GetType(TResponse), Settings)
        Dim MemStream As New MemoryStream(Encoding.Unicode.GetBytes(JSON))
        
        Try
            Return Serializer.ReadObject(MemStream)
        Catch
            Return Nothing
        End Try
    End Function
    
    Private Function SerialiseRequestObject(O As TRequest) As String
        Dim MS As New MemoryStream
        Dim Ser As New DataContractJsonSerializer(GetType(TRequest))
        Ser.WriteObject(MS, O)
        MS.Position = 0
        Dim SR As New StreamReader(MS)
        Return SR.ReadToEnd()
    End Function
    
End Class

Public Class [ResponseBasic]
    Public Success As Boolean
    Public StatusCode As HttpStatusCode
    Public RawText As String
    Public ErrorMessage As String = ""
End Class

Public Class [ResponseObject](Of T) : Inherits [ResponseBasic]
    Public Value As T = Nothing
End Class
End Namespace