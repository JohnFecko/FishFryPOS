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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicketPOS
{
    /// <summary>
    /// Interaction logic for OrderItem.xaml
    /// </summary>
    public partial class OrderItem : UserControl
    {
        private SaleItem _saleItem;
        private Action<SaleItem, int> _updateEvent;

        public OrderItem(SaleItem saleItem, Action<SaleItem, int> updateEvent)
        {
            InitializeComponent();
            _saleItem = saleItem;
            _updateEvent = updateEvent;
            InitComboBox();
            SetComboBoxValue(0f);
        }

        private void InitComboBox()
        {
            for (float i = 0; i < 51; i++)
            {
                comboQuantity.Items.Add(i);
            }
        }

        private void SetComboBoxValue(float f)
        {
            foreach (var item in comboQuantity.Items)
            {
                if (item.ToString() == f.ToString())
                {
                    comboQuantity.SelectedItem = item;
                    break;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lblItem.Content = _saleItem.Name;
            lblPrice.Content = _saleItem.Price.ToString("C");
          
        }

        private void comboQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int f = 0;
            if (comboQuantity.SelectedValue != null)
            {
                int.TryParse(comboQuantity.SelectedValue.ToString(), out f);
            }
            _updateEvent(_saleItem, f);
            var total = f*_saleItem.Price;
            txtTotal.Text = total.ToString("C");
        }

        public void Reset()
        {
            comboQuantity.SelectedIndex = 0;
            SetLocked(false);
        }

        public double GetQuantity()
        {
            double qty = 0;
            if (comboQuantity.SelectedItem != null)
            {
                double.TryParse(comboQuantity.SelectedItem.ToString(), out qty);
            }
            return qty;
        }

        public void SetQuantity(double d)
        {
            SetComboBoxValue((float)d);
        }

        public string GetItemName()
        {
            return lblItem.Content.ToString();
        }

        public void Lock()
        {
            SetLocked(true);
        }

        private void SetLocked(bool locked)
        {
            comboQuantity.IsReadOnly = locked;
            comboQuantity.IsEditable = false;
            comboQuantity.Focusable = !locked;
            comboQuantity.IsHitTestVisible = !locked;
        }
    }
}
