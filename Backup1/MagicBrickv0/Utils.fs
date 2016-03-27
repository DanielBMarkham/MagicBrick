//#if INTERACTIVE
//    ;;
//    #load "TypeUtils.fs"
//#else
//#endif
/// <remarks>
/// General-purpose utility module
/// </remarks>
module Utils

    open System
    open TypeUtils

    let SeqGivenFuncTillNull (f:unit->'a option) :seq<'a> =
        seq{
            let v = ref (f())
            while (!v) <> None do
              yield Option.get (!v)
              do v := f() }

    ///<summary>Gets rid of dupes in a list</summary>
    let removeDupes x:'a list =
        x |> Set.ofList |> Set.toList
    ///<wummary>When the function matches, replace the item in the list
    let listReplace (f:('a->bool))(newItem:'a) (sourceList:'a list):'a list =
        sourceList |> List.map(fun x->if f x then newItem else x)

    ///<summary>If anything in the list is present in the string, strip it out of the string.
    /// Useful when you list a list of bad words or such that you want to zap from the string</summary>
    let stripOutStrings (src:string) (strings:string list) = 
        strings |> List.iter(fun x->src.Replace(x, "")|>ignore)
        src
    /// <summary>Takes a list of RegEx strings pairs and uses them to trim a larger string. First pair identifies beginning, second pair identifies end of section</summary>
    let stripOutRegs (source:string) (regMarkers:(string*string)) = 
        let regStartFind    = new System.Text.RegularExpressions.Regex(fst regMarkers)
        let regEndFind      = new System.Text.RegularExpressions.Regex(snd regMarkers)
        let regBeg          = regStartFind.Matches(source).toArray
        let regEnd          = regEndFind.Matches(source).toArray
        let s1 =
            if regBeg.Length > 0 && regEnd.Length > 0 then
                let isSecondMarkerAfterFirst =
                    regEnd |> Seq.exists(fun x->x.Index>regBeg.[0].Index)
                if isSecondMarkerAfterFirst then 
                    let secondMarkerToUse =
                        regEnd |> Seq.find(fun x->x.Index>regBeg.[0].Index)
                    let m1 = regBeg.[0].Index
                    let m1l = regBeg.[0].Value.Length
                    let m2 = secondMarkerToUse.Index
                    let m2l = secondMarkerToUse.Value.Length
                    source.Substring(0, m1) + source.Substring(m2 + m2l)
                else
                    source
            else
                source
        s1
    /// <summary>Takes a list of RegEx strings pairs and uses them to return a smaller string from a larger string. First pair identifies beginning, second pair identifies end of section</summary>
    let sliceOutRegs (source:string) (regMarkers:(string*string)) = 
        let regStartFind    = new System.Text.RegularExpressions.Regex(fst regMarkers)
        let regEndFind      = new System.Text.RegularExpressions.Regex(snd regMarkers)
        let regBeg          = regStartFind.Matches(source).toArray
        let regEnd          = regEndFind.Matches(source).toArray
        let s1 =
            if regBeg.Length > 0 && regEnd.Length > 0 then
                let isSecondMarkerAfterFirst =
                    regEnd |> Seq.exists(fun x->x.Index>regBeg.[0].Index)
                if isSecondMarkerAfterFirst then 
                    let secondMarkerToUse =
                        regEnd |> Seq.find(fun x->x.Index>regBeg.[0].Index)
                    let m1 = regBeg.[0].Index
                    let m1l = regBeg.[0].Value.Length
                    let m2 = secondMarkerToUse.Index
                    let m2l = secondMarkerToUse.Value.Length
                    source.Substring(m1 + m1l, m2 - (m1 + m1l))
                else
                    source
            else
                source
        s1
    ///<summary>Works like stripOutRegs, taking a start and stop regex to mark an area of a string to slice</summary>
    let stripOutRegList (source:string) (regMarkerList:(string*string) list) = 
        let newString = regMarkerList |> List.fold(fun acc x->stripOutRegs acc x) source
        newString
    ///<summary>Works like stripOutRegs, taking a start and stop regex to mark an area of a string to slice</summary>
    let sliceOutRegList (source:string) (regMarkerList:(string*string) list) = 
        let newString = regMarkerList |> List.fold(fun acc x->sliceOutRegs acc x) source
        newString
    ///<summary>Takes a list of RegEx pattern strings and a list of things to replace
    /// goes throug the string finding the pattern and replacing it. So like RegEx.Replace, only
    /// with lists</summary>
    let regFindReplaceList (source:string) (regMatchReplaceList:(string*string) list) =
        let newString = regMatchReplaceList |> List.fold(fun acc x->
            //let regFindReplace = new System.Text.RegularExpressions.Regex(fst x)
            //regFindReplace.Replace(acc, snd x)) source
            System.Text.RegularExpressions.Regex.Replace(acc, fst x, snd x)) source
        newString

    let regexWordMatch = new System.Text.RegularExpressions.Regex("\W|\w+");;

    let wordCount (s:string) = regexWordMatch.Matches(s).Count;;

    let sentenceEnd = new System.Text.RegularExpressions.Regex("[\\.\\!\\?]\\s+", System.Text.RegularExpressions.RegexOptions.Multiline)


//          SOME DATE STUFF


    ///<summary>Go through the list. The first match from the list that exists in the string, return previous n characters. Case-insensitive.
    ///Useful when you have a string with some kind of units in it, like hrs, Hours, hours. This can return the numbers just before the units</summary>
    let getFirstMatchFromList (src:String) (strings:string list) (previousCount:int) =
        let exists = strings |> List.exists(fun x->src.ContainsCaseInsensitive x)
        if exists then
            let firstMatch = strings |> List.find(fun x->src.ContainsCaseInsensitive x)
            let i = src.IndexOf(firstMatch)
            src.Substring(i - previousCount, previousCount)
        else
            ""
    ///<remarks>Pieces of a date. Used for parsing strings that might have funky dates</remarks>
    type enumDateParts =
        | year = 0
        | month = 1
        | week = 2
        | day = 3
        | hour = 4
        | minute = 5
        | second = 6
    
    let possibleDateAbbreviations = 
        [|
            (enumDateParts.year, ["yr."; "yr"; "yrs"; "yrs"; "year"; "years"]);
            (enumDateParts.month, ["mo."; "mo"; "mos"; "mos."; "month"; "months"]);
            (enumDateParts.week, ["wk."; "wk"; "wks."; "wks"; "week"; "weeks"]);
            (enumDateParts.day, ["day"; "days"; ]);
            (enumDateParts.hour, ["hr."; "hr"; "hrs"; "hrs."; "hour"; "hours"]);
            (enumDateParts.minute, ["min."; "min"; "mins"; "mins."; "minute"; "minutes"]);
            (enumDateParts.second, ["sec."; "sec"; "secs"; "secs."; "second"; "seconds"]);
        |]

    ///<summary>Given the enum for the date part you are looking for, and a string to check, tries to find the date value
    /// by looking at the abbreviations and going back 3 characters from them</summary>    
    let findDatePart (datePart:enumDateParts) src =
        let dateAbbrev = possibleDateAbbreviations |> Array.find(fun x->fst x = datePart)
        let didFindIt = getFirstMatchFromList src (snd dateAbbrev) 3
        if didFindIt = "" then "0" else didFindIt

    ///<summary>Takes a relative time - "3 mins ago" and tries to make it into a real System.DateTime</summary>
    let processRelativeTime (tm:String):DateTime = 
        let year = System.DateTime.Now.Year
        let month = System.DateTime.Now.Month
        let day = System.DateTime.Now.Day
        let hour = System.DateTime.Now.Hour
        let minute = System.DateTime.Now.Minute
        let second = System.DateTime.Now.Second
        try
            let hrs = findDatePart enumDateParts.hour tm
            let mns = findDatePart enumDateParts.minute tm
            let days = findDatePart enumDateParts.day tm
            System.DateTime.Now.Subtract(new TimeSpan(System.Int32.Parse(days), System.Int32.Parse(hrs), System.Int32.Parse(mns), 0))
        with |_ -> System.DateTime.Parse("1/1/1901")

    let getPrettyDate(d:System.DateTime) = 
        // Get time span elapsed since the date.
        let s = DateTime.Now.Subtract(d)
        // Get total number of days elapsed.
        let dayDiff = (int)s.TotalDays
        // Get total number of seconds elapsed.
        let secDiff = (int)s.TotalSeconds
        // Don't allow out of range values.
        if (dayDiff < 0) || (dayDiff >= 31)
            then ""
            else
            // Handle same-day times.
            if dayDiff = 0
                then
                    // Less than one minute ago.
                    if secDiff < 60 then "just now"
                    // Less than 2 minutes ago.
                    elif secDiff < 120 then "1 minute ago"
                    // Less than one hour ago.
                    elif secDiff < 3600 then String.Format("{0} minutes ago", Math.Floor((double)secDiff / 60.0))
                    // Less than 2 hours ago.
                    elif secDiff < 7200 then "1 hour ago"
                    // Less than one day ago.
                    //elif secDiff < 86400 then String.Format("{0} hours ago", Math.Floor((double)secDiff / 3600.0))
                    else String.Format("{0} hours ago", Math.Floor((double)secDiff / 3600.0))
                // Handle previous days.
                elif dayDiff = 1 then
                    "yesterday"
                elif dayDiff < 7 then
                    String.Format("{0} days ago", dayDiff)
                elif dayDiff < 31 then
                    String.Format("{0} weeks ago",  Math.Ceiling((double)dayDiff / 7.0))
                elif dayDiff < 365 then
                    String.Format("{0} months ago",  Math.Ceiling((double)dayDiff / 30.0))
                else
                    String.Format("{0} years ago",  Math.Ceiling((double)dayDiff / 365.0))
