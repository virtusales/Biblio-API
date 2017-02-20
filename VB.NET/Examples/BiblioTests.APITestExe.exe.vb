Imports System.Collections.Generic
Imports System.Runtime.Serialization
Imports Virtusales.Biblio.API

Namespace BiblioAPISamples
    public class [exe]
        'This is the root URL to the site. End in /
        Private Const BiblioURL As String = "https://www.bibliolive.com/customername/"
        
        Private Shared BiblioService As BiblioService
        
        '# This is the Biblio Username and Password to use for API access
        Private Shared Username As String = "Username"
        Private Shared Password As String = "Password"
        
        '# This is the Work ID to use for the test
        Private Shared WorkID As Integer = 1001
        
        
        Shared Sub Main()
            Console.Clear()
            Console.WriteLine("-----------------------------------------------")
            Console.WriteLine("This is the Example Text EXE for the Biblio API")
            Console.WriteLine("-----------------------------------------------")
            Console.WriteLine("URL is: " & BiblioURL)
            
            BiblioService = New BiblioService("API Test", BiblioURL)
            BiblioService.DebugToConsole = True
            
            Console.WriteLine("Logging In....")
            Dim LoginError As String = ""
            If Not BiblioService.Login(Username, Password, LoginError) Then
                HandleError("Login Failed! " & LoginError)
            End If                  
            Console.WriteLine("Login succeeded!")
            
            Fetch_JSON_String()
            Fetch_XML_String()
            Fetch_CSV_String()
            Fetch_Object()
        End Sub
        
        Shared Sub Fetch_JSON_String()
            Console.WriteLine("Requesting some Data As a simple JSON string...")
            
            Dim APIRequest = BiblioService.CreateRequestBasic(Page := "work-editions")
            Dim APIResponse = APIRequest.PostRequest("{""id"": """ & WorkID & """}")
            
            If APIResponse.Success Then
                Console.WriteLine(APIResponse.RawText) 
            Else
                HandleError(APIResponse.ErrorMessage)
            End If
        End Sub
        
        Shared Sub Fetch_XML_String()
            Console.WriteLine("Requesting some Data As a simple XML string...")
            
            Dim APIRequest = BiblioService.CreateRequestBasic(Page := "work-editions", responseFormat := FormattingOption.[XML])
            Dim APIResponse = APIRequest.PostRequest("{""id"": """ & WorkID & """}")
            
            If APIResponse.Success Then
                Console.WriteLine(APIResponse.RawText) 
            Else
                HandleError(APIResponse.ErrorMessage)
            End If
        End Sub

        Shared Sub Fetch_CSV_String()
            Console.WriteLine("Requesting some Data As a simple CSV string...")
            
            Dim APIRequest = BiblioService.CreateRequestBasic(Page := "work-editions", responseFormat := FormattingOption.[CSV])
            Dim APIResponse = APIRequest.PostRequest("{""id"": """ & WorkID & """}")
            
            If APIResponse.Success Then
                Console.WriteLine(APIResponse.RawText) 
            Else
                HandleError(APIResponse.ErrorMessage)
            End If
        End Sub

        Shared Sub Fetch_Object()
            Console.WriteLine("Requesting some Data As an object...")
            
            Dim APIRequest = BiblioService.CreateRequestObject(Of ModelRequest, ModelResponse)(Page := "work-editions")
            
            Dim RequestM As New ModelRequest
            RequestM.Id = WorkID
            
            Dim APIResponse = APIRequest.PostRequest(RequestM)
            
            If APIResponse.Success Then
                Console.WriteLine(APIResponse.RawText)
                Console.WriteLine("Got " & APIResponse.Value.Editions.Count & " editions and " & APIResponse.Value.Products.Count & " products...")
                
                For Each KVP In APIResponse.Value.Editions
                    Dim Edition As ModelEdition = KVP.Value
                    Console.WriteLine(KVP.Key & "...")
                    Console.WriteLine("    " & Edition.Id & " ... " & Edition.EAN)
                Next
            Else
                HandleError(APIResponse.ErrorMessage)
            End If
        End Sub
        
        Private Shared Sub HandleError(ErrorMessage As String)
            Console.WriteLine("ERROR: " & ErrorMessage)
            Environment.Exit(0)
        End Sub
       
        <DataContract>
        Class ModelRequest
            <DataMember(IsRequired := True)>
            Public Id As String
        End Class       
       
        <DataContract>
        class ModelResponse
            <DataMember(Name := "editions", IsRequired := True)>
            Public Editions As New Dictionary(Of String, ModelEdition)
            <DataMember(Name := "products", IsRequired := True)>
            Public Products As New Dictionary(Of String, ModelEdition)
        End Class
       
        <DataContract>
        Class ModelEdition
            <DataMember(Name := "edition_id", IsRequired := True)>
            Public Id As String
            <DataMember(Name := "edition_ean")>
            Public EAN As String
        End Class
    End Class
End Namespace