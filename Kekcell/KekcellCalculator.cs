using Antlr4.Runtime;
using System;

namespace Kekcell
{
    static class KekcellCalculator
    {
        public static Table TableData { get; set; }
        public static double Evaluate(string expression, string currentCell)
        {
            if (TableData == null)
                throw new ArgumentNullException();

            var lexer = new KekcellLexer(new AntlrInputStream(expression));
            var errorListener = new ThrowExceptionErrorListener(currentCell);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);
            var tokens = new CommonTokenStream(lexer);
            var parser = new KekcellParser(tokens);
            parser.AddErrorListener(errorListener);
            var tree = parser.compileUnit();
            var visitor = new KekcellVisitor(TableData, currentCell);
            return visitor.Visit(tree);
        }
    }
}
