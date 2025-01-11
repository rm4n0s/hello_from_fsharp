// For more information see https://aka.ms/fsharp-console-apps
open HelloController

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open HelloController
open Giraffe

let confs: Configurations = {
    FriendUntilRepeatInSeconds = 3
    NameLength = 10
    SpaceLength = 100
}

let store: Storage = Storage(confs)


let helloCtrl = HelloController.HelloCtrl(confs, store)

let helloFrom (name : string) : HttpHandler =
    let greeting, _ = helloCtrl.HelloFrom name
    text greeting

let webApp =
    choose [
        routef "/hello_from/%s"      helloFrom 
    ]



let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0