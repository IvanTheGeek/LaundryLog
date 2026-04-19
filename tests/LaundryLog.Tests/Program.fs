module LaundryLog.Tests.Program

open Expecto
open LaundryLog.Tests.Tests
open LaundryLog.Tests.ServiceTests

[<EntryPoint>]
let main args =
    let all = testList "LaundryLog" [ allTests; allServiceTests ]
    runTestsWithCLIArgs [] args all
