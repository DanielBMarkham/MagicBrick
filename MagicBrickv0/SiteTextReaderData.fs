//#if INTERACTIVE
//    ;;
////    #r "HtmlAgilityPack.dll"
//#else
//#endif
/// <remarks>
/// This module provides data to be able to turn web sites into human readable text.
/// (The code is in other modules. This module is data-only)
/// </remarks>
module SiteTextReaderData

    open System
    open HtmlAgilityPack
    type tpTagPath = 
        {
            TagName     : string
            ID          : string
            Class       : string
            AnyAttName  : string
            AnyAttValue : string
            InnerText   : string
        }
        static member Empty =
            {
                TagName         = String.Empty
                ID              = String.Empty
                Class           = String.Empty
                AnyAttName      = String.Empty
                AnyAttValue     = String.Empty
                InnerText       = String.Empty
            }
    type tpSTRConfig =
        {
            domainRegEx         : string
            regKeep             : (string * string) list
            regExKillBefore     : (string*string) list // use this list of start and top regex locators to delete text before processing
            regExReplace        : (string*string) list // use this list of regex strings and replace string on text
            deleteList          : tpTagPath list // delete nodes and their children that match these paths
            ignoreList          : tpTagPath list // delete just the node that matches this path. Promote children
            regExKillAfter      : (string*string) list
            nextPageLink        : tpTagPath
        }
        static member Empty =
            {
                domainRegEx     = String.Empty
                regKeep         = []
                regExKillBefore = []
                regExReplace    = []
                deleteList      = []
                ignoreList      = []
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty
            }

    type tpWebsiteTextProcessing =
        {
            Url:string
            configList: tpSTRConfig list
            InitialHtml:string
            InitialDoc:HtmlDocument
            CleanedDoc:HtmlDocument
            FinalCleanHtml:string
            FinalCleanedDoc:HtmlDocument
            SentenceList:string list
            CleanedText:string
            ProcDateTime:DateTime
        }
        static member Empty =
            {
            Url                 = String.Empty
            configList          = []
            InitialHtml         = String.Empty
            InitialDoc          = new HtmlDocument()
            CleanedDoc          = new HtmlDocument()
            FinalCleanHtml      = String.Empty
            FinalCleanedDoc     = new HtmlDocument()
            SentenceList        = []
            CleanedText         = String.Empty
            ProcDateTime        = DateTime.MinValue
            }
    // EMPTY STRING MATCH EVERYTHING
    let siteList = 
        [
            {   domainRegEx     = ".*"
                regKeep         = []
                regExKillBefore = [(@"\<\!\-\- disqus \-\-\>", @"comments powered by Disqus\.\</a\>\</noscript\>");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [   {TagName = "^head$";        ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^script$";      ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^style$";       ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^iframe$";       ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^br$";       ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^noscript$";    ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""} ]
                ignoreList        = [   {TagName = "^b$";           ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^strong$";      ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^em$";          ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^i$";           ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^center$";      ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^a$";           ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^c$";           ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^label$";       ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^font$";        ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^blockquote$";  ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^q$";           ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^u$";           ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""}; 
                                        {TagName = "^span$";        ID="";      Class="";       AnyAttName="";      AnyAttValue="";     InnerText=""} ]
                regExKillAfter  = []   
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "realclearmarkets.com"
                regKeep         = []
                regExKillBefore = [(@"", @"");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [   {TagName = ""; ID="^navcontainer$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^article-tools$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^leftbox-latest$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^article-social-tools$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^related-footer$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^beta$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^footer$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID=""; Class="^recommendedArticles$";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^comments-Container$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        (*{TagName = ""; ID=""; Class="";AnyAttName="";AnyAttValue="";InnerText=""}*) ]
                ignoreList        = []
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "weeklystandard.com"
                regKeep         = []
                regExKillBefore = [(@"", @"");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [   {TagName = ""; ID="^column-container-left$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^banner$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        {TagName = ""; ID="^footer$"; Class="";AnyAttName="";AnyAttValue="";InnerText=""}; 
                                        (*{TagName = ""; ID=""; Class="";AnyAttName="";AnyAttValue="";InnerText=""}*) ]
                ignoreList        = []
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "BLANKTEMPLATE"
                regKeep         = []
                regExKillBefore = [(@"", @"");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [ {TagName="";ID=""; Class="";AnyAttName="";AnyAttValue=""; InnerText=""}]
                ignoreList        = [ {TagName=""; ID=""; Class="";AnyAttName="";AnyAttValue=""; InnerText=""}]
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "www.dailystar.com.lb"
                regKeep         = [("<!-- DIXERIT_START -->", "<!-- DIXERIT_STOP -->")]
                regExKillBefore = [(@"<\!-- Page Header Begins Here -->", @"<\!-- Page Header Ends here -->");
                                    ("<!--Left column begins here-->", "<!--Left column ends here-->");
                                    (" <!-- Begin More Articles -->", "<!-- End More Articles -->");
                                    ("<!-- Amazon Box here -->","<!-- Amazon Box Ends here -->");
                                    ("<!-- Botom Banner here -->","<!-- Botom Banner ends here -->");
                                    ("<!-- Footer begins here -->", "<!-- Footer ends here --");
                                    ("<!-- Regional Headlines Starts here -->", "<!-- Regional Headlines Ends here -->");
                                    ("<!-- Right col ads here -->", "<!-- Right col ads ends here -->");
                                    ("<!-- elections counter here -->", "<!-- elections counter ends here -->");
                                    ("<!-- Newsletter Subscription here -->", "<!-- End of Newsletter Subscription  -->");
                                    ("<!-- Center banner begins here --", "</table>");
                                    ("<!-- The Daily Star Network tab here -->", "<!-- The Daily Star Network Facebox content ends here -->");
                                    ("<!--Main table ends here -->", "<!-- Misc code ends here -->");
                                    ("<strong>THE DAILY STAR<\/strong>","\(www.project-syndicate.org\)\.<br \/>");
                                    ("<p align=\"right\"  class=\"articletext\">    Powered by","</p>");
                                    ("<p align=\"right\" class=\"articletext\"> <a href=\"http://voice.dixerit", "border=\"0\" align=\"middle\"></a></em></p>");
                                    ]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [ {TagName="ID";ID="^dsnetwork$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^footer$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^header$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^right$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^sidebar_left$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        ]
                ignoreList        = []
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "www.commentarymagazine.com/blogs"
                regKeep         = []
                regExKillBefore = [(@"", @"");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [ {TagName="";ID=""; Class="^postbuttons$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^footer$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^header$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^right$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                      {TagName="";ID=""; Class="^sidebar_left$";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        ]
                ignoreList        = []
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "BLANKTEMPLATE"
                regKeep         = []
                regExKillBefore = [(@"", @"");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [ {TagName="";ID=""; Class="";AnyAttName="";AnyAttValue=""; InnerText=""}]
                ignoreList        = [ {TagName=""; ID=""; Class="";AnyAttName="";AnyAttValue=""; InnerText=""}]
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};

            {   domainRegEx     = "usatoday.com"
                regKeep         = []
                regExKillBefore = [(@"", @"");]
                regExReplace    = [@"&nbsp;", " "]
                deleteList        = [ {TagName="div";ID="^Adv"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^applyHeader$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^seriesBar$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^subNav2$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^leaderboardContainer$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^globalNav$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^marketplace2$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""};
                                        {TagName="div";ID="^searchBar$"; Class="";AnyAttName="";AnyAttValue=""; InnerText=""}; ]
                ignoreList        = [ (*{TagName=""; ID=""; Class="";AnyAttName="";AnyAttValue=""; InnerText=""}*)]
                regExKillAfter  = []
                nextPageLink    = tpTagPath.Empty};
        ]

        
    let getConfigsForUrl (url:string) =
        let matchList = siteList |> List.filter(fun x->
            let regEx = new System.Text.RegularExpressions.Regex(x.domainRegEx)
            regEx.IsMatch(url)
            )
        matchList        


