open System.Linq
open System.Net
open System.Text.RegularExpressions

let crawlPage (client : WebClient) (pageUrl : string) =
  try
    let pageContent = client.DownloadString(pageUrl)

    let pageLinkMatches =
      Regex.Matches(
        pageContent,
        "a href=['\"](.[^'\"]+)['\"]",
        RegexOptions.Compiled).Cast<Match>()

    pageLinkMatches
    |> Seq.map (fun m -> m.Groups.Item(1).Value)
    |> List.ofSeq

  with | _ -> []

let rec crawlSite client curDepth moreDepth siteUrl =
  if moreDepth > 0
    then
      let links = crawlPage client siteUrl
      let prefix = String.replicate curDepth "-"

      for link in links do
        printfn "%s %s" prefix link
        crawlSite client (curDepth + 1) (moreDepth - 1) link

let crawl depth siteUrl =
  let client = new WebClient()
  crawlSite client 0 depth siteUrl

[<EntryPoint>]
let main argv =
  let usage () =
    printfn "USAGE: FsWebCrawler depth siteUrl\n\ndepth must be a whole number\nsiteUrl must be a valid URL"
    1

  match argv with
  | [| depthStr; siteUrl |] ->
    try
      let depth = System.Int32.Parse(depthStr)

      crawl depth siteUrl
      0
    with | _ -> usage ()

  | _ -> usage ()
