using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TicketPOS
{
    /// <summary>
    /// Interaction logic for QuickSaleWindow.xaml
    /// </summary>
    public partial class QuickSaleWindow : Window
    {
        public string Item = "";
        public double Quantity = 0;

        public QuickSaleWindow()
        {
            InitializeComponent();
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            var value = txtQuantity.Text;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            float numericValue = 0f;

            if (!float.TryParse(value + "0", out numericValue))
            {
                txtQuantity.Text = "0";
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var value = txtQuantity.Text.Trim();
            if (comboItem.SelectionBoxItem == null)
            {
                MessageBox.Show("Valid Item Required");
                return;
            }
            double numericValue = 0;

            if (!double.TryParse(value, out numericValue) || numericValue <= 0)
            {
                MessageBox.Show("Valid Quantity Required");
                return;
            }

            Item = comboItem.SelectionBoxItem.ToString();
            Quantity = numericValue;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
