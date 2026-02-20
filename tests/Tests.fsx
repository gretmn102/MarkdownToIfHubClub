#!/usr/bin/env -S dotnet fsi
#r "nuget: Expecto, 10.2.3"
#load "../src/Library.fsx"
open Expecto

open Library

[<Tests>]
let convertTests =
    testList "convertTests" [
        testCase "moveOutContentFromParagraph: 1" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "First line"
                        "Second line"
                        ""
                        "Next paragraph"
                        ""
                        "Next paragraph"
                    ]
                ))
                (String.concat "\n" [
                    "First line<br />"
                    "Second line"
                    ""
                    "Next paragraph"
                    ""
                    "Next paragraph"
                    ""
                    ""
                ])
                ""
        testCase "moveOutContentFromParagraph: 2" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "First line"
                        "Second line"
                        ""
                        "Next paragraph"
                        ""
                        "# Header"
                    ]
                ))
                (String.concat "\n" [
                    "First line<br />"
                    "Second line"
                    ""
                    "Next paragraph"
                    ""
                    "<h4 id=\"header\">Header</h4>"
                    ""
                ])
                ""
    ]

exit (
    runTestsWithCLIArgs [] fsi.CommandLineArgs[1..] (
        testList "all" [
            convertTests
        ]
    )
)
