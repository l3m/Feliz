module Feliz.UseMediaQuery

open System
open Browser
open Browser.Types
open Feliz

[<RequireQualifiedAccess>]
type ScreenSize =
    | Mobile
    | MobileLandscape
    | Tablet
    | Desktop
    | WideScreen

type Breakpoints = {
    MobileLandscape: int
    Tablet: int
    Desktop: int
    WideScreen: int
}

module Breakpoints = 
    let defaults = {
        MobileLandscape = 576
        Tablet = 768
        Desktop = 1024
        WideScreen = 1216
    }

let private makeQueries breakpoints =
    let mobileQuery = sprintf "(max-width: %ipx)" breakpoints.MobileLandscape
    let mobileLandscapeQuery = sprintf "(max-width: %ipx)" breakpoints.Tablet
    let tabletQuery = sprintf "(max-width: %ipx)" breakpoints.Desktop
    let desktopQuery = sprintf "(max-width: %ipx)" breakpoints.WideScreen
    mobileQuery, mobileLandscapeQuery, tabletQuery, desktopQuery

[<AutoOpen>]
module UseMediaQueryExtension =
     type React with
        static member useMediaQuery (query: string) =
            let (mq, setMq) = React.useState(fun () -> window.matchMedia(query))

            React.useEffect(fun () ->
                mq.addEventListener("change", fun e ->
                    let mqEvent = unbox<MediaQueryList>(e)
                    setMq mqEvent)
                {new IDisposable with
                     member this.Dispose() =
                        mq.removeEventListener("change", fun _ -> ())}

            , [| query :> obj |])

            mq.matches

        static member useResponsive(?breakpoints: Breakpoints) =
            let breakpoints = Option.defaultValue Breakpoints.defaults breakpoints
            let m, l, t, d = makeQueries breakpoints
            let mx = React.useMediaQuery m
            let lx = React.useMediaQuery l
            let tx = React.useMediaQuery t
            let dx = React.useMediaQuery d
            match mx, lx, tx, dx with
            | true, _, _, _ -> ScreenSize.Mobile
            | _, true, _, _ -> ScreenSize.MobileLandscape
            | _, _, true, _ -> ScreenSize.Tablet
            | _, _, _, true -> ScreenSize.Desktop
            | _ -> ScreenSize.WideScreen