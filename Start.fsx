#r "nuget: XPlot.Plotly"
#r @"./Mm1k/bin/Debug/net6.0/Mm1k.dll"

open XPlot.Plotly
open Hoge
open System

let averagePacket = []
let averageTime = []
let averageDisposePercentage = []
let N = []
let W = []
let Pk = []

let hoge lambda =
    let x = new MM1K(lambda, 1.0, 50, 1000, 0)
    x.StartSimulation()
    [x.AveragePacket; x.AverageTime; x.AverageDisposePercentage; x.N; x.W; x.Pk]

let foo lambda =
    [for _ in 0..9 -> hoge lambda]
    |> List.reduce (fun x y -> (List.map2 (fun a b -> a + b) x y))
    |> List.map (fun x -> x / 10.)

(foo 1.0) |> List.iter (stdout.WriteLine)

(*
[0..6] |> List.map (float >> fun x -> foo (Math.Round (0.7 + x * 0.05, 2))) |> List.iter (fun x -> stdout.WriteLine x)
*)


