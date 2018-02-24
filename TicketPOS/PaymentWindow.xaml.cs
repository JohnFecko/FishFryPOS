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
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private string _paymentType = "";
        public PaymentWindow()
        {
            InitializeComponent();
        }

        public string PaymentType
        {
            get { return _paymentType; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCharge_Click(object sender, RoutedEventArgs e)
        {
            _paymentType = "CHARGE";
            Close();
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            _paymentType = "CHECK";
            Close();
        }

        private void btnCash_Click(object sender, RoutedEventArgs e)
        {
            _paymentType = "CASH";
            Close();
        }
    }
}
