namespace HelloController
open System


type HelloCtrl(confs: Configurations, store:Storage) = 
    member private x.verifyName ( name:string) : VerifyNameError =
        if confs.NameLength < name.Length then 
            NameTooLong
        else 
            VerifyNameError.NoError 

    member private x.authenticateName (name: string) : AuthenticateNameError =
        match store.GetPerson(name) with 
        | Some person -> 
            if person.IsFriend then 
                if DateTime.Now.Subtract(person.LastSeen).Seconds < confs.FriendUntilRepeatInSeconds then 
                    Spammer
                else
                    AuthenticateNameError.NoError  
            else Spammer
        | None -> Unknown

    member private x.addNameToFriends (name:string) : AddNameToFriendsError =
        if confs.SpaceLength <= store.Count() then 
            NotEnoughSpace
        else
            store.AddPerson (name, {IsFriend = true; LastSeen = DateTime.Now}) |> ignore
            AddNameToFriendsError.NoError


    member private x.updateFriendToSpammer (name:string): UpdateFriendToSpammerError = 
        if not (store.PersonExists name) then 
            NameDoesntExist
        else
            store.UpdatePerson (name, {IsFriend = false ; LastSeen = DateTime.Now}) |> ignore
            UpdateFriendToSpammerError.NoError


    member x.HelloFrom (name: string) : string * HelloFromError =
        let vername = x.verifyName name 
        match vername with
        | NameTooLong ->
            "Get away from me",VerifyNameError(vername)
        | VerifyNameError.NoError -> 
            let anerr =   x.authenticateName name
            match anerr with 
            | Spammer -> 
                let uftserr = x.updateFriendToSpammer name 
                if uftserr.IsNameDoesntExist then 
                    "Someone touched my memory", UpdateFriendToSpammerError(uftserr)
                else
                    "Get away from me", AuthenticateNameError(anerr)
            | Unknown -> 
                let antferr = x.addNameToFriends(name)
                if antferr.IsNotEnoughSpace then 
                    "Not enough space in brain", AddNameToFriendsError(antferr)
                else 
                    $"Hello, nice to meet you {name}" , AuthenticateNameError(anerr)
                
            | AuthenticateNameError.NoError -> 
                store.UpdatePerson (name, {IsFriend = true ; LastSeen = DateTime.Now}) |> ignore
                $"Hello, my friend", HelloFromError.NoError


