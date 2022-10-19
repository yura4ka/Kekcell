using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Kekcell
{
    class Cell
    {
        private int _row;
        private int _column;
        private readonly string _name;
        private Dictionary<string, Cell> _references;
        private Dictionary<string, Cell> _dependencies;
        public string Expression { get; private set; }
        public string Value { get; private set; }

        public delegate void Notify(int row, int column, string value);
        public static event Notify ValueRecalculated;
        public int Row
        {
            get { return _row; }
            set
            {
                if (value < 0)
                    throw new IndexOutOfRangeException();
                _row = value;
            }
        }
        public int Column
        {
            get { return _column; }
            set
            {
                if (value < 0)
                    throw new IndexOutOfRangeException();
                _column = value;
            }
        }

        public Cell(int row, int column, string name, string text = "")
        {
            Row = row;
            Column = column;
            _name = name;
            Expression = text;
            Value = text;
            _references = new Dictionary<string, Cell>();
            _dependencies = new Dictionary<string, Cell>();
        }

        public Cell(int row, int column, string name, Cell from)
        {
            Row = row;
            Column = column;
            _name = name;
            Expression = from.Expression;
            Value = from.Value;
            _references = from._references.ToDictionary(x => x.Key, x => x.Value);
            _dependencies = from._dependencies.ToDictionary(x => x.Key, x => x.Value);
        }

        public void ChangeValue(string value)
        {
            Expression = value;
            Value = value;

            ClearDependencies();
            RecalculeteReferences();
        }

        public void ChangeExpression(string expression)
        {
            var dependecies = _dependencies.ToDictionary(x => x.Key, x => x.Value);
            ClearDependencies();
            try
            {
                string value = KekcellCalculator.Evaluate(expression, _name).ToString();
                Expression = expression;
                Value = value;
                ValueRecalculated?.Invoke(_row, _column, value);
                RecalculeteReferences();
            }
            catch
            {
                RestoreDependencies(dependecies);
                Expression = expression;
                throw;
            }
        }

        public void AddDependency(string name, Cell cell)
        {
            if (!_dependencies.ContainsKey(name))
                _dependencies.Add(name, cell);
        }

        public void AddReference(string name, Cell cell)
        {
            if (!_references.ContainsKey(name))
                _references.Add(name, cell);
        }

        private void ClearDependencies()
        {
            foreach (var d in _dependencies)
            {
                d.Value._references.Remove(_name);
            }
            _dependencies.Clear();
        }

        private void RecalculeteReferences()
        {
            foreach (var r in _references)
            {
                r.Value.RecalculateValue();
            }
        }

        private void RecalculateValue()
        {
            try
            {
                string newValue = KekcellCalculator.Evaluate(Expression, _name).ToString();
                Value = newValue;
                ValueRecalculated?.Invoke(_row, _column, newValue);
            }
            catch
            {
                SetErrorValue();
                throw;
            }
            RecalculeteReferences();
        }

        private void RestoreDependencies(Dictionary<string, Cell> dependencies)
        {
            if (_dependencies.Count == 0)
            {
                _dependencies = dependencies;
                foreach (var d in dependencies)
                {
                    d.Value.AddReference(_name, this);
                }
            }
        }

        private void SetErrorValue()
        {
            Value = "";
            ValueRecalculated?.Invoke(_row, _column, "");
            foreach (var r in _references)
            {
                r.Value.Value = "";
                ValueRecalculated?.Invoke(r.Value._row, r.Value._column, "");
            }
        }

        public bool CheckReference(string name)
        {
            if (_references.ContainsKey(name))
                return true;

            foreach (var r in _references)
            {
                if (r.Value.CheckReference(name))
                    return true;
            }

            return false;
        }

        public bool RemakeDependencies(int maxRow, int maxColumn, List<Cell> cells)
        {
            bool isDependenciesError = false;
            bool isReferencesError = false;

            var newDependencies = new Dictionary<string, Cell>();
            var newReferences = new Dictionary<string, Cell>();

            foreach (var d in _dependencies)
            {
                var name = d.Key;
                (int row, int column) = NameGenerator.CellNameToPosition(name);
                if (row >= maxRow || column >= maxColumn || name == _name)
                    isDependenciesError = true;
                else
                    newDependencies.Add(d.Key, cells[row * maxColumn + column]);
            }

            if (isDependenciesError)
            {
                Expression = "";
                Value = "";
            }

            foreach (var r in _references)
            {
                var name = r.Key;
                (int row, int column) = NameGenerator.CellNameToPosition(name);
                if (row >= maxRow || column >= maxColumn || name == _name)
                    isReferencesError = true;
                else
                    newReferences.Add(r.Key, cells[row * maxColumn + column]);
            }

            _dependencies = newDependencies;
            _references = newReferences;

            if (isDependenciesError)
                RecalculeteReferences();

            return isDependenciesError || isReferencesError;
        }

        public bool IsEmpty()
        {
            return Value == ""
                && Expression == ""
                && _references.Count == 0
                && _dependencies.Count == 0;
        }

        public XElement GetXElement()
        {
            var cell = new XElement(XmlTableNames.Cell);
            cell.Add(new XAttribute(XmlTableNames.Row, _row));
            cell.Add(new XAttribute(XmlTableNames.Column, _column));
            cell.Add(new XAttribute(XmlTableNames.Expression, Expression));
            cell.Add(new XAttribute(XmlTableNames.Value, Value));

            var dependencies = new XElement(XmlTableNames.Dependencies);
            foreach (var d in _dependencies)
            {
                var dependency = new XElement(XmlTableNames.Dependency);
                dependency.Add(new XAttribute(XmlTableNames.Key, d.Key));
                dependencies.Add(dependency);
            }

            var references = new XElement(XmlTableNames.References);
            foreach (var r in _references)
            {
                var reference = new XElement(XmlTableNames.Reference);
                reference.Add(new XAttribute(XmlTableNames.Key, r.Key));
                references.Add(reference);
            }

            cell.Add(dependencies);
            cell.Add(references);

            return cell;
        }

        public string SetFromXElement(XElement element, int maxRow, int maxColumn, List<Cell> cells)
        {
            Expression = element.Attribute(XmlTableNames.Expression).Value;
            Value = element.Attribute(XmlTableNames.Value).Value;

            foreach (XElement d in element
                .Element(XmlTableNames.Dependencies).Elements(XmlTableNames.Dependency))
            {
                string name = d.Attribute(XmlTableNames.Key).Value;
                (int row, int column) = NameGenerator.CellNameToPosition(name);
                if (row >= maxRow || column >= maxColumn || name == _name)
                    throw new ArgumentException();
                _dependencies.Add(name, cells[row * maxColumn + column]);
            }

            foreach (XElement r in element
                .Element(XmlTableNames.References).Elements(XmlTableNames.Reference))
            {
                string name = r.Attribute(XmlTableNames.Key).Value;
                (int row, int column) = NameGenerator.CellNameToPosition(name);
                if (row >= maxRow || column >= maxColumn || name == _name)
                    throw new ArgumentException();
                _references.Add(name, cells[row * maxColumn + column]);
            }

            return Value;
        }
    }
}
