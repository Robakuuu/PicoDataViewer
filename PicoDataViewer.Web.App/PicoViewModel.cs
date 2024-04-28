using System.IO.Ports;

namespace PicoDataViewer.Web.App
{
    public class PicoViewModel
    {
        private SerialPort _serialPort;
        public bool PortOpened { get; set; }
        public Report CurrentReport { get; set; }
        public int LineCounter { get; set; }

        public EventHandler<DataFromPico> OnDataReceived { get; set; }
        public EventHandler<Report> OnReportAdded { get; set; }
        public string COM { get; set; }

        public Type WorkingType { get; set; }
        public int TriggerDelay { get; set; }
        public int ShutterTime { get; set; }
        public int AutoDelay { get; set; }
        public int AdcPoint { get; set; }
        public int AdcHist { get; set; }

        public async Task ConnectToPico()
        {
            _serialPort = new SerialPort(COM, 115200, Parity.None, 8, StopBits.One);
            
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            _serialPort.DtrEnable=true;
            _serialPort.Open();
            CurrentReport = new Report();
               PortOpened = true;
        }
         void DataReceived(object sender, SerialDataReceivedEventArgs e)
         {
             var data = _serialPort.ReadLine();


             if ( data!="RaportEnd\n")
             {
                 switch (LineCounter)
                 {
                    case 1:
                        CurrentReport.RaportAdcBurst = data;
                        LineCounter++;
                        break;
                    case 2:
                        CurrentReport.SampleValue = data;
                        LineCounter++;
                        break;
                }
             }

            if (data == "RaportEnd\n")
             {
                OnReportAdded.Invoke(this, CurrentReport);
                 LineCounter = 0;
             }

             if (data == "RaportStart\n")
             {
                 LineCounter = 1;
               CurrentReport = new Report();
             }

            OnDataReceived.Invoke(this,new DataFromPico(){Data = data });
            // Invokes the delegate on the UI thread, and sends the data that was received to the invoked method.
            // ---- The "si_DataReceived" method will be executed on the UI thread, which allows populating the textbox.
       
         }

         public async Task SendParams()
         {
             string command = $"{(int)WorkingType} {TriggerDelay} {ShutterTime} {AutoDelay} {AdcPoint} {AdcHist}\n";
             _serialPort.Write(command);
         }
    }
}
