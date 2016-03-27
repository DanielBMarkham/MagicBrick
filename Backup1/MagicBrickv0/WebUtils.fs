#if INTERACTIVE
//    ;;
//    #load "TypeUtils.fs"
//    #load "Sink.fs"
//    #r "System.Xml.dll"
//    #r "C:\\Windows\\assembly\\GAC_32\\System.Web\\2.0.0.0__b03f5f7f11d50a3a\\System.Web.dll"
//    #r "System.ServiceModel.dll"
//    #r "C:\\Users\\Daniel Markham\\Documents\\Visual Studio 2010\\Projects\\antiap\\antiap\\bin\\Debug\\HtmlAgilityPack.dll"
//    #r "System.Net.dll"
//    #load "Utils.fs"
//    #load "SiteTextReaderData.fs"
module WebUtils
#else
module WebUtils
#endif
/// <remarks>
/// Module for all web functions and types
/// </remarks>

    open HtmlAgilityPack
    open System
    open System.Net
    open System.Web
    open System.IO
    open System.Xml
    open TypeUtils
    open Sink
    open Utils
    open SiteTextReaderData

    let jsonEscapeString (s:String) =
        s.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r")

    let decodeWordCount(s:string) = wordCount (System.Web.HttpUtility.HtmlDecode(s.Trim()).Trim());;

    let htmlDecode (s:string) = 
        let ret = (System.Web.HttpUtility.HtmlDecode(s.Trim()).Trim())
        ret.Replace("â€™", "'")

    let hmtlEncode (s:string) = (System.Web.HttpUtility.HtmlEncode(s))

    // at some point we may want to randomize the type of client the app shows to the world
    let makeNewWebClient() =
        let Client = new System.Net.WebClient()
        Client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5 (.NET CLR 3.5.30729)")
        Client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml")
        //Client.Headers.Add(HttpRequestHeader.Referer, "string");
        Client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5")
        Client

    /// Fetch the contents of a web page
    let http(url: string) =
        try
            if url <> "UGLY!" then
                let Client = makeNewWebClient()
                use strm = Client.OpenRead(url)
                use sr = new System.IO.StreamReader(strm);
                let html = sr.ReadToEnd()
                html.Replace("\t", "").Replace("\n", "") |> ignore
                html
            else
                "Could not decipher related link"
        with
            |e->
                Console.WriteLine(e.Message)
                "error interfacing with " + url + " : " + e.Message

    /// Fetch the contents of a web page
    let asyncHttp(url: string) =
        async{
            try
                if url <> "UGLY!" then
                    let Client = makeNewWebClient()
                    let! html = Client.AsyncDownloadString(Uri(url, UriKind.Absolute))
                    html.Replace("\t", "").Replace("\n", "") |> ignore
                    return html
                else
                    return "Could not decipher related link"
            with
                |e->
                    Console.WriteLine(e.Message)
                    Console.WriteLine(e.Source)
                    return "error interfacing with " + url + " : " + e.Message
        }

    /// Fetch the contents of a web page with error callback
    let cbAsyncHttp(url: string) (fError:string->unit)=
        async{
            try
                if url <> "UGLY!" then
                    let Client = makeNewWebClient()
                    let! html = Client.AsyncDownloadString(Uri(url, UriKind.Absolute))
                    html.Replace("\t", "").Replace("\n", "") |> ignore
                    return html
                else
                    let msg = "Could not decipher related link: " + url + " " + System.DateTime.Now.ToString()
                    fError msg
                    return msg
            with
                |e->
                    Console.WriteLine(e.Message)
                    Console.WriteLine(e.Source)
                    return "error interfacing with " + url + " : " + e.Message
        }

    /// Fetch the contents of a web page
    let agentHttp(url: string) (procReturn:string->unit) =
        Agent.Start(fun inbox ->
            let syncContext =
                let x = System.Threading.SynchronizationContext.Current
                if x = null then new System.Threading.SynchronizationContext() else x
            let samplingReturn  = new Event<string>()
            samplingReturn.Publish.Add procReturn
            let rec loop n = async{
                let! msg = inbox.Receive()
                let! s = asyncHttp msg
                syncContext.raiseEventOnGuiThread(samplingReturn,(s))
                return! loop (n + 1)
                }
            loop 0
            )        


    let getHtmlDoc(url:string):HtmlAgilityPack.HtmlDocument =
        try
            let html = http url
            let doc = new HtmlAgilityPack.HtmlDocument()
            doc.LoadHtml(html)
            doc
        with
            |e->
                Console.WriteLine(e.Message)
                new HtmlAgilityPack.HtmlDocument()
                //"error interfacing with web"
        
    let asyncGetHtmlDoc(url:string) =
        async{
            try
                let! html = asyncHttp url
                let doc = new HtmlAgilityPack.HtmlDocument()
                doc.LoadHtml(html)
                return doc
            with
                |e->
                    Console.WriteLine(e.Message)
                    return new HtmlAgilityPack.HtmlDocument()
        }
    // this time with callbacks
    let cbAsyncGetHtmlDoc(url:string) (fError:string->unit) =
        async{
            try
                let! html = asyncHttp url
                let doc = new HtmlAgilityPack.HtmlDocument()
                doc.LoadHtml(html)
                return doc
            with
                |e->
                    let msg = "Problem creating the htmldoc from : " + url + " " + System.DateTime.Now.ToString()
                    fError msg
                    Console.WriteLine(e.Message)
                    return new HtmlAgilityPack.HtmlDocument()
        }

    let getRSSFeedItems (feedUrl:string) = 
        let xm = System.Xml.XmlReader.Create(feedUrl)
        let items = System.ServiceModel.Syndication.SyndicationFeed.Load(xm).GetRss20Formatter().Feed.Items
        items

    let getAtomFeedItems (feedUrl:string) = 
        let xm = System.Xml.XmlReader.Create(feedUrl)
        let items = System.ServiceModel.Syndication.SyndicationFeed.Load(xm).GetAtom10Formatter().Feed.Items
        items

    let elinateDupeHtmlNodes nds:seq<HtmlAgilityPack.HtmlNode> =
        let dict = new System.Collections.Generic.Dictionary<string, HtmlAgilityPack.HtmlNode>()
        nds |> Seq.iter(fun (x:HtmlAgilityPack.HtmlNode) ->
                let key = x.Line.ToString() + ":" + x.LinePosition.ToString()
                if dict.ContainsKey key then
                    ()
                else dict.Add(key, x)
            )
        dict |> Seq.map(fun x->x.Value)

    let turnStringListIntoHtmlList (s: string list) (isNumbered:bool) =
        let outerTagCore =
            if isNumbered then "ol" else "ul"
        let leftOuterTag = "<" + outerTagCore + ">"
        let rightOuterTag = "</" + outerTagCore + ">"
        let hlis = s |> List.toSeq |> Seq.map(fun x->"<li>" + x + "</li>") |> String.Concat
        let ret = leftOuterTag + hlis + rightOuterTag
        ret

    let turnStringListIntoHtmlTagList (s: string list) (outerTag:string) (innerTag:string) =
        let outerTagCore = outerTag
        let leftOuterTag = "<" + outerTagCore + ">"
        let rightOuterTag = "</" + outerTagCore + ">"
        let innerTagCore = innerTag
        let leftInnerTag = "<" + innerTagCore + ">"
        let rightInnerTag = "</" + innerTagCore + ">"
        let hlis = s |> List.toSeq |> Seq.map(fun x->leftInnerTag + x + rightInnerTag) |> String.Concat
        let ret = leftOuterTag + hlis + rightOuterTag
        ret

    let nodeToTagString (nd:HtmlNode) = 
        let t1 = "<" + nd.Name + " "
        let t2 = String.Join(" ", nd.Attributes |> Seq.map(fun x->x.Name + "=\"" + x.Value + "\" "))
        let t3 = ">"
        t1 + t2 + t3

    let docWalker (doc:HtmlDocument) f = 
        let rec loop (nd:HtmlNode) =
            f nd
            nd.ChildNodes |> Seq.iter(fun x->loop x)
        loop doc.DocumentNode

    let rec docFold (f:'a->HtmlNode->'a) (accumulatingNode:'a) (sourceNode:'a) = 
        let rec loop targetNode (x:HtmlNode) =
            if x.HasChildNodes then
                let temp = f accumulatingNode sourceNode
                x.ChildNodes |> Seq.fold(fun acc2 y->
                    let temp2 = f temp y
                    docFold f temp2 y
                    ) accumulatingNode
                else
                    accumulatingNode.ParentNode
        loop accumulatingNode sourceNode

    let rec deepCopyNode (ndSource:HtmlNode) (parentNodeDestination:HtmlNode) (destDoc:HtmlDocument) =
        match ndSource.NodeType with
            | HtmlNodeType.Comment ->
                let tempNode = destDoc.CreateComment()
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append(tempNode)
                ndSource.ChildNodes |> Seq.iter(fun x->deepCopyNode x tempNode destDoc)                
            | HtmlNodeType.Text ->
                let tempNode = destDoc.CreateTextNode(ndSource.InnerText)
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append(tempNode)
                ndSource.ChildNodes |> Seq.iter(fun x->deepCopyNode x tempNode destDoc)                
            | HtmlNodeType.Element ->
                let tempNode = destDoc.CreateElement(ndSource.Name)
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append(tempNode)
                ndSource.ChildNodes |> Seq.iter(fun x->deepCopyNode x tempNode destDoc)                
            | HtmlNodeType.Document ->
                ndSource.Attributes.Iter(fun x->destDoc.DocumentNode.Attributes.Add(x))
                ndSource.ChildNodes |> Seq.iter(fun x->deepCopyNode x destDoc.DocumentNode destDoc)                
            |_ -> ()
    
    let shallowCopyAndAppendNode (ndSource:HtmlNode) (parentNodeDestination:HtmlNode) (destDoc:HtmlDocument) =
        match ndSource.NodeType with
            | HtmlNodeType.Element ->
                let tempNode = destDoc.CreateElement(ndSource.Name)
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append tempNode
                tempNode.Id
            | HtmlNodeType.Comment ->
                let tempNode = destDoc.CreateComment()
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append tempNode
                tempNode.Id
            | HtmlNodeType.Text ->
                let tempNode = destDoc.CreateTextNode(ndSource.InnerText)
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append tempNode
                tempNode.Id
            | HtmlNodeType.Document->
                raise (new System.ArgumentOutOfRangeException("node type out of bounds"))
                //ndSource.Attributes.Iter(fun x->destDoc.DocumentNode.Attributes.Add(x))
                //destDoc.DocumentNode
            |_ -> raise (new System.ArgumentOutOfRangeException("node type out of bounds"))
        
    let getDocHtml (srcDoc:HtmlDocument) =
        let sb = new System.Text.StringBuilder(16385)
        let rec loop (nd:HtmlNode) (sb:System.Text.StringBuilder) =
            match nd.HasChildNodes with
                |true ->nd.ChildNodes |> Seq.iter(fun x->loop x sb)
                |false->sb.Append(nd.OuterHtml) |> ignore
        loop srcDoc.DocumentNode sb
        sb.ToString()

    let copyDoc (srcDoc:HtmlDocument) =
        let html = getDocHtml srcDoc
        let ret = new HtmlAgilityPack.HtmlDocument()
        ret.LoadHtml(html)
        ret




    let getNodeByAtt (name:string) (value:string) (docRoot:HtmlDocument) =
        docRoot.DocumentNode.DescendantNodes() |> Seq.find(fun x->
            x.Attributes |> Seq.exists(fun y->y.Name=name && y.Value=value))            
    let tagNodes (doc:HtmlDocument) (pattern:tpTagPath) (tagName:string) (tagValue:string) =
        let ret = new HtmlAgilityPack.HtmlDocument()
        ret.OptionUseIdAttribute<-true
        let regTagName = new System.Text.RegularExpressions.Regex(pattern.TagName, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        let regAnyAttName = new System.Text.RegularExpressions.Regex(pattern.AnyAttName, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        let regAnyAttValue = new System.Text.RegularExpressions.Regex(pattern.AnyAttValue, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        let regClass = new System.Text.RegularExpressions.Regex(pattern.Class, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        let regID = new System.Text.RegularExpressions.Regex(pattern.ID, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        let regInnerText= new System.Text.RegularExpressions.Regex(pattern.InnerText, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        let attExists (nd:HtmlNode) (name:string) =
            nd.Attributes |> Seq.exists(fun x->x.Name=name)
        // either return the attribute with that name or return a new attribute with that name and no value
        let attOrEmpty (nd:HtmlNode) (attName:string) = 
            if nd.Attributes |> Seq.exists(fun x->x.Name.ToLower()=attName.ToLower()) then
                nd.Attributes |> Seq.find(fun x->x.Name.ToLower()=attName.ToLower())
            else
                nd.Attributes.Add(attName, "")
                nd.Attributes.Item(attName)
        let attNameMatch (attNameReg:System.Text.RegularExpressions.Regex) (nd:HtmlNode) =
            nd.Attributes |> Seq.exists(fun x->attNameReg.IsMatch(x.Name))
        let attValueMatch (attValueReg:System.Text.RegularExpressions.Regex) (nd:HtmlNode) =
            nd.Attributes |> Seq.exists(fun x->attValueReg.IsMatch(x.Value))

        //    let docFold (f:'a->HtmlNode->'a) (acc:'a) (initVal:'a) = 
        doc.DocumentNode |> docFold(fun acc x->
            let tag = Guid.NewGuid().ToString()
            let marker = Guid.NewGuid().ToString()
            x.Attributes.Add(tag, marker)
            let tempNodeID = shallowCopyAndAppendNode x acc ret
            let temp = getNodeByAtt tag marker ret
            if not (regTagName.IsMatch(x.Name) && regID.IsMatch((attOrEmpty x "ID").Value) && regClass.IsMatch((attOrEmpty x "class").Value) && (attNameMatch regAnyAttName x) && (attValueMatch regAnyAttValue x))
                then
                    temp
                else
                    if attExists x "xxx" then
                        temp
                    else
                        temp.Attributes.Add(tagName, tagValue)
                        temp
            ) ret.DocumentNode |> ignore
        ret

    let markDocIgnores (srcDoc:HtmlDocument) (ignoreList:tpTagPath list) = 
        ignoreList |> List.fold( fun acc x->
            tagNodes acc x "###" "IGNORE ME"            
            ) srcDoc
    let markDocDeletes (srcDoc:HtmlDocument) (ignoreList:tpTagPath list) = 
        ignoreList |> List.fold( fun acc x->
            tagNodes acc x "###" "DELETE ME"            
            ) srcDoc

    ///<summary> this walks the tree, copying the pieces over and making a new doc with only interesting nodes
    /// Nodes have previously been marked with attribute "###" and "DELETE ME" or "IGNORE ME" 
    /// (delete whole thing or delete just that node and copy children up</summary>
    /// <param name="doc">The source document</param>
    /// <param name="copyComments">Whether or not to copy comments over</param>
    /// <returns>A new document with nodes copied from the original document</returns>
    let filterDoc (doc:HtmlAgilityPack.HtmlDocument) (copyComments:bool) =
        let tempSourceDoc = copyDoc doc
        let newDoc = new HtmlAgilityPack.HtmlDocument()
        let rec loop (ndSource:HtmlAgilityPack.HtmlNode) (ndDestination:HtmlAgilityPack.HtmlNode) =
            let copyNode (ndSource:HtmlNode) (parentNodeDestination:HtmlNode) =
                let tempNode = newDoc.CreateElement(ndSource.Name)                        
                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
                parentNodeDestination.ChildNodes.Append(tempNode)
                ndSource.ChildNodes |> Seq.iter(fun x->loop x tempNode)                
            match ndSource.NodeType with
                | HtmlAgilityPack.HtmlNodeType.Document ->
                    ndSource.ChildNodes |> Seq.iter(fun x->loop x newDoc.DocumentNode)
                | HtmlAgilityPack.HtmlNodeType.Comment ->
                    if copyComments then
                        copyNode ndSource ndDestination
                    else
                        ()
                | HtmlAgilityPack.HtmlNodeType.Text ->
                    let tempText = ndSource.InnerText
                    let tempRealText = System.Web.HttpUtility.HtmlDecode(tempText).Trim()
                    if (tempText.Trim().Length > 0) && ( (wordCount tempRealText) > 0) then
                        ndDestination.ChildNodes.Append(newDoc.CreateTextNode("" + tempText + ""))
                    else
                        ()
                | HtmlAgilityPack.HtmlNodeType.Element ->
                    let magicTagValue =
                        let foundMagicTag = ndSource.Attributes |> Seq.exists(fun x->x.Name="###")
                        if foundMagicTag then ndSource.Attributes.Item("###").Value else ""
                    match magicTagValue with
                        | "DELETE ME" ->
                            ()
                        | "IGNORE ME" ->
                            ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
                        |_ -> copyNode ndSource ndDestination
                |_ -> raise (new System.ArgumentOutOfRangeException("what the heck kind of node is this?"))
        loop tempSourceDoc.DocumentNode newDoc.DocumentNode
        newDoc


    let processDocIgnoresAndDeletes (srcDoc:HtmlDocument) (cfg:tpSTRConfig list) =
        let processedIgnores = cfg |> List.fold (fun acc x->markDocIgnores acc x.ignoreList) srcDoc
        let processedBoth =    cfg |> List.fold (fun acc x->markDocDeletes acc x.deleteList) processedIgnores
        let filteredDoc = filterDoc processedBoth false
        filteredDoc

    let procUrl(url:string) = 
        let cfg = getConfigsForUrl url
        let htmlText = http url
        let initialRegZapHtml = cfg |> List.fold(fun acc x->stripOutRegList acc x.regExKillBefore) htmlText
        let regReplaceHtmlText = cfg |> List.fold(fun acc x->
            regFindReplaceList acc x.regExReplace) initialRegZapHtml
        let initialDoc = new HtmlAgilityPack.HtmlDocument()
        initialDoc.LoadHtml(regReplaceHtmlText)
        let cleanDoc = processDocIgnoresAndDeletes initialDoc cfg
        let tempHtml = getDocHtml cleanDoc
        let FinalCleanHtml = cfg |> List.fold(fun acc x->stripOutRegList acc x.regExKillAfter) tempHtml
        let FinalCleanedDoc = new HtmlAgilityPack.HtmlDocument()
        FinalCleanedDoc.LoadHtml(FinalCleanHtml)
        {
            Url                 = url
            configList          = cfg
            InitialHtml         = htmlText
            InitialDoc          = initialDoc
            CleanedDoc          = cleanDoc
            FinalCleanHtml      = FinalCleanHtml
            FinalCleanedDoc     = FinalCleanedDoc
            SentenceList        = []
            CleanedText         = String.Empty
            ProcDateTime        = DateTime.Now
        }
    ///<summary>Takes an url, loads the page, then returns a string list of the tree as it looked once it was cleaned of ads and menus</summary>
    let getCleanTreeFromUrl (url:string):string list =
        [""]
    let printCleanTreeFromUrl(url:string) = 
        let tree= getCleanTreeFromUrl url
        tree |> List.iter(fun x->Console.WriteLine x)

    ///<summary>Takes an url, loads the page, then attempts to get useful text from the page</summary>
    let getTextFromUrl (url:string) =
        ""
    let printTextFromUrl (url:string) =
        ""

    let getSentencesFromUrl (url:string) =
        [""]
    let getSentencesFromDoc (doc:HtmlDocument) =
        [""]