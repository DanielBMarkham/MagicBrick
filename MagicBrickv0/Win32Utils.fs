#if INTERACTIVE
    ;;
#else
#endif
/// <remarks>
/// Module for holding function references to a lot of Win32 code not provided in .NET
/// </remarks>
module Win32Utils

    open System
    open System.Runtime.InteropServices
    open System.IO
    open System.Windows.Forms

    let INVALID_HANDLE_VALUE = int (-1);

    [<DllImport("kernel32", SetLastError=true)>]
        extern IntPtr LoadLibrary(string lpFileName)
    /// <summary>
    /// To load the dll - dllFilePath dosen't have to be const - so I can read path from registry
    /// </summary>
    /// <param name="dllFilePath">file path with file name</param>
    /// <param name="hFile">use IntPtr.Zero</param>
    /// <param name="dwFlags">What will happend during loading dll
    /// <para>LOAD_LIBRARY_AS_DATAFILE</para>
    /// <para>DONT_RESOLVE_DLL_REFERENCES</para>
    /// <para>LOAD_WITH_ALTERED_SEARCH_PATH</para>
    /// <para>LOAD_IGNORE_CODE_AUTHZ_LEVEL</para>
    /// </param>
    /// <returns>Pointer to loaded Dll</returns>
    [<DllImport("kernel32")>]
    extern IntPtr LoadLibraryEx(string dllFilePath, IntPtr hFile, System.UInt32 dwFlags)

    [<DllImport("kernel32")>]
        extern bool FreeLibrary(IntPtr dllPointer)

    [<DllImport("kernel32")>]
        extern IntPtr GetProcAddress(IntPtr dllPointer, string functionName)

    [<DllImport("Kernel32.dll", SetLastError = true)>]
    extern bool ActivateActCtx(IntPtr hActCtx, UInt32 lpCookie)

//    [<StructLayout(LayoutKind.Sequential)>]
//    [<Struct>]
//    type ACTCTX() = 
//    {
//            cbSize:int;
//            uint dwFlags;
//            string lpSource;
//            ushort wProcessorArchitecture;
//            ushort wLangId;
//            string lpAssemblyDirectory;
//            string lpResourceName;
//            string lpApplicationName;
//            IntPtr hModule;
//    }
//    struct end

//    [<DllImport("Kernel32.dll", SetLastError = true)>]
//    extern IntPtr CreateActCtx(ref ACTCTX actctx)

    [<DllImport("Kernel32.dll", SetLastError = true)>]
    extern bool DeactivateActCtx(System.UInt32 dwFlags, System.UInt32 lpCookie)

    [<DllImport("Kernel32.dll", SetLastError = true)>]
    extern void ReleaseActCtx(IntPtr hActCtx)

    [<DllImport("user32.dll")>]
    extern bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, 
        [<MarshalAs(UnmanagedType.Bool)>] bool repaint)

    [<DllImport("user32.dll")>]
    extern IntPtr SetFocus(IntPtr hwnd)

    [<DllImport("user32.dll")>]
    extern IntPtr SendMessage(IntPtr hwnd, System.UInt32 msg, IntPtr wParam, IntPtr lParam)

    ///<summary>Create a rounded rectangle as a region. Useful for creating windows/controls with rounded corners</summary>
    [<DllImport("Gdi32.dll", EntryPoint="CreateRoundRectRgn")>]
    extern IntPtr CreateRoundRectRgn(
        int nLeftRect, // x-coordinate of upper-left corner
        int nTopRect, // y-coordinate of upper-left corner
        int nRightRect, // x-coordinate of lower-right corner
        int nBottomRect, // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
        )

    [<DllImport("User32.dll")>]
    extern bool MessageBeep(uint32 beepType)
    type enumGetType =
        | SM_CXSCREEN = 0l
        | SM_CYSCREEN = 1l
    [<DllImport("user32.dll", EntryPoint = "GetSystemMetrics")>]
    extern int GetSystemMetrics(enumGetType);
    [<DllImport("user32.dll")>]
    extern void SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int width, int height, uint32 flags);  

    let HWND_TOP = IntPtr.Zero
    let SWP_SHOWWINDOW = 64ul // 0×0040

    let screenSizeX = GetSystemMetrics(enumGetType.SM_CXSCREEN)
    let screenSizeY = GetSystemMetrics(enumGetType.SM_CYSCREEN)
    let setWinFullScreen (hwnd:System.IntPtr) = 
        SetWindowPos(hwnd, HWND_TOP, 0l, 0l, screenSizeX, screenSizeY, SWP_SHOWWINDOW)


    //  flag values for SoundFlags argument on PlaySound
    let   SND_SYNC              = 0x0000        // play synchronously
                                                // (default)
    let   SND_ASYNC             = 0x0001        // play asynchronously
    let   SND_NODEFAULT         = 0x0002        // silence (!default)
                                                // if sound not found
    let SND_MEMORY              = 0x0004        // pszSound points to
                                                // a memory file
    let SND_LOOP                = 0x0008        // loop the sound until
                                                // next sndPlaySound
    let SND_NOSTOP              = 0x0010        // don't stop any
                                                // currently playing
                                                // sound
    let SND_NOWAIT              = 0x00002000    // don't wait if the
                                                // driver is busy
    let SND_ALIAS               = 0x00010000    // name is a Registry
                                                // alias
    let SND_ALIAS_ID            = 0x00110000    // alias is a predefined
                                                // ID
    let SND_FILENAME            = 0x00020000    // name is file name
    let SND_RESOURCE            = 0x00040004    // name is resource name
                                                // or atom
    let SND_PURGE               = 0x0040        // purge non-static
                                                // events for task
    let SND_APPLICATION         = 0x0080        // look for application-
                                                // specific association    
    [<DllImport("winmm.DLL", EntryPoint = "PlaySound", SetLastError = true, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)>]
    extern bool PlaySound(byte[] szSound, System.IntPtr hMod, int flags);
    let playSoundFile (fileName:string) = 
        let bf = System.Text.Encoding.Unicode.GetBytes(fileName)
        PlaySound(bf, new System.IntPtr(), SND_FILENAME ||| SND_ASYNC ||| SND_NOSTOP)
    let stopPlayingSounds =
        PlaySound(null, new System.IntPtr(), SND_PURGE)
    let playMemoryFile (soundBits:byte array) =
        PlaySound(soundBits, IntPtr.Zero, SND_ASYNC ||| SND_MEMORY ||| SND_NOSTOP)

    type recWindowConfig = 
        {
            winState : FormWindowState;
            brdStyle : FormBorderStyle;
            topMost : bool;
            bounds : System.Drawing.Rectangle;
        }


    let isMaximized (formState:FormWindowState) = 
        formState.HasFlag(FormWindowState.Maximized)

    let saveFormState (targetForm:Form) = 
        {
            winState = targetForm.WindowState;
            brdStyle = targetForm.FormBorderStyle;
            topMost = targetForm.TopMost;
            bounds = targetForm.Bounds;
        }
 
    let Restore(targetForm:Form) (oldState:recWindowConfig) = 
        targetForm.WindowState <- oldState.winState
        targetForm.FormBorderStyle <- oldState.brdStyle
        targetForm.TopMost <- oldState.topMost
        targetForm.Bounds  <- oldState.bounds

    let maximizeForm(targetForm:Form) =
        if isMaximized targetForm.WindowState
            then
                ()
            else
                let oldState = saveFormState(targetForm)
                targetForm.WindowState <- FormWindowState.Maximized
                targetForm.FormBorderStyle <- FormBorderStyle.None
                targetForm.Tag <- oldState
                targetForm.TopMost <- true


