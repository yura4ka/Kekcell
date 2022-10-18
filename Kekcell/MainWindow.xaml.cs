using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Kekcell
{
    public partial class MainWindow : Window
    {
        private const int MIN_ROWS = 35;
        private const int MIN_COLUMNS = 30;
        private readonly Table _table;
        private readonly Help _helpWindow = new();
        private string _filePath = "";
        private bool hasChanges = false;
        public MainWindow(string filePath = "")
        {
            InitializeComponent();
            _filePath = filePath;
            double HEIGHT = SystemParameters.PrimaryScreenHeight - 50;
            double WIDTH = SystemParameters.PrimaryScreenWidth;

            Grid.RowHeight = HEIGHT / MIN_ROWS;
            Grid.ColumnWidth = WIDTH / MIN_COLUMNS;

            if (filePath == "")
            {
                _table = new(MIN_ROWS, MIN_COLUMNS);
            }
            else
            {
                XDocument doc = XDocument.Load(filePath);
                XElement root = doc.Element(XmlTableNames.Root);
                int rows = int.Parse(root.Attribute(XmlTableNames.Rows).Value);
                int columns = int.Parse(root.Attribute(XmlTableNames.Columns).Value);
                _table = new(root, rows, columns);
            }

            Grid.ItemsSource = _table.Data.DefaultView;
        }

        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void Grid_SelectedCellChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Grid.SelectedCells.Count == 0)
                return;
            var first = Grid.SelectedCells[0];
            var row = (DataGridRow)Grid.ItemContainerGenerator.ContainerFromItem(first.Item);
            CurrentCellName.Text = $"{first.Column.Header}{row.Header}";
            ExpressionInput.Text = _table.GetExpression(row.GetIndex(), first.Column.DisplayIndex);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid.CurrentCell = new DataGridCellInfo(Grid.Items[0], Grid.Columns[0]);
            Grid.SelectedCells.Add(Grid.CurrentCell);
        }

        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int row = e.Row.GetIndex();
            int column = e.Column.DisplayIndex;
            string value = (string)_table.Data.Rows[row][column];
            string newValue = ((TextBox)e.EditingElement).Text;
            if (value != newValue)
            {
                hasChanges = true;
                try
                {
                    _table.UpdateCellValue(row, column, newValue);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while parsing {newValue} at {row},{column}. {ex.Message}\n");
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        private void ExpressionInput_LostFocus(object sender, RoutedEventArgs e)
        {
            var expression = ExpressionInput.Text;

            var current = Grid.SelectedCells[0];
            var row = (DataGridRow)Grid.ItemContainerGenerator.ContainerFromItem(current.Item);
            var column = current.Column;

            int rowNumber = row.GetIndex();
            int columnNumber = column.DisplayIndex;

            if (_table.IsSameExpression(rowNumber, columnNumber, expression))
                return;

            try
            {
                hasChanges = true;
                if (expression == "")
                {
                    _table.UpdateCellValue(rowNumber, columnNumber, "");
                    return;
                }
                _table.UpdateCellExpression(rowNumber, columnNumber, expression);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while parsing {expression}. {ex.Message}\n");
                ShowErrorMessage(ex.Message);
            }
        }

        private void ShowErrorMessage(string error)
        {
            MessageBox.Show(error, "Kekcell", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            if (_helpWindow.IsVisible)
                _helpWindow.Activate();
            else
                _helpWindow.Show();
        }

        private MessageBoxResult AskIfClosing()
        {
            return MessageBox.Show(
                    "Ви впевненні, що хочете закрити таблицю. Незбережені дані будуть видалені",
                    "Kekcell",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (hasChanges)
            {
                var result = AskIfClosing();
                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
            _helpWindow.Close();
        }

        private void EditSize_Click(object sender, RoutedEventArgs e)
        {
            var sizeWindow = new TableSizeWindow(_table.Rows, _table.Columns);

            if (sizeWindow.ShowDialog() == true)
            {
                try
                {
                    if (!_table.ChangeSize(sizeWindow.Rows, sizeWindow.Columns))
                        MessageBox.Show(
                            "При зменшенні таблиці деякі вирази стали не вірні!",
                            "Kekcell",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    Grid.ItemsSource = _table.Data.DefaultView;
                    Grid.Items.Refresh();
                    hasChanges = true;

                    (int row, int column) = NameGenerator.CellNameToPosition(CurrentCellName.Text);
                    Grid.CurrentCell = new DataGridCellInfo(
                        Grid.Items[Math.Min(row, _table.Rows - 1)],
                        Grid.Columns[Math.Min(column, _table.Columns - 1)]);
                    Grid.UnselectAll();
                    Grid.SelectedCells.Add(Grid.CurrentCell);

                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_filePath == "")
                _filePath = FileDialogs.Save();
            if (_filePath == "")
                return;

            _table.GetTableXDocument().Save(_filePath);
            hasChanges = false;
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            string path = FileDialogs.Save();
            if (path == "")
                return;

            _table.GetTableXDocument().Save(path);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = FileDialogs.Open();
                if (filePath != "")
                {
                    if (hasChanges)
                    {
                        if (AskIfClosing() == MessageBoxResult.Cancel)
                            return;
                        else
                            hasChanges = false;
                    }
                    var mw = new MainWindow(filePath);
                    Close();
                    mw.Show();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, ex.Message);
                MessageBox.Show(
                    "Помилка при відкритті файлу",
                    "Kekcell",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (hasChanges)
            {
                if (AskIfClosing() == MessageBoxResult.Cancel)
                    return;
                else
                    hasChanges = false;
            }

            var mw = new MainWindow();
            Close();
            mw.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
