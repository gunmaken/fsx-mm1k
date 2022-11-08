#r "nuget: XPlot.Plotly"
#r @"./Mm1k/bin/Debug/net6.0/Mm1k.dll"

open XPlot.Plotly
open Hoge
open System

(*let averagePacket = [|for _ in 1..7 -> 0.0|]
let averageTime = [|for _ in 1..7 -> 0.0|]
let averageDisposePercentage = [|for _ in 1..7 -> 0.0|]


let rec loop1 i =
    
    let rec loop2 j =
        if j = 7 then ()
        else
            let a = new MM1K(Math.Round (0.7 + (j |> double) * 0.05, 2), 1., 50, 10000, 0)
            a.StartSimulation()
            a.Report()
            averagePacket.[j] <- a.AveragePacket + averagePacket.[j]
            averageTime.[j] <- a.AverageTime + averageTime.[j]
            averageDisposePercentage.[j] <- a.AverageDisposePercentage + averageDisposePercentage.[j]
            loop2 (j + 1)
    if i = 10 then ()
    else
        loop2 0
        loop1 (i + 1)

loop1 0
let plist = averagePacket |> Array.map (fun x -> x / 10.) |> Array.toList
plist |> List.iter (fun x -> stdout.WriteLine x)
*)

let rec loop2 j =
    if j = 7 then ()
    else
        let a = new MM1K(Math.Round (0.7 + (j |> double) * 0.05, 2), 1., 50, 10000, 0)
        a.StartSimulation()
        a.Report()
        a.Theoretical()
        printfn $"{a.N} {a.W} {a.Pk}"
        loop2 (j + 1)
loop2 0