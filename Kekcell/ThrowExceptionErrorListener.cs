using Antlr4.Runtime;

namespace Kekcell
{
    class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        private readonly string _cell;
        public ThrowExceptionErrorListener(string cell)
        {
            _cell = cell;
        }

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new SyntaxErrorException(_cell);
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new SyntaxErrorException(_cell);
        }
    }
}
