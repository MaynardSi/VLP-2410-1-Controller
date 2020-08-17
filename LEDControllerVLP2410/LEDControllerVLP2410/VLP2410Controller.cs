using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace LEDControllerVLP2410
{
    public class VLP2410Controller
    {
        public string StartControlStr = "@";
        public string EndControlStr = "\r\n";

        public VLP2410Controller()
        {
            SerialPort = new SerialPort();
        }

        public SerialPort SerialPort { get; set; }

        /// <summary>
        /// Opens a new serial port connection if the serial port is available.
        /// </summary>
        /// <param name="portName">The Serial Port Name. Default: COM3</param>
        /// <param name="baudRate">The Baud Rate. Default: 38400</param>
        /// <param name="dataBits">The length of standard data bits per byte. Default: 8</param>
        /// <param name="parity">The parity checking protocol. Default: None</param>
        /// <param name="stopBits">The standard number of stopbits per byte. Default: One</param>
        public void Open(string portName, int baudRate = 38400,
            int dataBits = 8, Parity parity = Parity.None,
            StopBits stopBits = StopBits.One)
        {
            if (SerialPort.IsOpen) Close();
            SerialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits
            };
            SerialPort.Open();
        }

        /// <summary>
        /// Closes and Disposes the SerialPort object.
        /// </summary>
        public void Close()
        {
            SerialPort.Close();
            SerialPort.Dispose();
        }
        #region Command Helper

        /// <summary>
        /// Sends the command string through the serial port.
        /// A delay of 10ms is present to give time for the
        /// command to be properly sent.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(string command)
        {
            if (!SerialPort.IsOpen) throw new Exception();
            SerialPort.Write(command);
            Thread.Sleep(10);
        }


        /// <summary>
        /// CheckSum is the sum of all bytes from the header to the ID. 
        /// The lowest byte is converted to hexadecimal, and two characters are sent.
        /// </summary>
        /// <remarks>
        /// The order of send data bytes for the command format is as follows:
        /// [1] Header (default: @)
        /// [2 - 3] Channel (default: 00)
        /// [4- 5(L)/4 - 7(F)] Send Command Instruction (F - Intensity or L - On/Off)
        /// [6 - 7(L)/8 - 9(F)] ID Specification (Fixed: 00)
        /// [8 - 9(L)/10 - 12(F)] Check Sum
        /// [10 - 11(L)/12 - 13(F)] Delimiter (Default: CRLF)
        /// </remarks>
        /// <see cref="https://stackoverflow.com/questions/4648130/how-calculate-checksum-using-8-bit-addition-of-all-bytes-in-the-data-structure"/>
        /// <param name="command"></param>
        /// <returns></returns>
        public string CheckSum(string command)
        {
            var num1 = Encoding.ASCII.GetBytes(command).Aggregate<byte, byte>(0, (current, num2) => (byte)(current + num2));
            return $"{num1:X2}";
        }
        #endregion
        #region Commands
        /// <summary>
        /// Turns on the light unit.
        /// </summary>
        /// <remarks>Sends @00L1007DCRLF.</remarks>
        public void SetOn()
        {
            string command = $"{StartControlStr}00L1007D{EndControlStr}";
            SendCommand(command);
        }

        /// <summary>
        /// Turns off the light unit.
        /// </summary>
        /// <remarks>Sends 00L0007C.</remarks>
        public void SetOff()
        {
            string command = $"{StartControlStr}00L0007C{EndControlStr}";
            SendCommand(command);
        }
        /// <summary>
        /// Sets the light intensity. Range is from 0 - 255.
        /// Automatically sets the light unit on.
        /// </summary>
        /// <param name="intensity"></param>
        public void SetIntensity(int intensity)
        {
            string partialCommand = $"{StartControlStr}00F{intensity:D3}00";
            string command = partialCommand + CheckSum(partialCommand) + EndControlStr;
            SendCommand(command);
            SetOn();
        }
        #endregion
    }
}
