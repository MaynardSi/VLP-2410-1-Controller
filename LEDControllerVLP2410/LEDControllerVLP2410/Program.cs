
using System.Threading;
using LEDControllerVLP2410;

namespace LEDControllerVLP2410Program
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var controller = new VLP2410Controller();
            controller.Open("COM3");

            Thread.Sleep(3000);

            for (int i = 0; i < 255; i++)
            {
                controller.SetIntensity(i);
            }

            controller.SetOff();
            controller.Close();
        }
    }
}
