using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Kekcell
{
    public partial class TableSizeWindow : Window
    {
        private readonly int _min = 1;
        private readonly int _max = 1000;
        private readonly int _originalRows;
        private readonly int _originalColumns;
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public TableSizeWindow(int rows, int columns)
        {
            InitializeComponent();
            Rows = rows;
            Columns = columns;
            _originalRows = rows;
            _originalColumns = columns;
            RowsInput.Text = Rows.ToString();
            ColumnsInput.Text = Columns.ToString();
        }

        private int IncreaseCounter(TextBox box, RepeatButton counterUp, RepeatButton counterDown)
        {
            int value = int.Parse(box.Text);
            value++;
            box.Text = value.ToString();
            counterDown.IsEnabled = true;
            if (value == _max) counterUp.IsEnabled = false;
            return value;
        }

        private int DecreaseCounter(TextBox box, RepeatButton counterUp, RepeatButton counterDown)
        {
            int value = int.Parse(box.Text);
            value--;
            box.Text = value.ToString();
            counterUp.IsEnabled = true;
            if (value == _min) counterDown.IsEnabled = false;
            return value;
        }

        private void RowCounterUp_Click(object sender, RoutedEventArgs e)
        {
            Rows = IncreaseCounter(RowsInput, ButtonRowUp, ButtonRowDown);
        }

        private void RowCounterDown_Click(object sender, RoutedEventArgs e)
        {
            Rows = DecreaseCounter(RowsInput, ButtonRowUp, ButtonRowDown);
        }

        private void ColumnCounterUp_Click(object sender, RoutedEventArgs e)
        {
            Columns = IncreaseCounter(ColumnsInput, ButtonColumnUp, ButtonColumnDown);
        }

        private void ColumnCounterDown_Click(object sender, RoutedEventArgs e)
        {
            Columns = DecreaseCounter(ColumnsInput, ButtonColumnUp, ButtonColumnDown);
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Rows == _originalRows && Columns == _originalColumns)
            {
                this.DialogResult = false;
                return;
            }

            if (Rows < _originalRows || Columns < _originalColumns)
            {
                var result = MessageBox.Show(
                    "Ви впевненні, що хочете зменшити таблицю? Деякі данні можуть бути втрачені!",
                    "Kekcell",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            this.DialogResult = true;
        }

        private int OnInputChanged(TextBox input, RepeatButton counterUp, RepeatButton counterDown)
        {
            if (!int.TryParse(input.Text, out int value))
            {
                input.Text = Rows.ToString();
                return Rows;
            }

            if (value <= _min)
            {
                input.Text = _min.ToString();
                counterDown.IsEnabled = false;
                counterUp.IsEnabled = true;
                return _min;
            }

            if (value >= _max)
            {
                input.Text = _max.ToString();
                counterDown.IsEnabled = true;
                counterUp.IsEnabled = false;
                return _max;
            }

            input.Text = value.ToString();
            return value;
        }

        private void RowsInput_LostFocus(object sender, EventArgs e)
        {
            Rows = OnInputChanged(RowsInput, ButtonRowUp, ButtonRowDown);
        }

        private void ColumnsInput_LostFocus(object sender, EventArgs e)
        {
            Columns = OnInputChanged(ColumnsInput, ButtonColumnUp, ButtonColumnDown);
        }
    }
}
