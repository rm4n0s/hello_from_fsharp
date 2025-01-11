module Tests

open System
open System.Threading
open Xunit
open HelloController
open System.Collections.Generic

let confs: Configurations =
    { NameLength = 10
      SpaceLength = 5
      FriendUntilRepeatInSeconds = 1 }

let store = Storage(confs)

let ctrl = HelloCtrl(confs, store)


[<Fact>]
let ``test name too long`` () =
    let (msg, err) = ctrl.HelloFrom("manoaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaas")
    Assert.Equal("Get away from me", msg)

    let mutable passedNamedTooLong = true

    match err with
    | VerifyNameError vferr ->
        match vferr with
        | NameTooLong -> passedNamedTooLong <- true
        | _ -> ()
    | _ -> ()

    Assert.True(passedNamedTooLong)


[<Fact>]
let ``make new friend`` () =
    store.Clear()
    let name = "manos"

    // check the name that does not exist in friends list
    Assert.True(store.GetPerson(name).IsNone)

    let (msg, err) = ctrl.HelloFrom(name)
    Assert.Equal($"Hello, nice to meet you {name}", msg)

    // check if the unknown error is received
    let mutable passedUnknown = true

    match err with
    | AuthenticateNameError anerr ->
        match anerr with
        | Unknown -> passedUnknown <- true
        | _ -> ()
    | _ -> ()


    Assert.True(passedUnknown)

    // check the name that exists in friend's list
    Assert.True(store.GetPerson(name).IsSome)


[<Fact>]
let ``say hi to a friend that you haven't say see for two seconds`` () =
    let name = "manos"

    store.Clear()

    // create a friend to say hi
    let added = store.AddPerson(
        name,
        { IsFriend = true
          LastSeen = DateTime.Now.AddSeconds(-2) }
    )
    Assert.True(added)


    let (msg, err) = ctrl.HelloFrom(name)

    Assert.Equal("Hello, my friend", msg)
    let mutable passedFriend = false

    match err with
    | NoError -> passedFriend <- true
    | _ -> ()

    Assert.True(passedFriend)
    
    // check the name that exists in friend's list
    Assert.True(store.GetPerson(name).IsSome)
    let friend = store.GetPerson(name).Value
    Assert.True(friend.IsFriend)
    Assert.Equal(DateTime.Now.ToString "H:mm:ss", friend.LastSeen.ToString "H:mm:ss")


[<Fact>]
let ``a friend becomes a spammer`` () =
    let name = "manos"
    store.Clear()
    // create a friend that will become a spammer
    let added = store.AddPerson(
        name,
        { IsFriend = true
          LastSeen = DateTime.Now }
    )
    Assert.True(added)


    let (msg, err) = ctrl.HelloFrom(name)


    Assert.Equal("Get away from me", msg)
    let mutable passedSpammer = false

    match err with
    | AuthenticateNameError anerr ->
        match anerr with
        | Spammer -> passedSpammer <- true
        | _ -> ()
    | _ -> ()

    Assert.True(passedSpammer)

    let person = store.GetPerson(name)
    Assert.True(person.IsSome)

    Assert.False(person.Value.IsFriend)

[<Fact>]
let ``test space limit of names`` () =
    store.Clear()
    for i = 1 to confs.SpaceLength  do 
        let name = $"manos{i}"
        store.AddPerson(
            name,
          { IsFriend = true
            LastSeen = DateTime.Now }
        ) |> ignore

    let (msg, err) = ctrl.HelloFrom("manos")


    Assert.Equal( "Not enough space in brain", msg)
    let mutable passedNotEnoughSpace = false

    match err with
    | AddNameToFriendsError anerr ->
        match anerr with
        |  NotEnoughSpace -> passedNotEnoughSpace <- true
        | _ -> ()
    | _ -> ()

    Assert.True(passedNotEnoughSpace)
