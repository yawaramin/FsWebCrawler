﻿open System.Linq
open System.Net
open System.Text.RegularExpressions

(**
<summary>Crawl a single page and return all links found
therein.</summary>

<param name="client">The WebClient object to use to crawl the
page.</param>

<param name="pageUrl">The URL of the page to crawl.</param>

<returns>A list of URLs which are links found in the crawled
page.</returns>
*)
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

(**
<summary>Crawl a given site to a given depth and print out the links
found therein.</summary>

<param name="client">The WebClient object to use to crawl the
site.</param>

<param name="curDepth">The depth at which we are currently crawling
relative to the initial site URL at which we started crawling.</param>

<param name="moreDepth">How many more levels of links to continue
crawling.</param>

<param name="siteUrl">The URL of the site to crawl.</param>

<returns>Nothing.</returns>
*)
let rec crawlSite client curDepth moreDepth siteUrl =
  if moreDepth > 0
    then
      let links = crawlPage client siteUrl
      let prefix = String.replicate curDepth "-"

      for link in links do
        printfn "%s %s" prefix link
        crawlSite client (curDepth + 1) (moreDepth - 1) link

(**
<summary>Crawl a given site to a given depth and print out the links
found therein.</summary>

<returns>Nothing.</returns>
*)
let crawl depth siteUrl =
  let client = new WebClient()
  crawlSite client 0 depth siteUrl

(**
Main function that drives the app. This function handles the given
command-line arguments and makes sure they're valid.
*)
[<EntryPoint>]
let main argv =
  let usage () =
    printfn @"USAGE: FsWebCrawler depth siteUrl

depth must be a whole number
siteUrl must be a valid URL"
    1

  match argv with
  | [| depthStr; siteUrl |] ->
    try
      let depth = System.Int32.Parse(depthStr)

      crawl depth siteUrl
      0
    with | _ -> usage ()

  | _ -> usage ()
