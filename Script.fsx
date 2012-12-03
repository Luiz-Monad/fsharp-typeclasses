﻿#r @"lib\InlineHelper.dll"
#load "Prelude.fs"
open Prelude


// Numerics

let res5_55:Integer * _ = properFraction 5.55M
let res111_20 = toRational 5.55
let res4_3    = toRational (12 % 9)
let res17_1   = toRational 17uy
let divisions = map ( quot/> 5G) [5;8;10;15;20]

let inline quadratic a b c =
    let root1 = ( -b + sqrt (  b **^ 2 - 4G * a * c) )  / (2G * a)
    let root2 = ( -b - sqrt (  b **^ 2 - 4G * a * c) )  / (2G * a)
    (root1,root2)

let res30_15  = quadratic 2.0  -3G -9G
let res30_15f = quadratic 2.0f -3G -9G
let resCmplx:System.Numerics.Complex * _ = quadratic 2G -3G 9G

// return

let resSome2 :option<_> = return' 2
let resSing2 :list<_>   = return' 2


// List Monad

// F#                           // Haskell
let result = 
    do' {                       // do {
        let! x1 = [1;2]         //   x1 <- [1;2]
        let! x2 = [10;20]       //   x2 <- [10;20]
        return ((+) x1 x2) }    //   return ((+) x1 x2) }

// desugared version
let lst11n21n12n22 = [1;2]  >>= (fun x1 -> [10;20] >>= (fun x2 ->  return'((+) x1 x2 )))


// IO Monad

let action = do' {
    do! putStrLn  "What is your first name?"
    let! fn = getLine
    do! putStrLn  ("Thanks, " + fn) 
    do! putStrLn  ("What is your last name?")
    let! ln = getLine
    let  fullname = fn + " " + ln
    do! putStrLn  ("Your full name is: " + fullname)
    return fullname }
// try > runIO action ;;


// Functors

let times2,minus3 = (*) 2, (-)/> 3

let resJust1      = fmap minus3 (Some 4G)
let noValue       = fmap minus3 None
let lstTimes2     = fmap times2 [1;2;3;4]
let fTimes2minus3 = fmap minus3 times2
let res39         = fTimes2minus3 21G
let getChars      = fmap (fun (x:string) -> x.ToCharArray() |> Seq.toList ) action
// try -> runIO getChars ;;


// Define a type Tree

type Tree<'a> =
    | Tree of 'a * Tree<'a> * Tree<'a>
    | Leaf of 'a
    static member map f (t:Tree<'a>  )  =
        match t with
        | Leaf x -> Leaf (f x)
        | Tree(x,t1,t2) -> Tree(f x, Tree.map f t1, Tree.map f t2)

// add ìnstance for Functor class
    static member instance (Functor.Fmap, x:Tree<_>, _) = fun f -> Tree.map f x

let myTree = Tree(6, Tree(2, Leaf(1), Leaf(3)), Leaf(9))
let mappedTree = fmap fTimes2minus3 myTree




// Monoids
#load "Monoid.fs"
open Data.Monoid

let emptyLst:list<int> = mempty()
let zeroInt:Sum<int>   = mempty()
let res10 = mappend (mempty()) (Sum 10)
let res6  = mconcat <| fmap Sum [0.4; 5.6]
let res8:Sum<Integer>  = mconcat [mempty(); Sum 2G; Sum 6G]
let res8n4 = [mempty(); [8;4]]
let res15 = mappend (Product 15) (mempty()) 
let resTrue = mconcat [mempty(); Any true]
let resFalse = mconcat (fmap All [true;false])
let resHi = mappend (mempty()) "Hi"
let res9823 = mconcat (fmap Dual [mempty();"3";"2";"8";"9"])
let resBA = (Dual "A" ) </mappend/> (Dual "B" )
let resEl00:list<int>*Sum<float> = mempty()
let resS3P20     = mappend (Sum 1G,Product 5.0) (Sum 2,Product 4G)
let res230       = mappend (mempty(),mempty()) ([2],[3.0])
let res243       = mappend  ([2;4],[3]) (mempty())
let res23        = mappend (mempty()) ([2],"3")
let res230hiSum2 = mappend (mempty(), mempty(), Sum 2) ([2], ([3.0], "hi"), mempty())
let res230hiS4P3 = mappend (mempty(), mempty()       ) ([2], ([3.0], "hi", Sum 4, Product (6 % 2)))
let tuple5 :string*(Any*string)*(All*All*All)*Sum<int>*string = mempty()

// Control Monad
#load "Monad.fs"
open Control.Monad.Base

let nameAndAddress = mapM (fun x -> putStrLn x >>= fun _ -> getLine) ["name";"address"]

let a:list<int> = mzero()
let res123      = mplus (mempty()) ([1;2;3])

// MonadPlus (sample code from http://en.wikibooks.org/wiki/Haskell/MonadPlus)
let pythags = do'{
  let! z = [1..50]
  let! x = [1..z]
  let! y = [x..z]
  do! guard (x*x + y*y == z*z)
  return (x, y, z)}

let pythags' = doPlus{
  let! z = [1..50]
  let! x = [1..z]
  let! y = [x..z]
  if (x*x + y*y == z*z) then return (x, y, z)}

let allCombinations = sequence [!"abc"; !"12"]


// Arrows
#load "Arrow.fs"
open Control.Arrow

let r5:List<_>  = (runKleisli (id'())) 5
let k = Kleisli (fun y -> [y; y * 2 ; y * 3]) <<< Kleisli (fun x -> [x + 3; x * 2])
let r8n16n24n10n20n30 = runKleisli k <| 5

let res3n6n9 = (arr (fun y -> [y; y * 2 ; y * 3])) 3
let resSome2n4n6:option<_> = runKleisli (arr (fun y -> [y; y * 2 ; y * 3])) 2

let res500n19 = ( (*) 100) *** ((+) 9)  <| (5,10)
let res500n14 = ( (*) 100) &&& ((+) 9)  <| 5
let (res10x13n10x20n15x13n15x20:list<_>) = runKleisli (Kleisli (fun y -> [y * 2; y * 3]) *** Kleisli (fun x -> [x + 3; x *  2] )) (5,10)
let (res10x8n10x10n15x8n15x10  :list<_>) = runKleisli (Kleisli (fun y -> [y * 2; y * 3]) &&& Kleisli (fun x -> [x + 3; x *  2] )) 5

// Arrow choice
let resLeft7       = ( (+) 2) +++ ( (*) 10)   <| Choice2Of2  5
let res7n50        = runKleisli (Kleisli (fun y -> [y; y * 2 ; y * 3]) ||| Kleisli (fun x -> [x + 2; x * 10] )) (Choice1Of2 5)
let resLeft5n10n15 = runKleisli (Kleisli (fun y -> [y; y * 2 ; y * 3]) +++ Kleisli (fun x -> [x + 3; x *  2] )) (Choice2Of2 5)

//Arrow Apply
let res7      = app() ( (+) 3 , 4)
let res4n8n12 = runKleisli (app()) (Kleisli (fun y -> [y; y * 2 ; y * 3]) , 4)


// Applicative functors

#load "Applicative.fs"
open Control.Applicative

// lists
let res3n4   = pure' ((+) 2) <*> [1;2]
let res2n4n8 = pure' ( **^) </ap/> pure' 2. <*> [1;2;3]

// functions
let res3 = pure' 3 "anything"
let res607 = fmap (+) ( (*) 100 ) 6 7
let res606 = ( (+) <*>  (*) 100 ) 6
let res508 = (fmap (+) ((+) 3 ) <*> (*) 100) 5

//ZipList
let res9n5   = fmap ((+) 1) (ZipList(seq [8;4]))
let res18n24 = pure' (+) <*> ZipList(seq [8;4]) <*> ZipList(seq [10;20])
let res6n7n8 = pure' (+) <*> pure' 5G <*> ZipList [1;2;3]
let res18n14 = pure' (+) <*> ZipList(seq [8;4]) <*> pure' 10

// Idiom brackets from http://www.haskell.org/haskellwiki/Idiom_brackets
type Ii = Ii
type Ji = Ji
type J = J
type Idiomatic = Idiomatic with
    static member inline ($) (Idiomatic, si) = fun sfi x -> (Idiomatic $ x) (sfi <*> si)
    static member        ($) (Idiomatic, Ii) = id
let inline idiomatic a b = (Idiomatic $ b) a
let inline iI x = (idiomatic << pure') x

let res3n4''  = iI ((+) 2) [1;2] Ii
let res3n4''' = iI (+) (pure' 2) [1;2] Ii
let res18n24' = iI (+) (ZipList(seq [8;4])) (ZipList(seq [10;20])) Ii
let res6n7n8' = iI (+) (pure' 5G          ) (ZipList [1;2;3]     ) Ii
let res18n14' = iI (+) (ZipList(seq [8;4])) (pure' 10            ) Ii

let inline join x =  x >>= id
type Idiomatic with static member inline ($) (Idiomatic, Ji) = fun xii -> join xii

let safeDiv x y = if y == 0 then None else Some (x </div/> y)
let resJust3    = join (iI safeDiv (Some 6) (Some 2) Ii)
let resJust3'   =       iI safeDiv (Some 6) (Some 2) Ji

let safeDivBy y = if y == 0 then None else Some (fun x -> x </div/> y)
let resJust2  = join (pure' safeDivBy  <*> Some 4G) <*> Some 8G
let resJust2' = join (   iI safeDivBy (Some 4G) Ii) <*> Some 8G

type Idiomatic with static member inline ($) (Idiomatic, J ) = fun fii x -> (Idiomatic $ x) (join fii)

let resJust2'' = iI safeDivBy (Some 4G) J (Some 8G) Ii
let resNothing = iI safeDivBy (Some 0G) J (Some 8G) Ii


// Foldable
#load "Foldable.fs"

open Data.Foldable

let resHW = foldMap (fun x -> Some ("hello " + x)) (Some "world")

module FoldableTree =
    type Tree<'a> =
        | Empty 
        | Leaf of 'a 
        | Node of (Tree<'a>) * 'a * (Tree<'a>)

        // add instance for Foldable class
        static member inline instance (Foldable.FoldMap, t:Tree<_>, _) =
            let rec _foldMap x f =
                match x with
                | Empty        -> mempty()
                | Leaf n       -> f n
                | Node (l,k,r) -> mappend (_foldMap l f) (mappend (f k) (_foldMap r f) )
            _foldMap t
        static member inline instance (_:Foldable.Foldr, x:Tree<_>, _) = fun (f,z) -> Foldable.foldr f z x
    
    let myTree = Node (Node (Leaf(1), 6, Leaf(3)), 2 , Leaf(9))
    let resSum21      = foldMap Sum     myTree
    let resProduct324 = foldMap Product myTree
    let res21         = foldr   (+) 0   myTree




// Traversable
#load "Traversable.fs"

open Data.Traversable

let f x = if x < 200 then [3 - x] else []
let g x = if x < 200 then Some (3 - x) else None

let resSomeminus100 = traverse f (Some 103)
let resLstOfNull    = traverse f None 
let res210          = traverse f [1;2;3]  
let resSome210      = traverse g [1;2;3]  
let resEmptyList    = traverse f [1000;2000;3000] 
let resEListOfElist = traverse f []
let resSome321  = sequenceA [Some 3;Some 2;Some 1]
let resNone     = sequenceA [Some 3;None  ;Some 1]
let res654      = sequenceA [ (+)3 ; (+)2 ; (+) 1] 3
let resCombined = sequenceA [ [1;2;3] ; [4;5;6]  ]
let get3strings = sequenceA [getLine;getLine;getLine]


#load "Cont.fs"
open Control.Monad.Cont

let square_C   x = return' (x * x)
let addThree_C x = return' (x + 3)

let res19 = runCont (square_C 4 >>= addThree_C) id
let res20 = runCont (pure' (+) <*> square_C 2 <*> square_C 4) id

let inline add_cont x y  = return' (x + y)
let inline square_cont x = return' (sqrt x)

let pythagoras_cont x y = do' {
    let! x_squared = square_cont x
    let! y_squared = square_cont y
    let! sum_of_squares = add_cont x_squared y_squared
    return sum_of_squares}

let resPyth373205 = runCont (pythagoras_cont 3. 4.) string

let foo n =
  callCC <| fun k -> do' {
    let n' = (n * n) + 3
    do! when' (n' > 20) <| k "over twenty"
    return (string <| n' - 4) }

let res''3''  = runCont (foo  2) id
let resOver20 = runCont (foo 16) id


#load "Reader.fs"
open Control.Monad.Reader

let calculateContentLen = do' {
    let! content = ask
    return (String.length content)}

let calculateModifiedContentLen = local ( (+) "Prefix ") calculateContentLen

let readerMain = do' {
    let s = "12345"
    let modifiedLen = runReader calculateModifiedContentLen s
    let len = runReader calculateContentLen s
    do!     putStrLn <| "Modified 's' length: " + (string modifiedLen)
    return! putStrLn <| "Original 's' length: " + (string len)
    }

// try -> runIO readerMain ;;


#load "State.fs"
open Control.Monad.State

// from http://www.haskell.org/haskellwiki/State_Monad
let x1 = runState (return' 'X') 1
let xf:State<int,_> = return' 'X'
let r11    = runState get 1
let rUnit5 = runState (put 5) 1

let rX5    = runState (do' { 
    do! put 5
    return 'X' }) 1

let postincrement = do' {
    let! x = get
    do! put (x+1)
    return x }

let r12 = runState postincrement 1

let tick :State<_,_> = do'{
    let! n = get
    do! put (n+1)
    return n}

let plusOne n = execState tick n
let plus  n x = execState (sequence <| replicate n tick) x


#load "Writer.fs"
open Control.Monad.Writer

let res12n44x55x1x2 = (+) <<|> Writer (3,[44;55]) </ap/> Writer (9,[1;2])


#load "MonadTrans.fs"
open Control.Monad.Trans
open Control.Monad.Trans.OptionT
open Control.Monad.Trans.ListT

let optionT4x6xN = fmap ((+) 2) (OptionT [Some 2; Some 4; None])
let optionT = OptionT [Some 2; Some 4] >>= fun x -> OptionT [Some x; Some (x+10)]

let listT2x4x6  = fmap ((+) 2) (ListT (Some [2; 4; 6]))
let listT  = ListT  (Some [2;4]    ) >>= fun x -> ListT  (Some [x; x+10]     )

let apOptionT = ap (OptionT [Some ((+) 3)] ) ( OptionT [Some  3 ] )
let apListT   = ap (ListT   (Some [(+) 3]) ) ( ListT   (Some [3]) )

let resListTSome2547 = (ListT (Some [2;4] )) >>=  (fun x -> ListT ( Some [x;x+3G]) )

let getAtLeast8Chars:OptionT<_> =  lift getLine >>= fun s -> (guard (String.length s >= 8) ) >>= fun _ -> return' s
//try -> runIO <| runOptionT getAtLeast8Chars


let isValid s = String.length s >= 8 && String.exists System.Char.IsLetter s && String.exists System.Char.IsNumber s && String.exists System.Char.IsPunctuation s

let getValidPassword:OptionT<_> =
    doPlus {
        let! s = (lift getLine)        
        do! guard (isValid s)  // if isValid s then return s
        return s
        }
    
let askPassword = do' {
    do! lift <| putStrLn "Insert your new password:"
    let! value = getValidPassword
    do! lift <| putStrLn "Storing in database..."
    return value
    }

let askPass = runOptionT askPassword

//try -> runIO askPass

let resLiftAsyncOptionT = liftAsync getLine : OptionT<Async<_>>


#load "ContT.fs"
open Control.Monad.ContT

// from http://en.wikibooks.org/wiki/Haskell/Continuation_passing_style

//askString :: (String -> ContT () IO String) -> ContT () IO String
let askString next = do' {
  do! (liftAsync <| putStrLn "Please enter a string") 
  let! s = liftAsync <| getLine
  return! next s}

//reportResult :: String -> IO ()
let reportResult s = do' {
  return! putStrLn ("You entered: " + s) }
  
let mainaction = runContT (callCC askString) reportResult
//try -> runIO mainaction


let show x = '\"' :: x ++ !"\""

let inline bar c s = do' {
  let! msg = callCC <| fun k -> do' {
    let s' = c :: s
    do! when' (s' == !"hello") <| k !"They say hello."
    let s'' = show s'
    return (!"They appear to be saying " ++ s'') }
  return (length msg) }

let res15'    = runCont             (bar 'h' !"ello")  id
let resSome15 = runCont (runOptionT (bar 'h' !"ello")) id
let resList29 = runCont (runListT   (bar 'h' !"i"   )) id


let resLiftAsyncContT = liftAsync getLine : ContT<Async<string>,_>

#load "ReaderT.fs"
open Control.Monad.ReaderT

let res15'' = runCont (runReaderT (bar 'h' !"ello") "anything") id


// from http://www.haskell.org/ghc/docs/6.10.4/html/libraries/mtl/Control-Monad-Reader.html
let printReaderContent = do' {
    let! content = ask()
    return! (liftAsync <| putStrLn ("The Reader Content: " + content)) }

let readerTMain = do'{
    return! (runReaderT printReaderContent "Some Content") }

let _ = runIO readerTMain
// try -> runIO readerTMain ;;


#load "StateT.fs"
open Control.Monad.StateT

// from http://www.haskell.org/haskellwiki/Simple_StateT_use
#nowarn "0025"  // Incomplete pattern match, list cannot be infinite if F#
let code  =
    let inline io (x: Async<_>)  : StateT<_,Async<_>> = liftAsync x
    let pop  = do' {
        let! (x::xs) = get()
        do! put xs
        return x}
    do' {
        let! x = pop
        do! io <| print x
        let! y = pop
        do! io <| print y
        return () }

let main = runStateT code [1..10] >>= fun _ -> return' ()

let resLiftAsyncStateT = liftAsync getLine : StateT<string,Async<_>>


#load "WriterT.fs"
open Control.Monad.WriterT

let toLower (s:char) = s.ToString().ToLower().Chars(0)
let toUpper (s:char) = s.ToString().ToUpper().Chars(0)

let chncase x = function
    | true -> ((toLower x), false) 
    | _    -> ((toUpper x), true)

let logchncase x = function
    | true -> (((toLower x), "Low "), false)
    | _    -> (((toUpper x), "Up " ), true)
                      
let statecase x = State (logchncase x)

let logstatecase x = WriterT (statecase x)

// runState (runWriterT (logstatecase 'a')) true  -> (char * string) * bool = (('a', "Low "), false)
// runState (runWriterT (logstatecase 'a')) false -> (char * string) * bool = (('A', "Up "), true)

let logstatecase3 x y z : WriterT<_> =  do' {
    let! u = logstatecase x
    let! v = logstatecase y
    let! w = logstatecase z
    do! tell "thats all"
    return [u,v,w]}

//runState (runWriterT (logstatecase3 'a' 'b' 'c')) true  -> ((char * char * char) list * string) * bool = (([('a', 'B', 'c')], "Low Up Low "), false)
//runState (runWriterT (logstatecase3 'a' 'b' 'c')) false -> ((char * char * char) list * string) * bool = (([('A', 'b', 'C')], "Up Low Up "), true)

let resLiftAsyncWriterT = liftAsync getLine : WriterT<Async<_ * string>>


// N-layers Monad Transformer

let res3Layers   = (lift << lift)         getLine : OptionT<ReaderT<string,_>>
let res3Layers'  = (lift << lift)         getLine : OptionT<WriterT<Async<_ * string>>>
let res3Layers'' = liftAsync              getLine : OptionT<WriterT<Async<_ * string>>>
let res4Layers   = (lift << lift << lift) getLine : ListT<OptionT<WriterT<Async<_ * string>>>>
let res4Layers'  = liftAsync              getLine : ListT<OptionT<WriterT<Async<_ * string>>>>