//#if INTERACTIVE
//    ;;
//    #load "TypeUtils.fs"
//#else
//#endif
/// <remarks>
/// Module for holding types and functions related to multi-threading
/// </remarks>
module Sink

    open TypeUtils    
    open System
    open System.Threading

    /// <remarks>
    /// renaming of the Mailbox processor to what it really is, an agent
    /// </remarks>
    type Agent<'T> = MailboxProcessor<'T>

    /// <remarks>
    /// helper type to wrap up varius sync context issues
    /// </remarks>
    type SynchronizationContext with
        /// <summary>A standard helper extension method to raise an event on the GUI thread</summary>
        member syncContext.RaiseEvent (event: Event<_>) args =
            // this line was in the original cribbed code, but caused the system to crash
            //let mutable syncContext : SynchronizationContext = null
            syncContext.Post((fun _ -> event.Trigger args),state=null)
        /// <summary>A standard helper extension method to capture the current synchronization context.
        /// If none is present, use a context that executes work in the thread pool.</summary>
        static member CaptureCurrent () =
            match SynchronizationContext.Current with
            | null -> new SynchronizationContext()
            | ctxt -> ctxt
        ///<summary>A standard helper to raise an event on the GUI thread</summary>
        member syncContext.raiseEventOnGuiThread (event:Event<_>, args) =
            syncContext.Post((fun _ -> event.Trigger args),state=null)

    ///<remarks> a type that limits the numbers of something that can run at the same time, eg, opensockets</remarks>
    type requestGate(n:int) =
        let semaphore = new Semaphore(initialCount=n, maximumCount=n)
        ///<summary>Call this method using a let! in your thread and your thread will wait until a gate becomes available. Useful for throttling various activities</summary>
        member x.AsyncAcquire(?timeout) = 
            async { 
                let! ok = Async.AwaitWaitHandle(semaphore, ?millisecondsTimeout=timeout)
                if ok
                    then return
                            { new System.IDisposable with 
                                member x.Dispose() = semaphore.Release() |> ignore
                            }
                    else return! failwith "couldn't acquire semaphore"
                }
    
    ///<remarks>Worker bee class for doing long-running threaded work</remarks>
    type AsyncWorker<'T>(jobs: seq<Async<'T>>) =
        /// <summary>Capture the synchronization context to allow us to
        /// raise events back on the GUI thread</summary>
        let syncContext =
            let x = System.Threading.SynchronizationContext.Current
            if x = null then new System.Threading.SynchronizationContext() else x
        let cancellationCapability = new CancellationTokenSource()
         
        // Each of these lines declares an F# event that we can raise
        let allCompleted    = new Event<'T[]>()
        let error           = new Event<System.Exception>()
        let canceled        = new Event<System.OperationCanceledException>()
        let jobCompleted    = new Event<int * 'T>()
 
        /// <summary>Start an instance of the work</summary>
        member x.Start()   =                                                                                                             
            // Capture the synchronization context to allow us to raise events back on the GUI thread
            let syncContext = SynchronizationContext.CaptureCurrent()
            // Mark up the jobs with numbers
            let jobs = jobs |> Seq.mapi (fun i job -> (job,i+1))
            // create an array of async jobs that all return back to result,
            // making a new array of function results
            let work = 
                Async.Parallel
                   [ for (job,jobNumber) in jobs ->
                       async { let! result = job
                               syncContext.RaiseEvent jobCompleted (jobNumber,result)
                               return result } ]
            // Start computing
            Async.StartWithContinuations
                ( work,
                  (fun res -> syncContext.RaiseEvent allCompleted res),
                  (fun exn -> syncContext.RaiseEvent error exn),
                  (fun exn -> syncContext.RaiseEvent canceled exn ),
                  cancellationCapability.Token)
 
        member x.CancelAsync() =
           cancellationCapability.Cancel()
        /// Raised when a particular job completes
        member x.JobCompleted   = jobCompleted.Publish
        /// Raised when all jobs complete
        member x.AllCompleted   = allCompleted.Publish
        /// Raised when the composition is cancelled successfully
        member x.Canceled       = canceled.Publish
        /// Raised when the composition exhibits an error
        member x.Error          = error.Publish

//    type flickeringClass() = 
//        let flickerOn           = new Event<_>()
//        let flickerOff          = new Event<_>()
//        let mutable group       = new CancellationTokenSource()
//        member x.FlickerOn      = flickerOn.Publish
//        member x.FlickerOff     = flickerOff.Publish
//
//        member x.BeginFlickering() =
//            let syncContext = SynchronizationContext.CaptureCurrent()
//            group <- new CancellationTokenSource()
//            let flickerer = 
//                async{
//                        let rand = new System.Random()
//                        let initialOnPeriod = 50
//                        let initialOffPeriod = 350
//                        let rec loop (currentOnPeriod:int) currentOffPeriod =
//                            syncContext.RaiseEvent flickerOn 1
//                            System.Threading.Thread.Sleep(currentOnPeriod)
//                            let decreaseAmount = rand.Next(50, 100)
//                            if currentOffPeriod - decreaseAmount > 0 then
//                                syncContext.RaiseEvent flickerOff 1            
//                                System.Threading.Thread.Sleep(currentOffPeriod)
//                                loop (currentOnPeriod + decreaseAmount * 2) (currentOffPeriod - decreaseAmount)
//                            else
//                                ()
//                        loop initialOnPeriod initialOffPeriod
//                    }
//            Async.Start(flickerer, group.Token)
//
//        member x.StopFlickering() =
//            group.Cancel();
//            group <- new CancellationTokenSource()


