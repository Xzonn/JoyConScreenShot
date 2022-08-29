using HidLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Events;
using static JoyConScreenShot.MainForm;

namespace JoyConScreenShot
{
    internal class Device
    {
        readonly HidDevice _device;
        readonly MainForm _form;
        readonly Thread _thread;
        bool IsCapturing = false;
        bool IsConnected = false;
        DateTime _lastCaptureTime = DateTime.MinValue;
        delegate void ReadHandlerDelegate(HidReport report);

        public string DevicePath
        {
            get
            {
                return _device.DevicePath;
            }
        }

        public Device(HidDevice device, MainForm form)
        {
            _device = device;
            _form = form;
            _thread = new Thread(new ThreadStart(ReadStart))
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public void ReadStart()
        {
            _device.ReadReport(ReadProcess);
        }

        public void Stop()
        {
            _thread.Abort();
        }

        private void ReadProcess(HidReport report)
        {
            _form.BeginInvoke(new ReadHandlerDelegate(ReadHandler), new object[] { report });
        }

        private void ReadHandler(HidReport report)
        {
            if (report.ReportId < 0x20)
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    _form.AddLog($"{(ProductId)_device.Attributes.ProductId} disconnected.", Color.Red);
                }
            }
            else if (report.ReportId < 0x70)
            {
                if (!IsConnected)
                {
                    IsConnected = true;
                    _form.AddLog($"{(ProductId)_device.Attributes.ProductId} connected.", Color.Green);
                }
                if ((report.ReportId == 0x3f && (report.Data[1] & 0x20) > 0) || (report.ReportId != 0x3f && (report.Data[3] & 0x20) > 0))
                {
                    if (!IsCapturing)
                    {
                        _lastCaptureTime = DateTime.Now;
                        IsCapturing = true;
                    }
                }
                else
                {
                    if (IsCapturing)
                    {
                        IsCapturing = false;
                        if ((DateTime.Now - _lastCaptureTime).TotalSeconds > 0.8)
                        {
                            _form.AddLog("Movie captured.", Color.Purple);
                            Simulate.Events().ClickChord(new KeyCode[] { KeyCode.LAlt, KeyCode.LWin, KeyCode.G }).Invoke();
                        }
                        else
                        {
                            _form.AddLog("Screenshot captured.", Color.Blue);
                            Simulate.Events().ClickChord(new KeyCode[] { KeyCode.LAlt, KeyCode.LWin, KeyCode.PrintScreen }).Invoke();
                        }
                    }
                    else
                    {
                        IsCapturing = false;
                    }
                }
            }
            _device.ReadReport(ReadProcess);
        }
    }
}
