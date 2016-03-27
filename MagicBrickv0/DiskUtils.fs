#if INTERACTIVE
//    #load "TypeUtils.fs"
#else
#endif
/// <remarks>
/// Disk types and functions
/// </remarks>
module DiskUtils

    open System

    ///<summary>Loads a file. Returns byte array</summary>
    let loadFileToByteArray(fileName:string) =
        try
            use str = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read)
            let rdr = new System.IO.BinaryReader(str)
            let bytesToRead = int str.Length
            rdr.ReadBytes(bytesToRead)
        with
            |e->
                Console.WriteLine(e.Message)
                Array.empty

