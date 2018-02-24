using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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
using TicketPOS.Properties;

namespace TicketPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DateTime _now;

        private DateTime _today;
        private DateTime _tomorrow;
        private string _orderNumber = "";
        private string _orderGuid = "";

        private ProgramState _programState;
        private double _orderTotal = 0;
        private List<SaleItem> _totalList = new List<SaleItem>();
        private List<DateTime> _dates = new List<DateTime>();
        private List<DateTime> _times = new List<DateTime>();

        private List<SaleItem> _orderItems = new List<SaleItem>();


        private List<OrderItem> _controls = new List<OrderItem>();
        private List<string> _namesList = new List<string>();

        private ScanState _scanState = ScanState.None;
        private float _savedTotal = 0;


        public MainWindow()
        {
            InitializeComponent();

            //_now = DateTime.Now;
            _now = DateTime.Now;
            _today = new DateTime(_now.Year, _now.Month, _now.Day, 0, 0, 0);
            _tomorrow = _today.AddDays(3);
            comboName.IsTextSearchEnabled = false;

            using (var context = new SalesEntities())
            {
                _orderItems = context.SaleItems.Where(x => x.Active).ToList();
                _dates = context.EventDates.Where(x => x.Date >= DateTime.Now).Select(x => x.Date).OrderBy(x => x).ToList();
                _times = context.EventDates.Where(x => x.Active).Select(x => x.Date).OrderBy(x => x).ToList();
                context.Database.ExecuteSqlCommand("UPDATE TicketSales Set Name=UPPER(NAME) WHERE Name<>UPPER(NAME) COLLATE Latin1_General_CS_AS ");
            }

            string[] args = Environment.GetCommandLineArgs();
            _programState = ProgramState.EventDay;
            if (args.Contains("presale"))
            {
                _programState = ProgramState.Presale;
                groupToGo.Visibility = Visibility.Collapsed;
                groupNotes.Visibility = Visibility.Collapsed;
            }

        }

        private DateTime GetEventDate()
        {
            DateTime results = DateTime.Now;
            var now = DateTime.Now;
            try
            {
                using (var context = new SalesEntities())
                {
                    var eventDates = context.EventDates.OrderBy(x => x.Date).ToList();
                    var lastDate = eventDates.FirstOrDefault(x => x.Date >= now.AddMinutes(-30) && x.Date <= now);
                    if (lastDate != null)
                    {
                        results = lastDate.Date;
                    }
                    else
                    {
                        var nextDate = eventDates.FirstOrDefault(x => x.Date >= now);
                        if (nextDate != null)
                        {
                            results = nextDate.Date;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return results;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SubmitOrder(false);
        }

        private void ResetScreen()
        {
            try
            {
                _orderGuid = "";
                _orderNumber = "";
                _savedTotal = 0;
                chkToGo.IsChecked = false;
                _orderNumber = "";
                _orderGuid = "";
                txtName.Text = "";
                txtTicket.Text = "";
                comboTimes.SelectedIndex = -1;
                comboDates.SelectedIndex = -1;
                comboName.SelectedIndex = -1;
                comboName.Text = "";
                txtNotes.Text = "";
                foreach (var item in _controls)
                {
                    item.Reset();
                }
                _totalList.Clear();

                LoadNameDropDown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_programState == ProgramState.EventDay)
            {
                txtName.Visibility = Visibility.Collapsed;
                groupTimes.Visibility = Visibility.Collapsed;
            }
            else if (_programState == ProgramState.Presale)
            {
                comboName.Visibility = Visibility.Collapsed;
                groupTicket.Visibility = Visibility.Collapsed;
            }
            if (groupTimes.IsVisible)
            {
                var distinctDates = new List<DateTime>();


                foreach (var date in _dates)
                {
                    if (!distinctDates.Contains(date.Date))
                    {
                        distinctDates.Add(date.Date);
                    }
                }

                foreach (var date in distinctDates)
                {
                    comboDates.Items.Add(date);
                }
            }

            foreach (var item in _orderItems)
            {
                var orderItem = new OrderItem(item, UpdateTotal);
                orderItem.Width = panelItems.Width;
                panelItems.Children.Add(orderItem);
                _controls.Add(orderItem);
            }
            WindowState = WindowState.Maximized;
            if (_programState == ProgramState.EventDay)
            {
                btnCancel.Content = "Clear";
                //btnPrintReport.Visibility = Visibility.Collapsed;
            }
            if (!Settings.Default.ShowReports)
            {
                btnPrintReport.Visibility = Visibility.Collapsed;
            }


            LoadNameDropDown();
        }


        private void LoadNameDropDown()
        {
            var names = new List<string>();
            var sales = new List<TicketSale>();
            using (var context = new SalesEntities())
            {

                sales =
                    context.TicketSales
                    .Where(x => x.EventDate >= _today && x.EventDate <= _tomorrow && !x.IsFulfilled && x.IsValid)
                        .Distinct()
                        .OrderBy(x => x.Name)
                        .ToList();

            }
            _namesList.Clear();
            foreach (var sale in sales)
            {
                var text = SaleToDropdownText(sale);
                _namesList.Add(text);
            }
            FilterNamesDropDown(_namesList);
        }

        private string SaleToDropdownText(TicketSale sale)
        {
            return string.Format("{0}     ({1})", sale.Name, sale.OrderNumber.Substring(sale.OrderNumber.Length - 4));
        }

        private void FilterNamesDropDown(List<string> names)
        {
            comboName.Items.Clear();
            foreach (var name in names)
            {
                comboName.Items.Add(name);
            }
        }

        public void UpdateTotal(SaleItem saleItem, int qty)
        {
            if (_totalList.Count(x => x.Name == saleItem.Name) < qty)
            {
                while (_totalList.Count(x => x.Name == saleItem.Name) < qty)
                {
                    _totalList.Add(saleItem);
                }
            }
            else if (_totalList.Count(x => x.Name == saleItem.Name) > qty)
            {
                while (_totalList.Count(x => x.Name == saleItem.Name) > qty)
                {
                    _totalList.Remove(_totalList.FirstOrDefault(x => x.Name == saleItem.Name));
                }
            }

            double total = _totalList.Sum(x => x.Price);
            _orderTotal = total;
            txtTotal.Text = _orderTotal.ToString("C");
        }

        private void comboDates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var combo = (ComboBox)sender;
                if (combo.SelectedIndex == -1) { return; }
                var selectedItem = (DateTime)combo.SelectedItem;
                var date = selectedItem.Date;


                comboTimes.Items.Clear();
                foreach (var time in _times.Where(x => x.Date == date))
                {
                    comboTimes.Items.Add(time);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_programState == ProgramState.Presale)
            {
                _orderGuid = "";
                _orderNumber = "";
                _savedTotal = 0;
                _scanState = ScanState.CancelOrder;
                var frm = new FrmScanner(ScanSuccess, ScanFailure);
                frm.ShowDialog();
                frm = null;
            }
            else
            {
                ResetScreen();
            }
        }


        public void CancelOrder(string orderNumber)
        {
            try
            {
                using (var context = new SalesEntities())
                {
                    var orders = context.TicketSales.Where(x => x.OrderNumber == orderNumber);
                    foreach (var order in orders)
                    {
                        order.IsValid = false;
                    }
                    context.SaveChanges();
                    MessageBox.Show("Order " + orderNumber + " cancelled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ScanSuccess(string text)
        {
            if (_scanState == ScanState.None)
            {
                return;
            }

            if (_scanState == ScanState.CancelOrder)
            {
                CancelOrder(text);
            }
            if (_scanState == ScanState.ScanTicket)
            {
                LoadDataFromOrderNumber(text);
            }


            _scanState = ScanState.None;
        }

        public void ScanFailure() { }

        private void btnPrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new SalesEntities())
                {
                    var salesItems = context.SaleItems.ToList();
                    var ticketSales = context.TicketSales.ToList();
                    var ticketDetails = context.TicketDetails.ToList();
                    var eventDates = context.EventDates.ToList();

                    var backup = new Backup()
                    {
                        EventDates = eventDates,
                        SaleItems = salesItems,
                        TicketDetails = ticketDetails,
                        TicketSales = ticketSales
                    };
                    BackupData(backup);

                    var reportsWindow = new ReportsWindow(context);
                    reportsWindow.ShowDialog();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BackupData(Backup backup)
        {
            try
            {
                var directory = Settings.Default.BackupLocationBase + "\\Backup\\";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = Serialize(backup);
                var jsonFile = directory + "Backup.txt";
                WriteFile(jsonFile, json);

                var salesCSVFile = directory + "Sales.csv";
                WriteFile(salesCSVFile, GetSalesCSV(backup.TicketSales));


                var detailsCSVFile = directory + "Details.csv";
                WriteFile(detailsCSVFile, GetDetailsCSV(backup.TicketDetails));

                //var salesByDateFile = directory + "SalesByDate.txt";
                //var salesByDate = SalesByDate(backup.SaleItems, backup.TicketSales, backup.TicketDetails);
                //WriteFile(salesByDateFile, salesByDate);

                //var salesByEventDateFile = directory + "SalesByEventDate.txt";
                //var salesByEventDate = SalesByEventDate(backup.SaleItems, backup.TicketSales, backup.TicketDetails);
                //WriteFile(salesByEventDateFile, salesByEventDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void WriteFile(string fileName, string text)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            File.WriteAllText(fileName, text);
        }

        public string GetDetailsCSV(List<TicketDetail> details)
        {
            var results = TicketDetail.GetTitlesCSV();
            foreach (var detail in details)
            {
                results += detail.ToCSV();
            }
            return results;
        }

        public string GetSalesCSV(List<TicketSale> sales)
        {
            var results = TicketSale.GetTitlesCSV();
            foreach (var sale in sales)
            {
                results += sale.ToCSV();
            }
            return results;
        }

        public string Serialize(object objectToSerialize)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(objectToSerialize.GetType());
                serializer.WriteObject(ms, objectToSerialize);
                ms.Position = 0;

                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }



        private void comboName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboName.SelectedValue == null || string.IsNullOrEmpty(comboName.SelectedValue.ToString()))
            {
                return;
            }

            if (comboName.SelectedIndex < 0)
            {
                return;
            }

            var text = comboName.SelectedValue.ToString();
            var orderNumber = "";
            using (var context = new SalesEntities())
            {
                var sale = context.TicketSales.ToList()
                    .FirstOrDefault(
                        x =>
                            x.EventDate >= _today && x.EventDate <= _tomorrow && text == SaleToDropdownText(x) && x.IsValid &&
                            !x.IsFulfilled);


                if (sale != null)
                {
                    orderNumber = sale.OrderNumber;
                }

            }
            LoadDataFromOrderNumber(orderNumber);
        }


        private void comboName_KeyUp(object sender, KeyEventArgs e)
        {
            var text = comboName.Text.ToUpper();
            if (string.IsNullOrEmpty(text))
            {
                FilterNamesDropDown(_namesList);
            }
            else
            {
                var names = _namesList.Where(x => x.Contains(text)).ToList();
                FilterNamesDropDown(names);

                comboName.IsDropDownOpen = names.Any();
            }
            comboName.Text = text;
            comboName.SetCaret();
        }



        private void LoadDataFromOrderNumber(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber)) { return; }

            using (var context = new SalesEntities())
            {
                var sale = context.TicketSales
                    .FirstOrDefault(x => x.OrderNumber == orderNumber);
                if (sale != null)
                {
                    if (sale.IsFulfilled)
                    {
                        if (
                            MessageBox.Show("Order has already been processed." + Environment.NewLine + "Reload?",
                                "Reload?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    _orderNumber = sale.OrderNumber;
                    if (string.IsNullOrEmpty(comboName.Text))
                    {
                        comboName.Text = sale.Name;
                    }

                    _orderGuid = sale.OrderGuid;
                    var orderItems = new Dictionary<string, int>();

                    var details = context.TicketDetails.Where(x => x.OrderGuid == _orderGuid).ToList();

                    foreach (var detail in details)
                    {
                        if (orderItems.ContainsKey(detail.ItemName))
                        {
                            orderItems[detail.ItemName] = orderItems[detail.ItemName] + 1;
                        }
                        else
                        {
                            orderItems.Add(detail.ItemName, 1);
                        }
                    }


                    foreach (var item in orderItems)
                    {
                        var detail = details.FirstOrDefault(x => x.ItemName == item.Key);
                        var price = detail != null ? detail.ItemPrice : 0;

                        var saleItem = new SaleItem()
                        {
                            Active = true,
                            Id = -1,
                            Name = item.Key,
                            Price = price
                        };

                        UpdateTotal(saleItem, item.Value);
                    }
                    _savedTotal = (float)details.Sum(x => x.ItemPrice);
                }
            }

            RefreshControls();
            LockControls();

        }

        private float FloatSafeParse(string s)
        {
            var r = 0f;
            float.TryParse(s.Replace("$", ""), out r);
            return r;
        }

        private void RefreshControls()
        {
            foreach (var control in _controls)
            {
                control.Reset();
                var name = control.GetItemName();
                var qty = _totalList.Count(x => x.Name == name);
                control.SetQuantity(qty);
            }
        }

        private void LockControls()
        {
            foreach (var control in _controls)
            {
                //control.Lock();
            }
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            _scanState = ScanState.ScanTicket;
            var frm = new FrmScanner(ScanSuccess, ScanFailure, true);
            frm.ShowDialog();
        }

        private void txtNotes_GotFocus(object sender, RoutedEventArgs e)
        {
            groupNotes.Height = 200;
        }

        private void txtNotes_LostFocus(object sender, RoutedEventArgs e)
        {
            groupNotes.Height = 75;
        }

        private void SubmitOrder(bool isQuickSale)
        {
            try
            {
                var shouldContinue = true;

                if (!isQuickSale)
                {
                    if (string.IsNullOrEmpty(txtName.Text.Trim()) && string.IsNullOrEmpty(comboName.Text.Trim()))
                    {
                        MessageBox.Show("Valid name required");
                        shouldContinue = false;
                    }
                    else if ((comboDates.SelectedItem == null || comboTimes.SelectedItem == null) &&
                             _programState == ProgramState.Presale)
                    {
                        MessageBox.Show("Valid date and time required");
                        shouldContinue = false;
                    }
                    else if (string.IsNullOrEmpty(txtTicket.Text) && _programState == ProgramState.EventDay &&
                             !chkToGo.IsChecked.Value)
                    {
                        MessageBox.Show("Enter valid table number");
                        shouldContinue = false;
                    }
                    else if (_orderTotal <= 0)
                    {
                        MessageBox.Show("Invalid total price");
                        shouldContinue = false;
                    }

                }
                if (shouldContinue)
                {
                    if (_savedTotal > 0 && Math.Abs(_savedTotal - FloatSafeParse(txtTotal.Text)) > 0)
                    {
                        MessageBox.Show("Total price must be " + _savedTotal.ToString("C"));
                        shouldContinue = false;
                    }
                }

                if (shouldContinue)
                {
                    if (_programState == ProgramState.EventDay && _savedTotal > 0)
                    {
                        //_orderNumber
                        using (var context = new SalesEntities())
                        {
                            var detailsToDelete = context.TicketDetails.Where(x => x.OrderGuid == _orderGuid);
                            if (detailsToDelete.Any())
                            {
                                context.TicketDetails.RemoveRange(detailsToDelete);
                            }

                            foreach (var item in _totalList)
                            {
                                var ticketDetails = new TicketDetail();
                                ticketDetails.ItemName = item.Name;
                                ticketDetails.ItemPrice = item.Price;
                                ticketDetails.OrderGuid = _orderGuid;
                                context.TicketDetails.Add(ticketDetails);
                            }
                            context.SaveChanges();
                        }
                    }

                    _savedTotal = 0;
                    var eventDate = GetEventDate();
                    var isToGo = chkToGo.IsChecked ?? false;
                    var isFulfilled = false;
                    var isWalkUp = false;
                    var tableNumber = txtTicket.Text ?? "";

                    if (_programState == ProgramState.Presale)
                    {
                        eventDate = (DateTime)comboTimes.SelectedItem;
                    }
                    else
                    {
                        isFulfilled = true;
                    }

                    if (_programState == ProgramState.EventDay && string.IsNullOrEmpty(_orderNumber))
                    {
                        isWalkUp = true;
                    }

                    var isNewOrder = false;

                    if (isWalkUp)
                    {
                        isNewOrder = true;
                    }
                    else if (_programState == ProgramState.Presale)
                    {
                        isNewOrder = true;
                    }

                    if (isNewOrder)
                    {

                        var paymentWindow = new PaymentWindow();
                        paymentWindow.ShowDialog();

                        var paymentType = paymentWindow.PaymentType;
                        if (string.IsNullOrEmpty(paymentType))
                        {
                            return;
                        }

                        var ticketSale = new TicketSale();
                        var guid = Guid.NewGuid().ToString();

                        ticketSale.Name = txtName.Text.Trim().ToUpper();
                        if (isQuickSale)
                        {
                            ticketSale.Name = "OTHER";
                        }
                        else if (_programState == ProgramState.EventDay)
                        {
                            ticketSale.Name = comboName.Text.ToUpper();
                        }
                        if (!string.IsNullOrEmpty(txtNotes.Text))
                        {
                            ticketSale.Notes = txtNotes.Text;
                        }
                        ticketSale.PaymentType = paymentType;
                        ticketSale.EventDate = eventDate;
                        ticketSale.OrderGuid = guid;
                        ticketSale.IsFulfilled = isFulfilled;
                        ticketSale.IsToGo = isToGo;
                        ticketSale.IsWalkUp = isWalkUp;
                        ticketSale.IsValid = true;
                        ticketSale.CreateDate = DateTime.Now;
                        ticketSale.OrderNumber = string.Format("{0}-{1}", DateTime.Now.Year,
                            Math.Round((DateTime.Now - new DateTime(DateTime.Now.Year, 1, 1)).TotalMilliseconds));

                        using (var context = new SalesEntities())
                        {
                            context.TicketSales.Add(ticketSale);
                            foreach (var item in _totalList)
                            {
                                var ticketDetails = new TicketDetail();
                                ticketDetails.ItemName = item.Name;
                                ticketDetails.ItemPrice = item.Price;
                                ticketDetails.OrderGuid = guid;
                                context.TicketDetails.Add(ticketDetails);
                            }
                            context.SaveChanges();

                            var printerLogic = new PrinterLogic();
                            if (!isQuickSale)
                            {
                                if (_programState == ProgramState.Presale)
                                {
                                    printerLogic.PrintPreSaleReceipt(_totalList, ticketSale.Name, ticketSale.EventDate,
                                        ticketSale.OrderNumber, string.Empty, "Men's Fellowship Copy");
                                    printerLogic.PrintPreSaleReceipt(_totalList, ticketSale.Name, ticketSale.EventDate,
                                        ticketSale.OrderNumber, string.Empty, "Customer Copy");
                                }
                                else
                                {
                                    printerLogic.PrintEventDaySaleReceipt(_totalList, ticketSale.Name,
                                        ticketSale.EventDate,
                                        ticketSale.OrderNumber, tableNumber, isToGo, ticketSale.Notes,
                                        "Men's Fellowship Copy");
                                    printerLogic.PrintEventDaySaleReceipt(_totalList, ticketSale.Name,
                                        ticketSale.EventDate,
                                        ticketSale.OrderNumber, tableNumber, isToGo, ticketSale.Notes, "Customer Copy");
                                    printerLogic.PrintKitchenReceipt(_totalList, ticketSale.Name, ticketSale.EventDate,
                                        ticketSale.OrderNumber, tableNumber, isToGo, isWalkUp, ticketSale.Notes);
                                }
                            }
                            var backup = new Backup()
                            {
                                EventDates = context.EventDates.ToList(),
                                SaleItems = context.SaleItems.ToList(),
                                TicketDetails = context.TicketDetails.ToList(),
                                TicketSales = context.TicketSales.ToList()
                            };
                            BackupData(backup);
                        }

                    }
                    else
                    {
                        using (var context = new SalesEntities())
                        {
                            var sale = context.TicketSales.FirstOrDefault(x => x.OrderNumber == _orderNumber);
                            if (sale != null)
                            {
                                sale.IsFulfilled = true;
                                sale.IsToGo = chkToGo.IsChecked ?? false;
                                if (!string.IsNullOrEmpty(txtNotes.Text))
                                {
                                    sale.Notes = txtNotes.Text;
                                }
                                context.SaveChanges();

                                var printerLogic = new PrinterLogic();
                                printerLogic.PrintEventDaySaleReceipt(_totalList, comboName.Text, DateTime.Now, _orderNumber, tableNumber, isToGo, txtNotes.Text, "Men's Fellowship Copy");
                                printerLogic.PrintKitchenReceipt(_totalList, comboName.Text, DateTime.Now, _orderNumber, tableNumber, isToGo, isWalkUp, sale.Notes);
                            }
                        }
                    }

                    if (_programState == ProgramState.EventDay && _totalList.Any(x => x.Name == "Drink" || x.Name == "Raffle"))
                    {
                        var totalDrinks = _totalList.Count(x => x.Name == "Drink");
                        var totalRaffle = _totalList.Count(x => x.Name == "Raffle");
                        if (totalDrinks > 0)
                        {
                            MessageBox.Show("Has customer received " + totalDrinks + " drink tickets?", "Drink Tickets",
                                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                        }

                        if (isNewOrder && totalRaffle > 0)
                        {
                            MessageBox.Show("Has customer received " + totalRaffle + " raffle tickets?", "Raffle Tickets",
                                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                        }
                    }

                    MessageBox.Show("Order Completed");
                    ResetScreen();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQuickSale_Click(object sender, RoutedEventArgs e)
        {
            var window = new QuickSaleWindow();
            window.ShowDialog();

            var item = window.Item;
            var quantity = window.Quantity;
            if (string.IsNullOrEmpty(item) || quantity <= 0) { return; }

            _totalList.Clear();

            _totalList.Add(new SaleItem()
            {
                Active = true,
                Name = item,
                Price = quantity
            });

            SubmitOrder(true);
        }
    }


    enum ProgramState
    {
        Presale,
        EventDay
    }

    enum ScanState
    {
        None,
        CancelOrder,
        ScanTicket
    }

    public class Backup
    {
        public List<EventDate> EventDates;
        public List<SaleItem> SaleItems;
        public List<TicketDetail> TicketDetails;
        public List<TicketSale> TicketSales;
    }

    public class CustomComboBox : ComboBox
    {
        private TextBox _textBox = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var myTextBox = GetTemplateChild("PART_EditableTextBox") as TextBox;
            if (myTextBox != null)
            {
                _textBox = myTextBox;
            }
        }

        public void SetCaret()
        {
            if (_textBox != null)
            {
                SetCaret(_textBox.Text.Length);
            }
        }

        public void SetCaret(int position)
        {
            if (_textBox != null)
            {
                _textBox.SelectionStart = position;
                _textBox.SelectionLength = 0;
            }
        }
    }
}
