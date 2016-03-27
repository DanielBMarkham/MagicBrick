#if INTERACTIVE
//    ;;
//    #load "TypeUtils.fs"
//    #load "Sink.fs"
//    #load "Utils.fs"
//    #r "System.Xml.dll"
//    #r "C:\\Windows\\assembly\\GAC_32\\System.Web\\2.0.0.0__b03f5f7f11d50a3a\\System.Web.dll"
//    #r "System.ServiceModel.dll"
//    #r "System.Net.dll"
//    #r "C:\\Users\\Daniel Markham\\Documents\\Visual Studio 2010\\Projects\\htmlagilitypack-74794\\Branches\\1.4.0\\HtmlAgilityPack\\bin\\Debug\\HtmlAgilityPack.dll"
//    #load "SiteTextReaderData.fs"
//    #load "WebUtils.fs"
//      #load "Utils.fs"
//module DHAP
#else
module DHAP
#endif
    open System
    open TypeUtils
    open HtmlAgilityPack
    open SiteTextReaderData
    open WebUtils
    open Utils

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
    let hapDOCToHtml (doc:HtmlDocument) =
        let sb = new System.Text.StringBuilder(257000)
        let sw = new System.IO.StringWriter(sb)
        doc.Save(sw)
        sw.Flush()
        sb.ToString()


    type tpHtmlDocument =
        val mutable Nodes                       : Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>
        val mutable private rootGID             : Guid option
        val mutable Url                         : string option
        val mutable configList                  : tpSTRConfig list
        val mutable InitialHtml                 : string
        val mutable InitialRegZappedCleanHtml   : string
        val mutable InitialRegZappedCleanNodes  : Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>
        val mutable DelPromTaggedNodes          : Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>
        val mutable DelPromCleanedNodes         : Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>
        val mutable DelPromCleanedDocHtml       : string
        val mutable FinalRegZapCleanHtml        : string
        val mutable FinalNodes                  : Map<Guid,tpHtmlNode>
        //val mutable SentenceList        : string list
        //val mutable CleanedText         : string
        //val mutable ProcDateTime        : DateTime
        new() as this =
            {
                Nodes                           = Map.empty //[(Guid.NewGuid(), tpHtmlNode.DocRoot)] |> Map.ofList
                rootGID                         = None
                Url                             = None
                configList                      = []
                InitialHtml                     = String.Empty
                InitialRegZappedCleanHtml       = String.Empty
                InitialRegZappedCleanNodes      = Map.empty
                DelPromTaggedNodes              = Map.empty
                DelPromCleanedNodes             = Map.empty
                DelPromCleanedDocHtml           = String.Empty
                FinalRegZapCleanHtml            = String.Empty
                FinalNodes                      = Map.empty
                //SentenceList    :string list
                //CleanedText     :string
                //ProcDateTime    :DateTime
            }
            then
                let rootNode = tpHtmlNode.DocRoot
                this.Nodes <- [(rootNode.GID, rootNode)] |> Map.ofList
                this.rootGID <- Some (rootNode.GID)
        new((nds:Map<Guid,tpHtmlNode>), (url:string option)) =
            {
                Nodes                           = nds
                rootGID                         = None
                Url                             = url
                configList                      = []
                InitialHtml                     = String.Empty
                InitialRegZappedCleanHtml       = String.Empty
                InitialRegZappedCleanNodes      = Map.empty
                DelPromTaggedNodes              = Map.empty
                DelPromCleanedNodes             = Map.empty
                DelPromCleanedDocHtml           = String.Empty
                FinalRegZapCleanHtml            = String.Empty
                FinalNodes                      = Map.empty
                //SentenceList    :string list
                //CleanedText     :string
                //ProcDateTime    :DateTime
            }

//        member this.CleanDoc (masterNodeMap:Map<Guid,tpHtmlNode>) =
//            let cfg =
//                if this.Url.IsSome then
//                    SiteTextReaderData.getConfigsForUrl this.Url.Value
//                else 
//                    SiteTextReaderData.getConfigsForUrl "*" // just use the default base setting
//            this.configList <- cfg
//            // zap with regex, then replace with regex. This is the initial cleaning
//            let initialRegZapHtml = cfg |> List.fold(fun acc x->stripOutRegList acc x.regExKillBefore) this.InitialHtml
//            let regReplaceHtmlText = cfg |> List.fold(fun acc x->
//                regFindReplaceList acc x.regExReplace) initialRegZapHtml
//            this.InitialCleanedHtml <- regReplaceHtmlText
//            // after zapping with regex, re-parse and reload (need to do this to make sure regex work hasn't destroyed doc)
//            let initialCleanedDoc = new HtmlAgilityPack.HtmlDocument()
//            initialCleanedDoc.LoadHtml(this.InitialCleanedHtml)
//            let newNodes = tpHtmlNode.CopyHAPNodeRecursive initialCleanedDoc.DocumentNode (this.RootNode masterNodeMap) 
//
//            let newCleanedNodes = tpHtmlDocument.UpdateAllSiblingInfo (this.RootNode masterNodeMap) masterNodeMap
//            this.RegZapCleanedNodes <- newCleanedNodes
//            let sb = new System.Text.StringBuilder(65536)
//            let sw = new System.IO.StringWriter(sb)
//            initialCleanedDoc.Save(sw)
//            this.RegZapCleanedDocHtml <- sb.ToString()
//            // now promote some nodes and delete other ones
//            let ignoreTaggedNodes = cfg |> List.fold(fun acc1 x->
//                tpHtmlDocument.AddAttributeTagByPatternList x.ignoreList "DHAPFlag" "to_ignore" acc1) newCleanedNodes
//            let deletedAndIgnoreTaggedNodes =  cfg |> List.fold(fun acc1 x->
//                tpHtmlDocument.AddAttributeTagByPatternList x.deleteList "DHAPFlag" "todelete" acc1) ignoreTaggedNodes
//            this.DelPromTaggedNodes<-deletedAndIgnoreTaggedNodes
//            let deletedAndPromotedNodes = tpHtmlDocument.filterNodes deletedAndIgnoreTaggedNodes
//            this.DelPromCleanedNodes <-deletedAndPromotedNodes
//            // after cleaning nodes, make into HTML, run final regex, then re-parse
//
//            let delPromHtml = tpHtmlDocument.SaveToHtml (this.RootNode deletedAndPromotedNodes) deletedAndPromotedNodes
//            this.DelPromCleanedDocHtml <- delPromHtml
//            let finalRegExClean = cfg |> List.fold(fun acc x->stripOutRegList acc x.regExKillAfter) this.DelPromCleanedDocHtml
//            this.FinalRegZapCleanHtml <- finalRegExClean
//            let finalCleanedDoc = new HtmlAgilityPack.HtmlDocument()
//            finalCleanedDoc.LoadHtml(this.InitialCleanedHtml)
//            let cleanNodes = tpHtmlNode.CopyHAPNodeRecursive finalCleanedDoc.DocumentNode (this.RootNode deletedAndPromotedNodes)
//            
//            
//            let retNodes = tpHtmlDocument.UpdateAllSiblingInfo (this.RootNode cleanNodes) cleanNodes
//            retNodes
//        static member private deleteFlaggedNodes (flagAttName:string) (flagAttValue:string) (taggedNodeMap:Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>) =
//            let listOfNodesToDelete = taggedNodeMap |> Map.toList |> List.filter(fun x->
//                (snd x).ContainsAttNameValue flagAttName flagAttValue)
//            let listOfDeleteNodeDescendentGIDs = List.concat (listOfNodesToDelete |> List.map(fun x->
//                let nd = snd x
//                tpHtmlNode.DescendentGIDs nd taggedNodeMap))
//            let GIDsToDelete =  listOfNodesToDelete |> List.map(fun x->fst x) |>
//                                    List.append listOfDeleteNodeDescendentGIDs
//            let DelGIDsNoDupes = removeDupes GIDsToDelete                                    
//            let inDelList gid =  DelGIDsNoDupes |> List.exists(fun x->x = gid)
//                
//            // if it's in the delete list, kill it
//            let nodesRawDelete = taggedNodeMap |> Map.filter(fun k v->
//                not (inDelList v.GID))
//            // if it's in the ancestor/descendent list, adjust par/child
//            let newNodes = nodesRawDelete |> Map.map(fun k v->
//                // if not, make sure it's not in the sibling or parent/descendent list
//                let newChildGIDList = v.ChildrenGIDList |> List.filter(fun x->not (inDelList x))
//                let newNode = {v with ChildrenGIDList = newChildGIDList}
//                newNode
//                )
//            newNodes
//        static member private promoteFlaggedNodes (flagAttName:string) (flagAttValue:string) (taggedNodeMap:Map<Guid,tpHtmlNode>) =
//            // FOR THE IGNORE ITEMS
//            // Loop through the document, check to see if the parent is an ignore
//            // if so, delete then update the kid's parent to be the grandparent
//            // also update the ancestor list for all of the kid's progeny
//            let ignoreMap = taggedNodeMap |> (Map.fold(fun (acc:Microsoft.FSharp.Collections.Map<Guid, tpHtmlNode>) k v->
//                let getByGIDLocalFirst gid =
//                    if acc.ContainsKey(gid) then acc.Item(gid) else taggedNodeMap.Item(gid)
//                let saveToLocalStream (nd:tpHtmlNode) =
//                    Map<_,_>.ReplaceAppendItem (nd.GID, nd) acc
//                // only do something if we are 2 or more kids down the tree
//                if v.AncestorGIDList.Length > 2 && v.NodeType <> enumNodeType.Document then
//                    let dad = getByGIDLocalFirst v.AncestorGIDList.Last
//                    let granddad = getByGIDLocalFirst dad.ParentGID
//                    if dad.ContainsAttNameValue flagAttName flagAttValue then
//                        let newAncestorList = (tpHtmlNode.Ancestors v taggedNodeMap) |> Map.filter(fun k y->y.GID<>dad.GID)
//                        let newSiblingList = granddad.ChildrenGIDList |> listReplace (fun y->y<>dad.GID) v.GID
//                        let newGranddad = { granddad with ChildrenGIDList = newSiblingList}
//                        let newKids = newGranddad.ChildrenGIDList |> List.map(fun x->getByGIDLocalFirst x) |> List.map(fun x->(x.GID,x)) |> Map.ofList
//                        let temp = tpHtmlDocument.updateParentAndKids newGranddad newKids
//                        let n = (fst temp)
//                        (snd temp) |> Map<_,_>.ReplaceAppendItem(n.GID, n) //newAcc
//                    else
//                        acc.Add(k,v)
//                else
//                    acc.Add(k,v)
//                ) Map.empty)
//            ignoreMap

//        static member filterNodes (taggedNodeMap:Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>) =
//            // DELETE THE FLAGGED ITEMS
//            let ml = taggedNodeMap |> Map.toList
//            let ndt = List.nth ml 512
//            let nd = snd ndt
//            let deletedNodes = tpHtmlDocument.deleteFlaggedNodes "DHAPFlag" "to_delete" taggedNodeMap
//            let ignoredNodes = tpHtmlDocument.promoteFlaggedNodes "DHAPFlag" "to_ignore" deletedNodes
//            ignoredNodes
        static member RootGID (masterMap:Map<Guid,tpHtmlNode>) =
            masterMap |> Map.findKey (fun k v->v.NodeType=enumNodeType.Root) 
        static member Root  (masterMap:Map<Guid,tpHtmlNode>) =
            let GID = tpHtmlDocument.RootGID masterMap
            masterMap.Item GID
        static member SaveToHtml (masterMap:Map<Guid,tpHtmlNode>) =
            let rootNode = tpHtmlDocument.Root masterMap
            let sb = new System.Text.StringBuilder(500000)
            let openTagAndValue (nd:tpHtmlNode) = 
                nd.ToString()
            let closeTag (nd:tpHtmlNode) =
                if nd.NodeType <> enumNodeType.Text then
                    "</" + nd.Name + ">"    
                else
                    ""
            let rec loop (nd:tpHtmlNode) (indentLevel:int) (parentNode:tpHtmlNode option) = 
                let indentSpace = ("").PadRight(indentLevel * 2)
                if nd.NodeType <> enumNodeType.Root && not (nd.NodeType = enumNodeType.Text && nd._innerText.Trim().Length = 0) && not (nd.Name = "#comment") then
                    if nd.NodeType <> enumNodeType.Text then
                        sb.Append ("\r\n") |> ignore
                    else
                        ()
                    sb.Append ( indentSpace + (openTagAndValue nd)) |> ignore
                else
                    ()
                // do the kids
                let kids = (tpHtmlNode.ChildNodes nd masterMap) |> Map.map(fun k x->masterMap.Item(x.GID))
                kids |> Map.iter(fun k x->loop x (indentLevel + 1) (Some nd))
                // close the tag
                if nd.NodeType <> enumNodeType.Text && (nd.Name <> "#comment") && (nd.NodeType <> enumNodeType.Root) then
                    sb.Append ( "\r\n" + indentSpace + (closeTag nd)) |> ignore
                else
                    ()
            loop rootNode 0 None
            sb.ToString()
        //static member DocumentNodes (masterNodeMap:Map<Guid,tpHtmlNode>) = (this.RootNode masterNodeMap).ChildNodes
        static member NodeByGID (gid:Guid) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            if masterNodeMap.ContainsKey gid then masterNodeMap.Item(gid)
            else raise (new System.ArgumentNullException("the html doc with this ID does not exist in the collection"))
        static member NodesByGID ndList (masterNodeMap:Map<Guid,tpHtmlNode>)=
            [ for ndGID in ndList do
                let existsInDoc = masterNodeMap.ContainsKey(ndGID)
                if existsInDoc then yield tpHtmlDocument.NodeByGID(ndGID) masterNodeMap else () ]
        // updates kids based on a parent. Sends modified kids and parent back
        static member updateParentAndKids (ndParent:tpHtmlNode) (kids:Map<Guid,tpHtmlNode>):(tpHtmlNode*Map<Guid,tpHtmlNode>) = 
            let areAllKidsPresentInParent = kids |> Map.fold(fun acc k x->acc && (ndParent.ChildrenGIDList |> List.exists(fun y->y=x.GID))) true
            let ndNewParent = 
                if areAllKidsPresentInParent then
                    ndParent
                else
                    {ndParent with ChildrenGIDList = kids |> Map.map(fun k x->x.GID) |> Map.toList |> List.map(fun x->fst x)}
            let commonAncestorList = [ndNewParent.GID] |> List.append ndNewParent.AncestorGIDList
            // for each kid, construct a list of their siblings (a tuple with their GID first, then the sibling list)
            let newKidSiblingList = ndNewParent.ChildrenGIDList |> List.map(fun x->
                (x, (ndNewParent.ChildrenGIDList |> List.filter(fun y->y<>x))))
            // make a list of the changed nodes
            let updatedKidsWithSiblings = newKidSiblingList |> List.map(fun x->
                let nd = 
                    if kids |> Map.containsKey (fst x)
                        then kids |> Map.find (fst x)
                        else raise (new System.Exception("bad"))
                let guidList = (snd x) |> List.map(fun x->x)
                let newNode = {nd with SiblingGIDList = guidList}
                newNode)
            // add the NextSibling and PreviousSibling GUID
            let updatedKidsWithNextPreviousSiblings = updatedKidsWithSiblings |> List.mapi(fun i x->
                let prevSib =
                    if i = 0 then None else Some((updatedKidsWithSiblings.Item (i - 1)).GID)
                let nextSib =
                    if i = updatedKidsWithSiblings.Length - 1 then None else Some((updatedKidsWithSiblings.Item(i + 1)).GID)
                {x with PreviousSiblingGIDopt = prevSib; NextSiblingGIDopt = nextSib}
                )
            let updatedKidsWithEverything = updatedKidsWithNextPreviousSiblings |> List.map(fun x->
                {x with AncestorGIDList = commonAncestorList}
                )
            let newKidMap = updatedKidsWithEverything |> List.map(fun x->(x.GID, x)) |> Map.ofList
            (ndNewParent, newKidMap)

        // used after a bulk Insert
        // It is assumed that each node knows who it's parent and children are (but not sibling info)
        static member UpdateAllSiblingInfo (rootNode:tpHtmlNode) (masterNodeLibrary:Map<Guid,tpHtmlNode>)  =
            let rec loop (ndParentGID:Guid) (acc1:Map<Guid,tpHtmlNode>) =
                // whatever we're looking up, use cache (acc) first, then master list (startingNodes) if not found in cache
                let FindNodeUseCacheFirst gid (accCache:Map<Guid,tpHtmlNode>) =
                    if acc1 |> Map.exists(fun k y->k=gid)
                        then acc1 |> Map.find gid
                        else masterNodeLibrary.Item(gid)
                // when saving, either add or replace existingNode
                let SaveNodeUseCacheFirst (nd:tpHtmlNode) (accCache:Map<Guid,tpHtmlNode>) = 
                    if accCache |> Map.exists(fun k x->k=nd.GID) then
                        Map<_,_>.ReplaceAppendItem (nd.GID, nd) (accCache |> Map.filter(fun k v->k<>nd.GID))
                    else
                        accCache |> Map<_,_>.ReplaceAppendItem (nd.GID, nd)
                // parent should come from the accumulator first, cache second
                let parent = FindNodeUseCacheFirst ndParentGID acc1
                 // same for kids
                let kids = parent.ChildrenGIDList |> List.map(fun x->FindNodeUseCacheFirst x acc1) |> List.map(fun x->(x.GID,x)) |> Map.ofList
                let newNuclearTuple = tpHtmlDocument.updateParentAndKids parent kids
                let newNuclearKids = (snd newNuclearTuple)
                let newAcc = newNuclearKids |> Map.fold(fun acc2 k x->SaveNodeUseCacheFirst x acc2) acc1
                let kidMap = newNuclearKids// |> List.map(fun x->(x.GID,x)) |> Map.ofList
                let ret = kidMap |> (Map.fold(fun (acc3:Map<Guid,tpHtmlNode>) k v->
                    loop k acc3
                    ) newAcc)
                ret |> Map<_,_>.ReplaceAppendItem(parent.GID, parent)
            loop rootNode.GID Map.empty
                
        static member UpdateSiblingInformationForNode (ndPar:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let rec loop (ndParent:tpHtmlNode) (acc:Map<Guid,tpHtmlNode>) =
                let parent = 
                    if acc |> Map.containsKey ndParent.GID
                        then acc |> Map.find ndParent.GID
                        else ndParent
                let newKidSiblingList = parent.ChildrenGIDList |> List.map(fun x->
                    (x, (parent.ChildrenGIDList |> List.filter(fun y->y<>x))))
                let updatedKidsWithSiblings = newKidSiblingList |> List.map(fun x->
                    let nd = 
                        if acc |> Map.containsKey (fst x)
                            then acc |> Map.find (fst x)
                            else masterNodeMap |> Map.find (fst x)
                    let guidList = (snd x) |> List.map(fun x->x)
                    let newNode = {nd with SiblingGIDList = guidList}
                    newNode)
                let updatedKidsWithNextPreviousSiblings = updatedKidsWithSiblings |> List.mapi(fun i x->
                    let prevSib =
                        if i = 0 then None else Some((updatedKidsWithSiblings.Item (i - 1)).GID)
                    let nextSib =
                        if i = updatedKidsWithSiblings.Length - 1 then None else Some((updatedKidsWithSiblings.Item(i + 1)).GID)
                    {x with PreviousSiblingGIDopt = prevSib; NextSiblingGIDopt = nextSib}
                    )
                if updatedKidsWithNextPreviousSiblings.Length > 0 then
                    let updatedKidsWithNextPreviousSiblingsMap = updatedKidsWithNextPreviousSiblings |> List.map(fun x->(x.GID,x)) |> Map.ofList
                    let newAcc = acc |> Map<_,_>.ReplaceAppendMap updatedKidsWithNextPreviousSiblingsMap
                    updatedKidsWithNextPreviousSiblings |> (List.fold(fun accum x->loop x accum) newAcc)
                else
                    let newAcc = acc
                    newAcc
            let newNodes = loop ndPar Map.empty
            let newNodesAddRoot = newNodes |> Map<_,_>.ReplaceAppendItem(ndPar.GID,ndPar)
            // delete the old nodes and add the new ones
            let masterMapDeleteOldDesc =  masterNodeMap |> Map<_,_>.DeleteMap (tpHtmlNode.Descendents ndPar masterNodeMap)
            let masterMapWithUpdatedNodes = masterMapDeleteOldDesc |> Map<_,_>.ReplaceAppendMap newNodesAddRoot
            masterMapWithUpdatedNodes

        static member UpdateNode (updtNode:tpHtmlNode) (masterNodeList:Map<Guid,tpHtmlNode>)=
            let newNodesRemoved = masterNodeList.Remove(updtNode.GID)
            let newNodesAdded = newNodesRemoved.Add(updtNode.GID, updtNode)
            newNodesAdded

        // single node update. Not the difference in implementation with the list
        // use the list method if you have a bunch. Don't repeatedly call this in a loop
        member this.UpdateDocNode (replacementNode:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            masterNodeMap |> Map<_,_>.ReplaceAppendItem(replacementNode.GID, replacementNode)

        // takes a list of changed nodes and puts them in the document
        static member UpdateDocNodes (replacementNodes:tpHtmlNode list) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let newNodes = masterNodeMap |> Map.map(fun k v->
                if replacementNodes |> List.exists(fun x->x.GID=k)
                    then
                        replacementNodes |> List.find(fun x->x.GID=k)
                    else
                        v
                )
            newNodes
        // simple delete of a node with no updating
        // note the difference with the version that takes a list
        // do not use this in a loop
            
//        // something happened to one of the kids. Update the family
        static member UpdateNodeFamilyRelationshipsByChangedParent (parentNode:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            tpHtmlDocument.UpdateSiblingInformationForNode parentNode masterNodeMap
        static member UpdateNodeFamilyRelationshipsByChangedParentGID (parentGID:Guid) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let par = tpHtmlDocument.NodeByGID parentGID masterNodeMap
            tpHtmlDocument.UpdateSiblingInformationForNode par masterNodeMap
//        // After a node is added with a valid parentID, this updates the parent-kid and sibling lists
        static member UpdateNodeFamilyRelationshipsByChangedChild (childNode:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            tpHtmlDocument.UpdateSiblingInformationForNode (childNode.Parent masterNodeMap)
        static member updateNodeFamilyRelationshipsByChangedChildGID (childNodeGID:Guid)  (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            let childNode = tpHtmlDocument.NodeByGID(childNodeGID) masterNodeMap
            tpHtmlDocument.UpdateNodeFamilyRelationshipsByChangedChild childNode

        static member GetAllDescendentsByGID (ID:Guid) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            masterNodeMap |> Map.filter(fun k t->
                let ancestorList = t.AncestorGIDList
                ancestorList |> List.exists(fun y->y=ID)
                )
        static member GetAllDescendents (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            tpHtmlDocument.GetAllDescendentsByGID nd.GID masterNodeMap

        member this.detachBranchFromDoc (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>)= 
            // make a new map of the root and the progeny, updating
            // the progeny's ancestor list and ownerdoc
            let progeny = tpHtmlNode.Descendents nd masterNodeMap
            let newNode:tpHtmlNode = {nd with AncestorGIDList= []}
            let newProgenyWithCorrectAncestors:Map<Guid,tpHtmlNode> = 
                progeny |> Map.map(fun k v->
                    let newAncestorList = v.AncestorGIDList|> List.filter(fun x->
                        nd.AncestorGIDList |> List.exists(fun y->y=x))
                    {v with AncestorGIDList = newAncestorList; }
                )
            let newBranch:Map<Guid,tpHtmlNode> = newProgenyWithCorrectAncestors.Add(newNode.GID, newNode)
            let newBranchMap = newBranch 
            // delete the node and progeny from the doc
            let progenyDelNodes = masterNodeMap |> Map<_,_>.DeleteMap newBranchMap
            // update the old parent and siblings
            tpHtmlDocument.UpdateNodeFamilyRelationshipsByChangedParentGID nd.ParentGID progenyDelNodes

        member this.attachBranchToDoc (branch:Map<Guid,tpHtmlNode>) (newParent:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let branchList = branch |> Map.toList |> List.map(fun x->snd x)
            let rootNode = branchList |> List.find(fun x->x.AncestorGIDList.Length = 0)
            let newNodeAncestorList = newParent.AncestorGIDList |> List.append [newParent.GID]
            let newRootNode = {rootNode with AncestorGIDList = newNodeAncestorList}
            let newBranchWithCorrectAncestors = 
                branchList |> List.map(fun x->
                let newAncestorList = newParent.AncestorGIDList |> List.append x.AncestorGIDList
                {x with AncestorGIDList = newAncestorList;})
            let nodesToUpdate = [newRootNode] |> List.append newBranchWithCorrectAncestors
            // update the stuff we've changed
            let updatedNodes = tpHtmlDocument.UpdateDocNodes nodesToUpdate masterNodeMap
            //update the sibling stuff
            tpHtmlDocument.UpdateNodeFamilyRelationshipsByChangedParent newParent updatedNodes

        // is there a descendent of the parent Node that has this attribute and value?
        member this.AttSubNodeExists (ndRoot:tpHtmlNode) (name:string) (value:string) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let progeny =  tpHtmlNode.Descendents ndRoot masterNodeMap
            let attMatch = progeny |> Map.exists(fun k x->x.AttributeList|>List.exists(fun y->y.Name=name && y.Value=value))
            attMatch
        // get me all the subnodes of this node that have the att and value
        member this.SubNodesByAtt (ndRoot:tpHtmlNode) (name:string) (value:string) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            if this.AttSubNodeExists ndRoot name value masterNodeMap then
                 tpHtmlNode.Descendents ndRoot masterNodeMap |> Map.filter(fun k x->x.AttributeList|>List.exists(fun y->y.Name=name && y.Value=value))
            else
                Map.empty

        member this.printDoc (filterCommon:bool) (textOnly:bool) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            let ret = new System.Text.StringBuilder(65536)
            let makeNodePrefix (nd:tpHtmlNode) = 
                let ancestorOrderList = tpHtmlNode.Ancestors nd masterNodeMap
                let ancestorFilterList = ancestorOrderList |> Map.filter(fun k ancestor->
                    if filterCommon then
                        ancestor.Name<>"#document" && ancestor.Name<>"html" && ancestor.Name<>"body" && ancestor.Name<>"Root" && ancestor.Name<>"#text"
                    else
                        true
                )
                let ancestorTagList = ancestorFilterList |> Map.toList |> List.map(fun x->(snd x).ToString())
                String.Join("", ancestorTagList)
            let textList = masterNodeMap |> Map.iter (fun k x->
                match textOnly, x.NodeType with
                    | true, enumNodeType.Text ->
                        if x.NodeType = enumNodeType.Text && x._innerText.Trim().Length > 0 then
                            ret.Append ((makeNodePrefix x) + (x.ToString()) + tpHtmlNode.InnerText x masterNodeMap + "\r\n") |> ignore
                        else
                            ()
                    | _, _ ->
                        ret.Append ((makeNodePrefix x) + (x.ToString()) + tpHtmlNode.InnerText x masterNodeMap + "\r\n") |> ignore
                ) 
            let t = ret.ToString()
            Console.WriteLine(t)


//        member this.Iter f = 
//            let rec loop (nd:tpHtmlNode) =
//                f nd
//                nd.ChildNodes |> Seq.iter(fun x->loop x)
//            loop this.DocumentNodes.First
//
//        member this.Fold (f:'a->tpHtmlNode->'a) (topNode:tpHtmlNode) (start:'a) = 
//            let rec loop (targetNode:tpHtmlNode) (accum:'a) =
//                let temp = f accum targetNode
//                if targetNode.HasChildNodes then
//                    targetNode.ChildNodes |> List.fold(fun acc2 y->
//                        this.Fold f y acc2
//                        ) temp
//                    else
//                        accum
//            loop topNode start

        member this.FilterByPattern (pattern:tpTagPath) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let temp = masterNodeMap |> Map.filter(fun k v->v.IsPatternMatch pattern masterNodeMap)
            temp          

        static member AddAttributeTagByPattern (pattern:tpTagPath) (tagName:string) (tagValue:string) (inputNodes:Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>) =
            let temp = inputNodes |> Map.map(fun k v->
                if v.IsPatternMatch pattern inputNodes
                    then v.AddAttributeNoSave tagName tagValue
                    else v
                )
            temp      
        static member AddAttributeTagByPatternList (patternList:tpTagPath list) (tagName:string) (tagValue:string) (inputNodes:Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>) =
            let temp = inputNodes |> Map.map(fun k v->
                let matchedOnOne = patternList |> List.exists(fun x->v.IsPatternMatch x inputNodes)
                if matchedOnOne
                    then v.AddAttributeNoSave tagName tagValue
                    else v
                )
            temp      


    and tpHtmlNode =
        {
            GID                     : Guid
            Name                    : String
            NodeType                : enumNodeType
            _innerText              : String
            AttributeList           : tpAttribute list
            ChildrenGIDList         : Guid list
            AncestorGIDList         : Guid list
            SiblingGIDList          : Guid list
            PreviousSiblingGIDopt   : Guid option
            NextSiblingGIDopt       : Guid option
            InitialOrder            : int
        }
        static member DocRoot =
            {
                GID                     = Guid.NewGuid()
                Name                    = "Root"
                NodeType                = enumNodeType.Document
                _innerText              = ""
                AttributeList           = []
                ChildrenGIDList         = []
                AncestorGIDList         = []
                SiblingGIDList          = []
                PreviousSiblingGIDopt   = None
                NextSiblingGIDopt       = None
                InitialOrder            = 0
            }
        static member Empty =
            {
                GID                     = Guid.NewGuid()
                Name                    = ""
                NodeType                = enumNodeType.None
                _innerText              = ""
                AttributeList           = []
                ChildrenGIDList         = []
                AncestorGIDList         = []
                SiblingGIDList          = []
                PreviousSiblingGIDopt   = None
                NextSiblingGIDopt       = None
                InitialOrder            = 0
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

        member this.ParentGID = this.AncestorGIDList.Last
        member this.Parent (masterNodeMap:Map<Guid,tpHtmlNode>) = masterNodeMap.Item this.ParentGID //this.OwnerDocumentRefopt.Value.Value.NodeByGID this.ParentGID
        member this.Root = this.AncestorGIDList.First
        member this.HasChildNodes = this.ChildrenGIDList |> List.length > 0
        static member ChildNodes (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) = 
            nd.ChildrenGIDList |> (List.fold(fun (acc:Map<Guid,tpHtmlNode>) x->
                let nd = tpHtmlDocument.NodeByGID x masterNodeMap
                acc.Add(nd.GID, nd)) Map.empty)
        static member InnerText (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let textList = (tpHtmlNode.Descendents nd masterNodeMap) |> Map.filter(fun k v->v.NodeType=enumNodeType.Text) |> Map.toList |> List.map(fun x->(snd x)._innerText)
            String.Join(" ", textList)
        static member Ancestors (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) :Map<Guid,tpHtmlNode> =
            let ancestorList = nd.AncestorGIDList
            masterNodeMap |> Map.filter(fun k v->
                ancestorList |> List.exists(fun x->x=v.GID)
                )
//        member this.AncestorGIDs =
//            if this.OwnerDocumentRefopt.IsNone then
//                List.empty
//            else
//                this.AncestorGIDList
        static member Descendents (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>):Map<Guid,tpHtmlNode> =
            masterNodeMap |> Map.filter(fun k v->
                let ancestorList = v.AncestorGIDList
                ancestorList |> List.exists(fun y->y=nd.GID)
                )
        static member DescendentGIDs (nd:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) =
            let filterForAncestors = masterNodeMap |> Map.filter(fun k v->
                let ancestorList = v.AncestorGIDList
                ancestorList |> List.exists(fun y->y=nd.GID)
                )
            let ret = filterForAncestors |> Map.toList
            ret |> List.map(fun x->
                let a,b = x
                a)
        member this.AddAttributeNoSave (attName:string) (attValue:string) = 
            let newAttList = this.AttributeList |> List.append [{Name=attName; Value=attValue}]
            {this with AttributeList = newAttList}
        member this.AddAttribute (attName:string) (attValue:string) (masterNodeList:Map<Guid,tpHtmlNode>)=
            let newNode = this.AddAttributeNoSave attName attValue
            tpHtmlDocument.UpdateNode newNode masterNodeList
        member this.AttNameExists (name:string) =
            this.AttributeList |> List.exists(fun x->x.Name=name)
        member inline this.Attribute (name:string) = 
            if this.AttNameExists name then this.AttributeList |> List.find(fun x->x.Name=name)
            else raise (new System.ArgumentOutOfRangeException("Attribute named " + name + "does not exist in the node " + this.ToString()))
        member inline this.regAttNameExists (reg:System.Text.RegularExpressions.Regex):bool =
            this.AttributeList |> List.exists(fun x->reg.IsMatch(x.Name))
        member inline this.regstrAttNameExists (regTxt):bool =
            this.AttributeList |> List.exists(fun x->System.Text.RegularExpressions.Regex.IsMatch(x.Name, regTxt, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        member inline this.regAttByName (reg:System.Text.RegularExpressions.Regex) =
            if this.regAttNameExists reg then
                this.AttributeList |> List.find(fun x->reg.IsMatch(x.Name))
            else
                raise (new System.ArgumentOutOfRangeException("RegAttMatch is asking to match an attribute that doesn't exist"))

        member this.AttValueExists (value:string) =
            this.AttributeList |> List.exists(fun x->x.Value=value)
        member this.AttributesByValue (value:string) = 
            if this.AttValueExists value then this.AttributeList |> List.filter(fun x->x.Value=value) else []
        member inline this.regAttValueExists (reg:System.Text.RegularExpressions.Regex):bool = 
            this.AttributeList |> List.exists(fun x->reg.IsMatch(x.Value))
        member inline this.regAttributesByValue (reg:System.Text.RegularExpressions.Regex) =
            if this.regAttValueExists reg then this.AttributeList |> List.filter(fun x->reg.IsMatch(x.Value)) else []
        member this.ContainsAttNameValue (attName:string) (attValue:string) =
            if this.AttNameExists attName then this.Attribute(attName).Value = attValue else false
        member this.IsPatternMatch (pattern:tpTagPath) (masterNodeMap:Map<Guid, tpHtmlNode>) =
            let inline (=*=) (pattern:string) (value:string) =
                if value="" then false else System.Text.RegularExpressions.Regex.IsMatch(value, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            let regInnerText= new System.Text.RegularExpressions.Regex(pattern.InnerText, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            let patternMatch (nd:tpHtmlNode) =
                // Empty strings always match
                let b1 = if pattern.TagName <> String.Empty then pattern.TagName =*= nd.Name else true
                let b2 = if pattern.ID <> String.Empty then  nd.regstrExistsAttValueByAttName pattern.ID "ID" else true
                let b3 = if pattern.Class <> String.Empty then  nd.regstrExistsAttValueByAttName pattern.Class "class" else true
                let b4 = if pattern.AnyAttName <> String.Empty then  nd.regstrAttNameExists pattern.AnyAttName else true
                let b5 = if pattern.AnyAttValue <> String.Empty then  nd.regstrAttNameExists pattern.AnyAttValue else true
                let b6 = if pattern.InnerText <> String.Empty then regInnerText.IsMatch(tpHtmlNode.InnerText nd masterNodeMap) else true
                b1 && b2 && b3 && b4 && b5 && b6
                
//                pattern.TagName =*= nd.Name && 
//                nd.regstrExistsAttValueByAttName pattern.ID "ID" && 
//                nd.regstrExistsAttValueByAttName pattern.Class "class" && 
//                nd.regstrAttNameExists pattern.AnyAttName &&
//                nd.regstrAttNameExists pattern.AnyAttValue &&
//                regInnerText.IsMatch(tpHtmlNode.InnerText nd masterNodeMap)
            patternMatch this
        // as opposed to other methods, this does not fail when the attribute does not exist. Instead it just returns no match
        member this.regstrExistsAttValueByAttName (regTxt:string) (attName:string)  =
            if this.AttNameExists attName then
                let v = this.Attribute attName
                System.Text.RegularExpressions.Regex.IsMatch(v.Value, regTxt, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            else false
        member inline this.regstrMatchAttValueByAttName (regTxt:string) (attName:string) =
            if this.AttNameExists attName then
                let v = this.Attribute attName
                v
            else
                raise (new System.ArgumentOutOfRangeException("Can't get attribute " + attName + " on this node " + this.ToString()))
        member inline this.regExistsAttNameByAttValue (attValue:string) (reg:System.Text.RegularExpressions.Regex) =
            if this.regAttValueExists reg then
                let ret = this.AttributesByValue attValue
                ret |> List.length > 0
            else
                false
        member inline this.regMatchAttNameByAttValue (attValue:string) (reg:System.Text.RegularExpressions.Regex) =
            if this.regAttValueExists reg then
                let ret = this.AttributesByValue attValue
                ret
            else
                []
    let FoldDoc (f:'a->tpHtmlNode->'a) (topNode:tpHtmlNode) (start:'a) (masterMap:Map<Guid,tpHtmlNode>) = 
        let rec loop (targetNode:tpHtmlNode) (accum:'a) =
            let temp = f accum targetNode
            if targetNode.HasChildNodes then
                targetNode.ChildrenGIDList |> List.fold(fun acc2 y->
                    loop (masterMap.Item(y)) acc2
                    ) temp
                else
                    accum
        loop topNode start


    let UpdateSiblingInformationForNode (ndPar:tpHtmlNode) (masterNodeMap:Map<Guid,tpHtmlNode>) =
        let rec loop (ndParent:tpHtmlNode) (acc:Map<Guid,tpHtmlNode>) =
            let parent = 
                if acc |> Map.containsKey ndParent.GID
                    then acc |> Map.find ndParent.GID
                    else ndParent
            let newKidSiblingList = parent.ChildrenGIDList |> List.map(fun x->
                (x, (parent.ChildrenGIDList |> List.filter(fun y->y<>x))))
            let updatedKidsWithSiblings = newKidSiblingList |> List.map(fun x->
                let nd = 
                    if acc |> Map.containsKey (fst x)
                        then acc |> Map.find (fst x)
                        else masterNodeMap |> Map.find (fst x)
                let guidList = (snd x) |> List.map(fun x->x)
                let newNode = {nd with SiblingGIDList = guidList}
                newNode)
            let updatedKidsWithNextPreviousSiblings = updatedKidsWithSiblings |> List.mapi(fun i x->
                let prevSib =
                    if i = 0 then None else Some((updatedKidsWithSiblings.Item (i - 1)).GID)
                let nextSib =
                    if i = updatedKidsWithSiblings.Length - 1 then None else Some((updatedKidsWithSiblings.Item(i + 1)).GID)
                {x with PreviousSiblingGIDopt = prevSib; NextSiblingGIDopt = nextSib}
                )
            if updatedKidsWithNextPreviousSiblings.Length > 0 then
                let updatedKidsWithNextPreviousSiblingsMap = updatedKidsWithNextPreviousSiblings |> List.map(fun x->(x.GID,x)) |> Map.ofList
                let newAcc = acc |> Map<_,_>.ReplaceAppendMap updatedKidsWithNextPreviousSiblingsMap
                updatedKidsWithNextPreviousSiblings |> (List.fold(fun accum x->loop x accum) newAcc)
            else
                let newAcc = acc
                newAcc
        let newNodes = loop ndPar Map.empty
        let newNodesAddRoot = newNodes |> Map<_,_>.ReplaceAppendItem(ndPar.GID,ndPar)
        // delete the old nodes and add the new ones
        let masterMapDeleteOldDesc =  masterNodeMap |> Map<_,_>.DeleteMap (tpHtmlNode.Descendents ndPar masterNodeMap)
        let masterMapWithUpdatedNodes = masterMapDeleteOldDesc |> Map<_,_>.ReplaceAppendMap newNodesAddRoot
        masterMapWithUpdatedNodes


    let mkNode (hapNode:HtmlNode) (parentNode:tpHtmlNode) (nodeOrder: int) = 
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
                ChildrenGIDList         = []
                AncestorGIDList         = newAncestors
                SiblingGIDList          = []
                PreviousSiblingGIDopt   = None
                NextSiblingGIDopt       = None
                InitialOrder            = nodeOrder
            }
        newNode
    // make a parent, add the kids into his children list
    // this updates the kids sibling stuff
    //let updateParentKids (parentNode:tpHtmlNode) (kidList:tpHtmlNode list) =

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
                ChildrenGIDList         = []
                AncestorGIDList         = []
                SiblingGIDList          = []
                PreviousSiblingGIDopt   = None
                NextSiblingGIDopt       = None
                InitialOrder            = 0
            }

        let rec loop (hapNode:HtmlNode) (dhapParentNode:tpHtmlNode) (acc1:Map<Guid,tpHtmlNode>) (pairedChildrenList:(tpHtmlNode*HtmlNode) List) orderAcc =
            // parent node doesn't know who it's kids are
            let newParentKidList = pairedChildrenList |> List.map(fun x->(fst x).GID)
            let newParent ={ dhapParentNode with ChildrenGIDList = newParentKidList; InitialOrder = orderAcc}
            // kids don't know who they're siblings are
            let updatedKidsWithSiblings = pairedChildrenList |> List.map(fun x->
                let kidSiblings = pairedChildrenList |> List.filter(fun y->(fst y).GID<>(fst x).GID) |> List.map(fun y->(fst y).GID)
                {(fst x) with SiblingGIDList = kidSiblings}
                )
            let updatedKidsWithNextPreviousSiblings = updatedKidsWithSiblings |> List.mapi(fun i x->
                let prevSib =
                    if i = 0 then None else Some((updatedKidsWithSiblings.Item (i - 1)).GID)
                let nextSib =
                    if i = updatedKidsWithSiblings.Length - 1 then None else Some((updatedKidsWithSiblings.Item(i + 1)).GID)
                {x with PreviousSiblingGIDopt = prevSib; NextSiblingGIDopt = nextSib}
                )
            let totalChangedMap = [newParent] |> List.append updatedKidsWithNextPreviousSiblings |> List.map(fun x->(x.GID, x)) |> Map.ofList
            let newAcc = acc1 |> Map<_,_>.ReplaceAppendMap totalChangedMap
                
            pairedChildrenList |> (List.fold(fun acc2 n->
                let grandkidNodes = (snd n).ChildNodes |> Seq.toList |> List.mapi(fun i x->(mkNode x (fst n) (orderAcc + i), x))
                loop (snd n) (fst n) acc2 grandkidNodes (orderAcc + 1 + grandkidNodes.Length)
                    ) newAcc)
            
        let originalPairedKidList = hapDoc.DocumentNode.ChildNodes  |> Seq.toList |> List.mapi(fun i x->(mkNode x rootNode (i + 1), x))
        loop hapDoc.DocumentNode rootNode Map.empty originalPairedKidList (1 + originalPairedKidList.Length)
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
    let cleanMap (nodeMap:Map<Guid,tpHtmlNode>) = 
        let ndExists x = nodeMap.ContainsKey x
        nodeMap |> Map.map(fun k v->
            let anc = v.AncestorGIDList |> List.filter(fun x->ndExists x)
            let chi = v.ChildrenGIDList |> List.filter(fun x->ndExists x)
            let sib = v.SiblingGIDList |> List.filter(fun x->ndExists x)
            let prevSib:Guid option =
                if v.PreviousSiblingGIDopt.IsSome
                 then if (ndExists (v.PreviousSiblingGIDopt.Value))
                            then v.PreviousSiblingGIDopt 
                            else None
                    else None
            let nextSib:Guid option =
                if v.NextSiblingGIDopt.IsSome
                 then if (ndExists (v.NextSiblingGIDopt.Value))
                            then v.NextSiblingGIDopt 
                            else None
                    else None
            {v with AncestorGIDList=anc; ChildrenGIDList=chi; SiblingGIDList=sib; PreviousSiblingGIDopt=prevSib; NextSiblingGIDopt=nextSib}
            )
    let deleteFlaggedNodes (flagAttName:string) (flagAttValue:string) (taggedNodeMap:Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>) =
        let listOfNodesToDelete = taggedNodeMap |> Map.toList |> List.filter(fun x->
            (snd x).ContainsAttNameValue flagAttName flagAttValue)
        let listOfDeleteNodeDescendentGIDs = List.concat (listOfNodesToDelete |> List.map(fun x->
            let nd = snd x
            tpHtmlNode.DescendentGIDs nd taggedNodeMap))
        let GIDsToDelete =  listOfNodesToDelete |> List.map(fun x->fst x) |>
                                List.append listOfDeleteNodeDescendentGIDs
        let DelGIDsNoDupes = removeDupes GIDsToDelete                                    
        let inDelList gid =  DelGIDsNoDupes |> List.exists(fun x->x = gid)
                
        // if it's in the delete list, kill it
        let nodesRawDelete = taggedNodeMap |> Map.filter(fun k v->
            not (inDelList v.GID))
        // if it's in the ancestor/descendent list, adjust par/child
        let newNodes = nodesRawDelete |> Map.map(fun k v->
            // if not, make sure it's not in the sibling or parent/descendent list
            let newChildGIDList = v.ChildrenGIDList |> List.filter(fun x->not (inDelList x))
            let newNode = {v with ChildrenGIDList = newChildGIDList}
            newNode
            )
        nodeSanityCheck newNodes |> ignore
        newNodes



    type tpNodeFamily = 
        {
            Dad: tpHtmlNode
            Kids: tpHtmlNode list
            Grandkids: tpHtmlNode list
        }
    let promoteFlaggedNodes (flagAttName:string) (flagAttValue:string) (inputNodeMap:Map<Guid,tpHtmlNode>) =
        let isFlagged (v:tpHtmlNode) = v.ContainsAttNameValue flagAttName flagAttValue
        let kidIsFlagged (v:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>) = v.ChildrenGIDList |> List.map(fun x->nodeMap.Item x) |> List.exists(fun x->isFlagged x)
        let nodeListToMap (ndList:tpHtmlNode list) = ndList |> List.map(fun x->(x.GID, x)) |> Map.ofList
        let kids (v:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>) = v.ChildrenGIDList |> List.map(fun x->nodeMap.Item x)
        let grandkids (v:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>) = List.concat (kids v nodeMap |> List.map(fun x->x.ChildrenGIDList |> List.map(fun y->nodeMap.Item y)))
        let allDescendents (v:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>) = nodeMap |> Map.filter(fun k v->v.AncestorGIDList |> List.exists(fun x->x=v.GID))
        let makeFamily (v:tpHtmlNode) (nodeMap:Map<Guid,tpHtmlNode>) = {Dad = v; Kids = (kids v nodeMap); Grandkids = (grandkids v nodeMap)}
        let updateFamilyToMap (fam:tpNodeFamily) (nodeMap:Map<Guid,tpHtmlNode>) =
            let new1 = nodeMap |> Map<_,_>.ReplaceAppendItem(fam.Dad.GID, fam.Dad)
            let kidMap = nodeListToMap fam.Kids
            let new2 = new1 |> Map<_,_>.ReplaceAppendMap kidMap
            let grandkidMap = nodeListToMap fam.Grandkids
            let new3 = new2 |> Map<_,_>.ReplaceAppendMap grandkidMap
            new3
        // it is assumed that everything above "dad" has already been fixed
        // when using this function, it must be called from top down, one level at a time
        let removeKidFromFamily (parentNode:tpHtmlNode) (family:tpNodeFamily) (funcMasterNodes:Map<Guid,tpHtmlNode>) =
            // helper functions. Need to be sure we are using the local copy of the map (funMasterNodes) as this is the copy
            // that will continuously update each time the func is called
            let nodesToRemove = family.Kids |> List.filter(fun x->isFlagged x)
            //remove the kid from dad's children list and replace it with all of the children's kids
            let newDadKidGIDs = List.concat (family.Dad.ChildrenGIDList |> List.map(fun x->
                let foundNodeToDelete = nodesToRemove |> List.exists(fun y->y.GID = x)
                if not foundNodeToDelete then [x] else  (nodesToRemove |> List.find(fun y->y.GID = x)).ChildrenGIDList
                ))
            let newDadKids = newDadKidGIDs |> List.map(fun x->funcMasterNodes.Item x)
            let newDad = {family.Dad with ChildrenGIDList = newDadKidGIDs}
            // for the kids, remove the node from their sibling list and update their next/previous
            let updatedKidsWithSiblings = newDadKids |> List.map(fun x->
                let kidSiblings = newDadKids |> List.filter(fun y->y.GID<>x.GID) |> List.map(fun y->y.GID)
                {x with SiblingGIDList = kidSiblings}
                )
            let updatedKidsWithNextPreviousSiblings = updatedKidsWithSiblings |> List.mapi(fun i x->
                let prevSib =
                    if i = 0 then None else Some((updatedKidsWithSiblings.Item (i - 1)).GID)
                let nextSib =
                    if i = updatedKidsWithSiblings.Length - 1 then None else Some((updatedKidsWithSiblings.Item(i + 1)).GID)
                {x with PreviousSiblingGIDopt = prevSib; NextSiblingGIDopt = nextSib}
                )
            // for all descendents of this node, remove this node's GID from their ancestor list
            let newMapWithAncestorChanges = funcMasterNodes |> Map.map(fun k v->
                if v.AncestorGIDList |> List.exists(fun x->nodesToRemove |> List.exists(fun y->y.GID=x))
                    then
                        let newAncs = v.AncestorGIDList |> List.filter(fun x->not (nodesToRemove |> List.exists(fun y->y.GID=x)))
                        {v with AncestorGIDList=newAncs}
                    else
                        v)
            //make rest of changes and return adjusted map
            let nodesToRemoveMap = nodesToRemove |> nodeListToMap
            let mapWithDeletes = newMapWithAncestorChanges |> Map<_,_>.DeleteMap nodesToRemoveMap
            let familyKidsWithDelete = family.Kids |> List.filter(fun x->not (nodesToRemoveMap.ContainsKey x.GID))
            let newFamily = {family with Kids = familyKidsWithDelete; Dad = newDad}
            updateFamilyToMap newFamily mapWithDeletes

        let rec loop (ndParent:tpHtmlNode) (acc1:Map<Guid,tpHtmlNode>) =
            let parentRet = acc1
            let kidRet = ndParent.ChildrenGIDList |> (List.fold(fun (acc2:Map<Guid,tpHtmlNode>) x->
                // kid may have already been deleted
                let newAcc = 
                    if acc2.ContainsKey x then
                        let dadOfNodesToProcess = acc2.Item x 
                        let p=6
                        let ret = if kidIsFlagged dadOfNodesToProcess acc2 then
                                            let fam = makeFamily dadOfNodesToProcess acc2
                                            let ret = removeKidFromFamily dadOfNodesToProcess fam acc2
                                            ret
                                        else
                                            acc2
                        // remember, we just changed the kids for nodeToProcess
                        let r= 8
                        let newNodeToProcess = ret.Item dadOfNodesToProcess.GID
                        nodeSanityCheck ret |> ignore
                        loop newNodeToProcess ret
                    else
                        acc2
                newAcc                        
                ) parentRet)
            kidRet
        let rootNode = tpHtmlDocument.Root inputNodeMap
        let ret = loop rootNode inputNodeMap
//        nodeSanityCheck ret |> ignore
        ret
//        let parentOfFlaggedNodes = taggedNodeMap |> Map.filter(fun k v->
//            kids v |> List.exists(fun x->isFlagged x))
//    let promoteFlaggedNodes (flagAttName:string) (flagAttValue:string) (taggedNodeMap1:Map<Guid,tpHtmlNode>) =
//        let taggedNodeMap = cleanMap taggedNodeMap1
//        let flaggedNodes = taggedNodeMap |> Map.filter(fun k v->
//            let flagged = v.ContainsAttNameValue flagAttName flagAttValue
//            flagged
//            )
//        // get rid of the ones with no kids
//        let ignoredAndChildless = flaggedNodes |> Map.filter(fun k v->
//            let noKids = v.ChildrenGIDList.Length = 0
//            noKids
//            )
//        let taggedRemovedChildless =
//            taggedNodeMap |> Map<_,_>.DeleteMap ignoredAndChildless
//        // now for the ones with no kids that we just zapped, find their parents and remove them from the kid list
//        let parentsThatLostTheirKids = taggedRemovedChildless |> Map.filter(fun k v->
//            // does the kid list contain somebody we just deleted?
//            v.ChildrenGIDList |> List.exists(fun x->ignoredAndChildless.ContainsKey x)
//            )
//        let updatedChildlessParentNodes = parentsThatLostTheirKids |> Map.map(fun k v->
//            let newkids = v.ChildrenGIDList |> List.filter(fun x-> not (ignoredAndChildless.ContainsKey x))
//            {v with ChildrenGIDList = newkids}
//            )
//        let taggedRemovedChildlessAndUpdatedParents = taggedRemovedChildless |> Map<_,_>.ReplaceAppendMap updatedChildlessParentNodes
//        //for the rest of these guys (the ones that have kids), do the tough processing
//        let ignoredAndNotChildless = flaggedNodes |> Map.filter(fun k v->
//            let hasKids = v.ChildrenGIDList.Length > 0
//            hasKids
//            )
//        //nodeSanityCheck taggedRemovedChildlessAndUpdatedParents |> ignore
//        // do the work, taking the list of nodes that need to be deleted that had kids,
//        // adjusting the tree structure, and creating a new map of all the modified nodes
//        let adjustedNodes = 
//            ignoredAndNotChildless |> (Map.fold(fun (acc:Map<Guid,tpHtmlNode>) k v->
//                //let acc = fst accTuple
//                //let masterNodeList = snd accTuple
//                let getByGIDLocalFirst (gid:Guid)  =
//                    if acc.ContainsKey gid then
//                        acc.Item gid
//                    else
//                        //raise (new System.Exception("Looking for nodes in all the wrong places"))
//                        if taggedRemovedChildlessAndUpdatedParents.ContainsKey gid then
//                            taggedRemovedChildlessAndUpdatedParents.Item gid
//                        else
//                            raise (new System.Exception("looking for a node that's not in cache or master list. Something's wrong"))
//                let existsAndIsNotFlagged x = 
//                    let ret = (acc.ContainsKey x)
//                                || ((taggedRemovedChildlessAndUpdatedParents.ContainsKey x)
//                                    && not ((taggedRemovedChildlessAndUpdatedParents.Item x).ContainsAttNameValue flagAttName flagAttValue))
//                    ret
//                let existsInCache x =
//                    let ret = (acc.ContainsKey x)
//                                || (taggedRemovedChildlessAndUpdatedParents.ContainsKey x)
//                    ret
//                // helper function to find all progeny of current node using the acc variable first, then the master list
//                let findAllDescendents (nd:tpHtmlNode) = 
//                    let descFromAcc = acc |> Map.filter(fun k v->v.AncestorGIDList|>List.exists(fun x->x=nd.GID))
//                    let descFromMasterNode = taggedRemovedChildlessAndUpdatedParents |> Map.filter(fun k v->v.AncestorGIDList|>List.exists(fun x->x=nd.GID))
//                    let ret = descFromAcc |> Map<_,_>.ReplaceAppendMap descFromMasterNode
//                    ret
//                // We have the node that is flagged
//                // find the dad of this node to be ignored
//                let dad = 
//                    // starting with most recent dad, find the nearest parent that's not ignored
//                    let newAncList = v.AncestorGIDList|> List.rev |> List.filter(fun x->
//                        ((acc.ContainsKey x) || (taggedRemovedChildlessAndUpdatedParents.ContainsKey x)) && (not ((taggedRemovedChildlessAndUpdatedParents.Item x).ContainsAttNameValue flagAttName flagAttValue)))
//                        
//                    let dadGID = newAncList |> List.find(fun x->
//                        let candidate = getByGIDLocalFirst x
//                        not (candidate.ContainsAttNameValue flagAttName flagAttValue)
//                        )
//                    getByGIDLocalFirst dadGID
//                // take his kid list of dad's. Remove the node-to-be-ignored. Add in the node-to-be ignored
//                // kids where it used to be. (All the grandkids become kids of grandad since dad died)
//                let newDadKids1 = dad.ChildrenGIDList |> List.map(fun x->
//                    let foundNodeToDelete = x=v.GID
//                    if not foundNodeToDelete then [x] else v.ChildrenGIDList
//                    )
//                let newDadKids = List.concat newDadKids1
//                // Take the node-to-be processed kid's and the parent node's kids and re-do the sibling lists
//                // put the node we are processing in the Nodes to delete list
//                // This is our only delete -- the node that is flagged
//                
//                //|> listInsertList(fun x->x<>v.GID)
//                let newDad = {dad with ChildrenGIDList = newDadKids}
//                // PROMOTE ALL DESCENDENTS
//                let nodeDescendents = findAllDescendents v
//                let descWithAncListAdjusted = nodeDescendents |> Map.map(fun k2 v2->
//                    let newAnc = v2.AncestorGIDList |> List.filter(fun x->x<>v.GID)
//                    {v2 with AncestorGIDList = newAnc})
//
//                // for the descendents that were immediate kids of this node, now that they are promoted, 
//                // all of their sibling information is bad and needs to be redone
//                let kids = newDadKids |> List.filter(fun x->existsInCache x) |> List.map(fun x->getByGIDLocalFirst x)
//                let updatedKidsWithSiblings = kids |> List.map(fun x->
//                    let kidSiblings = kids |> List.filter(fun y->y.GID<>x.GID) |> List.map(fun y->y.GID)
//                    {x with SiblingGIDList = kidSiblings}
//                    )
//                let updatedKidsWithNextPreviousSiblings = updatedKidsWithSiblings |> List.mapi(fun i x->
//                    let prevSib =
//                        if i = 0 then None else Some((updatedKidsWithSiblings.Item (i - 1)).GID)
//                    let nextSib =
//                        if i = updatedKidsWithSiblings.Length - 1 then None else Some((updatedKidsWithSiblings.Item(i + 1)).GID)
//                    {x with PreviousSiblingGIDopt = prevSib; NextSiblingGIDopt = nextSib}
//                    )
//                let updatedKidMap = updatedKidsWithNextPreviousSiblings |> List.map(fun x->x.GID, x) |> Map.ofList
//                // do all of our persisting to the stream
//                let newAcc1 = acc |> Map<_,_>.ReplaceAppendMap updatedKidMap
//                let newAcc = newAcc1 |> Map<_,_>.ReplaceAppendItem(newDad.GID, newDad)
//                let tempCheck = newAcc.ContainsKey (new System.Guid("{1f15672f-5af0-4813-a03a-97d7187d7e08}"))
//                // delete the node we've been processing
//                let dadNodeDeleted = newAcc.Remove v.GID
//                //nodeSanityCheck dadNodeDeleted |> ignore
//                dadNodeDeleted
//                ) taggedRemovedChildlessAndUpdatedParents )
//        //nodeSanityCheck (adjustedNodes)|> ignore
//        //let ret = (fst adjustedNodes) |> Map<_,_>.DeleteMap (snd adjustedNodes)
//        let finalClean = cleanMap adjustedNodes
//        nodeSanityCheck finalClean |> ignore
//        finalClean
    let filterNodes (taggedNodeMap:Microsoft.FSharp.Collections.Map<Guid,tpHtmlNode>) =
        // DELETE THE FLAGGED ITEMS

        let deletedNodes = deleteFlaggedNodes "DHAPFlag" "to_delete" taggedNodeMap
        let ignoredNodes1 = promoteFlaggedNodes "DHAPFlag" "to_ignore" deletedNodes
        let ignoredNodes2 = promoteFlaggedNodes "DHAPFlag" "to_ignore" ignoredNodes1
        let ignoredNodes3 = promoteFlaggedNodes "DHAPFlag" "to_ignore" ignoredNodes2
        ignoredNodes3

    let MakeDoc (urlopt:string option) (htmlTextopt: string option) =
        let urlAndTextAndCfg = 
            match urlopt, htmlTextopt with
            | None, None->raise (new System.ArgumentException("Can't make a doc without an url or htmlText"))
            | None, Some(htmlTextopt)->
                let cfg = SiteTextReaderData.getConfigsForUrl "*" // just use the default base setting
                ("", htmlTextopt, cfg)
            | Some(urlopt), None->
                let txt = http urlopt
                let cfg = SiteTextReaderData.getConfigsForUrl urlopt
                (urlopt, txt, cfg)
            | Some urlopt, Some htmlTextopt->
                let txt = http urlopt
                let cfg = SiteTextReaderData.getConfigsForUrl urlopt
                (urlopt, txt, cfg)
        let url, initialHtmlText, cfg = urlAndTextAndCfg
        let retDoc = new tpHtmlDocument()
        // Store the initial html (for debugging purposes)
        retDoc.InitialHtml<-initialHtmlText
        
        // STEP 1 -- ZAP THE INPUT TEXT WITH REGEX LIST
        let initSlice = cfg |> List.fold(fun acc x->sliceOutRegList acc x.regKeep) retDoc.InitialHtml

        let initialRegZapHtml = cfg |> List.fold(fun acc x->stripOutRegList acc x.regExKillBefore) initSlice
        let regReplaceHtmlText = cfg |> List.fold(fun acc x->
            regFindReplaceList acc x.regExReplace) initialRegZapHtml
        retDoc.InitialRegZappedCleanHtml <- regReplaceHtmlText

        // STEP 2 - Load the initial zapped html into an initial set of tpHtmlNodes
        // after zapping with regex, re-parse and reload (need to do this to make sure regex work hasn't destroyed doc)
        // we are saving as we go along for debugging later on
        let initialNodes = getNodes regReplaceHtmlText
        retDoc.InitialRegZappedCleanNodes <- initialNodes

        // STEP 3 run the filter and promote config list against the nodes
        // this should elminate lots of document noise
        let ignoreTaggedNodes = cfg |> List.fold(fun acc1 x->
            tpHtmlDocument.AddAttributeTagByPatternList x.ignoreList "DHAPFlag" "to_ignore" acc1) initialNodes
        let deletedAndIgnoreTaggedNodes =  cfg |> List.fold(fun acc1 x->
            tpHtmlDocument.AddAttributeTagByPatternList x.deleteList "DHAPFlag" "to_delete" acc1) ignoreTaggedNodes
        retDoc.DelPromTaggedNodes<-deletedAndIgnoreTaggedNodes
        let configProcessedNodes = filterNodes deletedAndIgnoreTaggedNodes
        retDoc.DelPromCleanedNodes <-configProcessedNodes

        // STEP 4. After filtering, turn back into html for final regex zap
        let filteredHtml = tpHtmlDocument.SaveToHtml configProcessedNodes
        retDoc.DelPromCleanedDocHtml <- filteredHtml
        let finalRegExClean = cfg |> List.fold(fun acc x->stripOutRegList acc x.regExKillAfter) filteredHtml
        retDoc.FinalRegZapCleanHtml <- finalRegExClean
        
        // STEP 5. Reload the nodes with the final zapped html
        let finalNodes = getNodes regReplaceHtmlText
        retDoc.FinalNodes <- finalNodes

        retDoc