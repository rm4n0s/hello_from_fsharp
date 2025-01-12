module Tests

open System
open Xunit
open FsHttp
open System.Threading

[<Fact>]
let ``test from an unknown to a friend to a spammer`` () =
    let rnd: Random = System.Random()
    let num = rnd.Next(900) + 1
    let name = $"manos{num}"
    let output = 
        get $"http://localhost:5000/hello/{name}"     
        |> Request.send
        |> Response.toText

    Assert.Equal($"Hello, nice to meet you {name}", output)

    Thread.Sleep(60000)
    let output = 
        get $"http://localhost:5000/hello/{name}"     
        |> Request.send
        |> Response.toText

    Assert.Equal($"Hello, my friend", output)


    let output = 
        get $"http://localhost:5000/hello/{name}"     
        |> Request.send
        |> Response.toText

    Assert.Equal($"Get away from me", output)

