namespace FsWebAkka.Commands

open System
open System.Collections.Generic
open FsWebAkka.Events
open EventStorage
open System.Threading.Tasks
open FsWebAkka
open Events

[<CLIMutable>]
type CommandWrapper = 
    {   ConnectionId : string
        Id : Guid
        Created : DateTimeOffset
        Command : string }
        
    static member CreateCommand connectionId (command : string) =         
        {   Id = (Guid.NewGuid())
            Created = (DateTimeOffset.Now)
            ConnectionId = connectionId
            Command = command }
    

// The verbs (actions) of the system (in imperative mood / present tense)
module CommandHandler =
 
    let Storage = new EventStorage() 

    let AsyncHandle (cmd:CommandWrapper) = 
        async { // async workflow
       
            let { ConnectionId = connectionId
                  Id = id  
                  Created = created
                  Command = tradingCommand } = cmd
           
            printfn "Sending message %s" cmd.Command
        }

        
    let Handle (msg:CommandWrapper) =  
                let handleResponse = AsyncHandle msg |> Async.Catch |> Async.RunSynchronously
                match handleResponse with 
                | Choice1Of2(_) -> ()
                | Choice2Of2(exn) -> ()

