#r "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.0\\Profile\\Client\System.dll";;
let parser=makeParser "<html><head><title>My Awesome Title!</title><script src=\"http://www.cnn.com\"></script></head><body><h1>Cat1</h1><p>cat2</p><p><strong>I am the walrus</strong></p><div id=\"section 1\"></div><div id=\"section 2\"><p><em>Very important text here</em></p><p>Another paragraph</p></div></body></html>";;
let docSeq = parseNext parser;;
let lis = docSeq |> Seq.toList;;
docSeq |> Seq.iter(fun x->Console.WriteLine (x.Value.GenerateHTML));;