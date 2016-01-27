namespace FsWebAkka.Events

open System
open System.Threading.Tasks
open System.Collections.Generic



// The verbs of the system (in imperfect form)
module Events =

    // Events implemented as discriminated union. 
    // If you use a big solution, change to a base type 
    // or just use many event storages and concatenate / merge them 
    type Event =
        | SomethingReceivedEvent of Guid * string
        
        override x.ToString() = 
            match x with

            | SomethingReceivedEvent(id, message) -> 
                    sprintf "Item Id %A - Message - %s" id message
            
        
        static member CreateEventDescriptor (id:Guid, eventData:Event) = 
                EventDescriptor(id, eventData)

    // Container to capsulate events
    and EventDescriptor(id:Guid, eventData:Event) = 
        member x.Id = id
        member x.EventData = eventData

