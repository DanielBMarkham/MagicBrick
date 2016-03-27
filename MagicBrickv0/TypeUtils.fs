/// <remarks>
/// Module for holding utilities related to .NET type manipulation
/// </remarks>
module TypeUtils

    open System
    open System.Collections
    open System.Collections.Generic
    open System.Text.RegularExpressions



//          DOT NET TYPE ADDITIONS
    
    

    /// <summary>
    /// Unzip a tupled function into a "normal" F# one
    /// </summary>
    /// <param name="f">The function that used to use a tuple (typically a .NET function)</param>
    /// <param name="a">First parameter of the tuple</param>
    /// <param name="b">Second parameter of the tuple</param>
    /// <returns>Returns a method that can be pased in parts around and curried like other f# methods</returns>
    let uncurry f (a, b) = f a b
    ///<summary>Go through the input map. If the key already exists, replace the node. Otherwise add it</summary>
    type Map<'K, 'T when 'K:comparison> with
        static member ReplaceAppendItem (k, v) (input:Microsoft.FSharp.Collections.Map<_,_>) =
            if input.ContainsKey(k) then
                let newMap = input.Remove k
                newMap.Add (k,v)
            else
                input.Add (k, v)
        
        static member ReplaceAppendMap (x:Microsoft.FSharp.Collections.Map<_,_>) (input:Microsoft.FSharp.Collections.Map<_,_>) =
            x |> Map.fold(fun acc k v->
                    Map<_,_>.ReplaceAppendItem (k,v) acc
                ) input
        static member DeleteMap (x:Microsoft.FSharp.Collections.Map<_,_>) (input:Microsoft.FSharp.Collections.Map<_,_>) =
            x |> Map.fold(fun (acc:Map<_,_>) k v->
                if acc.ContainsKey k then
                    acc.Remove k
                else
                    acc
                ) input
        static member DeleteKeys (x:'a list) (input:Microsoft.FSharp.Collections.Map<_,_>) =
            x |> List.fold(fun (acc:Map<_,_>) k ->
                if acc.ContainsKey k then
                    acc.Remove k
                else
                    acc
                ) input
    let listReplace (f:('a->bool)) (newItem:'a) (lis:'a list) =
        lis |> List.map(fun x->
            let found = f x
            if found then newItem else x)

    type System.Collections.Generic.IEnumerable<'T> with
        /// <summary>Get the first element in the collection</summary>
        member this.First = 
            this |> Seq.nth 0
        /// <summary>Get the last element in the collection</summary>
        member this.Last = 
            this |> Seq.toArray |> (fun x->x.[x.Length - 1])
        // is function f true for all of the seq
        // use: I want to check if list is sorted, so I ask
        // is each item greater than the next item (fun a b -> a > b)
        // works for any function over a seq. UNTESTED
        //member this.relForAll r = Seq.pairwise >> Seq.forall (uncurry r)
        //member this.relTryFind r = Seq.pairwise >> Seq.tryFindIndex (uncurry r)
        //member this.relMap r = Seq.pairwise >> Seq.map (uncurry r)

    type System.Collections.IEnumerable with
        /// <summary>Get the first element in the collection</summary>
        member this.First = 
            this |> Seq.cast |> Seq.nth 0
        /// <summary>Get the last element in the collection</summary>
        member this.Last  = 
            this |> Seq.cast |> Seq.toArray |> (fun x->x.[x.Length - 1])

    /// <summary>Get the first element in the collection (inline version)</summary>
    let inline First ienumColl = 
        (^a : (member First : unit -> 'b) ienumColl)
    /// <summary>Get the last element in the collection (inline version)</summary>
    let inline Last ienumColl = 
        (^a : (member Last : unit -> 'b) ienumColl)

    type System.Collections.IEnumerable with 
        /// <summary>Apply the given function to each item in the collection</summary>
        member this.Iter<'T>(f:'T->unit) =
            for item in this do
                f (item:?>'T)
        /// <summary>Apply the function to each item in the collection, creating a new collection</summary>
        member this.Map(f:'a -> 'b) =
            let newColl = new System.Collections.Generic.List<'b>()
            for item in this do
                newColl.Add(f item) |> ignore
            newColl
        /// <summary>Apply function to each item, creating a new HashSet</summary>
        member this.HashSetMap<'T>(f:'a -> 'T) =
            let newColl = new System.Collections.Generic.HashSet<'T>()
            for item in this do
                newColl.Add(f item) |> ignore
            newColl
        /// <summary>Apply the function to each item in the collection, creating a new ResizeArray (.NET Array)</summary>
        member this.ResizeArrayMap<'T>(f:'a -> 'T) =
            let newColl = new ResizeArray<'T>()
            for item in this do
                newColl.Add(f item) |> ignore
            newColl
        /// <summary>Apply the function to each item, create a new F# List</summary>
        member this.MapList<'T>(f:'a -> 'T):'T list =
            [for item in this -> (f item)]
        /// <summary>Apply the function to each item, create a new F# array</summary>
        member this.MapArray<'T>(f:'a -> 'T):'T array =
            [|for item in this -> (f item)|]
        /// <summary>How many items in collection</summary>
        member this.Count =
            let i = ref 0
            this.Iter (fun x->incr i)
            !i
        /// <summary>Return the first N items of the collection</summary>
        member this.FirstN<'T> n = 
            let enumCount = this.Count
            let enumer = this.GetEnumerator()
            let len = if n > enumCount then enumCount else n
            if len > 0 then
                enumer.MoveNext() |> ignore
                [for i in 1..len ->
                    let ret = enumer.Current
                    enumer.MoveNext() |> ignore
                    ret :?> 'T]
                else
                    []
        /// <summary>Return the last N items of the collection</summary>
        member this.LastN<'T> n = 
            let enumCount = this.Count
            let enumer = this.GetEnumerator()
            let len = if n > enumCount then enumCount else n
            let startPos = enumCount - len
            if startPos > 0 then
                enumer.MoveNext() |> ignore
                for i in 1..startPos do
                    enumer.MoveNext() |> ignore
                [for i in 1..len ->
                    let ret = enumer.Current
                    enumer.MoveNext() |> ignore
                    ret :?> 'T]
                else
                    []
        /// <summary>Return a slice of the collection</summary>
        member this.SliceNM<'T> n m = 
            if n < m then 
                let enumCount = this.Count
                let enumer = this.GetEnumerator()
                let startPos = if n > enumCount then enumCount else n
                let endPos = if m > enumCount then enumCount else m
                let len = endPos - startPos
                if startPos >0 then enumer.MoveNext() |> ignore else ()
                for i in 1..startPos do
                    enumer.MoveNext() |> ignore
                [for i in 1..len ->
                    let ret = enumer.Current
                    enumer.MoveNext() |> ignore
                    ret :?> 'T]
                else
                    []

    type String with
        /// <summary>Does the string contain a version of the other string no matter what the case of the letters are?</summary>
        member this.ContainsCaseInsensitive (s:string) =
            this.ToLower().Contains(s.ToLower())
        /// <summary>Gets rightmost part of the string</summary>
        member this.Right iLen =
            if iLen <= this.Length then
                this.Substring(this.Length - iLen, iLen)
            else
                this
        ///<summary>Gets the leftmost part of the string</summary>
        member this.Left iLen = 
            if iLen <= this.Length then
                this.Substring(0, iLen)
            else
                this
        ///<summary>Trim n characters from left of string</summary>
        member this.TrimLeft iCount =
            if iCount <= this.Length then
                this.Substring(iCount, this.Length - iCount)
            else
                this
        ///<summary>Tim n characters from right of string</summary>
        member this.TrimRight iCount =
            if iCount <= this.Length then
                this.Substring(0, this.Length - iCount)
            else
                this
        ///<summary>Trim x characters from left of string, and y characters from right of string</summary>
        member this.TrimBoth iLeft iRight = 
            (this.TrimLeft iLeft) |> (fun x-> this.TrimRight iRight)
        ///<summary>Return string between two tokens. Any error defaults to input string</summary>
        member this.StripSub (startToken:string) (endToken:string) =
            let start = this.IndexOf(startToken) + startToken.Length
            let stop = this.IndexOf(endToken, start)
            if ( (startToken = "") && (endToken = "") ) then this else this.Substring(start, stop-start)

//    let stripFirstN (str:String) (i:int) =
//        if str.Length > i then
//            str.Substring(i, str.Length - i)
//        else
//            ""
//    let stripLastN (str:String) (i:int) =
//        if str.Length > i then
//            str.Substring(0, str.Length - i)
//        else
//            ""

    type System.Text.RegularExpressions.Regex with
        /// <summary>
        /// Works like the regular RegEx split, but keeps the token as part of the returned array
        // (Token is kept at the end of the string it delimits
        /// </summary>
        /// <param name="s">The string you want to split</param>
        /// <returns>Returns an Array of strings from the split (just like normal Split)</returns>
        member this.SplitKeepToken s = 
            let mts = this.Matches(s)
            let sb = new System.Text.StringBuilder(s)
            let rec loop (lastBeginPos:Int32) matchCount lis =
                match matchCount with
                    | x when x < mts.Count ->
                        let curMatchIndex = if mts.[matchCount].Index >= s.Length then s.Length else mts.[matchCount].Index + 1
                        let subPiece = s.Substring(lastBeginPos, curMatchIndex - lastBeginPos)
                        Array.append [|subPiece|] (loop curMatchIndex (matchCount + 1) lis)
                    |_ ->lis
            loop 0 0 [||]

    type System.Text.RegularExpressions.MatchCollection with
        /// <summary>Return the last match</summary>
        member this.Last =
            this.Last :> Match
        /// <summary>Return the first match</summary>
        member this.First =
            this.First :> Match
        /// <summary>Make the match collection into a sequence</summary>
        member this.toSeq =
            seq {for i = 0 to this.Count - 1 do yield this.[i]}
        /// <summary>Make the match collection into an F# array</summary>
        member this.toArray =
            [|for i = 0 to this.Count - 1 do yield this.[i] |]

    // I think these should be written as type extensions, but alas, my kung fu is weak

    /// <summary>Make the match collection into a sequence</summary>
    let mapStringKeyCaseInsContainsKey (s:string) (mp:Microsoft.FSharp.Collections.Map<_,_>) = 
        mp.ContainsKey(s) || mp.ContainsKey(s.ToUpper()) || mp.ContainsKey(s.ToLower())
    
    /// <summary>Make the match collection into a sequence</summary>
    let mapStringKeyCaseInsItem (s:string) (mp:Microsoft.FSharp.Collections.Map<_,_>) = 
        if mp.ContainsKey(s) then mp.Item(s)
        elif mp.ContainsKey(s.ToLower()) then mp.Item(s.ToLower())
        elif mp.ContainsKey(s.ToUpper()) then mp.Item(s.ToUpper())
        else raise (new System.ArgumentOutOfRangeException("The map does not contain this key"))



//          FUNCTIONS THAT DEAL WITH TYPES



    /// <summary>Returns the amount of physical space the type uses in memory</summary>
    let getPrimitiveTypeSize (t:Type) =
        System.Runtime.InteropServices.Marshal.SizeOf(t)

    /// <summary>Instantiates a value given its parent object and its property info</summary>
    let getPrimitiveValFromPropertyInfo<'A> parentObject (pi:Reflection.PropertyInfo) =
        pi.GetValue(parentObject, null) :?> 'A    

    /// <summary>dynamically create a generic collection</summary>
    // <example>to make a list of strings, createGeneric typeof<List<>> string.GetType()</example>
    let createGeneric (generic:Type) (innerType:Type) (args:obj []) =
        let specificType = generic.MakeGenericType(innerType)
        Activator.CreateInstance(specificType, args)

    /// <summary>Given an object that you know it's type, and you have a generic obj, make the "real" typed object from the obj
    /// this is different from casting because with casting there is a "hidden" type. Here you really have no idea where it's from</summary>
    /// <example>You're given an object from a datareader that goes in a type. Make it the type you know it to be</example>
    let makeObjectFromTypeAndObj (typeName:string) (ob:obj) = 
        let tp = Type.GetType(typeName)
        if tp.IsPrimitive then
            let mutable newob = System.Activator.CreateInstance(tp)
            newob <- ob
            newob
        elif tp.FullName = "System.String" then
            ob
        elif tp.BaseType.FullName = "System.Enum" then
            let newobEnum = System.Activator.CreateInstance(tp)
            //let newob = (newobEnum :?> System.Enum).Parse b
            if System.DBNull.Value.Equals ob then                
                let firstVal = tp.GetEnumNames().[0]
                Enum.Parse(tp, firstVal)
            else
                let bint = ob :?> int
                bint :> obj
        else
            System.Activator.CreateInstance(tp, [ob])

    /// <summary>take a type that is generic and might have many enclosed generics, and return the nested type name.
    // Useful if you have a type foo with a bunch of generic junk in it, and just want to know what it is kn English</summary>
    let toGenericTypeString (t:Type) =
        let rec loop(t:Type) = 
            match t.IsGenericType with
                | false -> t.Name
                | true  ->
                    let genericTypeName = t.GetGenericTypeDefinition().Name.Substring(0, t.GetGenericTypeDefinition().Name.IndexOf('`'))
                    let genericTypeArgs = t.GetGenericArguments() |> Seq.map(fun x->loop x)
                    let joinedArgs = String.Join(",", genericTypeArgs).ToString()
                    genericTypeName + "<" + joinedArgs + ">"
        loop t


    /// <summary>If you only have a type and don't know what type owns it, this will get you the type info for that type</summary>
    let getObjectTypeInfo (ob:'A) = 
        let declaringType = ob.GetType().DeclaringType
        let obName = ob.GetType().FullName
        let proplis = declaringType.GetProperties()
        let propFind = proplis |> Seq.tryFind(fun x->x.PropertyType.FullName = obName)
        if propFind.IsSome then
            propFind.Value
        else
            declaringType.GetProperty(obName)

    /// <summary>The inner loop of the typeTree Functions. Give it a parent object and a property info for that object, and it returns a list of members.
    /// Note that it takes an object -- something with values in it -- not a type</summary>
    let rec typeTreeInner (parentObject:'A) (opi:Reflection.PropertyInfo) = 
        let iEnumType = typeof<System.Collections.IEnumerable>
        let tp = parentObject.GetType()
        if tp.IsPrimitive
            // If it's a primitive, cool. That's where we want to end up
            then
                let tpName = tp.FullName
                let tpSize = getPrimitiveTypeSize(tp)
                [(opi.Name, tpName, tpSize, opi, parentObject)]
            // if it's not a primitive, check for IEnumerable<> next. If it's that, loop over the enums
            elif iEnumType.IsAssignableFrom(tp)
                then
                    let enum:IEnumerable = 
                        box parentObject :?> IEnumerable
                    let enumList = enum.MapList<'A * Reflection.PropertyInfo>(fun x->(x, opi ))
                    List.concat(enumList |> List.map(fun x->
                            let emptyObArray:obj [] = [||]
                            let a,c = x
                            (typeTreeInner a c ) 
                        ))
            // not a primitive or collection, but a generic. Ouch
            // pull out the generic pieces, get the real objects corresponding to them,
            // then walk those objects
            elif tp.IsGenericType
                then
                    let members = tp.GetProperties(Reflection.BindingFlags.Instance ||| Reflection.BindingFlags.Public ||| Reflection.BindingFlags.NonPublic) |> Seq.toArray
                    let genericType = tp.GetGenericTypeDefinition()
                    let genericArgs = tp.GetGenericArguments() |> Seq.toArray
                    List.concat( seq { for i in 0 .. (members.Length - 1) do
                                        let emptyObArray:obj [] = [||] in 
                                        let a = members.[i].GetValue(parentObject, emptyObArray)  // if opi.IsSome then opi.Value.GetValue(parentObject, emptyObArray) else parentObject in 
                                        let c = members.[i] in
                                            yield (typeTreeInner a c) })
                // It's not a primitive, not a generic, and not a collection
                // Right now we're assuming that it's a class
                // Pull out the public properties and walk the code using each of those          
                else
                    let members = tp.GetProperties(Reflection.BindingFlags.Instance ||| Reflection.BindingFlags.Public ||| Reflection.BindingFlags.NonPublic)
                    let memblis = members |> Seq.toList
                    List.concat (memblis |> List.map(fun (x:Reflection.PropertyInfo)->
                        match x.PropertyType.FullName with
                            | "System.String" ->
                                let nm      = "System.String"
                                let strOb   = getPrimitiveValFromPropertyInfo<System.String> parentObject x
                                let sz      = strOb.Length
                                [(x.Name, nm, sz, opi, strOb :> obj)]
                            | "System.DateTime" ->
                                let nm      = "System.DateTime"
                                let strOb   = getPrimitiveValFromPropertyInfo<System.DateTime> parentObject x
                                let sz      = sizeof<System.DateTime> //8
                                [(x.Name, nm, sz, opi, strOb :> obj)]
                            | "System.Guid" ->
                                let nm      = "System.Guid"
                                let strOb   = getPrimitiveValFromPropertyInfo<System.Guid> parentObject x
                                let sz      = sizeof<System.Guid>
                                [(x.Name, nm, sz, opi, strOb :> obj)]
                            |_ ->
                                // skip it if it has parameters (an indexer)
                                if x.GetGetMethod() = null || x.GetGetMethod().GetParameters().Length >0
                                    then
                                        []
                                    else
                                        let emptyObArray:obj [] = [||]
                                        let a = x.GetValue(parentObject, emptyObArray) //x.GetValue(parentObject, emptyObArray)
                                        let c = x
                                        (typeTreeInner a c) ) ) 

    /// <summary>This is the version of type tree that takes a type -- no instantiated oject required. It returns a list with the member name, member type, and memory size of the things inside the type</summary>
    let rec genericTypeTree (tp:Type) (opi:Reflection.PropertyInfo) = 
        let iEnumType = typeof<System.Collections.IEnumerable>
        if tp.IsPrimitive
            // If it's a primitive, cool. That's where we want to end up
            then
                let tpName = tp.FullName
                let tpSize = getPrimitiveTypeSize(tp)
                [(opi.Name, tpName, tpSize)]
            // not a primitive or collection, but a generic. Ouch
            // pull out the generic pieces, get the real objects corresponding to them,
            // then walk those objects
            elif tp.IsGenericType
                then
                    let members = tp.GetProperties(Reflection.BindingFlags.Instance ||| Reflection.BindingFlags.Public ||| Reflection.BindingFlags.NonPublic) |> Seq.toArray
                    let genericType = tp.GetGenericTypeDefinition()
                    let genericArgs = tp.GetGenericArguments() |> Seq.toArray
                    List.concat( seq { for i in 0 .. (members.Length - 1) do
                                        let emptyObArray:obj [] = [||] in 
                                        //let a = members.[i].GetValue(parentObject, emptyObArray)  // if opi.IsSome then opi.Value.GetValue(parentObject, emptyObArray) else parentObject in 
                                        let c = members.[i] in
                                        let ct = c.PropertyType
                                        yield (genericTypeTree ct c) })
                // It's not a primitive, not a generic, and not a collection
                // Right now we're assuming that it's a class
                // Pull out the public properties and walk the code using each of those          
                else
                    let members = tp.GetProperties(Reflection.BindingFlags.Instance ||| Reflection.BindingFlags.Public ||| Reflection.BindingFlags.NonPublic)
                    let memblis = members |> Seq.toList
                    List.concat (memblis |> List.map(fun (x:Reflection.PropertyInfo)->
                        match x.PropertyType.FullName with
                            | "System.String" ->
                                let nm  = "System.String"
                                let sz  = 2 //strOb.Length // 2 IS BOGUS VALUE
                                [(x.Name, x.PropertyType.ToString(), sz)]
                            | "System.DateTime" ->
                                let nm  = "System.DateTime"
                                let sz  = sizeof<System.DateTime> //8
                                [(x.Name, x.PropertyType.ToString(), sz)]
                            | "System.Guid" ->
                                let nm  = "System.Guid"
                                let sz  = sizeof<System.Guid>
                                [(x.Name, x.PropertyType.ToString(), sz)]
                            |_ ->
                                if x.PropertyType.IsEnum then
                                    [(x.Name, x.PropertyType.FullName, 2)] // 2 is bogus here
                                else
                                    // skip it if it has parameters (an indexer)
                                    if x.GetGetMethod() = null || x.GetGetMethod().GetParameters().Length >0
                                        then
                                            []
                                        else
                                            let emptyObArray:obj [] = [||]
                                            //let a = x.GetValue(parentObject, emptyObArray) //x.GetValue(parentObject, emptyObArray)
                                            let c = x
                                            (genericTypeTree x.PropertyType c) ) ) 

    
    /// <summary>Given an object that is instantiated, give me a list of all the members inside, their values, their types, and their sizes</summary>
    let typeTree (parentObject:'A) = 
        typeTreeInner parentObject (getObjectTypeInfo parentObject)

    /// <summary>Given an object that is instantiated, apply function f to all the members inside</summary>
    let fnTypeTree (parentObject:'A) (f) =  
        let typeInfoList = typeTreeInner parentObject (getObjectTypeInfo parentObject)
        typeInfoList |> List.iter(f)

    /// <summary></summary>
    let cGenericTypeTree<'A> =
        genericTypeTree typeof<'A> (typeof<'A>.GetProperties().[0])

    /// <summary>Perform function f on all the members of an uninstantiated type</summary>
    let fnGenericTypeTree<'A> (f) =
        let typeInfoList = cGenericTypeTree<'A>
        typeInfoList |> List.iter(fun x->f x)    

    /// <summary>Gets the value of a field when you only know it's name. Pass in the object and the fieldname. Returns the field and the typeName of the field</summary>
    let getSimpleFieldValue (parentObject:'A) (fieldName:string) = 
        let lis = typeTree parentObject
        let field = lis |> List.find(fun x->
            let a,b,c,d,e = x
            a = fieldName)
        let a,b,c,d,e = field
        ((makeObjectFromTypeAndObj b e) , b)
