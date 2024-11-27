open System
open System.Numerics
open System.Collections.Generic

type VariableType =
    | IntType
    | FloatType

type terminal = 
    Add | Sub | Mul | Div | Rem | Lpar | Rpar | Pow | Dot | End | Int of int | Float of float
    | Sin | Cos | Log | Exp | Tan | Sqrt | Pi | Assign | Variable of string | TypeDecl of VariableType

let str2lst s = [for c in s -> c]
let isblank c = System.Char.IsWhiteSpace c
let isdigit c = System.Char.IsDigit c
let isalpha c = System.Char.IsLetter c
let lexError = System.Exception("Lexer error")
let intVal (c:char) = (int)((int)c - (int)'0')
let parseError = System.Exception("Parser error")
let divideByZero = System.Exception("Attempted to divide by zero")
let undeclaredVariable = System.Exception("Variable used before assignment")
let typeMismatch = System.Exception("Type mismatch during assignment or usage")

let symbolTable = Dictionary<string, float>()
let variableTypes = Dictionary<string, VariableType>()

let rec scFloat(iStr, iVal) = 
    match iStr with
    | '.' :: tail -> 
        let rec parseDecimal tail multiplier value =
            match tail with
            | c :: t when isdigit c -> parseDecimal t (multiplier / 10.0) (value + (float (intVal c)) * multiplier)
            | _ -> (tail, Float value)
        let (restStr, decimalVal) = parseDecimal tail 0.1 (float iVal)
        (restStr, decimalVal)
    | c :: tail when isdigit c -> scFloat(tail, 10 * iVal + intVal c)
    | _ -> (iStr, Int iVal)

let lexer input = 
    let rec scan input =
        match input with
        | [] -> []
        | '+'::tail -> Add :: scan tail
        | '-'::tail -> Sub :: scan tail
        | '*'::tail -> Mul :: scan tail
        | '/'::tail -> Div :: scan tail
        | '%'::tail -> Rem :: scan tail
        | '('::tail -> Lpar:: scan tail
        | ')'::tail -> Rpar:: scan tail
        | '^'::tail -> Pow :: scan tail
        | '.'::tail -> 
            symbolTable.["lastInputType"] <- 1.0
            Dot :: scan tail
        | ';'::tail -> End :: scan tail
        | '='::tail -> Assign :: scan tail
        | 'p'::'i'::tail -> Pi :: scan tail
        | 's'::'i'::'n'::tail -> Sin :: scan tail
        | 'c'::'o'::'s'::tail -> Cos :: scan tail 
        | 'l'::'o'::'g'::tail -> Log :: scan tail 
        | 'e'::'x'::'p'::tail -> Exp :: scan tail 
        | 't'::'a'::'n'::tail -> Tan :: scan tail 
        | 's'::'q'::'r'::'t'::tail -> Sqrt :: scan tail 
        | 'i'::'n'::'t'::tail -> TypeDecl IntType :: scan tail
        | 'f'::'l'::'o'::'a'::'t'::tail -> TypeDecl FloatType :: scan tail
        | c :: tail when isblank c -> scan tail
        | c :: tail when isdigit c -> 
            let (iStr, iVal) = scFloat(tail, intVal c)
            match iVal with
            | Float v -> 
                symbolTable.["lastInputType"] <- 1.0
                Float v :: scan iStr
            | Int v -> 
                symbolTable.["lastInputType"] <- 0.0
                Int v :: scan iStr    
        | c :: tail when isalpha c -> 
            let rec parseVar tail value =
                match tail with
                | c :: t when isalpha c -> parseVar t (value + string c)
                | _ -> (tail, Variable value)
            let (tail, var) = parseVar tail (string c)
            var :: scan tail
        | _ -> raise lexError

    scan (str2lst input)

    // Grammar in BNF:
// <E>        ::= <T> <Eopt>
// <Eopt>     ::= "+" <T> <Eopt> | "-" <T> <Eopt> | <empty>
// <T>        ::= <P> <Topt>
// <Topt>     ::= "*" <P> <Topt> | "/" <P> <Topt> | "%" <P> <Topt> | <empty>
// <P>        ::= <Number> <Popt>
// <Popt>     ::= "^" <Number> <Popt> | <empty>
// <Number>   ::= <Numb> | <Float>
// <Numb>     ::= "Num" <value> | "(" <E> ")" | "-" <Numb> 
// <Float>    ::= "Num" <value> "." "Num" <value> | "-" <Float> | "Pi" | "Sin" <Numb> | "Tan" <Numb> | "Cos" <Numb> | "Exp" <Numb> | "Sqrt" <Numb> | "log" <Numb>

let rec parseNeval tList =
    let rec S tList =
        match tList with 
        | TypeDecl typ :: Variable varName :: Assign :: tail ->
            let (tLst, value) = E tail
            match typ with
            | IntType when value % 1.0 <> 0.0 -> raise typeMismatch
            | IntType ->
                symbolTable.[varName] <- value
                variableTypes.[varName] <- IntType
            | FloatType ->
                symbolTable.[varName] <- value
                variableTypes.[varName] <- FloatType
            if tLst <> [] && tLst.Head = End then
                (tLst.Tail, value)
            else raise parseError
        | TypeDecl typ :: Variable varName :: tail ->
            variableTypes.[varName] <- typ
            symbolTable.[varName] <- 0.0
            (tail, 0.0)
        | Variable varName :: Assign :: tail -> 
            let (tLst, value) = E tail
            if variableTypes.ContainsKey(varName) then 
                match variableTypes.[varName] with
                | IntType when value % 1.0 <> 0.0 -> raise typeMismatch
                | _ -> symbolTable.[varName] <- value
            else  
                let inferredType = if value % 1.0 = 0.0 then IntType else FloatType
                variableTypes.[varName] <- inferredType
                symbolTable.[varName] <- value
            if tLst <> [] && tLst.Head = End then
                (tLst.Tail, value)
            else raise parseError
        | _ -> E tList
    and E tList = (T >> Eopt) tList
    and Eopt (tList, value) = 
        match tList with
        | Add :: tail -> let (tLst, tval) = T tail
                         Eopt (tLst, value + tval)
        | Sub :: tail -> let (tLst, tval) = T tail 
                         Eopt (tLst, value - tval)                            
        | _ -> (tList, value)
    and T tList = (P >> Topt) tList
    and Topt (tList, value) =
        match tList with
        | Mul :: tail -> let (tLst, tval) = P tail
                         Topt (tLst, value * tval)
        | Div :: tail -> let (tLst, tval) = P tail
                         match tval with
                         | 0.0 -> raise divideByZero 
                         | _ ->
                                let result =
                                    match (value % 1.0, tval % 1.0) with
                                    | (0.0, 0.0) -> 
                                        let intValue = int value
                                        let intTval = int tval
                                        if symbolTable.ContainsKey "lastInputType" && symbolTable.["lastInputType"] = 1.0 then
                                            value / tval
                                        else
                                            float (intValue / intTval)
                                    | _ -> value / tval
                                Topt (tLst, result)

        | Rem :: tail -> let (tLst, tval) = P tail
                         match tval with
                         | 0.0 -> raise divideByZero
                         | _ -> Topt (tLst, value % tval)
        | _ -> (tList, value)
    and P tList = (Numb >> Popt) tList
    and Popt (tList, value) =
        match tList with
        | Pow :: tail -> let (tLst, tval) = Numb tail
                         Popt (tLst, Math.Pow(value, tval))
        | _ -> (tList, value)
    and Numb tList =
        match tList with 
        | Int value :: tail -> (tail, float value)
        | Float value :: tail -> (tail, value)
        | Variable varName :: tail -> 
            if symbolTable.ContainsKey(varName) then
                (tail, symbolTable.[varName])
            else 
                raise undeclaredVariable
        | Pi :: tail -> (tail, Math.PI)
        | Lpar :: tail -> let (tLst, tval) = E tail
                          match tLst with 
                          | Rpar :: tail -> (tail, tval)
                          | _ -> raise parseError
        | Sub :: tail -> let (tLst, tval) = Numb tail
                         (tLst, -tval)
        | Sin :: tail -> let (tLst, tval) = Numb tail
                         (tLst, Math.Sin(tval))
        | Tan :: tail -> let (tLst, tval) = Numb tail
                         (tLst, Math.Tan(tval))
        | Cos :: tail -> let (tLst, tval) = Numb tail
                         (tLst, Math.Cos(tval))
        | Log :: tail -> let (tLst, tval) = Numb tail
                         (tLst, Math.Log(tval))
        | Exp :: tail -> let (tLst, tval) = Numb tail
                         (tLst, Math.Exp(tval))
        | Sqrt :: tail -> let (tLst, tval) = Numb tail
                          (tLst, Math.Sqrt(tval))
        | _ -> raise parseError
    S tList

let rec processInput(input: string) =

    try
        let tokens = lexer input
        let (_, result) = parseNeval tokens
        Console.WriteLine("Result: {0}", result)
    with
    | :? System.Exception as ex ->
        Console.WriteLine("Error: {0}", ex.Message)

[<EntryPoint>]
let main argv  =
    Console.WriteLine("Advanced Interpreter with Optional Features: ")
    let rec loop() =
        Console.Write(">> ")
        let input = Console.ReadLine()
        processInput input
        loop()
    loop()
    0
