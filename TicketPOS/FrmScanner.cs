using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Camera_NET;
using FastMember;


namespace TicketPOS
{
    public partial class FrmScanner : Form
    {

        private bool _isLoaded = false;
        private CameraChoice _cameraChoice = new CameraChoice();

        private Bitmap _latestFrame;
        private Timer _timer;
        private ZXing.BarcodeReader _reader;

        private Action<string> _success;
        private Action _cancel;

        private bool _isScanTicket = false;

        public FrmScanner(Action<string> success, Action cancel)
        {
            _success = success;
            _cancel = cancel;
            InitializeComponent();
        }

        public FrmScanner(Action<string> success, Action cancel, bool isScanTicket)
        {
            _success = success;
            _cancel = cancel;
            _isScanTicket = isScanTicket;
            InitializeComponent();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            StopCapturing();
            _cancel();
            Close();
        }

        private void FrmScanner_Load(object sender, EventArgs e)
        {
            _cameraChoice.UpdateDeviceList();
            foreach (var cam in _cameraChoice.Devices)
            {
                comboBox.Items.Add(cam.Name);
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
                if (comboBox.Items.Count > 1)
                {
                    foreach (var item in comboBox.Items)
                    {
                        if (item.ToString().ToUpper().Contains("REAR") || item.ToString().ToUpper().Contains("LIFECAM"))
                        {
                            comboBox.SelectedIndex = comboBox.Items.IndexOf(item);
                            break;
                        }
                    }
                }
            }
            StartCapturing();
            _isLoaded = true;
        }

        private void StartCapturing()
        {
            try
            {
                var moniker = _cameraChoice.Devices[comboBox.SelectedIndex].Mon;
                //cameraControl.SetCamera(_cameraChoice.Devices[comboBox.SelectedIndex].Mon, null);
                
                var resolutions = Camera.GetResolutionList(moniker);
                if (resolutions.Any())
                {
                    var resolution = resolutions[0];
                    foreach (var res in resolutions)
                    {
                        if ((res.Height * res.Width) > (resolution.Height * resolution.Width))
                        {
                            resolution = res;
                        }
                    }

                    cameraControl.SetCamera(moniker, resolution);
                }
 
                _timer = new Timer();
                _timer.Interval = 200;
                _timer.Tick += _timer_Tick;
                _reader = new ZXing.BarcodeReader();
                _reader.Options.TryHarder = true;
                _timer.Start();
            }
            catch (Exception ex)
            {
                comboBox.Text = "Select A Camera";
                MessageBox.Show(ex.Message);
            }
        }
        private void StopCapturing()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            cameraControl.CloseCamera();

            _timer = null;
            cameraControl = null;
            _reader = null;
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoaded)
            {
                StopCapturing();
                StartCapturing();
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            _latestFrame = cameraControl.SnapshotOutputImage();

            if (_latestFrame == null) { return; }
            var results = _reader.Decode(_latestFrame);
            if (results != null)
            {
                _timer.Stop();
                StopCapturing();
                
                Close();
                if (_isScanTicket)
                {
                    _success(results.Text);
                }
                else if (MessageBox.Show("Cancel Order Number: " + results.Text, "Confirm", MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
                {
                    _success(results.Text);
                }
                else
                {
                    _cancel();
                }
            }
        }
    }
}
