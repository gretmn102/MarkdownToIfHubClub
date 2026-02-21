#r "nuget: Markdig, 0.45.0"
open Markdig
open Markdig.Helpers
open Markdig.Syntax
open Markdig.Syntax.Inlines

let shiftHeaderLevel (markdownDocument: Syntax.MarkdownDocument) =
    let rec mapBlocks (blocks: Syntax.Block seq) =
        blocks
        |> Seq.iter (function
            | :? Syntax.HeadingBlock as headingBlock ->
                headingBlock.Level <- headingBlock.Level + 2
            | _ -> ()
        )
    mapBlocks markdownDocument

let truncateToCut (markdownDocument: MarkdownDocument) =
    let rec mapBlocks (blocks: ContainerBlock) =
        blocks
        |> Seq.iteri (fun currentBlockIndex ->
            function
            | :? Syntax.HtmlBlock as htmlBlock ->
                if htmlBlock.Type = HtmlBlockType.Comment
                    && htmlBlock.Lines.Lines[0].Slice.IndexOf "truncate" > 0
                then
                    blocks[currentBlockIndex] <- new HtmlBlock (
                        parser = null,
                        Lines = StringLineGroup "<cut>"
                    )
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
    let rec mapBlocks (blocks: Syntax.ContainerBlock) =
        let getNextBlock currentBlockIndex =
            let nextBlockIndex = currentBlockIndex + 1
            if not (nextBlockIndex < blocks.Count) then
                None
            else
                Some blocks[nextBlockIndex]
        blocks
        |> Seq.iteri (fun currentBlockIndex currentBlock ->
            match currentBlock with
            | :? QuoteBlock as quoteBlock ->
                mapBlocks quoteBlock
            | :? ParagraphBlock as paragraphBlock ->
                let block = RawTextBlock()
                block.Inline <-
                    LiteralInline (
                        match getNextBlock currentBlockIndex with
                        | None -> "\n"
                        | Some (:? ParagraphBlock | :? HeadingBlock) ->
                            "\n\n"
                        | _ -> "\n"
                    )
                    |> paragraphBlock.Inline.AppendChild
                blocks[currentBlockIndex] <- block
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
    truncateToCut document
    moveOutContentFromParagraph document
    use writer = new System.IO.StringWriter()
    let render = Renderers.HtmlRenderer writer
    render.ObjectRenderers.Add(RawTextRenderer())
    render.Render(document).ToString()
