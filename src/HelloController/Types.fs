namespace HelloController

open System
open System.Collections.Concurrent

type Configurations =
    { NameLength: int
      SpaceLength: int
      FriendUntilRepeatInSeconds: int }

type Person = { IsFriend: bool; LastSeen: DateTime }

type Storage(confs: Configurations) =
    let people = ConcurrentDictionary<string, Person>()
    let _lock = Object()

    member x.Clear() = lock _lock (fun () -> people.Clear())

    member x.Count() = people.Count

    member x.AddPerson(name, person) =
        lock _lock (fun () -> people.TryAdd(name, person))


    member x.UpdatePerson(name, person) =
        lock _lock (fun () ->
            let exists, oldperson = people.TryGetValue(name)
            if exists then
                people.TryUpdate(name, person, oldperson) |> ignore
        )

    member x.GetPerson name : Option<Person> =
        lock _lock (fun () ->
            let exists, person = people.TryGetValue(name)
            if exists then Some(person) else None)

    member x.PersonExists name : bool = people.ContainsKey name

type VerifyNameError =
    | NoError
    | NameTooLong

type AuthenticateNameError =
    | NoError
    | Spammer
    | Unknown

type AddNameToFriendsError =
    | NoError
    | NotEnoughSpace

type UpdateFriendToSpammerError =
    | NoError
    | NameDoesntExist


type HelloFromError =
    | NoError
    | VerifyNameError of VerifyNameError
    | AuthenticateNameError of AuthenticateNameError
    | UpdateFriendToSpammerError of UpdateFriendToSpammerError
    | AddNameToFriendsError of AddNameToFriendsError
