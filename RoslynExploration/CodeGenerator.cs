using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynExploration;

public class CodeGenerator {
    
    public CodeGenerator() {}

    private readonly StringBuilder stringBuilder = new StringBuilder();
    private int currentIndentation = 0;
    private bool atLineStart = true;

    public string generate(SyntaxNode root) {
        stringBuilder.Clear();
        currentIndentation = 0;
        atLineStart = true;
        
        visitNode(root);

        return stringBuilder.ToString();
    }

    private void trailingNewLine() {
        if (stringBuilder.Length == 0) return;
        if (stringBuilder[^1] != '\n')
            appendLine();
    }

    private void indent() {
        const string indentString = "  ";
        if (!atLineStart) return;
        for (int i = 0; i < currentIndentation; i++)
            stringBuilder.Append(indentString);
        atLineStart = false;
    }

    private void appendTokens(SyntaxTokenList tokens) {
        bool first = true;
        foreach (SyntaxToken token in tokens) {
            if (!first) stringBuilder.Append(' ');
            first = false;

            stringBuilder.Append(token.Text);
        }
    }

    private void appendLine() {
        if (atLineStart) indent();
        stringBuilder.AppendLine();
        atLineStart = true;
    }

    private void appendLine(string str) {
        if (atLineStart) indent();
        stringBuilder.AppendLine(str);
        atLineStart = true;
    }

    private void visitNode(SyntaxNode? node) {
        if (node == null) return;
        
        switch (node.Kind()) {
            case SyntaxKind.CompilationUnit: {
                visitChildren(node);
                break;
            }
            
            case SyntaxKind.ClassDeclaration: {
                ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax) node;
                indent();

                if (classDeclaration.Modifiers.Any()) {
                    appendTokens(classDeclaration.Modifiers);
                    stringBuilder.Append(' ');
                }

                stringBuilder.Append("class ");
                stringBuilder.Append(classDeclaration.Identifier.Text);
                if (classDeclaration.TypeParameterList != null)
                    visitNode(classDeclaration.TypeParameterList);
                appendLine();

                visitToken(classDeclaration.OpenBraceToken);

                foreach (SyntaxNode member in classDeclaration.Members) {
                    visitNode(member);
                }

                visitToken(classDeclaration.CloseBraceToken);
                break;
            }
                
            case SyntaxKind.MethodDeclaration: {
                MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax) node;
                indent();

                if (methodDeclaration.Modifiers.Any()) {
                    appendTokens(methodDeclaration.Modifiers);
                    stringBuilder.Append(' ');
                }

                visitNode(methodDeclaration.ReturnType);
                stringBuilder.Append(' ');
                stringBuilder.Append(methodDeclaration.Identifier.Text);
                visitNode(methodDeclaration.ParameterList);

                if (methodDeclaration.Body != null) {
                    appendLine();
                    visitNode(methodDeclaration.Body);
                }
                else if (methodDeclaration.ExpressionBody != null) {
                    stringBuilder.Append(' ');
                    visitNode(methodDeclaration.ExpressionBody);
                    appendLine();
                }

                break;
            }
            case SyntaxKind.ParameterList: {
                ParameterListSyntax parameterList = (ParameterListSyntax) node;
                stringBuilder.Append('(');

                bool first = true;
                foreach (ParameterSyntax parameter in parameterList.Parameters) {
                    if (!first) stringBuilder.Append(", ");
                    first = false;
                    visitNode(parameter);
                }

                stringBuilder.Append(')');
                break;
            }
            
            case SyntaxKind.Parameter: {
                ParameterSyntax parameter = (ParameterSyntax) node;
                if (parameter.Modifiers.Any()) {
                    appendTokens(parameter.Modifiers);
                    stringBuilder.Append(' ');
                }

                visitNode(parameter.Type);
                if (parameter.Type != null) stringBuilder.Append(' ');
                stringBuilder.Append(parameter.Identifier.Text);
                break;
            }

            case SyntaxKind.Block: {
                BlockSyntax block = (BlockSyntax) node;
                visitToken(block.OpenBraceToken);
                foreach (SyntaxNode statement in block.Statements) 
                    visitNode(statement);
                visitToken(block.CloseBraceToken);
                break;
            }

            case SyntaxKind.LocalDeclarationStatement: {
                LocalDeclarationStatementSyntax localDeclarationStatement = (LocalDeclarationStatementSyntax) node;
                indent();
                visitNode(localDeclarationStatement.Declaration);
                visitToken(localDeclarationStatement.SemicolonToken);
                appendLine();
                break;
            }

            case SyntaxKind.VariableDeclaration: {
                VariableDeclarationSyntax variableDeclaration = (VariableDeclarationSyntax) node;
                visitNode(variableDeclaration.Type);
                stringBuilder.Append(' ');
                bool first = true;
                foreach (SyntaxNode variable in variableDeclaration.Variables) {
                    if (!first) stringBuilder.Append(", ");
                    first = false;
                    visitNode(variable);
                }

                break;
            }

            case SyntaxKind.VariableDeclarator: {
                VariableDeclaratorSyntax variableDeclarator = (VariableDeclaratorSyntax) node;
                stringBuilder.Append(variableDeclarator.Identifier.Text);
                if (variableDeclarator.Initializer != null) {
                    stringBuilder.Append(" = ");
                    visitNode(variableDeclarator.Initializer.Value);
                }

                break;
            }

            case SyntaxKind.ExpressionStatement: {
                ExpressionStatementSyntax expressionStatement = (ExpressionStatementSyntax) node;
                indent();
                visitNode(expressionStatement.Expression);
                visitToken(expressionStatement.SemicolonToken);
                appendLine();
                break;
            }
                
            case SyntaxKind.IfStatement: {
                IfStatementSyntax ifStatement = (IfStatementSyntax) node;
                indent();
                stringBuilder.Append("if (");
                visitNode(ifStatement.Condition);
                appendLine(")");
                if (ifStatement.Statement is BlockSyntax)
                    visitNode(ifStatement.Statement);
                else {
                    indent();
                    visitNode(ifStatement.Statement);
                    appendLine();
                }

                if (ifStatement.Else != null) {
                    indent();
                    appendLine("else");
                    if (ifStatement.Else.Statement is BlockSyntax)
                        visitNode(ifStatement.Else.Statement);
                    else {
                        indent();
                        visitNode(ifStatement.Else.Statement);
                        appendLine();
                    }
                }

                break;
            }

            case SyntaxKind.InvocationExpression: {
                InvocationExpressionSyntax invocationExpression = (InvocationExpressionSyntax) node;
                visitNode(invocationExpression.Expression);
                visitNode(invocationExpression.ArgumentList);
                break;
            }

            case SyntaxKind.ArgumentList: {
                ArgumentListSyntax argumentList = (ArgumentListSyntax) node;
                stringBuilder.Append('(');
                bool first = true;
                foreach (ArgumentSyntax argument in argumentList.Arguments) {
                    if (!first) stringBuilder.Append(", ");
                    first = false;
                    visitNode(argument);
                }

                stringBuilder.Append(')');
                break;
            }

            case SyntaxKind.Argument: {
                ArgumentSyntax argument = (ArgumentSyntax) node;
                visitNode(argument.Expression);
                break;
            }

            case SyntaxKind.SimpleMemberAccessExpression: {
                MemberAccessExpressionSyntax memberAccessExpression = (MemberAccessExpressionSyntax) node;
                visitNode(memberAccessExpression.Expression);
                stringBuilder.Append('.');
                stringBuilder.Append(memberAccessExpression.Name.Identifier.Text);
                break;
            }

            case SyntaxKind.IdentifierName: {
                IdentifierNameSyntax identifierName = (IdentifierNameSyntax) node;
                stringBuilder.Append(identifierName.Identifier.Text);
                break;
            }

            case SyntaxKind.PredefinedType: {
                PredefinedTypeSyntax predefinedType = (PredefinedTypeSyntax) node;
                stringBuilder.Append(predefinedType.Keyword.Text);
                break;
            }

            case SyntaxKind.NumericLiteralExpression:
            case SyntaxKind.StringLiteralExpression: {
                LiteralExpressionSyntax literalExpression = (LiteralExpressionSyntax) node;
                stringBuilder.Append(literalExpression.Token.Text);
                break;
            }
            
            case SyntaxKind.EqualsValueClause: {
                EqualsValueClauseSyntax equalsValueClause = (EqualsValueClauseSyntax) node;
                stringBuilder.Append(" = ");
                visitNode(equalsValueClause.Value);
                break;
            }

            default:
                visitChildren(node);
                break;
        }
    }

    void visitChildren(SyntaxNode node) {
        foreach (SyntaxNodeOrToken child in node.ChildNodesAndTokens())
            if (child.IsNode) visitNode(child.AsNode());
            else visitToken(child.AsToken());
    }

    void visitToken(SyntaxToken token) {
        if (token.IsMissing) {
            stringBuilder.Append($"/*Missing {token.Kind()}*/");
            return;
        }

        switch (token.Kind()) {
            case SyntaxKind.SemicolonToken:
                // appendLine(";");
                // atLineStart = true;
                stringBuilder.Append(';');
                break;
            
            case SyntaxKind.OpenBraceToken:
                indent();
                appendLine("{");
                currentIndentation++;
                atLineStart = true;
                break;
            
            case SyntaxKind.CloseBraceToken:
                currentIndentation = Math.Max(0, currentIndentation - 1);
                indent();
                appendLine("}");
                atLineStart = true;
                break;
            
            case SyntaxKind.OpenParenToken:
                stringBuilder.Append("(");
                break;

            case SyntaxKind.CloseParenToken:
                stringBuilder.Append(")");
                break;

            case SyntaxKind.CommaToken:
                stringBuilder.Append(", ");
                break;

            case SyntaxKind.DotToken:
                stringBuilder.Append(".");
                break;

            case SyntaxKind.EqualsToken:
                stringBuilder.Append(" = ");
                break;
            
            case SyntaxKind.ElementAccessExpression:
                stringBuilder.Append(token.Text);
                break;
            
            default:
                if (needsSpaceBefore(token) && !atLineStart && stringBuilder.Length > 0) {
                    char last = stringBuilder[^1];
                    if (!char.IsWhiteSpace(last) && last != '(' && last != '.' && last != ',')
                        stringBuilder.Append(' ');
                }

                stringBuilder.Append(token.Text);
                atLineStart = false;
                break;
        }
    }

    private bool needsSpaceBefore(SyntaxToken token) {
        if (token.IsKind(SyntaxKind.IdentifierToken)) return true;
        if (SyntaxFacts.IsKeywordKind(token.Kind())) return true;
        if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.NumericLiteralToken)) return true;
        if (token.IsKind(SyntaxKind.ThisKeyword) || token.IsKind(SyntaxKind.BaseKeyword)) return true;
        return false;
    }

}
