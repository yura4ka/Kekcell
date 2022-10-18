using System;
using System.Data;
using System.Linq;

namespace Kekcell
{
    class KekcellVisitor : KekcellBaseVisitor<double>
    {
        private readonly Table _table;
        private readonly string _currentCell;
        public KekcellVisitor(Table table, string currentCell)
        {
            _table = table;
            _currentCell = currentCell;
        }

        private double WalkLeft(KekcellParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<KekcellParser.ExpressionContext>(0));
        }

        private double WalkRight(KekcellParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<KekcellParser.ExpressionContext>(1));
        }

        public override double VisitCompileUnit(KekcellParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(KekcellParser.NumberExprContext context)
        {
            return double.Parse(context.GetText());
        }

        public override double VisitDegreeNumberExpr(KekcellParser.DegreeNumberExprContext context)
        {
            var text = context.GetText();
            text = text.Substring(0, text.Length - 3);
            double result = double.Parse(text) / 180.0 * Math.PI;

            if (double.IsInfinity(result) || double.IsNaN(result))
                throw new CellException(_currentCell, "");
            return result;
        }

        public override double VisitCellExpr(KekcellParser.CellExprContext context)
        {
            string cellName = context.GetText();
            if (cellName == _currentCell)
                throw new SameCellReferenceException(cellName);
            if (_table.CheckCycles(_currentCell, cellName))
                throw new CircularReferenceException(_currentCell, cellName);

            (int cellRow, string cellColumn) = NameGenerator.SepareteCellName(cellName);
            string value = _table.Data.Rows[cellRow - 1].Field<string>(cellColumn);
            double result = 0;
            if (value != "" && !double.TryParse(value, out result))
                throw new WrongValueException(_currentCell, cellName);

            _table.AddCellDependency(_currentCell, cellName);
            _table.AddCellReference(cellName, _currentCell);

            return result;
        }

        public override double VisitParenthesizedExpr(KekcellParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitExponentialExpr(KekcellParser.ExponentialExprContext context)
        {
            double result = Math.Pow(WalkLeft(context), WalkRight(context));
            if (double.IsInfinity(result) || double.IsNaN(result))
                throw new CellOverflowException(_currentCell);
            return result;
        }

        public override double VisitAdditiveExpr(KekcellParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            double result = context.operatorToken.Type == KekcellParser.ADD
                ? left + right
                : left - right;
            if (double.IsInfinity(result) || double.IsNaN(result))
                throw new CellOverflowException(_currentCell);
            return result;
        }

        public override double VisitUnaryMinus(KekcellParser.UnaryMinusContext context)
        {
            double result = -Visit(context.expression());
            if (double.IsInfinity(result) || double.IsNaN(result))
                throw new CellOverflowException(_currentCell);
            return result;
        }

        public override double VisitMultiplicativeExpr(KekcellParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            double result = context.operatorToken.Type switch
            {
                KekcellParser.MULTIPLY => left * right,
                KekcellParser.DIVIDE => left / right,
                _ => left % right
            };

            if (double.IsInfinity(result) || double.IsNaN(result))
                throw right == 0
                    ? new CellZeroDivisionException(_currentCell)
                    : new CellOverflowException(_currentCell);
            return result;
        }

        public override double VisitStatisticFuncExpr(KekcellParser.StatisticFuncExprContext context)
        {
            int paramCount = context.paramlist.ChildCount - context.paramlist.ChildCount / 2;
            int i = 0;
            double[] paramlist = new double[paramCount];

            foreach (var child in context.paramlist.children.OfType<KekcellParser.ExpressionContext>())
            {
                double childValue = Visit(child);
                paramlist[i++] = childValue;
            }

            try
            {
                double result = context.operatorToken.Type switch
                {
                    KekcellParser.MAX => paramlist.Max(),
                    KekcellParser.MIN => paramlist.Min(),
                    KekcellParser.AVERAGE => paramlist.Average(),
                    KekcellParser.MEDIAN => MathFunctions.Median(paramlist),
                    KekcellParser.MODE => MathFunctions.Mode(paramlist),
                    KekcellParser.PRODUCT => MathFunctions.Product(paramlist),
                    _ => MathFunctions.Sum(paramlist),
                };

                if (double.IsInfinity(result) || double.IsNaN(result))
                    throw new CellOverflowException(_currentCell);
                return result;
            }
            catch (OverflowException)
            {
                throw new CellOverflowException(_currentCell);
            }
        }

        public override double VisitNumberFuncExpr(KekcellParser.NumberFuncExprContext context)
        {
            if (context.ChildCount == 1)
                throw new SyntaxErrorException(_currentCell);

            double value = WalkLeft(context);

            double result = context.operatorToken.Type switch
            {
                KekcellParser.CEIL => Math.Ceiling(value),
                KekcellParser.FLOOR => Math.Floor(value),
                KekcellParser.ROUND => Math.Round(value),
                KekcellParser.SIN => Math.Sin(value),
                KekcellParser.COS => Math.Cos(value),
                KekcellParser.TAN => Math.Tan(value),
                KekcellParser.COT => 1.0 / Math.Tan(value),
                KekcellParser.SINH => Math.Sinh(value),
                KekcellParser.COSH => Math.Cosh(value),
                KekcellParser.TANH => Math.Tanh(value),
                KekcellParser.COTH => 1.0 / Math.Tanh(value),
                KekcellParser.LOG2 => Math.Log(value, 2),
                KekcellParser.LOG10 => Math.Log10(value),
                KekcellParser.LOG => Math.Log(value),
                KekcellParser.EXP => Math.Exp(value),
                KekcellParser.SQRT => Math.Sqrt(value),
                _ => Math.Abs(value),
            };

            if (double.IsInfinity(result) || double.IsNaN(result))
                throw new WrongArgumentException(_currentCell, value.ToString());

            return result;
        }

        public override double VisitDegreeExpr(KekcellParser.DegreeExprContext context)
        {
            return WalkLeft(context) / 180.0 * Math.PI;
        }

        public override double VisitLogExpr(KekcellParser.LogExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            return Math.Log(right, left);
        }

        public override double VisitConstExpr(KekcellParser.ConstExprContext context)
        {
            return context.token.Type switch
            {
                KekcellParser.E => Math.E,
                _ => Math.PI,
            };
        }
    }
}
