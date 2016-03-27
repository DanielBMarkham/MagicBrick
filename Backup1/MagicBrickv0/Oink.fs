#if INTERACTIVE
    #load "TypeUtils.fs"
    #load "Utils.fs"
    #r "C:\\Users\\Daniel Markham\\Documents\\Visual Studio 2010\\Projects\\antiap\\antiap\\bin\\Debug\\HtmlAgilityPack.dll"
    #r "C:\\Users\\Daniel Markham\\Documents\\Visual Studio 2010\\Projects\\MagicBrickv0\\htmlparser_v314\\HTMLparser\\bin\\Debug\\HTMLparserLib.dll"
#else
module Oink
#endif
    open System
    open TypeUtils
    open Utils
    open HtmlAgilityPack


    type enumNodeType = 
        | None      = 0
        | Root      = 1
        | Comment   = 2
        | Document  = 3
        | Element   = 4
        | Text      = 5
    type tpAttribute =
        {
            Name    : String
            Value   : String
        }
    type tpHtmlNode =
        {
            GID                     : Guid
            Name                    : String
            NodeType                : enumNodeType
            _innerText              : String
            AttributeList           : tpAttribute list
            AncestorGIDList         : Guid list
            ChildrenGIDList         : Guid list
            InitialOrder            : int
        }
        override this.ToString() =
            if this.NodeType = enumNodeType.Text then
                this._innerText
            else
                let t1 = "<" + this.Name
                let t2 = 
                    if this.AttributeList.Length > 0 then 
                        let temp = " " + String.Join(" ", this.AttributeList |> List.map(fun x->x.Name + "=\"" + x.Value + "\" "))
                        temp.TrimRight 1
                    else
                        ""
                let t3 = ">"
                t1 + t2 + t3
    type tpNodeMatchPattern = 
        {
            TagName     : string
            ID          : string
            Class       : string
            AnyAttName  : string
            AnyAttValue : string
            InnerText   : string
        }

    type tpCommand =
        | RegDelete         = 0
        | RegSlice          = 1
        | HtmlToNodes       = 2
        | DeleteNodes       = 3
        | PromoteNodes      = 4
        | NodesToHtml       = 5

    type tpCommandDataTypes =
        | RegStringPair     of (string*string)
        | HtmlText          of string
        | HtmlNode          of tpHtmlNode
        | NodeMatchPattern  of string

    let nodeListToMap (ndList:tpHtmlNode list) = ndList |> List.map(fun x->(x.GID, x)) |> Map.ofList
    let treeWalkBottomUp (rootNode:tpHtmlNode) (f:(tpHtmlNode->tpHtmlNode->Map<Guid,tpHtmlNode>->unit)) (nodeMap:Map<Guid,tpHtmlNode>) =
        let rec loop (ndParent:tpHtmlNode) (nd:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>)=
            if nd.ChildrenGIDList.Length > 0 then
                nd.ChildrenGIDList |> List.iter(fun x->loop nd (nodeMap.Item x) nodeMap)
            else
                f ndParent nd nodeMap
        rootNode.ChildrenGIDList |> List.iter(fun x->loop rootNode (nodeMap.Item x) nodeMap)
    let treeWalkTopDown (rootNode:tpHtmlNode) (f:(tpHtmlNode->Map<Guid,tpHtmlNode>->unit)) (nodeMap:Map<Guid,tpHtmlNode>) =
        let rec loop (nd:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>)=
            f nd nodeMap
            nd.ChildrenGIDList |> List.iter(fun x->loop (nodeMap.Item x) nodeMap)
        loop rootNode nodeMap

    ///<summary> send me the root node, the function that takes a parent, node and map and returns a map, and the original map,
    ///and I will return back to you the modified map after applying the function to all nodes in the tree, from bottom up</summary>
    let treeMapBottomUp (rootNode:tpHtmlNode) (f:(tpHtmlNode->tpHtmlNode->Map<Guid,tpHtmlNode>->Map<Guid,tpHtmlNode>)) (nodeMap:Map<Guid,tpHtmlNode>) =
        let rec loop (ndParent:tpHtmlNode) (nd:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>)=
            let newAcc = 
                if nd.ChildrenGIDList.Length >0 then
                    nd.ChildrenGIDList |> (List.fold(fun (acc:Map<Guid,tpHtmlNode>) x->
                    loop nd (acc.Item x) acc
                    ) nodeMap)
                else
                    f ndParent nd nodeMap
            newAcc
        rootNode.ChildrenGIDList |> (List.fold(fun acc x->loop rootNode (nodeMap.Item x) acc) nodeMap)
    ///<summary> send me the root node, the function that takes a parent, node and map and returns a map, and the original map,
    ///and I will return back to you the modified map after applying the function to all nodes in the tree, from top down</summary>
    let treeMapTopDown (rootNode:tpHtmlNode) (f:(tpHtmlNode->Map<Guid,tpHtmlNode>->Map<Guid,tpHtmlNode>)) (nodeMap:Map<Guid,tpHtmlNode>) =
        let rec loop (nd:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>)=
            let ret = f nd nodeMap
            nd.ChildrenGIDList |> (List.fold(fun (acc:Map<Guid,tpHtmlNode>) x->
            loop (acc.Item x) acc
            ) ret)        
        loop rootNode nodeMap

    let treeFilterTopDown (rootNode:tpHtmlNode) (f:(tpHtmlNode->Map<Guid,tpHtmlNode>->bool)) (nodeMap:Map<Guid,tpHtmlNode>) =
        let rec loop (nd:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>)=
            // the the function matches, kill this node and all the subnodes
            if f nd nodeMap then
                let temp1 = nodeMap.Remove(nd.GID)
                let kidMap = treeMapTopDown nd (fun nd mp ->mp.Remove(nd.GID)) temp1
                temp1
            else
                nd.ChildrenGIDList |> (List.fold(fun acc x->
                    loop (acc.Item x) acc
                    ) nodeMap)
        loop rootNode nodeMap

    let nodeSanityCheck (nodeMap:Map<Guid,tpHtmlNode>) =
        let parentsHaveValidKids = nodeMap |> (Map.fold(fun acc k v->
            let kidGuidsExistInMap = (v.ChildrenGIDList.Length=0) || v.ChildrenGIDList |> List.exists(fun x->nodeMap.ContainsKey x)
            let killme = 9
            acc && kidGuidsExistInMap
            ) true)        
        let kidsHaveValidAncestors = nodeMap |> (Map.fold(fun acc k v->
            let ancestorsExistInMap = (v.AncestorGIDList.Length =0 ) || (v.AncestorGIDList |> List.exists(fun x->nodeMap.ContainsKey x))
            let killme = 9
            acc && ancestorsExistInMap
            ) true)
        parentsHaveValidKids && kidsHaveValidAncestors

    let mkNode (hapNode:HtmlAgilityPack.HtmlNode) (parentNode:tpHtmlNode) (nodeOrder: int) = 
        let nodeType = 
            match hapNode.NodeType with
                | HtmlNodeType.Comment -> enumNodeType.Comment
                | HtmlNodeType.Document -> enumNodeType.Document
                | HtmlNodeType.Element -> enumNodeType.Element
                | HtmlNodeType.Text -> enumNodeType.Text
                |_ -> enumNodeType.None // error
        let newAncestors = [parentNode.GID] |> List.append parentNode.AncestorGIDList
        let attList = hapNode.Attributes |> Seq.toList |> List.map(fun x->{Name=x.Name; Value=x.Value})
        let innerTxt = 
            if hapNode.NodeType = HtmlNodeType.Text then hapNode.InnerText else ""
        let newNode = 
            {
                GID                     = Guid.NewGuid()
                Name                    = hapNode.Name
                NodeType                = nodeType
                _innerText              = innerTxt
                AttributeList           = attList
                AncestorGIDList         = newAncestors
                ChildrenGIDList         = []
                InitialOrder            = nodeOrder
            }
        newNode

    // convert HtmlText nodes to tpHtmlNodes
    let getNodes (htmlText:string):Map<Guid,tpHtmlNode> =
        let hapDoc = new HtmlAgilityPack.HtmlDocument()
        hapDoc.LoadHtml(htmlText)
        let rootNode =
            {
                GID                     = Guid.NewGuid()
                Name                    = "Root"
                NodeType                = enumNodeType.Root
                _innerText              = ""
                AttributeList           = []
                AncestorGIDList         = []
                InitialOrder            = 0
                ChildrenGIDList         = []
            }
        let rec loop (hapNode:HtmlNode) (dhapParentNode:tpHtmlNode) (acc1:Map<Guid,tpHtmlNode>) (pairedChildrenList:(tpHtmlNode*HtmlNode) List) orderAcc =
            // parent node doesn't know who it's kids are
            let newParentKidList = pairedChildrenList |> List.map(fun x->(fst x).GID)
            let newParent ={ dhapParentNode with ChildrenGIDList = newParentKidList; InitialOrder = orderAcc}
            let totalChangedMap:Map<Guid,tpHtmlNode> = [newParent] |> nodeListToMap
            let newAcc = 
               totalChangedMap |> Map.fold(fun (acc:Map<Guid,tpHtmlNode>) k v->
                        if acc.ContainsKey(k) then
                            let newMap = acc.Remove(k)
                            newMap.Add (k,v)
                        else
                            acc.Add (k, v)
                    ) acc1            
            pairedChildrenList |> (List.fold(fun acc2 n->
                let grandkidNodes = (snd n).ChildNodes |> Seq.toList |> List.mapi(fun i x->(mkNode x (fst n) (orderAcc + i), x))
                loop (snd n) (fst n) acc2 grandkidNodes (orderAcc + 1 + grandkidNodes.Length)
                    ) newAcc)
        let originalPairedKidList = hapDoc.DocumentNode.ChildNodes  |> Seq.toList |> List.mapi(fun i x->(mkNode x rootNode (i + 1), x))
        let temp = loop hapDoc.DocumentNode rootNode Map.empty originalPairedKidList (1 + originalPairedKidList.Length)
        temp

    let makeParser (htmlText:string) =
        let oP = new Majestic12.HTMLparser()
        // This is optional, but if you want high performance then you may
        // want to set chunk hash mode to FALSE. This would result in tag params
        // being added to string arrays in HTMLchunk object called sParams and sValues, with number
        // of actual params being in iParams. See code below for details.
        //
        // When TRUE (and its default) tag params will be added to hashtable HTMLchunk (object).oParams
        oP.SetChunkHashMode(false)

        // if you set this to true then original parsed HTML for given chunk will be kept - 
        // this will reduce performance somewhat, but may be desireable in some cases where
        // reconstruction of HTML may be necessary
        oP.bKeepRawHTML<-false

        // if set to true (it is false by default), then entities will be decoded: this is essential
        // if you want to get strings that contain final representation of the data in HTML, however
        // you should be aware that if you want to use such strings into output HTML string then you will
        // need to do Entity encoding or same string may fail later
        oP.bDecodeEntities<-true

        // we have option to keep most entities as is - only replace stuff like &nbsp; 
        // this is called Mini Entities mode - it is handy when HTML will need
        // to be re-created after it was parsed, though in this case really
        // entities should not be parsed at all
        oP.bDecodeMiniEntities<-true

        if (not oP.bDecodeEntities) && oP.bDecodeMiniEntities then
            oP.InitMiniEntities()
        else
            ()

        // if set to true, then in case of Comments and SCRIPT tags the data set to oHTML will be
        // extracted BETWEEN those tags, rather than include complete RAW HTML that includes tags too
        // this only works if auto extraction is enabled
        oP.bAutoExtractBetweenTagsOnly<-true

        // if true then comments will be extracted automatically
        oP.bAutoKeepComments<-true

        // if true then scripts will be extracted automatically: 
        oP.bAutoKeepScripts<-true

        // if this option is true then whitespace before start of tag will be compressed to single
        // space character in string: " ", if false then full whitespace before tag will be returned (slower)
        // you may only want to set it to false if you want exact whitespace between tags, otherwise it is just
        // a waste of CPU cycles
        oP.bCompressWhiteSpaceBeforeTag<-true

        // if true (default) then tags with attributes marked as CLOSED (/ at the end) will be automatically
        // forced to be considered as open tags - this is no good for XML parsing, but I keep it for backwards
        // compatibility for my stuff as it makes it easier to avoid checking for same tag which is both closed
        // or open
        oP.bAutoMarkClosedTagsWithParamsAsOpen<-false
        
        // NOTE: bear in mind that when you deal with content which uses non-Latin chars, then you
        // need to ensure that correct encoding is set, this often set in HTML itself, but sometimes
        // only in HTTP headers for a given page - some pages use BOTH, but browsers seem to 
        // consider HTTP header setting as more important, so it is best to behave in similar way.

        // See below for code that deals with META based charset setting, similarly you need to call
        // it here if charset is set in Content-Type header

        // we will track whether encoding was set or not here, this is important
        // because we may have to do re-encoding of text found BEFORE META tag, this typically 
        // happens for TITLE tags only - if we had no encoding set and then had it set, then
        // we need to reencode it, highly annoying, but having garbage in title is even more annoying
        let bEncodingSet=false;

        // debug:
        oP.SetEncoding(System.Text.Encoding.GetEncoding("iso-8859-1"));
        oP.Init(htmlText)
        oP

    let parseNext (oP:Majestic12.HTMLparser) =                
        seq {   let v = ref (oP.ParseNext())
                while (!v) <> null do
                    yield v
                do v := oP.ParseNext() };;

    
    let killme =
        let p = makeParser "<html><head><title>My Awesome Title!</title><script src=\"http://www.cnn.com\"></script></head><body><h1>Cat1</h1><p>cat2</p><p><strong>I am the walrus</strong></p><div id=\"section 1\"></div><div id=\"section 2\"><p><em>Very important text here</em></p><p>Another paragraph</p></div></body></html>"
        let pN = parseNext p
        pN |> Seq.iter(fun x->printf "%s" x.Value.sTag)
