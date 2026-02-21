#!/usr/bin/env -S dotnet fsi
#r "nuget: Expecto, 10.2.3"
#load "../src/Library.fsx"
open Expecto

open Library

[<Tests>]
let convertTests =
    testList "convertTests" [
        testCase "shiftHeaderLevel" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "# 1"
                        "## 2"
                        "### 3"
                        "## 2"
                    ]
                ))
                (String.concat "\n" [
                    "<h4 id=\"section\">1</h4>"
                    "<h5 id=\"section-1\">2</h5>"
                    "<h6 id=\"section-2\">3</h6>"
                    "<h5 id=\"section-3\">2</h5>"
                    ""
                ])
                ""
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
        testCase "moveOutContentFromParagraph: in blocks" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "> hello world"
                    ]
                ))
                (String.concat "\n" [
                    "<blockquote>"
                    "hello world"
                    "</blockquote>"
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
