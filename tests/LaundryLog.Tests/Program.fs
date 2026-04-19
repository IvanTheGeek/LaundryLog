module LaundryLog.Tests.Program

open Expecto
open LaundryLog.Tests.Tests

[<EntryPoint>]
let main args =
    runTestsWithCLIArgs [] args allTests
