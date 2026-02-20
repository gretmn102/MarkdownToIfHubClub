#!/usr/bin/env -S dotnet fsi
#r "nuget: Markdig, 0.45.0"
open Markdig

let shiftHeaderLevel (markdownDocument: Syntax.MarkdownDocument) =
    let rec mapBlocks (blocks: Syntax.Block seq) =
        blocks
        |> Seq.iter (function
            | :? Syntax.HeadingBlock as headingBlock ->
                headingBlock.Level <- headingBlock.Level + 3
            | _ -> ()
        )
    mapBlocks markdownDocument

let private markdownPipeline =
    let pipe = MarkdownPipelineBuilder()

    pipe.UseSoftlineBreakAsHardlineBreak() |> ignore

    let opt = Extensions.AutoLinks.AutoLinkOptions()
    opt.OpenInNewWindow <- true
    opt.UseHttpsForWWWLinks <- true
    pipe.UseAutoLinks opt |> ignore

    pipe.UseCitations() |> ignore
    pipe.UseFigures() |> ignore
    pipe.UseFooters() |> ignore

    pipe.UseFootnotes() |> ignore

    pipe.UseAutoIdentifiers Extensions.AutoIdentifiers.AutoIdentifierOptions.AutoLink |> ignore

    pipe.UseMediaLinks() |> ignore

    pipe.UsePipeTables() |> ignore

    pipe.UseGenericAttributes() |> ignore

    pipe.UseEmphasisExtras() |> ignore // for ~~strike~~

    pipe.Build()

let convert rawMarkdown =
    let document = Markdig.Markdown.Parse(rawMarkdown, markdownPipeline)
    shiftHeaderLevel document
    use writer = new System.IO.StringWriter()
    let render = Renderers.HtmlRenderer writer
    render.Render(document).ToString()

#if !INTERACTIVE
[<EntryPoint>]
#endif
let main args =
    match args with
    | [|outputPath; inputPath|] ->
        try
            let rawMarkdown = System.IO.File.ReadAllText inputPath
            let result = convert rawMarkdown
            System.IO.File.WriteAllText(outputPath, result)
            0
        with e ->
            eprintfn "%s" e.Message
            1
    | _ ->
        eprintfn "expected `script input_path output_path`, but: %A" args
        1

#if INTERACTIVE
main fsi.CommandLineArgs[1..]
#endif
