namespace fsadvent

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Templating

[<JavaScript>]
module Types =
    type Bookmark =
        {
            url: string
            homepage: bool
            tags: string []
        }

[<JavaScript>]
module Templates =   
    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.PerRequest>

[<JavaScript>]
module Client =
    let Search () =
        let searchText = Var.Create ""
        let searchFunction (s: string) =
            // if text starts with tag:, search within the tags otherwise search in the text url
            let elementsToShow =
                if s.StartsWith "tag:" then
                    let tag = s.Replace("tag:", "")
                    JS.Document.QuerySelectorAll(sprintf ".bookmark:has(a[data-tags*=\"%s\"])" tag)
                else
                    JS.Document.QuerySelectorAll(sprintf ".bookmark:has(a[href*=\"%s\"])" s)
            JS.Document
                .QuerySelectorAll(".bookmark")
                .ForEach((fun (n, _, _, _) -> 
                    (As<Dom.Element> n).ClassList.Add "hidden"
                ), null)
            elementsToShow
                .ForEach((fun (n, _, _, _) -> 
                    (As<Dom.Element> n).ClassList.Remove "hidden"
                ), null)

        Templates.MainTemplate.Search()
            .SearchText(searchText)
            .Search(fun _ ->
                searchText.View
                |> View.Get searchFunction
            )
            .Doc()
