//// Simple Interpreter in F#
//// Author: R.J. Lapeer 
//// Date: 23/10/2022
//// Reference: Peter Sestoft, Grammars and parsing with F#, Tech. Report

//open System

//type terminal = 
//    Add | Sub | Mul | Div | Rem | Lpar | Rpar | Pow | Dot | Num of float // int -- Before
//    //ideas - turn 'Num' into a normal token and then make two new ones, 'Int' and 'Float' that are of their type. 'Num' has to be one of those two which is determined by whether there's a 'dot' in the number or not
//    //also need to add unary negatives...

//let str2lst s = [for c in s -> c]
//let isblank c = System.Char.IsWhiteSpace c
//let isdigit c = System.Char.IsDigit c
//let lexError = System.Exception("Lexer error")
//let intVal (c:char) = (int)((int)c - (int)'0')
//let parseError = System.Exception("Parser error")
//let divideByZero = System.Exception("Attempted to divide by zero")



////let rec scInt(iStr, iVal) = -- Before
////    match iStr with -- Before
////    c :: tail when isdigit c -> scInt(tail, 10*iVal+(intVal c)) --Before
////    | _ -> (iStr, iVal) -- Before

//// Edited by Krish
//let rec scFloat(iStr, iVal) = 
//    match iStr with 
//    | '.' :: tail ->
//        let rec parseDecimal tail multiplier value =
//            match tail with
//            | c :: t when isdigit c -> parseDecimal t (multiplier / 10.0) (value + (float (intVal c)) * multiplier)
//            | _ -> (tail, value)
//        let (restStr, decimalVal) = parseDecimal tail 0.1 (float iVal)
//        (restStr, decimalVal)
//    | c :: tail when isdigit c -> scFloat(tail, 10.0 * iVal + float (intVal c))
//    | _ -> (iStr, iVal)
//// Edit Completed by Krish


//let lexer input = 
//    let rec scan input =
//        match input with
//        | [] -> []
//        | '+'::tail -> Add :: scan tail
//        | '-'::tail -> Sub :: scan tail
//        | '*'::tail -> Mul :: scan tail
//        | '/'::tail -> Div :: scan tail
//        | '%'::tail -> Rem :: scan tail
//        | '('::tail -> Lpar:: scan tail
//        | ')'::tail -> Rpar:: scan tail
//        | '^'::tail -> Pow :: scan tail
//        | '.'::tail -> Dot :: scan tail
//        | c :: tail when isblank c -> scan tail
//        | c :: tail when isdigit c -> let (iStr, iVal) = scFloat(tail, float (intVal c)) // scInt(tail, intVal c) -- Before
//                                      Num iVal :: scan iStr
//        | _ -> raise lexError
//    scan (str2lst input)

//let getInputString() : string = 
//    Console.Write("Enter an expression: ")
//    Console.ReadLine()

//// Grammar in BNF:
//// <E>        ::= <T> <Eopt>
//// <Eopt>     ::= "+" <T> <Eopt> | "-" <T> <Eopt> | <empty>
//// <T>        ::= <P> <Topt>
//// <Topt>     ::= "*" <P> <Topt> | "/" <P> <Topt> | "%" <P> <Topt> | <empty>
//// <P>        ::= <Numb> <Popt>
//// <Popt>     ::= "^" <Numb> <Popt> | <empty>
//// <Numb>     ::= "Num" <value> | "(" <E> ")" | "-" <Numb>
//// <Float>    ::= "Num" <value> "." "Num" <value>


//let parser tList = 
//    let rec E tList = (T >> Eopt) tList         // >> is forward function composition operator: let inline (>>) f g x = g(f x)
//    and Eopt tList = 
//        match tList with
//        | Add :: tail -> (T >> Eopt) tail
//        | Sub :: tail -> (T >> Eopt) tail
//        | _ -> tList
//    and T tList = (P >> Topt) tList
//    and Topt tList =
//        match tList with
//        | Mul :: tail -> (P >> Topt) tail
//        | Div :: tail -> (P >> Topt) tail
//        | Rem :: tail -> (P >> Topt) tail
//        | _ -> tList
//    and P tList = (Numb >> Popt) tList
//    and Popt tList =
//        match tList with
//        | Pow :: tail -> (Numb >> Popt) tail
//        | _ -> tList
//    and Numb tList =
//        match tList with 
//        | Num value :: tail -> tail
//        | Lpar :: tail -> match E tail with 
//                          | Rpar :: tail -> tail
//                          | _ -> raise parseError
//        //| Sub :: tail -> match Numb tail with -- Before 
//        //                  | Num value ::tail -> tail -- Before 
//        //                  | _ -> raise parseError -- Before 
//        //| _ -> raise parseError -- Before
//        // Edited by Krish
//        | Sub :: tail -> Numb tail
//        | _ -> raise parseError
//        // Edit Completed by Krish
//    E tList

//let parseNeval tList =

//    let rec E tList = (T >> Eopt) tList
//    and Eopt (tList, value) = 
//        match tList with
//        | Add :: tail -> let (tLst, tval) = T tail
//                         Eopt (tLst, value + tval)
//        | Sub :: tail -> let (tLst, tval) = T tail 
//                         Eopt (tLst, value - tval)                            
//        | _ -> (tList, value)
//    and T tList = (P >> Topt) tList
//    and Topt (tList, value) =
//        match tList with
//        | Mul :: tail -> let (tLst, tval) = P tail
//                         Topt (tLst, value * tval)
//        | Div :: tail -> let (tLst, tval) = P tail
//                         match tval with
//                         //| 0 -> raise divideByZero -- Before
//                         // Edited by Krish
//                         | 0.0 -> raise divideByZero
//                         // Edit Completed by Krish
//                         | _ -> Topt (tLst, value / tval)
//        | Rem :: tail -> let (tLst, tval) = P tail
//                         //Topt (tLst, value % tval) -- Before 
//                         // Edited by Krish
//                         match tval with
//                         | 0.0 -> raise divideByZero
//                         | _ -> Topt (tLst, value % tval)
//                         // Edit Completed by Krish
//        | _ -> (tList, value)
//    and P tList = (Numb >> Popt) tList
//    and Popt (tList, value) =
//        match tList with
//        | Pow :: tail -> let (tLst, tval) = Numb tail
//                         //Popt (tLst, value + tval) //how turn value into float?? so not doing exponention (**) operator -- Before
//                         // Edited by Krish
//                         Popt (tLst, Math.Pow(value, tval))
//                         // Edit Completed by Krish
//        | _ -> (tList, value)
//    and Numb tList =
//        match tList with 
//        | Num value :: tail -> (tail, value)
//        | Lpar :: tail -> let (tLst, tval) = E tail
//                          match tLst with 
//                          | Rpar :: tail -> (tail, tval)
//                          | _ -> raise parseError
//        | Sub :: tail -> let (tLst, tval) = Numb tail
//                         //match tLst with
//                         //| Num value :: tail -> (tail, -tval)
//                         //| _ -> raise parseError
//                         // Edited by Krish
//                         (tLst, -tval)
//                         // Edit Completed by Krish
//        | _ -> raise parseError
//    E tList

//let rec printTList (lst:list<terminal>) : list<string> = 
//    match lst with
//    //head::tail -> Console.Write("{0} ",head.ToString())
//    //              printTList tail
//    // Edited By Krish
//    | head :: tail -> Console.Write("{0} ",head.ToString())
//                      printTList tail
//    // Edit Completed by Krish
                  
//    | [] -> Console.Write("EOL\n")
//            []

//[<EntryPoint>]
//let main argv  =
//    Console.WriteLine("Simple Interpreter")
//    let input:string = getInputString()
//    let oList = lexer input
//    let sList = printTList oList;
//    let pList = printTList (parser oList)
//    let Out = parseNeval oList
//    Console.WriteLine("Result = {0}", snd Out)
//    0
  




//open System
//open System.Numerics // For complex numbers

//type terminal = 
//    Add | Sub | Mul | Div | Rem | Lpar | Rpar | Pow | Dot | Num of float | ComplexNum of Complex | RationalNum of (int * int) 
//    | Sin | Cos | Log | Exp | Tan | Sqrt | Pi

//let str2lst s = [for c in s -> c]
//let isblank c = System.Char.IsWhiteSpace c
//let isdigit c = System.Char.IsDigit c
//let lexError = System.Exception("Lexer error")
//let intVal (c:char) = (int)((int)c - (int)'0')
//let parseError = System.Exception("Parser error")
//let divideByZero = System.Exception("Attempted to divide by zero")

//let rec scFloat(iStr, iVal) = 
//    match iStr with
//    | '.' :: tail -> 
//        let rec parseDecimal tail multiplier value =
//            match tail with
//            | c :: t when isdigit c -> parseDecimal t (multiplier / 10.0) (value + (float (intVal c)) * multiplier)
//            | _ -> (tail, value)
//        let (restStr, decimalVal) = parseDecimal tail 0.1 (float iVal)
//        (restStr, decimalVal)
//    | c :: tail when isdigit c -> scFloat(tail, 10.0 * iVal + float (intVal c))
//    | _ -> (iStr, iVal)

//let lexer input = 
//    let rec scan input =
//        match input with
//        | [] -> []
//        | '+'::tail -> Add :: scan tail
//        | '-'::tail -> Sub :: scan tail
//        | '*'::tail -> Mul :: scan tail
//        | '/'::tail -> Div :: scan tail
//        | '%'::tail -> Rem :: scan tail
//        | '('::tail -> Lpar:: scan tail
//        | ')'::tail -> Rpar:: scan tail
//        | '^'::tail -> Pow :: scan tail
//        | '.'::tail -> Dot :: scan tail
//        | 'p'::'i'::tail -> Pi :: scan tail
//        | 's'::'i'::'n'::tail -> Sin :: scan tail
//        | 'c'::'o'::'s'::tail -> Cos :: scan tail 
//        | 'l'::'o'::'g'::tail -> Log :: scan tail 
//        | 'e'::'x'::'p'::tail -> Exp :: scan tail 
//        | 't'::'a'::'n'::tail -> Tan :: scan tail 
//        | 's'::'q'::'r'::'t'::tail -> Sqrt :: scan tail 
//        | c :: tail when isblank c -> scan tail
//        | c :: tail when isdigit c -> 
//            let (iStr, iVal) = scFloat(tail, float (intVal c))
//            if iStr <> [] && iStr.Head = '/' then
//                scRational(iStr, iVal)
//            elif iStr <> [] && iStr.Head = '+' then
//                scComplex(iStr, iVal)
//            else
//                Num iVal :: scan iStr                         
//        | _ -> raise lexError

//    and scRational(input, num) = 
//        match input with
//        | '/' :: tail -> 
//            match tail with
//            | c :: t when isdigit c ->
//                let (rest, den) = scFloat(t, float(intVal c))
//                RationalNum(int num, int den) :: scan rest
//            | _ -> raise lexError
//        | _ -> scan input

//    and scComplex(input, realPart) =
//        match input with
//        | '+' :: tail ->
//            match tail with
//            | c :: t when isdigit c ->
//                let (iStr, imagPart) = scFloat(t, float (intVal c))
//                match iStr with
//                | 'i' :: rest  -> ComplexNum(Complex(realPart, imagPart)) :: scan rest
//                | _ -> raise lexError
//            | _ -> raise lexError
//        | _ -> scan input

//    scan (str2lst input)

//let getInputString() : string = 
//    Console.Write("Enter an expression: ")
//    Console.ReadLine()

//let rec parseNeval tList =
//    let rec E tList = (T >> Eopt) tList
//    and Eopt (tList, value) = 
//        match tList with
//        | Add :: tail -> let (tLst, tval) = T tail
//                         Eopt (tLst, value + tval)
//        | Sub :: tail -> let (tLst, tval) = T tail 
//                         Eopt (tLst, value - tval)                            
//        | _ -> (tList, value)
//    and T tList = (P >> Topt) tList
//    and Topt (tList, value) =
//        match tList with
//        | Mul :: tail -> let (tLst, tval) = P tail
//                         Topt (tLst, value * tval)
//        | Div :: tail -> let (tLst, tval) = P tail
//                         match tval with
//                         | 0.0 -> raise divideByZero
//                         | _ -> Topt (tLst, value / tval)
//        | Rem :: tail -> let (tLst, tval) = P tail
//                         match tval with
//                         | 0.0 -> raise divideByZero
//                         | _ -> Topt (tLst, value % tval)
//        | _ -> (tList, value)
//    and P tList = (Numb >> Popt) tList
//    and Popt (tList, value) =
//        match tList with
//        | Pow :: tail -> let (tLst, tval) = Numb tail
//                         Popt (tLst, Math.Pow(value, tval))
//        | _ -> (tList, value)
//    and Numb tList =
//        match tList with 
//        | Num value :: tail -> (tail, value)
//        | ComplexNum cval :: tail -> (tail, Complex.Abs(cval))
//        | RationalNum (n, d) :: tail -> (tail, float n / float d)
//        | Pi :: tail -> (tail, Math.PI)
//        | Lpar :: tail -> let (tLst, tval) = E tail
//                          match tLst with 
//                          | Rpar :: tail -> (tail, tval)
//                          | _ -> raise parseError
//        | Sub :: tail -> let (tLst, tval) = Numb tail
//                         (tLst, -tval)
//        | Sin :: tail -> let (tLst, tval) = Numb tail
//                         (tLst, Math.Sin(tval))
//        | Cos :: tail -> let (tLst, tval) = Numb tail
//                         (tLst, Math.Cos(tval))
//        | Log :: tail -> let (tLst, tval) = Numb tail
//                         (tLst, Math.Log(tval))
//        | Exp :: tail -> let (tLst, tval) = Numb tail
//                         (tLst, Math.Exp(tval))
//        | Sqrt :: tail -> let (tLst, tval) = Numb tail
//                          (tLst, Math.Sqrt(tval))
//        | _ -> raise parseError
//    E tList

//let rec printTList (lst:list<terminal>) : list<string> = 
//    match lst with
//    //head::tail -> Console.Write("{0} ",head.ToString())
//    //              printTList tail
//    // Edited By Krish
//    | head :: tail -> Console.Write("{0} ",head.ToString())
//                      printTList tail
//    // Edit Completed by Krish
                  
//    | [] -> Console.Write("EOL\n")
//            []

//[<EntryPoint>]
//let main argv  =
//    Console.WriteLine("Advanced Interpreter with Optional Features: ")
//    let input = getInputString()
//    let oList = lexer input
//    let sList = printTList oList;
//    let Out = parseNeval oList
//    Console.WriteLine("Result = {0}", snd Out)
//    0








//open System
//open System.Collections.Generic

//type Terminal =
//    | Add | Sub | Mul | Div | Assign | Lpar | Rpar | Num of float | Var of string | EndOfStmt

//// Symbol table to store variable values
//let symbolTable = Dictionary<string, float>()

//// Lexer: Convert input string to a list of terminals
//let lexer (input: string) =
//    let rec tokenize chars =
//        match chars with
//        | [] -> []
//        | ' ' :: rest -> tokenize rest
//        | ';' :: rest -> EndOfStmt :: tokenize rest
//        | '+' :: rest -> Add :: tokenize rest
//        | '-' :: rest -> Sub :: tokenize rest
//        | '*' :: rest -> Mul :: tokenize rest
//        | '/' :: rest -> Div :: tokenize rest
//        | '=' :: rest -> Assign :: tokenize rest
//        | '(' :: rest -> Lpar :: tokenize rest
//        | ')' :: rest -> Rpar :: tokenize rest
//        | c :: rest when Char.IsDigit(c) ->
//            let number, remaining = parseNumber (c :: rest)
//            Num number :: tokenize remaining
//        | c :: rest when Char.IsLetter(c) ->
//            let variable, remaining = parseVariable (c :: rest)
//            Var variable :: tokenize remaining
//        | _ -> failwith "Lexer error: Invalid character"
//    and parseNumber chars =
//        let rec collectDigits acc chars =
//            match chars with
//            | c :: rest when Char.IsDigit(c) || c = '.' -> collectDigits (acc + string c) rest
//            | _ -> System.Double.Parse(acc), chars
//        collectDigits "" chars
//    and parseVariable chars =
//        let rec collectLetters acc chars =
//            match chars with
//            | c :: rest when Char.IsLetter(c) -> collectLetters (acc + string c) rest
//            | _ -> acc, chars
//        collectLetters "" chars
//    tokenize (List.ofSeq input)

//// Parser and evaluator for expressions with basic arithmetic
//let rec parseAndEvaluate tokens =
//    match tokens with
//    | Var varName :: Assign :: rest ->
//        let exprValue, remaining = parseExpression rest
//        symbolTable.[varName] <- exprValue
//        remaining
//    | _ -> failwith "Parser error: Expected variable assignment"

//and parseExpression tokens =
//    let rec parseTerm tokens =
//        match tokens with
//        | Num n :: rest -> n, rest
//        | Var v :: rest ->
//            if symbolTable.ContainsKey(v) then
//                symbolTable.[v], rest
//            else failwithf "Error: Undefined variable %s" v
//        | Lpar :: rest ->
//            let exprValue, remaining = parseExpression rest
//            match remaining with
//            | Rpar :: rest -> exprValue, rest
//            | _ -> failwith "Parser error: Expected closing parenthesis"
//        | _ -> failwith "Parser error: Invalid term"

//    and parseFactors tokens =
//        let rec factorLoop acc tokens =
//            match tokens with
//            | Mul :: rest ->
//                let termValue, remaining = parseTerm rest
//                factorLoop (acc * termValue) remaining
//            | Div :: rest ->
//                let termValue, remaining = parseTerm rest
//                factorLoop (acc / termValue) remaining
//            | _ -> acc, tokens
//        let termValue, remaining = parseTerm tokens
//        factorLoop termValue remaining

//    let rec exprLoop acc tokens =
//        match tokens with
//        | Add :: rest ->
//            let factorValue, remaining = parseFactors rest
//            exprLoop (acc + factorValue) remaining
//        | Sub :: rest ->
//            let factorValue, remaining = parseFactors rest
//            exprLoop (acc - factorValue) remaining
//        | _ -> acc, tokens
//    let factorValue, remaining = parseFactors tokens
//    exprLoop factorValue remaining

//// Function to split and process each statement in the input
//let processInput (input: string) =
//    input.Split(';')
//    |> Array.map (fun stmt -> stmt.Trim())
//    |> Array.iter (fun stmt ->
//        if stmt.Length > 0 then
//            let tokens = lexer stmt
//            let remainingTokens = parseAndEvaluate tokens
//            if remainingTokens <> [] then
//                printfn "Parser error: Unprocessed tokens after statement")

//// Main function to run the interpreter
//[<EntryPoint>]
//let main argv =
//    Console.WriteLine("Enter statements (e.g., x = 10; y = x + 5):")
//    let input = Console.ReadLine()
//    processInput input
//    // Print symbol table values
//    symbolTable |> Seq.iter (fun kvp -> Console.WriteLine(kvp.Key + " = " + kvp.Value.ToString()))
//    0




open System
open System.Collections.Generic

type VariableType =
    | IntType
    | FloatType

type Terminal =
    | Add 
    | Sub 
    | Mul 
    | Div 
    | Pow
    | Assign 
    | Lpar 
    | Rpar 
    | Num of float 
    | Var of string 
    | EndOfStmt
    | TypeDecl of VariableType  // Added for type declaration

// Symbol table to store variable values and types
let symbolTable = Dictionary<string, float>()
let variableTypes = Dictionary<string, VariableType>()

// Lexer: Convert input string to a list of terminals
let lexer (input: string) =
    let rec tokenize chars =
        match chars with
        | [] -> []
        | ' ' :: rest -> tokenize rest
        | ';' :: rest -> EndOfStmt :: tokenize rest
        | '+' :: rest -> Add :: tokenize rest
        | '-' :: rest -> Sub :: tokenize rest
        | '*' :: rest -> Mul :: tokenize rest
        | '/' :: rest -> Div :: tokenize rest
        | '^' :: rest -> Pow :: tokenize rest
        | '=' :: rest -> Assign :: tokenize rest
        | '(' :: rest -> Lpar :: tokenize rest
        | ')' :: rest -> Rpar :: tokenize rest
        | c :: rest when Char.IsDigit(c) ->
            let number, remaining = parseNumber (c :: rest)
            Num number :: tokenize remaining
        | c :: rest when Char.IsLetter(c) ->
            let variable, remaining = parseVariable (c :: rest)
            Var variable :: tokenize remaining
        | _ -> failwith "Lexer error: Invalid character"
    and parseNumber chars =
        let rec collectDigits acc chars =
            match chars with
            | c :: rest when Char.IsDigit(c) || c = '.' -> collectDigits (acc + string c) rest
            | _ -> System.Double.Parse(acc), chars
        collectDigits "" chars
    and parseVariable chars =
        let rec collectLetters acc chars =
            match chars with
            | c :: rest when Char.IsLetter(c) -> collectLetters (acc + string c) rest
            | _ -> acc, chars
        collectLetters "" chars

    tokenize (List.ofSeq input)

// Parser and evaluator for expressions with basic arithmetic
let rec parseAndEvaluate tokens =
    match tokens with
    | Var varName :: Assign :: rest ->
        let exprValue, remaining = parseExpression rest
        symbolTable.[varName] <- exprValue
        remaining
    | _ -> failwith "Parser error: Expected variable assignment"

and parseExpression tokens =
    let rec parseTerm tokens =
        match tokens with
        | Num n :: rest -> n, rest
        | Var v :: rest ->
            if symbolTable.ContainsKey(v) then
                symbolTable.[v], rest
            else failwithf "Error: Undefined variable %s" v
        | Lpar :: rest ->
            let exprValue, remaining = parseExpression rest
            match remaining with
            | Rpar :: rest -> exprValue, rest
            | _ -> failwith "Parser error: Expected closing parenthesis"
        | _ -> failwith "Parser error: Invalid term"

    and parseFactors tokens =
        let rec factorLoop acc tokens =
            match tokens with
            | Mul :: rest ->
                let termValue, remaining = parseTerm rest
                factorLoop (acc * termValue) remaining
            | Div :: rest ->
                let termValue, remaining = parseTerm rest
                factorLoop (acc / termValue) remaining
            | _ -> acc, tokens
        let termValue, remaining = parseExponents tokens
        factorLoop termValue remaining

    and parseExponents tokens =         // New function for handling exponentiation
        let rec exponentLoop acc tokens =
            match tokens with
            | Pow :: rest ->
                let baseValue, remaining = parseTerm rest
                exponentLoop (Math.Pow(acc, baseValue)) remaining
            | _ -> acc, tokens
        let termValue, remaining = parseTerm tokens
        exponentLoop termValue remaining

    let rec exprLoop acc tokens =
        match tokens with
        | Add :: rest ->
            let factorValue, remaining = parseFactors rest
            exprLoop (acc + factorValue) remaining
        | Sub :: rest ->
            let factorValue, remaining = parseFactors rest
            exprLoop (acc - factorValue) remaining
        | _ -> acc, tokens
    let factorValue, remaining = parseFactors tokens
    exprLoop factorValue remaining

// Function to split and process each statement in the input
let processInput (input: string) =
    input.Split(';')
    |> Array.map (fun stmt -> stmt.Trim())
    |> Array.iter (fun stmt ->
        if stmt.Length > 0 then
            let tokens = lexer stmt
            let remainingTokens = parseAndEvaluate tokens
            if remainingTokens <> [] then
                printfn "Parser error: Unprocessed tokens after statement")

// Main function to run the interpreter
[<EntryPoint>]
let main argv =
    Console.WriteLine("Enter statements (e.g., x = 10; y = x + 5):")
    let input = Console.ReadLine()
    processInput input
    // Print symbol table values
    symbolTable |> Seq.iter (fun kvp -> Console.WriteLine(kvp.Key + " = " + kvp.Value.ToString()))
    0





