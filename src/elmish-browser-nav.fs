/// This library is primarily about treating the address bar as an input to your program. 
namespace Elmish.Browser.Navigation

open Fable.Import.Browser
open Elmish

/// Parser is a function to turn the string in the address bar into
/// data that is easier for your app to handle.
type Parser<'a> = (Location -> 'a)

type Navigable<'msg> = 
    | Change of Location
    | UserMsg of 'msg

module Program =
  /// Add the navigation to a program made with `mkProgram` or `mkSimple`.
  /// urlUpdate: similar to `update` function, but receives parsed url instead of message as an input.
  let withNavigation (parser:Parser<'a>) (urlUpdate:'a->'model->('model * Cmd<'msg>)) (program:Program<'a,'model,'msg>) =
    let map (model, cmd) = 
        model, cmd |> Cmd.map UserMsg
    
    let update msg model =
        match msg with
        | Change location ->
            urlUpdate (parser location) model
        | UserMsg userMsg ->
            program.update userMsg model
        |> map

    let locationChanges dispatch = 
        window.addEventListener_hashchange(fun _ -> window.location |> (Change >> dispatch) |> box)
    
    let subs model =
        Cmd.batch
          [ [locationChanges]
            program.subscribe model |> Cmd.map UserMsg ]
    
    let init () = 
        program.init (parser window.location) |> map

    { init = init 
      update = update
      subscribe = subs }


