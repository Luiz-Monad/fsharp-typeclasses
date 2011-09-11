﻿#load "Prelude.fs"
open Prelude


// return

let anOption :option<_> = return' 2
let aList    :list<_>   = return' 2


// List Monad

// F#                           // Haskell
let result = 
    do' {                       // do {
        let! x1 = [1;2]         //   x1 <- [1;2]
        let! x2 = [10;20]       //   x2 <- [10;20]
        return ((+) x1 x2) }    //   return (f x1 x2) }

// desugared version
let binded = [1;2]  >>= (fun x1 -> [10;20] >>= (fun x2 ->  return'((+) x1 x2 )))

// IO Monad

let action = do' {
    let! _  = putStrLn  "What is your first name?"
    let! fn = getLine
    let! _  = putStrLn  ("Thanks, " + fn) 
    let! _  = putStrLn  ("What is your last name?")
    let! ln = getLine
    let  fullname = fn + " " + ln
    let! _  = putStrLn  ("Your full name is: " + fullname)
    return fullname }
// try -> IO.Invoke action ;;


// Functors

let times2,plus3 = (*) 2, (+) 3

let valTimes3   = fmap plus3 (Some 4)
let noValue     = fmap plus3 None
let lstTimes2   = fmap times2 [1;2;3;4]
let times2plus3 = fmap times2 plus3
let getChars    = fmap (fun (x:string) -> x.ToCharArray() |> Seq.toList ) action
// try -> IO.Invoke getChars ;;




// Define a type Tree

type Tree<'a> =
    | Tree of 'a * Tree<'a> * Tree<'a>
    | Leaf of 'a
    static member map f (t:Tree< 'a>  )  =
        match t with
        | Leaf x -> Leaf (f x)
        | Tree(x,t1,t2) -> Tree(f x, Tree.map f t1,  Tree.map f t2)

// add instance for Functor class
    static member (?) (x:Tree<_>,cs:Fmap )   = fun f -> Tree.map    f x

let myTree = Tree(6, Tree(2, Leaf(1), Leaf(3)), Leaf(9))
let mappedTree = fmap times2plus3 myTree