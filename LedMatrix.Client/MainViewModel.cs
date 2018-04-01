using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace LedMatrix.Client
{
   public class MainViewModel : NotifyChangeBase
   {
      private const int MatrixCount = 3;
      private const string DefaultPortName = "COM3";
      private const int DefaultBaudRate = 115200;
      private Random _Random;
      private IEnumerator<FramePacket>[] _Programs = new IEnumerator<FramePacket>[MatrixCount];

      public MainViewModel()
      {
         _Random = new Random();
         SerialPorts = SerialPort.GetPortNames();
         BaudRates = new[] { 300, 600, 1200, 2400, 9600, 14400, 19200, 38400, 57600, 115200 };
         SelectedSerialPort = SerialPorts.Where(p => p == DefaultPortName).FirstOrDefault();
         SelectedBaudRate = BaudRates.Where(r => r == DefaultBaudRate).FirstOrDefault();
         InitialiseSerialPort();
      }

      private void InitialiseSerialPort()
      {
         _Port = new SerialPort(SelectedSerialPort, SelectedBaudRate);
         _Port.DataReceived += _Port_DataReceived;
         _Port.ErrorReceived += _Port_ErrorReceived;
         UpdatePortStatus();
      }

      private void _Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
      {
         PongText += "ERROR\r\n";
      }

      private void _Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
      {
         string data = _Port.ReadExisting();
         if (!string.IsNullOrEmpty(data))
         {
            if (CapturePongText)
               PongText += data;
         }
      }

      public string[] SerialPorts { get; set; }
      public int[] BaudRates { get; set; }
      SerialPort _Port;

      private string _SelectedSerialPort;
      public string SelectedSerialPort
      {
         get { return _SelectedSerialPort; }
         set { SetPropertyValue(ref _SelectedSerialPort, value); }
      }

      private int _SelectedBaudRate = 0;
      public int SelectedBaudRate
      {
         get { return _SelectedBaudRate; }
         set { SetPropertyValue(ref _SelectedBaudRate, value); }
      }

      private string _InputText = "";
      public string InputText
      {
         get { return _InputText; }
         set { SetPropertyValue(ref _InputText, value); }
      }

      private bool _CapturePingText;
      public bool CapturePingText
      {
         get { return _CapturePingText; }
         set { SetPropertyValue(ref _CapturePingText, value); }
      }

      private string _PingText = "";
      public string PingText
      {
         get { return _PingText; }
         set { SetPropertyValue(ref _PingText, value); }
      }

      private bool _CapturePongText = true;
      public bool CapturePongText
      {
         get { return _CapturePongText; }
         set { SetPropertyValue(ref _CapturePongText, value); }
      }

      private string _PongText = "";
      public string PongText
      {
         get { return _PongText; }
         set { SetPropertyValue(ref _PongText, value); }
      }

      private bool _IsPortOpen;
      public bool IsPortOpen
      {
         get { return _IsPortOpen; }
         set { SetPropertyValue(ref _IsPortOpen, value); }
      }

      private string _PortActionText = "";
      public string PortActionText
      {
         get { return _PortActionText; }
         set { SetPropertyValue(ref _PortActionText, value); }
      }

      private int _MatrixId;
      public int MatrixId
      {
         get { return _MatrixId; }
         set { SetPropertyValue(ref _MatrixId, value); }
      }

      public int[] MatrixList
      {
         get { return Enumerable.Range(0, MatrixCount).ToArray(); }
      }

      public void NextTimerTick()
      {
         foreach(var program in _Programs)
         {
            if (program != null && program.MoveNext())
            {
               if (CapturePingText)
                  PingText += $"{program.Current}\r\n";

               SendPortMessage(program.Current.ToString());
            }
         }
      }

      public void InitialiseRandomProgram()
      {
         _Programs[MatrixId] = RandomSequence(MatrixId).GetEnumerator();
      }

      public void InitialiseSinWaveProgram()
      {
         _Programs[MatrixId] = SinWaveSequence(MatrixId).GetEnumerator();
      }

      public void InitialiseCountProgram()
      {
         _Programs[MatrixId] = CountSequence(MatrixId).GetEnumerator();
      }

      public void ClearPrograms()
      {
         for(int i=0; i < _Programs.Length; i++)
         {
            _Programs[i] = null;
            SendPortMessage(new FramePacket(0, (byte)i, 0).ToString());
         }
      }

      private IEnumerable<FramePacket> RandomSequence(int matrixId)
      {
         while (true)
         {
            var data = Enumerable.Range(0, 8).Select(n => (byte)_Random.Next(0, 255)).ToArray();
            yield return new FramePacket(0, (byte)matrixId, data);
         }
      }

      private IEnumerable<FramePacket> CountSequence(int matrixId)
      {
         int n = 0;
         while (true)
         {
            n++;
            yield return new FramePacket(0, (byte)matrixId, (byte)(n % 0xFF));
         }
      }

      private IEnumerable<FramePacket> SinWaveSequence(int matrixId)
      {
         const int speed = 20;
         const int frequency = 20;
         const int rows = 8;
         var sinData = new double[rows];
         var angleData = Enumerable.Range(0, rows).Select(n => n * frequency).ToArray();

         while (true)
         {
            for(int i = 0; i < rows; i++)
            {
               angleData[i] = (angleData[i] + speed) % 180;
               sinData[i] = 0.05 + Math.Sin(Math.PI * angleData[i] / 180.0);
            }

            Func<double, byte> ConvertToBits = (d) =>
            {
               double sep = 0.125;
               int n = (d >= sep * 8 ? 0x01 : 0x00) |
                     (d >= sep * 7 ? 0x02 : 0x00) |
                     (d >= sep * 6 ? 0x04 : 0x00) |
                     (d >= sep * 5 ? 0x08 : 0x00) |
                     (d >= sep * 4 ? 0x10 : 0x00) |
                     (d >= sep * 3 ? 0x20 : 0x00) |
                     (d >= sep * 2 ? 0x40 : 0x00) | 
                     0x80;
               return (byte)(n & 0xFF);
            };

            yield return new FramePacket(0, (byte)matrixId, sinData.Select(d => ConvertToBits(d)).ToArray());
         }
      }

      public void SendInputText()
      {
         SendPortMessage(InputText);
      }

      public void ToggleSerialPort()
      {
         if (_Port.IsOpen)
            CloseSerialPort();
         else
            OpenSerialPort();
      }

      private void CloseSerialPort()
      {
         ClearPrograms();
         _Port.Close();
         UpdatePortStatus();
         PingText = "";
         PongText = "";
      }

      private void OpenSerialPort()
      {
         if (string.IsNullOrEmpty(SelectedSerialPort))
            return;

         if (SelectedBaudRate <= 0)
            return;

         _Port.PortName = SelectedSerialPort;
         _Port.BaudRate = SelectedBaudRate;
         _Port.DataBits = 8;
         _Port.Parity = Parity.None;
         _Port.StopBits = StopBits.One;
         _Port.Open();
         UpdatePortStatus();
      }

      private void UpdatePortStatus()
      {
         IsPortOpen = _Port.IsOpen;
         PortActionText = IsPortOpen ? "Close" : "Open";
      }

      private void SendPortMessage(string message)
      {
         if (_Port.IsOpen)
         {
            _Port.WriteLine(message);
         }
      }
   }
}
