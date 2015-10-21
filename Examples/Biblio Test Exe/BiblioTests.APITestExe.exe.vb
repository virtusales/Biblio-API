imports microsoft.visualbasic
imports system.xml
imports system.collections
imports system.net
imports system.runtime.serialization
imports system.runtime.serialization.json
imports virtusales.biblio.api
imports system
imports system.collections.generic
imports system.io
imports system.text
imports system.text.regularexpressions
imports system.threading
imports system.web
imports system.web.httputility

Namespace BiblioTests
    public class [exe]
        'This is the Biblio username and password to use for API access
        private shared Username as string = "yourusername"
        private shared Password as string = "yourpassword"        
        
        'This is the WorkID to use for the tests below
        private shared WorkID as string = "55288"
        
        'This is the root URL to the site. End in /
        private shared URL as string = "http://www.bibliolive.com/customername/"
    
        Shared Sub Main()
            console.clear
            dbg("-----------------------------------------------")
            dbg("This is the Example Text EXE for the Biblio API")
            dbg("-----------------------------------------------")
        
            dbg("URL is: " & URL)
            dbg("Logging In....")
            dim Session as new virtusales.biblio.api.session(URL,Username,Password)
            if not Session.Login then
                dbg("Login Failed: " & session.detail)
                exit sub
            end if
            session.DebugToConsole=true

            dbg("Login succeeded. " & session.detail)
            dbg("Your session ID is " & session.sessionid)
            
            Fetch_JSON_String(session)
            Fetch_XML_String(session)
            Fetch_CSV_String(session)
            Fetch_Object(session)
        end sub
        
        shared sub Fetch_JSON_String(Session as virtusales.biblio.api.session)
            
            dbg("Requesting some Data as a simple JSON string...")
            dim Req = Session.NewRequest(Page:="work-editions",Path:="default")
            Req.RequestProtocol="json"
            Req.Post="{""id"": """ & workid & """}"
            Req.FetchString
            dbg(req.response) 

        end sub
        
        shared sub Fetch_XML_String(Session as virtusales.biblio.api.session)
        
            dbg("Requesting some Data as a simple XML string...")
            dim Req = Session.NewRequest(Page:="work-editions",Path:="default")
            Req.View="xml"
            Req.RequestProtocol="text"
            Req.Post="id=" & workid
            Req.FetchString
            dbg(req.response) 

        end sub

        shared sub Fetch_CSV_String(Session as virtusales.biblio.api.session)
            
            dbg("Requesting some Data as a simple CSV string...")
            dim Req = Session.NewRequest(Page:="work-editions",Path:="default")
            Req.View="csv"
            Req.RequestProtocol="text"
            Req.Post="id=" & workid
            Req.FetchString
            dbg(req.response) 

        end sub

        shared sub Fetch_Object(Session as virtusales.biblio.api.session)
        
            dbg("Requesting some Data as an object...")
            dim Req = Session.NewRequest(Page:="work-editions",Path:="default")
            dim ReqObject as new RequestObject
            ReqObject.id=workid
            dim Results as ReturnObject = Req.SendObject(of RequestObject,ReturnObject)(ReqObject)
            dbg(req.response) 
            dbg("Got " & Results.editions.count & " editions and " & results.products.count & " products...")
            for each kvp in Results.editions
                dim edition as edition=kvp.value
                dbg(kvp.key & "...")
                dbg("    " & edition.edition_id & " ... " & edition.edition_ean)
            next
        
        end sub
        
        shared sub dbg(Txt as string)
            console.foregroundcolor=consolecolor.yellow
            console.writeline(txt)
            console.foregroundcolor=consolecolor.gray
        end sub
        
        <DataContract>
        class RequestObject
            <DataMember>
            public id as string
        end class        
        
        <DataContract>
        class ReturnObject
            <DataMember>
            public editions as new dictionary(of string,edition)
            <DataMember>
            public products as new dictionary(of string,edition)
        end class
        
        <DataContract>
        class Edition
            <DataMember>
            public edition_id as string
            <DataMember>
            public edition_ean as string
        end class
        
    end class
end namespace