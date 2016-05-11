imports system.net
imports system.runtime.serialization
imports system.runtime.serialization.json

'auto <-- Add this to automatically action all suggestions ('auto alone on a line)
'SUGGESTION: Following [3] Lines could be 'automatic-importblock-system
imports system
imports system.io
imports system.text

namespace virtusales.biblio.api
public class request
    public URL as string
    public Page as string
    public Path as string
    public View as string="json"
    public AuthenticationString as string
    public RequestProtocol as string="json"
    public Post as string
    public Response as string
    public DebugToConsole as boolean
    
    function MakeQueryString as string
        return "fwpath=" & path & "&fwview=" & view & AuthenticationString
    end function
    
    function MakeURL as string
        return URL & Page & ".aspx?" & MakeQueryString
    end function

    public function FetchString as string
        dbg("----------------------- REQUEST -----------------------")
        dim WC as new WebClient
        WC.Headers("User-Agent")="API:APITests" 'Ensure your string starts API: and then add a string describing your application.
        
        dim WholeURL as string = MakeURL

        dbg("URL: " & WholeURL)

        if Post="" then
            Response = WC.DownloadString(WholeURL)
        else
            select case requestprotocol.tolower
                case "text","form",""
                    WC.Headers("Content-Type")="application/x-www-form-urlencoded"
                    dbg("Set the request protocol headers as Encoded Text Form")
                case "json"
                    WC.Headers("Content-Type")="application/json"
                    dbg("Set the request protocol headers as JSON")
                case "xml"
                    WC.Headers("Content-Type")="application/xml"
                    dbg("Set the request protocol headers as XML")
                case else
                    throw new exception("Invalid Request Protocol")
            end select
            dbg("Post: " & Post)
            Response = WC.UploadString(WholeURL,Post)
        end if
        dbg("----------------------- /REQUEST -----------------------")
        dbg("----------------------- RESPONSE -----------------------")
        dbg(Response)
        dbg("----------------------- /RESPONSE -----------------------")
        
        return response
    End function
    
    public function SendObject(of RequestObject,ResponseObject)(O as RequestObject) as ResponseObject
        Post = Serialise(O)
        View="json"
        RequestProtocol="json"
        return DeSerialise(of ResponseObject)(FetchString)
    end function
    
    public function FetchObject(of ResponseObject) as ResponseObject
        View="json"
        return DeSerialise(of ResponseObject)(FetchString)
    end function    
    
    function DeSerialise(of ObjType)(JSON as string) as ObjType
        dim Settings as new DataContractJsonSerializerSettings
        settings.UseSimpleDictionaryFormat = true
        dim Serializer as new DataContractJsonSerializer(gettype(ObjType),Settings)
        dim MemStream as new MemoryStream(Encoding.Unicode.GetBytes(JSON))
        dim RetObject as ObjType
        try
            RetObject = Serializer.ReadObject(MemStream)    
        catch
            throw new exception("Could not deserialise object of type [" & gettype(ObjType).tostring & "] from JSON " & JSON)
        end try
        return RetObject
    end function
    
    function Serialise(of ObjType)(O as ObjType) as string
        dim MS as new MemoryStream
        dim Ser as new DataContractJsonSerializer(O.GetType)
        ser.WriteObject(MS, O)
        MS.Position = 0
        dim SR as new StreamReader(MS)
        return SR.ReadToEnd
    end function
    
    sub Dbg(Str as string)
        if DebugToConsole then console.writeline(str)
    end sub
    
end class
end namespace