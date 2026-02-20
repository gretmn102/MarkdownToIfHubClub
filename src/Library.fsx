#r "nuget: Markdig, 0.45.0"
open Markdig
open Markdig.Syntax
open Markdig.Syntax.Inlines

let shiftHeaderLevel (markdownDocument: Syntax.MarkdownDocument) =
    let rec mapBlocks (blocks: Syntax.Block seq) =
        blocks
        |> Seq.iter (function
            | :? Syntax.HeadingBlock as headingBlock ->
                headingBlock.Level <- headingBlock.Level + 3
            | _ -> ()
        )
    mapBlocks markdownDocument

type RawTextBlock() =
    inherit LeafBlock null
    member val Inline : Inline = null with get, set

type RawTextRenderer() =
    inherit Renderers.Html.HtmlObjectRenderer<RawTextBlock>()
    override _.Write(writer: Renderers.HtmlRenderer, block: RawTextBlock) =
        if not (isNull block.Inline) then
            writer.Write block.Inline

let moveOutContentFromParagraph (markdownDocument: MarkdownDocument) =
    let rec mapBlocks (blocks: Syntax.Block seq) =
        blocks
        |> Seq.iteri (fun i ->
            function
            | :? ParagraphBlock as paragraphBlock ->
                let block = RawTextBlock()
                block.Inline <- paragraphBlock.Inline
                markdownDocument[i] <- block
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
    moveOutContentFromParagraph document
    use writer = new System.IO.StringWriter()
    let render = Renderers.HtmlRenderer writer
    render.ObjectRenderers.Add(RawTextRenderer())
    render.Render(document).ToString()
