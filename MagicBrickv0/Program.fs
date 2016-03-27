// Learn more about F# at http://fsharp.net
open System
open HtmlAgilityPack



//    ///<summary> this walks the tree, copying the pieces over and making a new doc with only interesting nodes
//    /// Nodes have previously been marked with attribute "###" and "DELETE ME" or "IGNORE ME" 
//    /// (delete whole thing or delete just that node and copy children up</summary>
//    /// <param name="doc">The source document</param>
//    /// <param name="copyComments">Whether or not to copy comments over</param>
//    /// <returns>A new document with nodes copied from the original document</returns>
//    let filterDoc (doc:HtmlAgilityPack.HtmlDocument) (copyComments:bool) =
//        let newDoc = new HtmlAgilityPack.HtmlDocument()
//        let bracketInSpaces (nd:HtmlNode) = 
//            let parentDoc = nd.OwnerDocument
//            let firstSpace = parentDoc.CreateTextNode(" " )
//            let lastSpace = parentDoc.CreateTextNode(" " )
//            nd.PrependChild firstSpace |> ignore
//            nd.AppendChild lastSpace
//
//        let rec loop (ndSource:HtmlAgilityPack.HtmlNode) (ndDestination:HtmlAgilityPack.HtmlNode) =
//            let copyNode (ndSource:HtmlNode) (parentNodeDestination) =
//                let tempNode = newDoc.CreateElement(ndSource.Name)                        
//                ndSource.Attributes.Iter(fun x->tempNode.Attributes.Add(x))
//                parentNodeDestination.ChildNodes.Append(tempNode)
//                ndSource.ChildNodes |> Seq.iter(fun x->loop x tempNode)
//
//            match ndSource.NodeType with
//                | HtmlAgilityPack.HtmlNodeType.Document ->
//                    ndSource.ChildNodes |> Seq.iter(fun x->loop x newDoc.DocumentNode)
//                | HtmlAgilityPack.HtmlNodeType.Comment ->
//                    if copyComments then
//                        copyNode ndSource ndDestination
//                    else
//                        ()
//                | HtmlAgilityPack.HtmlNodeType.Text ->
//                    let tempText = ndSource.InnerText
//                    let tempRealText = System.Web.HttpUtility.HtmlDecode(tempText).Trim()
//                    if (tempText.Trim().Length > 0) && ( (wordCount tempRealText) > 0) then
//                        ndDestination.ChildNodes.Append(newDoc.CreateTextNode(" " + tempText + " "))
//                    else
//                        ()
//                | HtmlAgilityPack.HtmlNodeType.Element ->
//                    match ndSource.Name with
//                        | "head" | "HEAD" ->
//                            ()
//                        | "input" | "INPUT" ->
//                            ()
//                        | "button" | "BUTTON" ->
//                            ()
//                        | "form" | "FORM" ->
//                            ()
//                        | "iframe" | "IFRAME" ->
//                            ()
//                        | "a" | "A" ->
//                            ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        //| "div" | "DIV" ->
//                        //    ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        | "img" | "IMG" ->
//                            let possAltText = ndSource.Attributes |> Seq.filter(fun x->x.Name = "alt" || x.Name = "ALT")
//                            if possAltText |> Seq.length > 0 then
//                                let altAtt = (possAltText |> Seq.nth 0)
//                                if altAtt.Value.Trim().Length > 0 then
//                                    ()//ndDestination.ChildNodes.Append(newDoc.CreateTextNode(" [PIC: " + (altAtt.Value) + "] "))
//                                else
//                                    // picture no alt text
//                                    ()//ndDestination.ChildNodes.Append(newDoc.CreateTextNode(" [PIC] "))
//                            else
//                                ()
//                        | "blockquote" | "BLOCKQUOTE" | "q" | "Q" ->
//                            let tempNode = newDoc.CreateElement("p")
//                            tempNode.AppendChild(newDoc.CreateTextNode(" --[quote]-- ")) |> ignore
//                            ndDestination.ChildNodes.Append(tempNode)
//                            ndSource.ChildNodes |> Seq.iter(fun x->loop x tempNode)
//                        | "style" | "STYLE" ->
//                            ()
//                        | "script" | "SCRIPT"  | "noscript" | "NOSCRIPT"->
//                            ()
//                        //| "span" | "SPAN" ->
//                        //    ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        | "b" | "B" | "strong" | "STRONG" | "em" | "EM" | "i" | "I" | "center" | "CENTER" | "c" | "C" | "font" | "FONT" ->
//                            bracketInSpaces ndSource |> ignore
//                            ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        | "p" | "P" ->
//                            // make sure there are kid nodes
//                            // for some reason we get p nodes that are empty
//                            if ndSource.InnerText <> "" || ndSource.HasChildNodes then
//                                // make sure there is an end to the sentence on a p -- assumed that if it's
//                                // in a p tag, it's a sentence/paragraph
//                                let trimTxt = ndSource.LastChild.InnerText.Trim()
//                                let trimTxtLen = trimTxt.Length
//                                let lastCharacter = 
//                                    if trimTxt.Length > 0 then trimTxt.Substring(trimTxtLen - 1, 1) else ""
//                                if  sentenceEnd.IsMatch(lastCharacter) then
//                                    // if the new node is a text node, and there is already a text node, replace it
//                                    // with a new text node with the text concatenated
//                                    // note that we replace it in the source document node, then proceed with processing
//                                    let tempText = ndSource.LastChild.InnerText
//                                    let replacementNode = ndSource.OwnerDocument.CreateTextNode(tempText + ".")
//                                    ndSource.ChildNodes.Replace(ndSource.ChildNodes.Count - 1, replacementNode)
//                                    ndSource.ChildNodes |> Seq.iter(fun x-> loop x replacementNode)
//                                else
//                                    let tempNode = newDoc.CreateElement(ndSource.Name)                        
//                                    ndSource.Attributes.ForEach(fun x->tempNode.Attributes.Add(x))
//                                    ndDestination.ChildNodes.Append(tempNode)
//                                    ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode)
//
//                            else // no kid nodes (no text). Skip
//                            ()
//                        |_ ->
//                            let tempNode = newDoc.CreateElement(ndSource.Name)
//                            ndSource.Attributes.ForEach(fun x->tempNode.Attributes.Add(x))
//                            ndDestination.ChildNodes.Append(tempNode)
//                            ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode)
//                |_ ->
//                    () // should never be here. I think we have all nodetypes covered
//        loop doc.DocumentNode newDoc.DocumentNode
//        newDoc;;











//
//    // this walks the tree, making a new doc with only interesting nodes
//    let stripOutDocCrap (doc:HtmlAgilityPack.HtmlDocument) = 
//        let newDoc = new HtmlAgilityPack.HtmlDocument()
//        let badEleTextList = ["</form>"; "</FORM>"]
//        let inBadList (s:string) = badEleTextList |> Seq.exists(fun x->x = s)
//        let bracketInSpaces (nd:HtmlNode) = 
//            let parentDoc = nd.OwnerDocument
//            let firstSpace = parentDoc.CreateTextNode(" " )
//            let lastSpace = parentDoc.CreateTextNode(" " )
//            nd.PrependChild firstSpace |> ignore
//            nd.AppendChild lastSpace
//
//        let rec loop (ndSource:HtmlAgilityPack.HtmlNode) (ndDestination:HtmlAgilityPack.HtmlNode) =
//            match ndSource.NodeType with
//                | HtmlAgilityPack.HtmlNodeType.Document ->
//                    ndSource.ChildNodes |> Seq.iter(fun x->loop x newDoc.DocumentNode)
//                | HtmlAgilityPack.HtmlNodeType.Comment ->
//                    ()
//                | HtmlAgilityPack.HtmlNodeType.Text ->
//                    let tempText = ndSource.InnerText
//                    let tempRealText = System.Web.HttpUtility.HtmlDecode(tempText).Trim()
//                    if (tempText.Trim().Length > 0) && ( (wordCount tempRealText) > 0) && (not (inBadList tempRealText)) then
//                        ndDestination.ChildNodes.Append(newDoc.CreateTextNode(" " + tempText + " "))
//                    else
//                        ndDestination.ChildNodes.Append(newDoc.CreateTextNode(""))
//                | HtmlAgilityPack.HtmlNodeType.Element ->
//                    match ndSource.Name with
//                        | "head" | "HEAD" ->
//                            ()
//                        | "input" | "INPUT" ->
//                            ()
//                        | "button" | "BUTTON" ->
//                            ()
//                        | "form" | "FORM" ->
//                            ()
//                        | "iframe" | "IFRAME" ->
//                            ()
//                        | "a" | "A" ->
//                            ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        //| "div" | "DIV" ->
//                        //    ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        | "img" | "IMG" ->
//                            let possAltText = ndSource.Attributes |> Seq.filter(fun x->x.Name = "alt" || x.Name = "ALT")
//                            if possAltText |> Seq.length > 0 then
//                                let altAtt = (possAltText |> Seq.nth 0)
//                                if altAtt.Value.Trim().Length > 0 then
//                                    ()//ndDestination.ChildNodes.Append(newDoc.CreateTextNode(" [PIC: " + (altAtt.Value) + "] "))
//                                else
//                                    // picture no alt text
//                                    ()//ndDestination.ChildNodes.Append(newDoc.CreateTextNode(" [PIC] "))
//                            else
//                                ()
//                        | "blockquote" | "BLOCKQUOTE" | "q" | "Q" ->
//                            let tempNode = newDoc.CreateElement("p")
//                            tempNode.AppendChild(newDoc.CreateTextNode(" --[quote]-- ")) |> ignore
//                            ndDestination.ChildNodes.Append(tempNode)
//                            ndSource.ChildNodes |> Seq.iter(fun x->loop x tempNode)
//                        | "style" | "STYLE" ->
//                            ()
//                        | "script" | "SCRIPT"  | "noscript" | "NOSCRIPT"->
//                            ()
//                        //| "span" | "SPAN" ->
//                        //    ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        | "b" | "B" | "strong" | "STRONG" | "em" | "EM" | "i" | "I" | "center" | "CENTER" | "c" | "C" | "font" | "FONT" ->
//                            bracketInSpaces ndSource |> ignore
//                            ndSource.ChildNodes |> Seq.iter(fun x->loop x ndDestination)
//                        | "p" | "P" ->
//                            // make sure there are kid nodes
//                            // for some reason we get p nodes that are empty
//                            if ndSource.InnerText <> "" || ndSource.HasChildNodes then
//                                // make sure there is an end to the sentence on a p -- assumed that if it's
//                                // in a p tag, it's a sentence/paragraph
//                                let trimTxt = ndSource.LastChild.InnerText.Trim()
//                                let trimTxtLen = trimTxt.Length
//                                let lastCharacter = 
//                                    if trimTxt.Length > 0 then trimTxt.Substring(trimTxtLen - 1, 1) else ""
//                                if  sentenceEnd.IsMatch(lastCharacter) then
//                                    // if the new node is a text node, and there is already a text node, replace it
//                                    // with a new text node with the text concatenated
//                                    // note that we replace it in the source document node, then proceed with processing
//                                    let tempText = ndSource.LastChild.InnerText
//                                    let replacementNode = ndSource.OwnerDocument.CreateTextNode(tempText + ".")
//                                    ndSource.ChildNodes.Replace(ndSource.ChildNodes.Count - 1, replacementNode)
//                                    ndSource.ChildNodes |> Seq.iter(fun x-> loop x replacementNode)
//                                else
//                                    let tempNode = newDoc.CreateElement(ndSource.Name)                        
//                                    ndSource.Attributes.ForEach(fun x->tempNode.Attributes.Add(x))
//                                    ndDestination.ChildNodes.Append(tempNode)
//                                    ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode)
//
//                            else // no kid nodes (no text). Skip
//                            ()
//                        |_ ->
//                            let tempNode = newDoc.CreateElement(ndSource.Name)
//                            ndSource.Attributes.ForEach(fun x->tempNode.Attributes.Add(x))
//                            ndDestination.ChildNodes.Append(tempNode)
//                            ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode)
//                |_ ->
//                    () // should never be here. I think we have all nodetypes covered
//        loop doc.DocumentNode newDoc.DocumentNode
//        newDoc;;
//
//    let allTextDescendents (nd:HtmlAgilityPack.HtmlNode) = 
//        nd.DescendantNodes() |> Seq.filter(fun x->x.NodeType=HtmlAgilityPack.HtmlNodeType.Text)
//    let allTextDescendentsLessThanXWords (nd:HtmlAgilityPack.HtmlNode) x =
//        (allTextDescendents nd) |> Seq.forall(fun y->((decodeWordCount y.InnerText) < x))
//
//    let isPossibleMenu (nd:HtmlAgilityPack.HtmlNode) = 
//        // if it's 4 3-word items or less all by themselves? Prob menu
//        // also 6 4-word items, and 10 5-word items
//        let ret1 = (allTextDescendentsLessThanXWords nd 4) && ((allTextDescendents nd) |> Seq.length > 3)
//        let ret2 = (allTextDescendentsLessThanXWords nd 7) && ((allTextDescendents nd) |> Seq.length > 5)
//        let ret3 = (allTextDescendentsLessThanXWords nd 8) && ((allTextDescendents nd) |> Seq.length > 9)
//        ret1 || ret2 || ret3
//
//    // This function re-walks the tree, makes a new doc 
//    // trying to guess menus and other useless blurbs to hack out
//    let stripOutUIElements (doc:HtmlAgilityPack.HtmlDocument) =
//        let newDoc = new HtmlAgilityPack.HtmlDocument()
//        let badEleTextList = ["</form>"; "</FORM>"]
//        let inBadList (s:string) = badEleTextList |> Seq.exists(fun x->x = s)
//        let rec loop (ndSource:HtmlAgilityPack.HtmlNode) (ndDestination:HtmlAgilityPack.HtmlNode) =
//            match ndSource.NodeType with
//                | HtmlAgilityPack.HtmlNodeType.Document ->
//                    ndSource.ChildNodes |> Seq.iter(fun x->loop x newDoc.DocumentNode)
//                | HtmlAgilityPack.HtmlNodeType.Text ->
//                    ndDestination.ChildNodes.Append(newDoc.CreateTextNode(ndSource.InnerText))
//                | HtmlAgilityPack.HtmlNodeType.Element ->
//                    let classAtt = ndSource.Attributes.Item("class")
//                    match ndSource.Name with
//                        |_ ->
//                            // it's a menu 
//                            if isPossibleMenu ndSource then
//                                ()
//                            elif classAtt <> null && 
//                                (classAtt.Value.Contains("comment") || 
//                                    (["LINK"; "LINKS"; "EXTENDEDBIO"; "BIO"; "AUTHORS"; "SPONSOR"; "SKIPNAV"; "AUTHOR"; "HEADLINE"; "PRICE"; "NEWSLETTERS"; "NEWSLETTER"; "SUBSCRIBE" ; "FB-LIKE-EXPLAINER"] |> List.exists(fun x->x=classAtt.Value.ToUpper())))
//                                then ()
//                            else
//                                let tempNode = newDoc.CreateElement(ndSource.Name)
//                                ndDestination.ChildNodes.Append(tempNode)
//                                ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode)
//                |_ ->
//                    () // should never be here. I think we have all nodetypes covered
//        loop doc.DocumentNode newDoc.DocumentNode
//        newDoc;;
//   
//
//    let stripOutStringCrap(s:string) = 
//        let regDisqusStartFind = new System.Text.RegularExpressions.Regex(@"\<\!\-\- disqus \-\-\>")
//        let regDisqusEndFind = new System.Text.RegularExpressions.Regex(@"comments powered by Disqus\.\</a\>\</noscript\>")
//        let regDisqusBeg = regDisqusStartFind.Matches(s).toArray
//        let regDisqusEnd = regDisqusEndFind.Matches(s).toArray
//        let s1 =
//            if regDisqusBeg.Length > 0 && regDisqusEnd.Length > 0 then
//                let m1 = regDisqusBeg.[0].Index
//                let m1l = regDisqusBeg.[0].Value.Length
//                let m2 = regDisqusEnd.[0].Index
//                let m2l = regDisqusEnd.[0].Value.Length
//                s.Substring(0, m1) + s.Substring(m2 + m2l)
//            else
//                s
//        let regNBSP = new System.Text.RegularExpressions.Regex(@"&nbsp;")
//        let s2 = regNBSP.Replace(s1, " ")
//        s2
//
//
//    type ndInd = HtmlNode * int
//    type ndIndOpt = ndInd option
//    let collapseTextNodes (sourceDoc:HtmlDocument) = 
//        let newDoc = new HtmlAgilityPack.HtmlDocument()
//        let rec loop (ndSource:HtmlAgilityPack.HtmlNode) (ndDestination:HtmlAgilityPack.HtmlNode) (firstTextNode:ndIndOpt) =
//            match ndSource.NodeType with
//                | HtmlNodeType.Document ->
//                    ndSource.ChildNodes |> Seq.iter(fun x->loop x newDoc.DocumentNode None)
//                | HtmlNodeType.Text ->
//                    let tempText = ndSource.InnerText
//                    if firstTextNode.IsSome then
//                        // if the new node is a text node, and there is already a text node, replace it
//                        // with a new text node with the text concatenated
//                        let replacementNode = newDoc.CreateTextNode((fst firstTextNode.Value).InnerText + tempText)
//                        ndDestination.ChildNodes.Replace((snd firstTextNode.Value), replacementNode)
//                    else
//                        // if it's a text node and there is no new node
//                        // if children, loop the kids with no last text node
//                        // no kids, go to the next node
//                        if ndSource.HasChildNodes then
//                            let tempNode = newDoc.CreateTextNode(ndSource.InnerText)
//                            ndDestination.ChildNodes.Append(tempNode)
//                        else
//                            let tempNode = newDoc.CreateTextNode(ndSource.InnerText)
//                            ndDestination.ChildNodes.Append(tempNode)
//                            ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode None)
//                |_ ->
//                    let tempNode = newDoc.CreateElement(ndSource.Name)                        
//                    ndDestination.ChildNodes.Append(tempNode)
//                    ndSource.ChildNodes |> Seq.iter(fun x-> loop x tempNode None)
//        loop sourceDoc.DocumentNode newDoc.DocumentNode None
//        newDoc
//    let printNode (nd:HtmlNode) = 
//        let dispNode (nd:HtmlNode) =
//            let t1 = "<" + nd.Name
//            let t2 = String.Join(" ", nd.Attributes |> Seq.map(fun x->x.Name + "=\"" + x.Value + "\" "))
//            let t3 = ">"
//            t1 + t2 + t3
//        let hasTextKids (par:HtmlNode) =
//            par.ChildNodes |> Seq.exists(fun x->x.NodeType = HtmlNodeType.Text)
//        let rec loop indentLevel (theNode:HtmlNode) = 
//            if theNode.DescendantNodes() |> Seq.length > 0 then
//                Console.Write ("\r\n" + "".PadLeft(indentLevel * 2))
//                Console.Write (dispNode theNode)
//                if hasTextKids theNode
//                    then
//                        // print kid nodes
//                        theNode.ChildNodes |> Seq.filter(fun x->x.NodeType=HtmlNodeType.Text && x.InnerText.Trim().Length > 0) |> Seq.iter(fun x->
//                            Console.Write ("\r\n" + "".PadLeft((indentLevel + 1) * 2))
//                            Console.Write ("'" + htmlDecode (x.InnerText.Trim()) + "'" )
//                            )
//                    else
//                        ()
//                // process the nontext kids
//                theNode.ChildNodes |> Seq.filter(fun x->x.NodeType<>HtmlNodeType.Text) |> Seq.iter(fun x->
//                    loop (indentLevel + 1) x)
//                else
//                    ()
//        loop 0 nd
//
//    let dispTextNodes sUrl = 
//        let htmlText = http sUrl
//        let cleanHtmlText = stripOutStringCrap htmlText
//        let sourceDoc = new HtmlAgilityPack.HtmlDocument()
//        sourceDoc.LoadHtml(cleanHtmlText)
//        let cleanDoc = stripOutDocCrap sourceDoc
//        let cleanDocZapUI = stripOutUIElements cleanDoc
//        printNode cleanDocZapUI.DocumentNode
//
//    let mkText sUrl = 
//        let htmlText = http sUrl
//        let cleanHtmlText = stripOutStringCrap htmlText
//        let sourceDoc = new HtmlAgilityPack.HtmlDocument()
//        sourceDoc.LoadHtml(cleanHtmlText)
//        let cleanDoc = stripOutDocCrap sourceDoc
//        let cleanDocZapUI = stripOutUIElements cleanDoc
//        let cleanNoEmpties = collapseTextNodes cleanDocZapUI
//        let cleanDocNodeList = cleanNoEmpties.DocumentNode.DescendantNodes() |> Seq.toList
//        let cleanDocTextList = cleanDocNodeList |> List.filter(fun x->x.NodeType = HtmlNodeType.Text) |> List.map(fun x->htmlDecode x.InnerText)
//        let cleanDocText = String.Join(" ",(cleanDocTextList))
//        // eliminate dupe spaces
//        let regElimDupes = new System.Text.RegularExpressions.Regex("\s+")
//        let noDupeSpaces = regElimDupes.Replace(cleanDocText, " ")
//        noDupeSpaces
//
//    let mkSentences sUrl = 
//        let noDupeSpaces = mkText sUrl
//        // now I have one long stream of text. Time to make sentences
//        let regMakeSentences = new System.Text.RegularExpressions.Regex("[\\.\\!\\?]\\s+", System.Text.RegularExpressions.RegexOptions.Multiline)
//        let sentences = regMakeSentences.splitKeepToken(noDupeSpaces)
//        sentences |> List.mapi(fun i x->(i,x))
//
//    // determine how likely the words are in a sentence compared to average English sentences
//    let scoreSentenceByWordRarity (s:string) =
//        let wordSplit = s.Split([|' '; ','; '!'; '?'; ':'; ':'|]) //regexWordMatch.Split(s)
//        let wordCountForScored = wordSplit |> Seq.filter(fun x->mapStringKeyCaseInsContainsKey x wordFrequencyMap) |> Seq.length
//        let lookupWordFreq (w:string) = 
//            if (mapStringKeyCaseInsContainsKey w wordFrequencyMap) then (mapStringKeyCaseInsItem w wordFrequencyMap) else 0.0
//        let sentenceScore = wordSplit |> Seq.fold(fun (acc:float) x->acc + lookupWordFreq(x)) 0.0
//        let averageWordScore = sentenceScore / (float wordCountForScored)
//        averageWordScore
//
//    let topNUncommonSentences n (sl:(int*string) list) = 
//        let calcList = sl |> List.map(fun x->
//            let order = fst x
//            let score = scoreSentenceByWordRarity(snd x)
//            let text = snd x
//            (order, score, text))
//        let rankList = calcList |> List.sortWith(fun x y->
//            let order1, score1, text1 = x 
//            let order2, score2, text2 = x
//            compare (score1) (score2))
//        let topMost = rankList |> List.toSeq |> Seq.take n |> Seq.toList
//        let putBackInOriginalOrder = topMost |> List.sortWith(fun x y ->
//            let order1, score1, text1 = x 
//            let order2, score2, text2 = x
//            compare (order1) (order2))
//        putBackInOriginalOrder |> List.map(fun x->
//            let order,score,text = x
//            (order,text))
//       
//    (*in reallity one needs a more sophisticated tokenizer*)  
//    let breakIntoWords (doc_text: string) =   
//        let sep = [|' '; '\r'; '\n'; '-'; '.'; ',';'\t';'!';'?';'\''|] 
//        let opt =  System.StringSplitOptions.RemoveEmptyEntries
//        doc_text.Split(sep, opt)  
//        |> Array.map (fun (str: string) -> str.ToLower())
//
//    let phraseDistForNSizedChunks (s:string) (n:int) = 
//        let words = breakIntoWords s |> Seq.ofArray
//        let wordChunks = words |> (Seq.windowed n)
//        wordChunks |> Seq.countBy id |> Seq.sortBy(fun a-> -(snd a))
//
//    let phraseDist (s:string) (chunkSize:int) (topCommonWordsToDiscard:int) =
//        let phraseDist = phraseDistForNSizedChunks s chunkSize
//        let topCommonWords = getTopNSetWordFrequency topCommonWordsToDiscard
//        let filterPhrase = phraseDist |> Seq.filter(fun x->
//            let a,b = x
//            let itsAllCommonWords = a |> Array.forall(fun x->topCommonWords.ContainsKey x || topCommonWords.ContainsKey (x.ToUpper()))
//            not itsAllCommonWords)
//        let sum = filterPhrase |> Seq.sumBy(fun x->snd x)
//        let filterPhraseWithFreq = filterPhrase |> Seq.map(fun x->
//            let wordArr, wordCount = x
//            let wordString = String.Join(" ", wordArr)
//            let wordFreq = float wordCount / float sum
//            (wordString, wordFreq)
//            )
//        let fMap = filterPhraseWithFreq |> Map.ofSeq
//        fMap
//
//    let getTopNPhrasesInText (txt:string) (chunkSize:int) (n:int) = 
//        let phraseList = phraseDist txt chunkSize 100
//        if phraseList |> Seq.length > n then
//            phraseList |> Map.toSeq |> (Seq.take n) |> Map.ofSeq
//        else
//            phraseList
//
//    let scoreSentenceByDocFreq (sentence:string) (documentWordPhraseFreq:Map<string, float>) (chunkSize:int) = 
//        let sentencePhraseFreq = phraseDistForNSizedChunks sentence chunkSize
//        let spfJoined = sentencePhraseFreq |> Seq.map(fun x->
//            let phrase = String.Join(" ", (fst x))
//            (phrase, snd x)) |> Map.ofSeq
//        let sentenceScore = spfJoined |> Seq.sumBy(fun x->
//            let count = x.Value
//            let wordPhrase = x.Key
//            let wordCount = wordCount sentence
//            let docFreq = if documentWordPhraseFreq.ContainsKey(wordPhrase) then documentWordPhraseFreq.Item(wordPhrase) else 0.0
//            let itemTotal = float count * docFreq / float wordCount
//            itemTotal)
//        sentenceScore
//
//    let rankSentencesByDocFreq (sentences:(int*string) list) (doc:string) (chunkSize:int) = 
//        let docPhraseFreq = phraseDist doc chunkSize 50
//        let scoredSentences = sentences |> List.map(fun x->
//            let order = fst x
//            let score = scoreSentenceByDocFreq (snd x) docPhraseFreq chunkSize
//            (order, score, (snd x)))
//        let sortedSents = scoredSentences |> List.sortBy(fun x->
//            let order, score, text = x
//            score)
//        sortedSents |> List.rev
//
//    let topNSentenncesByDocFreq (sentences:(int*string) list) (doc:string) (chunkSize:int) (cnt:int) =
//        let sents = rankSentencesByDocFreq sentences doc chunkSize
//        let trunkList = sents |> List.toSeq |> Seq.take cnt |> Seq.toList 
//        let retList = trunkList |> List.map(fun x->
//            let a,b,c = x
//            (a,c))
//        retList
//
//    let shortenDocNSentences (docSent:(int*string) list) (n:int) =
//        let sentTry c = int ((float n / float 15) * float c + 0.5)
//        let doc = String.Join(" " , docSent)
//        let r = (topNUncommonSentences (sentTry 3) docSent)
//        let s1 = topNSentenncesByDocFreq docSent doc 1 (sentTry 4)
//        let s2 = topNSentenncesByDocFreq docSent doc 2 (sentTry 3)
//        let s3 = topNSentenncesByDocFreq docSent doc 3 (sentTry 3)
//        let s4 = topNSentenncesByDocFreq docSent doc 4 (sentTry 2)
//        let masterList = List.concat [r; s1; s2; s3; s4]
//        let masterSortedList = masterList |> List.sortBy(fun x->fst x)
//        masterSortedList |> Set.ofList |> Set.toList

    //mkSentences "http://activevoice.charlesbivona.com/2010/07/28/if-you%E2%80%99re-so-damn-smart-why-aren%E2%80%99t-you-rich/"
    //let r = topNUncommonSentences 10 (mkSentences "http://dontgetburnedblog.com/why-most-people-dont-succeed/")
    //let docSent = (mkSentences "http://www.theglobeandmail.com/news/opinions/teenage-girls-can-change-the-world/article1712789/")
    //"http://www.theatlantic.com/technology/archive/2010/08/market-data-firm-spots-the-tracks-of-bizarre-robot-traders/60829/") //"http://www.codinghorror.com/blog/2010/09/youtube-vs-fair-use.html") //"http://sealedabstract.com/rants/why-i-stopped-reading-hn/")
    //let condens = (shortenDocNSentences docSent 15) |> List.map(fun x->snd x)
    //let cText = String.Join(" ", condens )
    //Console.WriteLine cText

//let htmlText = WebUtils.http "http://www.realclearworld.com/articles/2010/09/28/pakistan_and_the_us_exit_from_afghanistan_99203.html"
//let nds = DHAP.getNodes htmlText
let dog = DHAP.MakeDoc (Some "http://www.dailystar.com.lb/article.asp?edition_id=10&categ_id=5&article_id=119811#axzz1101b42qk") None
let docHtml = DHAP.tpHtmlDocument.SaveToHtml dog.FinalNodes
dog.printDoc true true
0