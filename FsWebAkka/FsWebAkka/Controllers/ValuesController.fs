namespace FsWebAkka.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http

open System.Reactive.Subjects
open FsWebAkka.Commands

/// Retrieves values.
[<RoutePrefix("api2/values")>]
type ValuesController() =
    inherit ApiController()
    let values = [|"value1";"value2"|]
    
    
    let subject = new Subject<CommandWrapper>()



    /// Gets all values.
    [<Route("")>]
    member x.Get() = 
        let cmd = CommandWrapper.CreateCommand (sprintf "Get all Ids") "Command Get All"
        subject.OnNext(cmd)
        values

    /// Gets the value with index id.
    [<Route("{id:int}")>]
    member x.Get(id) : IHttpActionResult =
        if id > values.Length - 1 then
            x.BadRequest() :> _
        else
            let cmd = CommandWrapper.CreateCommand (sprintf "Get id %d" id) "Command Get"
            subject.OnNext(cmd)
            x.Ok(values.[id]) :> _

    interface IObservable<CommandWrapper> with
        member this.Subscribe observer = subject.Subscribe observer
    
    override this.Dispose disposing = 
        if disposing then 
            subject.Dispose()
        base.Dispose disposing