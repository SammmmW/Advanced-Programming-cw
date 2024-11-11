open System
open System.Numerics // For complex numbers

type terminal = 
    Add | Sub | Mul | Div | Rem | Lpar | Rpar | Pow | Dot | Num of float | ComplexNum of Complex | RationalNum of (int * int) 
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
        (restStr, decimalVal)
    | c :: tail when isdigit c -> scFloat(tail, 10.0 * iVal + float (intVal c))
    | _ -> (iStr, iVal)

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
            let (iStr, iVal) = scFloat(tail, float (intVal c))
            if iStr <> [] && iStr.Head = '/' then 
            //These tasks are not essential and don't work currently, do not prioritise this!
                scRational(iStr, iVal)
            elif iStr <> [] && iStr.Head = '+' then
                scComplex(iStr, iVal)
            else
                Num iVal :: scan iStr                         
        | _ -> raise lexError

    and scRational(input, num) = 
        match input with
        | '/' :: tail -> 
            match tail with
            | c :: t when isdigit c ->
                let (rest, den) = scFloat(t, float(intVal c))
                RationalNum(int num, int den) :: scan rest
            | _ -> raise lexError
        | _ -> scan input

    and scComplex(input, realPart) =
        match input with
        | '+' :: tail ->
            match tail with
            | c :: t when isdigit c ->
                let (iStr, imagPart) = scFloat(t, float (intVal c))
                match iStr with
                | 'i' :: rest  -> ComplexNum(Complex(realPart, imagPart)) :: scan rest
                | _ -> raise lexError
            | _ -> raise lexError
        | _ -> scan input

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
        | Num value :: tail -> (tail, value)
        | ComplexNum cval :: tail -> (tail, Complex.Abs(cval))
        | RationalNum (n, d) :: tail -> (tail, float n / float d)
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
    //head::tail -> Console.Write("{0} ",head.ToString())
    //              printTList tail
    // Edited By Krish
    | head :: tail -> Console.Write("{0} ",head.ToString())
                      printTList tail
    // Edit Completed by Krish
                  
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
