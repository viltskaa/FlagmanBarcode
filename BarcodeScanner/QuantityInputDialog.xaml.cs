using System.Windows;

namespace BarcodeScanner
{
    public partial class QuantityInputDialog : Window
    {
        public int Quantity { get; private set; }
        private int Min { get; }
        private int Max { get; }

        public QuantityInputDialog(int min, int max)
        {
            InitializeComponent();
            Min = min;
            Max = max;
            QuantityInputTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityInputTextBox.Text, out int quantity) && quantity >= Min && quantity <= Max)
            {
                Quantity = quantity;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show($"Введите значение в диапазоне от {Min} до {Max}.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
