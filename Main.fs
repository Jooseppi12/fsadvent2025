namespace fsadvent

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /bookmarks">] Bookmarks

module Templating =

    let Main (title: string) (body: Doc list) =
        Content.Page(
            Templates.MainTemplate()
                .Title(title)
                .Body(body)
                .Doc()
        )


module Site =
    open WebSharper.UI.Html

    open type WebSharper.UI.ClientServer

    let HomePage ctx (data: Types.Bookmark []) =
        let dataForHomepage = 
            data
            |> Array.filter (fun x -> x.homepage)
        Templating.Main "Homepage" [
            div [] [
                h1 [] [
                    a [attr.href "/bookmarks.html"] [text "List"]
                ]
            ]
            div [attr.``class`` "grid"] [
                div [attr.``class`` "container"] [
                    dataForHomepage
                    |> Array.map (fun x ->
                        let initial =
                            let uri = System.Uri(x.url)
                            // Extract out the initial character
                            uri.Host.Replace("www.", "")[0..0]
                        let tags =
                            x.tags |> String.concat ","
                        Templates.MainTemplate.GridItem()
                            .InitialCharacter(initial)
                            .Tags(tags)
                            .Url(x.url)
                            .Doc()   
                    )
                    |> Doc.Concat
                ]
            ]
        ]

    let BookmarkPage ctx (data: Types.Bookmark []) =
        Templating.Main "Bookmarks" [
            div [] [
                h1 [] [
                    a [attr.href "/"] [text "Home"]
                ]
            ]
            div [] [
                client <@ Client.Search () @>
                div [] [
                    data
                    |> Array.map (fun x ->
                        div [attr.``class`` "bookmark"] [
                            let tags =
                                x.tags |> String.concat ","
                            a [attr.href x.url; attr.``data-`` "tags" tags; attr.target "_blank"] [
                                span [] [text x.url]
                            ]
                        ]   
                    )
                    |> Doc.Concat
                ]
            ]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            async {
                let data = System.IO.File.ReadAllText (System.IO.Path.Combine(__SOURCE_DIRECTORY__,"bookmarks.json"))
                let bookmarks = System.Text.Json.JsonSerializer.Deserialize<Types.Bookmark []> data
                return!
                    match action with
                    | Home -> HomePage ctx bookmarks
                    | Bookmarks -> BookmarkPage ctx bookmarks
            }
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; Bookmarks]

[<assembly: Website(typeof<Website>)>]
do ()
