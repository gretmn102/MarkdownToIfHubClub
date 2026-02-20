#!/usr/bin/env -S dotnet fsi
#load "src/Library.fsx"
open Library

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
