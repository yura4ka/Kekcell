using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;

namespace Kekcell
{
    public class Table
    {
        private List<Cell> _cells;
        public DataTable Data { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public Table(int rows, int columns)
        {
            if (rows < 1 || columns < 1)
                throw new ArgumentException();

            NameGenerator.ResetColumnName();
            Rows = rows;
            Columns = columns;
            _cells = new List<Cell>(rows * columns);
            Data = new DataTable();

            string[] names = new string[columns];
            for (int i = 0; i < columns; i++)
            {
                var columnName = NameGenerator.GenerateColumnName();
                names[i] = columnName;
                Data.Columns.Add(columnName, typeof(string));
            }

            for (int i = 0; i < rows; i++)
            {
                var row = Data.NewRow();
                for (int j = 0; j < columns; j++)
                {
                    _cells.Add(new Cell(i, j, $"{names[j]}{i + 1}"));
                    row[names[j]] = _cells[i * columns + j].Value;
                }
                Data.Rows.Add(row);
            }

            Cell.ValueRecalculated += OnCellValueUpdated;
            KekcellCalculator.TableData = this;
        }

        public Table(XElement root, int rows, int columns) : this(rows, columns)
        {
            foreach (XElement cell in root.Elements(XmlTableNames.Cell))
            {
                int row = int.Parse(cell.Attribute(XmlTableNames.Row).Value);
                int column = int.Parse(cell.Attribute(XmlTableNames.Column).Value);
                if (row >= rows || column >= columns)
                    throw new ArgumentException();
                Data.Rows[row][column] = _cells[row * columns + column].SetFromXElement(cell, rows, columns, _cells);
            }
        }

        public void UpdateCellValue(int row, int column, string value)
        {
            Data.Rows[row][column] = value;
            _cells[row * Columns + column].ChangeValue(value);
        }

        public void UpdateCellExpression(int row, int column, string expression)
        {
            _cells[row * Columns + column].ChangeExpression(expression);
        }

        public void AddCellDependency(string cell, string dependency)
        {
            (int cellRow, int cellColumn) = NameGenerator.CellNameToPosition(cell);
            (int dependencyRow, int dependencyColumn) = NameGenerator.CellNameToPosition(dependency);
            var Cell = _cells[dependencyRow * Columns + dependencyColumn];
            _cells[cellRow * Columns + cellColumn].AddDependency(dependency, Cell);
        }

        public void AddCellReference(string cell, string reference)
        {
            (int cellRow, int cellColumn) = NameGenerator.CellNameToPosition(cell);
            (int referenceRow, int referenceColumn) = NameGenerator.CellNameToPosition(reference);
            var Cell = _cells[referenceRow * Columns + referenceColumn];
            _cells[cellRow * Columns + cellColumn].AddReference(reference, Cell);
        }

        private void OnCellValueUpdated(int row, int column, string value)
        {
            Data.Rows[row][column] = value;
        }

        public bool IsSameExpression(int row, int column, string expression)
        {
            return _cells[row * Columns + column].Expression == expression;
        }

        public string GetExpression(int row, int column)
        {
            return _cells[row * Columns + column].Expression;
        }

        public bool CheckCycles(string cell1, string cell2)
        {
            (int row, int column) = NameGenerator.CellNameToPosition(cell1);
            return _cells[row * Columns + column].CheckReference(cell2);
        }

        public bool ChangeSize(int rows, int columns)
        {
            if (rows < 1 || columns < 1)
                return false;

            bool isOk = true;
            var newCells = new List<Cell>(rows * columns);
            var newData = new DataTable();

            string[] names = new string[columns];
            NameGenerator.ResetColumnName();
            for (int i = 0; i < columns; i++)
            {
                var columnName = NameGenerator.GenerateColumnName();
                names[i] = columnName;
                newData.Columns.Add(columnName, typeof(string));
            }

            for (int i = 0; i < rows; i++)
            {
                var row = newData.NewRow();
                for (int j = 0; j < columns; j++)
                {
                    if (j < Columns && i < Rows)
                    {
                        newCells.Add(new Cell(i, j, $"{names[j]}{i + 1}", _cells[i * Columns + j]));
                        row[names[j]] = Data.Rows[i][j];
                    }
                    else
                    {
                        newCells.Add(new Cell(i, j, $"{names[j]}{i + 1}"));
                        row[names[j]] = "";
                    }
                }
                newData.Rows.Add(row);
            }

			Rows = rows;
			Columns = columns;
			Data = newData;
			_cells = newCells;

			foreach (var cell in newCells)
            {
                isOk = !cell.RemakeDependencies(rows, columns, newCells) && isOk;
            }

            return isOk;
        }

        public XDocument GetTableXDocument()
        {
            var doc = new XDocument();
            var root = new XElement(XmlTableNames.Root);
            root.Add(new XAttribute(XmlTableNames.Rows, Rows));
            root.Add(new XAttribute(XmlTableNames.Columns, Columns));

            foreach (var c in _cells)
                if (!c.IsEmpty())
                    root.Add(c.GetXElement());

            doc.Add(root);
            return doc;
        }
    }

    public static class XmlTableNames
    {
        public static readonly string Root = "Table";
        public static readonly string Rows = "rows";
        public static readonly string Columns = "columns";
        public static readonly string Cell = "Cell";
        public static readonly string Row = "row";
        public static readonly string Column = "column";
        public static readonly string Expression = "expression";
        public static readonly string Value = "value";
        public static readonly string References = "References";
        public static readonly string Dependencies = "Dependencies";
        public static readonly string Key = "key";
        public static readonly string Reference = "Reference";
        public static readonly string Dependency = "Dependency";
    }
}
