namespace FsWebAkka

open System
open System.Collections.Generic
open System.IO
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Mvc
open System.Web.Routing
open System.Web.Http
open System.Threading
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers
open System.Reactive
open FsWebAkka.Controllers
open System
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Mvc
open System.Web.Routing
open System.Web.Optimization
open FsWebAkka.Commands
open FSharp.Reactive
open Akka
open Akka.Actor
open Akka.FSharp
open CommandHandler

type CompositionRoot(tradingRequestObserver:IObserver<CommandWrapper>) =
    interface IHttpControllerActivator with
        member this.Create(request, controllerDescriptor, controllerType) =
            if controllerType = typeof<ValuesController> then
                let c = new ValuesController()
                c   // c.Subscribe  agent.Post
                |> Observable.subscribeObserver tradingRequestObserver                
                |> request.RegisterForDispose
                c :> IHttpController
            else
                raise
                <| ArgumentException(
                    sprintf "Unknown controller type requested: %O" controllerType,
                    "controllerType")

type BundleConfig() =
    static member RegisterBundles (bundles:BundleCollection) =
        bundles.Add(ScriptBundle("~/bundles/jquery").Include([|"~/Scripts/jquery-{version}.js"|]))

        // Use the development version of Modernizr to develop with and learn from. Then, when you're
        // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
        bundles.Add(ScriptBundle("~/bundles/modernizr").Include([|"~/Scripts/modernizr-*"|]))

        bundles.Add(ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js"))

        bundles.Add(StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/site.css"))

/// Route for ASP.NET MVC applications
type Route = { 
    controller : string
    action : string
    id : UrlParameter }

type HttpRoute = {
    controller : string
    id : RouteParameter }



type ActorCommand(system) =
    let actorComand system = 
        spawn system "CommandActor" (fun inbox ->
            let rec loop() = 
                actor{
                    let! msg = inbox.Receive()
                    AsyncHandle msg |> Async.Start
                    return! loop() }
            loop())

    member x.StartActor() = actorComand system

type Global() =
    inherit System.Web.HttpApplication() 
    
    static let systemActor = System.create "aspMvcActorSystem" (Configuration.defaultConfig())

    static let actor = 
        ActorCommand(systemActor).StartActor()

    static member RegisterWebApi(config: HttpConfiguration) =
        // Configure routing
        config.MapHttpAttributeRoutes()
        config.Routes.MapHttpRoute(
            "DefaultApi", // Route name
            "api/{controller}/{id}", // URL with parameters
            { controller = "{controller}"; id = RouteParameter.Optional } // Parameter defaults
        ) |> ignore

        // Configure serialization
        config.Formatters.XmlFormatter.UseXmlSerializer <- true
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

        config.Services.Replace(typeof<IHttpControllerActivator>,
                        // I create a subscription controller to the Agent
                        // Each time a message come in (post) the publisher send the message (OnNext)
                        // to all the subscribers, in this case the Agent 
                        CompositionRoot( Observer.Create(fun x -> actor.Tell(x))))



        // Additional Web API settings

    static member RegisterFilters(filters: GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())

    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        routes.MapRoute(
            "Default", // Route name
            "{controller}/{action}/{id}", // URL with parameters
            { controller = "Home"; action = "Index"; id = UrlParameter.Optional } // Parameter defaults
        ) |> ignore

    member x.Application_Start() =
        AreaRegistration.RegisterAllAreas()
        GlobalConfiguration.Configure(Action<_> Global.RegisterWebApi)
        Global.RegisterFilters(GlobalFilters.Filters)
        Global.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles BundleTable.Bundles