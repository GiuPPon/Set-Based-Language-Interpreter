module Semantics

(* polymorphic environments, the same definitions work both for static and dynamic environments *)

type variable = Name of string

type 'a singleLevelDecs = Map<variable, 'a>

type 'a environment = 'a singleLevelDecs list

exception UndeclaredVariable of variable

exception AlreadyDeclaredVariable of variable
let emptyLevel: 'a singleLevelDecs = Map.empty
let initialEnv: 'a environment = [ emptyLevel ] (* the empty top-level *)

(* enterLevel: 'a environment -> 'a environment *)

let enterLevel (env: 'a environment) : 'a environment =
    emptyLevel :: env (* enters a new nested level *)

(* exitLevel: 'a environment -> 'a environment *)
(* only used in the dynamic semantics *)

let exitLevel: 'a environment -> 'a environment =
    function (* removes the innermost level, only needed for the dynamic semantics *)
    | _ :: env -> env
    | [] -> failwith "unexpected error" (* should never happen *)

(* variable lookup *)
(* lookup: variable -> environment<'a> -> 'a *)

(* lookup uses Map.tryFind: ('a -> Map<'a,'b> -> 'b option) when 'a: comparison *)

let rec lookup var : 'a environment -> 'a =
    function
    | level :: env ->
        match Map.tryFind var level with
        | Some res -> res
        | None -> lookup var env
    | [] -> raise (UndeclaredVariable var)

(* variable declaration *)
(* dec: variable -> 'a -> environment<'a> -> environment<'a> *)

(* example:
   dec x ty env1 = env2 means that 'env2' is the new environment after declaring variable 'x' of type 'ty' in the environment 'env1'
   dec x value env1 = env2 means that 'env2' is the new environment after declaring variable 'x' initialized with value 'value' in the environment 'env1'
*)

(* dec uses Map.containsKey: ('a -> Map<'a,'b> -> bool) when 'a: comparison *)

let dec var info : 'a environment -> 'a environment =
    function
    | level :: env ->
        if Map.containsKey var level then
            raise (AlreadyDeclaredVariable var)
        else
            Map.add var info level :: env
    | [] -> failwith "unexpected error" (* should never happen *)

(* variable update *)
(* update: variable -> 'a -> environment<'a> -> environment<'a> *)

(* only used in the dynamic semantics *)

(* update uses Map.containsKey *)

let rec update var info : 'a environment -> 'a environment =
    function
    | level :: env ->
        if Map.containsKey var level then
            Map.add var info level :: env
        else
            level :: update var info env
    | [] -> raise (UndeclaredVariable var)

(* abstract syntax of the language *)

(* AST of expressions *)
type exp =
    | Add of exp * exp // addition
    | And of exp * exp // logical AND
    | BoolLiteral of bool // boolean literal
    | Diff of exp * exp // *** new: set difference
    | Eq of exp * exp // equality
    | Fst of exp // first element of a pair
    | IntLiteral of int // integer literal
    | IsIn of exp * exp // *** new: set membership
    | Minus of exp // unary subtraction
    | Mul of exp * exp // multiplication
    | Not of exp // logical NOT
    | PairLit of exp * exp // pair constructor
    | SetEnum of variable * exp * exp // *** new: set enumeration
    | SetLit of exp // *** new: set constructor
    | Size of exp // *** new: set size
    | Snd of exp // second element of a pair
    | Union of exp * exp // *** new: set union
    | Variable of variable // variable

(* AST of statements and sequence of statements, mutually recursive *)
type stmt =
    | AssertStmt of exp // assert statement
    | AssignStmt of variable * exp // assignment
    | IfStmt of exp * block * block // if-then-else statement
    | PrintStmt of exp // print statement
    | VarStmt of variable * exp // declaration statement
    | WhileStmt of exp * block // *** new: while statement

and block = Block of stmtSeq // non-empty block

and stmtSeq =
    | EmptyStmtSeq // empty sequence of statements
    | NonEmptyStmtSeq of stmt * stmtSeq // non-empty sequence of statements

(* AST of programs *)
type prog = LangProg of stmtSeq // program

(* static semantics of the language *)

(* static types *)

type staticType =
    | Bool
    | Int
    | PairType of staticType * staticType
    | SetType of staticType

(* examples
    PairType(IntType,BoolType) corresponds to int * bool
    PairType(IntType,PairType(IntType,BoolType)) corresponds to int * (int * bool)
*)

type staticEnv = staticType environment

(* static errors *)

exception ExpectingTypeError of staticType

exception ExpectingPairError

exception ExpectingSetError

(* static semantic functions *)


(*
    typecheckExp: staticEnv -> exp -> staticType
    typecheckExpHasType: staticType -> staticEnv -> exp -> staticTyp
    mutually recursive functions, typecheckExpHasType auxiliary
*)

(* typecheckExp env exp = ty means that expressions 'exp' is type correct in the environment 'env' and has static type 'ty' *)
(* typecheckExpHasType expectedTy env exp = ty means that 'exp' has type 'ty' in 'env' and 'ty'='expectedTy' *)

let rec typecheckExp (env: staticEnv) =
    function
    | Add (exp1, exp2)
    | Mul (exp1, exp2) ->
        typecheckExpHasType Int env exp1 |> ignore // returned value ignored
        typecheckExpHasType Int env exp2
    | And (exp1, exp2) ->
        typecheckExpHasType Bool env exp1 |> ignore // returned value ignored
        typecheckExpHasType Bool env exp2
    | BoolLiteral _ -> Bool
    | Diff (exp1, exp2) // *** new
    | Union (exp1, exp2) -> // *** new
        let type1 = typecheckExp env exp1

        match type1 with
        | SetType _ -> typecheckExpHasType type1 env exp2
        | _ -> raise ExpectingSetError
    | Eq (exp1, exp2) ->
        let type1 = typecheckExp env exp1
        typecheckExpHasType type1 env exp2 |> ignore // returned value ignored
        Bool
    | Fst exp ->
        match typecheckExp env exp with
        | PairType (type1, _) -> type1
        | _ -> raise ExpectingPairError
    | IntLiteral _ -> Int
    | IsIn (exp1, exp2) -> // *** new
        let elemType = typecheckExp env exp1

        typecheckExpHasType (SetType elemType) env exp2
        |> ignore

        Bool
    | Minus exp -> typecheckExpHasType Int env exp
    | Not exp -> typecheckExpHasType Bool env exp
    | PairLit (exp1, exp2) ->
        let type1 = typecheckExp env exp1
        let type2 = typecheckExp env exp2
        PairType(type1, type2)
    | SetEnum (variable, setExp, elemExp) -> // *** new
        let setType = typecheckExp env setExp

        match setType with
        | SetType elType -> SetType(typecheckExp (env |> enterLevel |> dec variable elType) elemExp)
        | _ -> raise ExpectingSetError
    | SetLit exp -> SetType(typecheckExp env exp) // *** new
    | Size exp -> // *** new
        match typecheckExp env exp with
        | SetType _ -> Int
        | _ -> raise ExpectingSetError
    | Snd exp ->
        match typecheckExp env exp with
        | PairType (_, type2) -> type2
        | _ -> raise ExpectingPairError
    | Variable var -> lookup var env

and typecheckExpHasType expectedTy env exp =
    let foundType = typecheckExp env exp

    if foundType = expectedTy then
        foundType
    else
        raise (ExpectingTypeError expectedTy)

(* mutually recursive functions

 typecheckStmt : staticEnv -> stmt -> staticEnv
 typecheckBlock : staticEnv -> block -> unit
 typecheckStmtSeq : staticEnv -> stmtSeq -> unit

*)

(* typecheckStmt env1 st = env2 means that statement 'st' is type correct in the environment 'env1' and defines the new environment 'env2' *)
(* typecheckBlock env block = () means that the block 'block' is type correct in the environment 'env' *)
(* typecheckStmtSeq env1 stSeq = env2 means that statement sequence 'stSeq' is type correct in the environment 'env1' and defines the new environment 'env2' *)

let rec typecheckStmt (env: staticEnv) =
    function
    | AssertStmt exp ->
        typecheckExpHasType Bool env exp |> ignore // returned value ignored
        env
    | AssignStmt (var, exp) ->
        let type1 = lookup var env
        typecheckExpHasType type1 env exp |> ignore // returned value ignored
        env
    | IfStmt (exp, thenBlock, elseBlock) ->
        typecheckExpHasType Bool env exp |> ignore // returned value ignored
        typecheckBlock env thenBlock
        typecheckBlock env elseBlock
        env
    | PrintStmt exp ->
        typecheckExp env exp |> ignore // returned value ignored
        env
    | VarStmt (var, exp) -> dec var (typecheckExp env exp) env
    | WhileStmt (exp, block) -> // *** new
        typecheckExpHasType Bool env exp |> ignore // returned value ignored
        typecheckBlock env block
        env

and typecheckBlock env (Block stmtSeq) =
    typecheckStmtSeq (enterLevel env) stmtSeq

and typecheckStmtSeq (env: staticEnv) =
    function
    | EmptyStmtSeq -> ()
    | NonEmptyStmtSeq (stmt, stmtSeq) -> typecheckStmtSeq (typecheckStmt env stmt) stmtSeq

(*
  typecheckProg : prog -> unit
*)

(* typecheckProg p = () means that program 'p' is well defined with respect to the static semantics *)

let typecheckProg (LangProg stmtSeq) = typecheckStmtSeq initialEnv stmtSeq

(* dynamic semantics of the language *)

(* values *)

type value =
    | IntValue of int
    | BoolValue of bool
    | PairValue of value * value
    | SetValue of value Set

(* examples
    PairLit(IntLiteral 2,BoolLiteral false) corresponds to  2,false
    PairLit(IntLiteral 2,PairLit(IntLiteral 3,BoolLiteral true)) corresponds to 2,(3,true)
*)

type dynamicEnv = value environment

(* dynamic errors *)

exception ExpectingDynTypeError of string (* dynamic conversion error *)

(* auxiliary functions *)

(* dynamic conversion to int type *)
(* toInt : value -> int *)

let toInt =
    function
    | IntValue i -> i
    | _ -> raise (ExpectingDynTypeError "int")

(* dynamic conversion to bool type *)
(* toBool : value -> bool *)

let toBool =
    function
    | BoolValue b -> b
    | _ -> raise (ExpectingDynTypeError "bool")

(* toPair : value -> value * value *)
(* dynamic conversion to product  type *)

let toPair =
    function
    | PairValue (e1, e2) -> e1, e2
    | _ -> raise (ExpectingDynTypeError "pair")

let toSet =
    function
    | SetValue s -> s
    | _ -> raise (ExpectingDynTypeError "set")

(* fst and snd operators *)
(* fst: 'a * 'b -> 'a  and snd: 'a * 'b -> 'b predefined in F# *)

(* conversion to string *)

(* toString : value -> string *)

let rec toString =
    function (* uses interpolated strings *)
    | IntValue i -> $"{i}"
    | BoolValue b -> $"{b}"
    | PairValue (v1, v2) -> $"({toString v1},{toString v2})"
    | SetValue s -> $"setOfSize({Set.count s})"

(* printing function *)

(* printf "%s": string -> unit predefined in F# *)

(* evalExp : dynamicEnv -> exp -> value *)
(* evalExp env exp = val means that expressions 'exp' successfully evaluates to 'val' in the environment 'env' *)

let rec evalExp (env: dynamicEnv) =
    function
    | Add (exp1, exp2) ->
        (evalExp env exp1 |> toInt)
        + (evalExp env exp2 |> toInt)
        |> IntValue
    | And (exp1, exp2) ->
        (evalExp env exp1 |> toBool
         && evalExp env exp2 |> toBool)
        |> BoolValue
    | BoolLiteral b -> BoolValue b
    | Diff (exp1, exp2) -> // *** new
        Set.difference (evalExp env exp1 |> toSet) (evalExp env exp2 |> toSet)
        |> SetValue
    | Eq (exp1, exp2) -> evalExp env exp1 = evalExp env exp2 |> BoolValue
    | Fst exp -> evalExp env exp |> toPair |> fst
    | IntLiteral i -> IntValue i
    | IsIn (exp1, exp2) -> // *** new
        Set.contains (evalExp env exp1) (evalExp env exp2 |> toSet)
        |> BoolValue
    | Minus exp -> evalExp env exp |> toInt |> (~-) |> IntValue // (~-) is the unary minus
    | Mul (exp1, exp2) ->
        (evalExp env exp1 |> toInt)
        * (evalExp env exp2 |> toInt)
        |> IntValue
    | Not exp -> evalExp env exp |> toBool |> not |> BoolValue
    | PairLit (exp1, exp2) -> (evalExp env exp1, evalExp env exp2) |> PairValue
    | SetEnum (variable, setExp, elemExp) -> // *** new
        let set = evalExp env setExp |> toSet
        let newEnv = enterLevel env |> dec variable (IntValue 0) // nuovo ambiente dove 'variable' viene dichiarato a livello annidato, un qualsiasi valore di inizializzazione va bene (per esempio 'IntValue 0')

        Set.fold // itera sugli elementi 'setElem' di 'set', accumula il risultato in un insieme inizialmente vuoto ('Set.empty') aggiungendo i valori ottenuti valutando 'elemExp' nel nuovo ambiente 'newEnv' dove il valore di 'variable' è aggiornato con 'setElem' (ossia, i valori accumulati corrispondono a 'evalExp (newEnv |> update variable setElem) elemExp' per ogni 'setElem' in 'set')
            (fun accSet setElem -> Set.add (evalExp (newEnv |> update variable setElem) elemExp) accSet)
            Set.empty
            set
        |> SetValue // *** new
    | SetLit exp -> Set.add (evalExp env exp) Set.empty |> SetValue // *** new
    | Size exp -> evalExp env exp |> toSet |> Set.count |> IntValue // *** new
    | Snd exp -> evalExp env exp |> toPair |> snd
    | Union (exp1, exp2) -> // *** new
        Set.union (evalExp env exp1 |> toSet) (evalExp env exp2 |> toSet)
        |> SetValue
    | Variable var -> lookup var env

(* mutually recursive
   executeStmt : dynamicEnv -> stmt -> dynamicEnv
   executeBlock : dynamicEnv -> block -> dynamicEnv
   executeStmtSeq : dynamicEnv -> stmtSeq -> dynamicEnv
*)

(* executeStmt env1 'stmt' = env2 means that the execution of statement 'stmt' in environment 'env1' successfully returns the new environment 'env2' *)
(* executeBlock env1 block = env2 means that the execution of block 'block' in environment 'env1' successfully returns the new environment 'env2' *)
(* executeStmtSeq env1 stmtSeq = env2 means that the execution of sequence 'stmtSeq' in environment 'env1' successfully returns the new environment 'env2' *)
(* executeStmt, executeBlock and executeStmtSeq write on the standard output if some 'print' statement is executed *)

let rec executeStmt env : stmt -> dynamicEnv =
    function
    | AssignStmt (var, exp) -> update var (evalExp env exp) env
    | AssertStmt exp ->
        if evalExp env exp |> toBool |> not then
            failwith "Assertion error"
        else
            env
    | IfStmt (exp, thenBlock, elseBlock) ->
        if toBool (evalExp env exp) then
            executeBlock env thenBlock
        else
            executeBlock env elseBlock
    | PrintStmt exp ->
        evalExp env exp |> toString |> printfn "%s"
        env
    | VarStmt (var, exp) -> dec var (evalExp env exp) env
    | WhileStmt (exp, block) as whileStmt ->
        if evalExp env exp |> toBool then
            executeStmt (executeBlock env block) whileStmt
        else
            env

and executeBlock env =
    function (* note the differences with the static semantics *)
    | Block stmtSeq ->
        executeStmtSeq (enterLevel env) stmtSeq
        |> exitLevel

and executeStmtSeq (env: dynamicEnv) : stmtSeq -> dynamicEnv =
    function
    | EmptyStmtSeq -> env
    | NonEmptyStmtSeq (stmt, stmtSeq) -> executeStmtSeq (executeStmt env stmt) stmtSeq

(* executeProg : prog -> unit *)
(* executeProg prog = () means that program 'prog' has been executed successfully, by possibly writing on the standard output *)

let executeProg =
    function
    | LangProg stmtSeq ->
        executeStmtSeq initialEnv stmtSeq |> ignore // returned value ignored
        ()
