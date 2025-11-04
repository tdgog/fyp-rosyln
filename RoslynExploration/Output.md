For the input:
```csharp
class Example {
    void Foo( {
        int x = 10
        if (x > 5) {
            Console.WriteLine("ok")
        } else {
            Console.WriteLine("bad")
        }
    }
}
```

These errors were found:
```
CS1026: ) expected @ : (2,14)-(2,15)
CS1002: ; expected @ : (3,18)-(3,18)
CS1002: ; expected @ : (5,35)-(5,35)
CS1002: ; expected @ : (7,36)-(7,36)
```

This syntax tree was generated:
```
CompilationUnit
    ClassDeclaration
        ClassKeyword
        IdentifierToken
        OpenBraceToken
        MethodDeclaration
            PredefinedType
                VoidKeyword
            IdentifierToken
            ParameterList
                OpenParenToken
                CloseParenToken <-- ERROR CS1026 ) expected
            Block
                OpenBraceToken
                LocalDeclarationStatement
                    VariableDeclaration
                        PredefinedType
                            IntKeyword
                        VariableDeclarator
                            IdentifierToken
                            EqualsValueClause
                                EqualsToken
                                NumericLiteralExpression
                                    NumericLiteralToken
                    SemicolonToken <-- ERROR CS1002 ; expected
                IfStatement
                    IfKeyword
                    OpenParenToken
                    GreaterThanExpression
                        IdentifierName
                            IdentifierToken
                        GreaterThanToken
                        NumericLiteralExpression
                            NumericLiteralToken
                    CloseParenToken
                    Block
                        OpenBraceToken
                        ExpressionStatement
                            InvocationExpression
                                SimpleMemberAccessExpression
                                    IdentifierName
                                        IdentifierToken
                                    DotToken
                                    IdentifierName
                                        IdentifierToken
                                ArgumentList
                                    OpenParenToken
                                    Argument
                                        StringLiteralExpression
                                            StringLiteralToken
                                    CloseParenToken
                            SemicolonToken <-- ERROR CS1002 ; expected
                        CloseBraceToken
                    ElseClause
                        ElseKeyword
                        Block
                            OpenBraceToken
                            ExpressionStatement
                                InvocationExpression
                                    SimpleMemberAccessExpression
                                        IdentifierName
                                            IdentifierToken
                                        DotToken
                                        IdentifierName
                                            IdentifierToken
                                    ArgumentList
                                        OpenParenToken
                                        Argument
                                            StringLiteralExpression
                                                StringLiteralToken
                                        CloseParenToken
                                SemicolonToken <-- ERROR CS1002 ; expected
                            CloseBraceToken
                CloseBraceToken
        CloseBraceToken
    EndOfFileToken
```

And this output code was generated:
```
class Example
{
  void Foo( <-- ERROR: CS1026 ) expected
  {
    int x = 10 <-- ERROR: CS1002 ; expected
    if (x>5)
    {
      Console.WriteLine("ok") <-- ERROR: CS1002 ; expected
    }
    else
    {
      Console.WriteLine("bad") <-- ERROR: CS1002 ; expected
    }
  }
}
```
