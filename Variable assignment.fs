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
    | Var of string             // Variable name
    | EndOfStmt                 // End of statement ";"
    | TypeDecl of VariableType  // Added for type declaration (int, float)

let symbolTable = Dictionary<string, float>()           // Symbol table to store variable values and types
let variableTypes = Dictionary<string, VariableType>()


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
        | 'i' :: 'n' :: 't' :: rest -> TypeDecl IntType :: tokenize rest
        | 'f' :: 'l' :: 'o' :: 'a' :: 't' :: rest -> TypeDecl FloatType :: tokenize rest
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


let rec parseAndEvaluate tokens =
    match tokens with
    | TypeDecl declType :: Var varName :: Assign :: rest ->
        let exprValue, remaining = parseExpression rest
        match declType with
        | IntType -> 
            let intValue = Math.Round(exprValue : float)
            if intValue <> exprValue then 
                failwith "Type error: Expected integer value for int variable"
            symbolTable.[varName] <- intValue
            variableTypes.[varName] <- IntType
        | FloatType ->
            symbolTable.[varName] <- exprValue
            variableTypes.[varName] <- FloatType
        remaining
    | Var varName :: Assign :: rest ->
        let exprValue, remaining = parseExpression rest
        if variableTypes.ContainsKey(varName) then
            match variableTypes.[varName] with
            | IntType ->
                let intValue = Math.Round(exprValue : float)
                if intValue <> exprValue then 
                    failwith "Type error: Expected integer value for int variable"
                symbolTable.[varName] <- intValue
            | FloatType ->
                symbolTable.[varName] <- exprValue
        else
            symbolTable.[varName] <- exprValue
            variableTypes.[varName] <- FloatType
        remaining
    | _ -> failwith "Parser error: Expected variable assignment"

and parseExpression tokens =
    let rec parseFactor tokens =
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
        | _ -> failwith "Parser error: Invalid factor"

    and parsePower tokens =
        let rec powerLoop baseValue tokens =
            match tokens with
            | Pow :: rest ->
                let exponent, remaining = parseFactor rest
                powerLoop (Math.Pow(baseValue, exponent)) remaining
            | _ -> baseValue, tokens
        let baseValue, remaining = parseFactor tokens
        powerLoop baseValue remaining

    and parseTerms tokens =
        let rec termLoop acc tokens =
            match tokens with
            | Mul :: rest ->
                let powerValue, remaining = parsePower rest
                termLoop (acc * powerValue) remaining
            | Div :: rest ->
                let powerValue, remaining = parsePower rest
                termLoop (acc / powerValue) remaining
            | _ -> acc, tokens
        let powerValue, remaining = parsePower tokens
        termLoop powerValue remaining

    let rec exprLoop acc tokens =
        match tokens with
        | Add :: rest ->
            let termValue, remaining = parseTerms rest
            exprLoop (acc + termValue) remaining
        | Sub :: rest ->
            let termValue, remaining = parseTerms rest
            exprLoop (acc - termValue) remaining
        | _ -> acc, tokens
    let termValue, remaining = parseTerms tokens
    exprLoop termValue remaining

let processInput (input: string) =
    input.Split(';')
    |> Array.map (fun stmt -> stmt.Trim())
    |> Array.iter (fun stmt ->
        if stmt.Length > 0 then
            let tokens = lexer stmt
            let remainingTokens = parseAndEvaluate tokens
            if remainingTokens <> [] then
                printfn "Parser error: Unprocessed tokens after statement")

[<EntryPoint>]
let main argv =
    Console.WriteLine("Enter statements (e.g., int x = 2; float y = x^2 + 2.5):")
    let input = Console.ReadLine()
    processInput input
    // Prints symbol table values
    symbolTable |> Seq.iter (fun kvp -> Console.WriteLine(kvp.Key + " = " + kvp.Value.ToString()))
    0