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
                    "<h3 id=\"section\">1</h3>"
                    "<h4 id=\"section-1\">2</h4>"
                    "<h5 id=\"section-2\">3</h5>"
                    "<h4 id=\"section-3\">2</h4>"
                    ""
                ])
                ""
        testCase "truncateToCut" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "<!-- truncate -->"
                    ]
                ))
                (String.concat "\n" [
                    "<cut>"
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
                    "<h3 id=\"header\">Header</h3>"
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
        testCase "removeCodeFromPre: one line" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "```"
                        "code"
                        "```"
                    ]
                ))
                (String.concat "\n" [
                    "<pre>code</pre>"
                    ""
                ])
                ""
        testCase "removeCodeFromPre: two line" <| fun () ->
            Expect.equal
                (convert (
                    String.concat "\n" [
                        "```"
                        "code"
                        "code2"
                        "```"
                    ]
                ))
                (String.concat "\n" [
                    "<pre>code"
                    "code2</pre>"
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
