using System;
using System.Text;
using System.Xml.Linq;

namespace Kekcell
{
    static class NameGenerator
    {
        private static readonly char[] _alphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private static readonly StringBuilder _name = new("");

        public static int NameToIndex(string name)
        {
            int sum = 0;
            int exp = 1;
            string alphabet = new(_alphabet);
            for (int i = 0; i < name.Length; i++)
            {
                sum += (alphabet.IndexOf(name[name.Length - 1 - i]) + 1) * exp;
                exp *= alphabet.Length;
            }
            return sum - 1;
        }

        public static string GenerateColumnName()
        {
            if (_name.ToString() == "")
            {
                _name.Append("A");
                return "A";
            }
            int reminder = 1;
            for (int i = _name.Length - 1; i >= 0; i--)
            {
                char last = (char)(_name[i] + reminder);
                reminder = last > 'Z' ? 1 : 0;
                if (last > 'Z') last = 'A';
                _name[i] = last;
            }

            if (reminder != 0) _name.Insert(0, 'A');
            return _name.ToString();
        }

        public static (int, string) SepareteCellName(string name)
        {
            int index = name.IndexOfAny("0123456789".ToCharArray());
            string rowName = name.Substring(index);
            if (!int.TryParse(rowName, out int parsedRowName))
                throw new InvalidCastException($"Name {name} is not correct");

            string columnName = name.Substring(0, index);
            return (parsedRowName, columnName);
        }

        public static (int, int) CellNameToPosition(string name)
        {
            (int row, string columnName) = SepareteCellName(name);
            int column = NameToIndex(columnName);
            return (row - 1, column);
        }

        public static void ResetColumnName()
        {
            _name.Clear();
        }
    }
}
