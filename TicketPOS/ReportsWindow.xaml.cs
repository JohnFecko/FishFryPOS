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
    enum ReportType
    {
        SalesByDate,
        SalesByEventDate,
        TotalsBySeatingTime,
        WalkUpBySeatingTime,
        UnfulfilledOrders
    }

    public partial class ReportsWindow : Window
    {
        private const string Salesbydate = "Sales By Date";
        private const string Salesbyeventdate = "Sales By Event Date";
        private const string Totalsbyseatingtime = "Totals By Seating Time";
        private const string Walkupsbyseatingtime = "Walk Ups By Seating Time";
        private const string Unfulfilledorders = "Unfulfilled Orders";
        private const string ReceiptsByDate = "Receipts By Date";



        private SalesEntities _context;
        private List<string> _reportsList;


        public ReportsWindow(SalesEntities context)
        {
            InitializeComponent();
            _context = context;
            _reportsList = new List<string>()
            {
                ReceiptsByDate,
                Salesbydate,
                Salesbyeventdate,
                Totalsbyseatingtime,
                Unfulfilledorders,
                Walkupsbyseatingtime
            };
        }

        private string GetReportText(string reportText)
        {
            var results = reportText + Environment.NewLine;
            try
            {
                var ticketSales = _context.TicketSales.Where(x => x.IsValid).ToList();
                var ticketDetails = _context.TicketDetails.ToList();
                var eventDates = _context.EventDates.ToList();
                var saleItems = _context.SaleItems.ToList();

                var dates = FilterReportDates(reportText, ticketSales, eventDates);
                foreach (var date in dates)
                {
                    results += date.ToString(DateFormat(reportText)) + Environment.NewLine;
                    var sales = FilterReportItems(reportText, ticketSales, date);
                    var details = ticketDetails.Where(x => sales.Any(y => y.OrderGuid == x.OrderGuid));
                    if (reportText == ReceiptsByDate)
                    {
                        var paymentTypes = sales.Select(x => x.PaymentType).Distinct().ToList();
                        double total = 0;
                        foreach (var type in paymentTypes)
                        {
                            var typeText = type;
                            if (string.IsNullOrEmpty(typeText))
                            {
                                typeText = "OTHER";
                            }
                            var subTotal = sales
                                .Join(details, s => s.OrderGuid, d => d.OrderGuid, (s, d) => new { s, d })
                                .Where(x => x.s.PaymentType == type && x.s.IsValid)
                                .Sum(x => x.d.ItemPrice);
                            total += subTotal;
                            results += string.Format("{0}: {1}{2}", typeText, subTotal.ToString("C"),Environment.NewLine);
                        }
                        results += string.Format("{0}: {1}{2}", "TOTAL", total.ToString("C"), Environment.NewLine);
                    }
                    else
                    {
                        foreach (var item in saleItems)
                        {
                            results += string.Format("{0}: {1}{2}", item.Name,
                                details.Count(x => x.ItemName == item.Name),
                                Environment.NewLine);
                        }
                    }

                    results += Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return results;
        }

        private string DateFormat(string reportText)
        {
            var results = "MM/dd/yyyy HH:mm";
            switch (reportText)
            {
                case ReceiptsByDate:
                case Salesbydate:
                    results = "MM/dd/yyyy";
                    break;
            }

            return results;
        }

        private List<TicketSale> FilterReportItems(string reportText, List<TicketSale> sales, DateTime date)
        {
            var results = new List<TicketSale>();
            switch (reportText)
            {
                case ReceiptsByDate:
                case Salesbydate:
                    results = sales.Where(x => x.CreateDate.Date == date.Date).ToList();
                    break;
                case Salesbyeventdate:
                case Totalsbyseatingtime:
                    results = sales.Where(x => x.EventDate >= date && x.EventDate <= date.AddMinutes(1)).ToList();
                    break;
                case Unfulfilledorders:
                    results = sales.Where(x => x.EventDate == date && !x.IsFulfilled).ToList();
                    break;
                case Walkupsbyseatingtime:
                    results = sales.Where(x => x.EventDate >= date && x.EventDate <= date.AddMinutes(1) && x.IsWalkUp).ToList();
                    break;
            }

            return results;
        }

        private List<DateTime> FilterReportDates(string reportText, List<TicketSale> sales, List<EventDate> eventDates)
        {
            var results = new List<DateTime>();
            switch (reportText)
            {
                case ReceiptsByDate:
                case Salesbydate:
                    results = sales
                        .Where(x => x.CreateDate >= dateStart.SelectedDate.Value && x.CreateDate <= dateEnd.SelectedDate.Value.AddDays(1))
                        .Select(x => x.CreateDate.Date).OrderBy(x => x).Distinct().ToList();
                    break;
                case Salesbyeventdate:
                    results = sales
                        .Where(x => x.EventDate >= dateStart.SelectedDate.Value && x.EventDate <= dateEnd.SelectedDate.Value.AddDays(1))
                        .Select(x => x.EventDate).OrderBy(x => x).Distinct().ToList();
                    break;
                case Totalsbyseatingtime:
                case Unfulfilledorders:
                case Walkupsbyseatingtime:
                    results = eventDates.Where(x => x.Date >= dateStart.SelectedDate.Value.Date && x.Date <= dateEnd.SelectedDate.Value.AddDays(1))
                        .Select(x => x.Date).OrderBy(x => x).Distinct().ToList();
                    break;
            }

            return results;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            SetDatesToday();
            SetReportsList();
            txtOutput.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

        }

        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            SetDatesToday();
        }

        private void SetDatesToday()
        {
            dateEnd.SelectedDate = DateTime.Today;
            dateStart.SelectedDate = DateTime.Today;
        }

        private void SetReportsList()
        {
            comboReports.Items.Clear();
            foreach (var report in _reportsList)
            {
                comboReports.Items.Add(report);
            }
        }

        private void btnLoadReport_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(comboReports.Text))
            {
                txtOutput.Text = GetReportText(comboReports.Text);
            }
        }

        private void btnPrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(comboReports.Text))
            {
                var receiptText = GetReportText(comboReports.Text);
                txtOutput.Text = receiptText;
                var printer = new PrinterLogic();
                printer.PrintSalesReport(receiptText);
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
