imports virtusales.biblio.api.request
imports system.runtime.serialization
imports system.runtime.serialization.json

'auto <-- Add this to automatically action all suggestions ('auto alone on a line)
'SUGGESTION: Following [1] Line could be 'automatic-importblock-system
imports system

namespace virtusales.biblio.api
public class session
    public URL as string
    public Username as string
    public Password as string
    public Detail as string
    public AuthenticationString as string
    public SessionID as string
    public DebugToConsole as boolean

    sub New(URL as string,Username as string,Password as string)
        me.url=url
        me.username=username
        me.password=password
    end sub
    
    <DataContract>
    private class APILoginResponse
        <DataMember> public success as string
        <DataMember> public detail as string
        <DataMember> public sessionid as string
    end class    

    <DataContract>
    private class APILoginRequest
        <DataMember> public username as string
        <DataMember> public password as string
    end class    

    function Login as boolean
        dim RequestData as new APILoginRequest with {
            .username=username,
            .password=password
        }
        dim Req = NewRequest("apilogin","DoLogin")
        dim Resp = Req.SendObject(of APILoginRequest,APILoginResponse)(RequestData)
        Detail=Resp.detail
        SessionID=Resp.SessionID
        AuthenticationString = "&sessioncookie=" & Resp.SessionID & "&usercookie=" & username
        return Resp.Success
    end function
    
    function NewRequest(Page as string,Path as string) as Request
        dim R as new request
        R.URL=URL
        R.AuthenticationString=AuthenticationString
        R.Page=Page
        R.Path=Path
        R.DebugToConsole=DebugToConsole
        return R
    end function

end class
end namespace