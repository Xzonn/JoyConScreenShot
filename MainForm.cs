using HidLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JoyConScreenShot
{
    public partial class MainForm : Form
    {
        public enum VendorId
        {
            Nintendo = 0x057e,
        }

        public enum ProductId
        {
            JoyCon_Left = 0x2006,
            // JoyCon_Right = 0x2007,
            // Pro_Controller = 0x2009,
        }

        public delegate void ReadHandlerDelegate(HidReport report);

        public MainForm()
        {
            InitializeComponent();
            ScanTimer.Start();
        }

        readonly List<Device> Devices = new List<Device>();

        public void AddLog(string str, Color? color = null)
        {
            ConsoleBox.Select(ConsoleBox.Text.Length, 0);
            if (color != null)
            {
                ConsoleBox.SelectionColor = (Color)color;
            }
            ConsoleBox.SelectedText = $"[{DateTime.Now}] {str}{Environment.NewLine}";
        }

        private void ScanDevices(object sender, EventArgs e)
        {
            HidFastReadEnumerator enumerator = new HidFastReadEnumerator();
            foreach (HidFastReadDevice device in enumerator.Enumerate(vendorId: (int)VendorId.Nintendo, productIds: Enum.GetValues(typeof(ProductId)).Cast<int>().ToArray()))
            {
                if (Devices.All(x => x.DevicePath != device.DevicePath))
                {
                    device.OpenDevice();
                    device.MonitorDeviceEvents = true;
                    AddLog($"Device added: {(ProductId)device.Attributes.ProductId}.");

                    Device newDevice = new Device(device, this);
                    Devices.Add(newDevice);
                    newDevice.ReadStart();
                }
            }
        }
    }
}
