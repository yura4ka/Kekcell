using System;

namespace Kekcell
{
    public class CellException : Exception
    {
        private const string HEADER = "Помилка при обчисленні";
        public CellException(string cell, string msg) : base($"{HEADER} {cell}! {msg}") { }
    }

    public class SameCellReferenceException : CellException
    {
        public SameCellReferenceException(string cell)
            : base(cell, $"Комірка не може посилатися на себе.") { }
    }

    public class CircularReferenceException : CellException
    {
        public CircularReferenceException(string cell1, string cell2)
            : base(cell1, $"Циклічна залежність із {cell2}.") { }
    }

    public class WrongValueException : CellException
    {
        public WrongValueException(string cell1, string cell2)
            : base(cell1, $"{cell2} може містити тільки число.") { }
    }

    public class SyntaxErrorException : CellException
    {
        public SyntaxErrorException(string cell)
            : base(cell, $"Вираз записан невірно.") { }
    }

    public class CellOverflowException : CellException
    {
        public CellOverflowException(string cell)
            : base(cell, $"Результуючий вираз занадто великий.") { }
    }

    public class WrongArgumentException : CellException
    {
        public WrongArgumentException(string cell, string arg)
            : base(cell, $"Функція невизначена при значенні аргументу {arg}.") { }
    }

    public class CellZeroDivisionException : CellException
    {
        public CellZeroDivisionException(string cell)
            : base(cell, $"Ділення на нуль заборонено.") { }
    }

    public class CellReferencesErrorException : Exception
    {
        public CellReferencesErrorException(string cell)
            : base($"Помилка, при обчисленні комірок, що посилаються на {cell}.") { }
    }
}
