open System
open System.Numerics

type terminal = 
    Add | Sub | Mul | Div | Rem | Lpar | Rpar | Pow | Dot | Int of int | Float of float
    | Sin | Cos | Log | Exp | Tan | Sqrt | Pi

let str2lst s = [for c in s -> c]
let isblank c = System.Char.IsWhiteSpace c
let isdigit c = System.Char.IsDigit c
let lexError = System.Exception("Lexer error")
let intVal (c:char) = (int)((int)c - (int)'0')
let parseError = System.Exception("Parser error")
let divideByZero = System.Exception("Attempted to divide by zero")

let rec scFloat(iStr, iVal) = 
    match iStr with
    | '.' :: tail -> 
        let rec parseDecimal tail multiplier value =
            match tail with
            | c :: t when isdigit c -> parseDecimal t (multiplier / 10.0) (value + (float (intVal c)) * multiplier)
            | _ -> (tail, value)
        let (restStr, decimalVal) = parseDecimal tail 0.1 (float iVal)
        (restStr, Float(decimalVal))
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
        | '.'::tail -> Dot :: scan tail
        | 'p'::'i'::tail -> Pi :: scan tail
        | 's'::'i'::'n'::tail -> Sin :: scan tail
        | 'c'::'o'::'s'::tail -> Cos :: scan tail 
        | 'l'::'o'::'g'::tail -> Log :: scan tail 
        | 'e'::'x'::'p'::tail -> Exp :: scan tail 
        | 't'::'a'::'n'::tail -> Tan :: scan tail 
        | 's'::'q'::'r'::'t'::tail -> Sqrt :: scan tail 
        | c :: tail when isblank c -> scan tail
        | c :: tail when isdigit c -> 
            let (iStr, iVal) = scFloat(tail, intVal c)
            match iVal with
            | Float v when v % 1.0 = 0.0 -> Int (int v) :: scan iStr
            | _ -> iVal :: scan iStr                    
        | _ -> raise lexError

    scan (str2lst input)

let getInputString() : string = 
    Console.Write("Enter an expression: ")
    Console.ReadLine()

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
    let rec E tList = (T >> Eopt) tList
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
                         //match tval with
                         //| 0.0 -> raise divideByZero 
                         //| _ ->
                         //       let result =
                         //           match (value, tval) with
                         //           | (v1, v2) when v1 % 1.0 = 0.0 && v2 % 1.0 = 0.0 -> int v1 / int v2 |> float
                         //           | _ -> value / tval
                         //       Topt (tLst, result)
                         match tval with
                         | 0.0 -> raise divideByZero
                         | _ -> Topt (tLst, value / tval)
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
        | Int value :: tail -> (tail, value)
        | Float value :: tail -> (tail, float value)
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
    E tList

let rec printTList (lst:list<terminal>) : list<string> = 
    match lst with
    | head :: tail -> Console.Write("{0} ",head.ToString())
                      printTList tail
                  
    | [] -> Console.Write("EOL\n")
            []

[<EntryPoint>]
let main argv  =
    Console.WriteLine("Advanced Interpreter with Optional Features: ")
    let input = getInputString()
    let oList = lexer input
    let sList = printTList oList;
    let Out = parseNeval oList
    Console.WriteLine("Result = {0}", snd Out)
    0
