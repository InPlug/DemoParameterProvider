using NetEti.ApplicationControl;
using NetEti.ApplicationEnvironment;

namespace DemoParameterProviderDemo
{
    /// <summary>
    /// Demo-Programm für DemoParameterProvider.
    /// </summary>
    /// <remarks>
    /// File: DemoParameterProviderDemo
    /// Autor: Erik Nagel
    ///
    /// 07.06.2015 Erik Nagel: erstellt
    /// 14.06.2019 Erik Nagel: überarbeitet.
    /// </remarks>
    internal class Program
    {
        /// <summary>
        /// Demonstriert die Benutzung von DemoParameterProvider.
        /// </summary>
        /// <param name="args">Kommandozeilen-Parameter.</param>
        public static void Main(string[] args)
        {
            InfoController.GetInfoSource().RegisterInfoReceiver(
                new ViewerAsWrapper(Program.HandleMessages), new[] { InfoType.Info });

            Program._provider.ParametersReloaded += Provider_ParametersReloaded;
            Program._provider.Init("Übergebener Parameter|M:1");
            Console.ReadLine();
        }
        static Program()
        {
            Program._provider = new DemoParameterProvider();
        }

        private static DemoParameterProvider _provider;

        private static void Provider_ParametersReloaded(object? sender, EventArgs e)
        {
            // Die Parameter aus DemoParameterProvider:
            Console.WriteLine($"GesuchterParameter: {Program._provider.ReadParameter("GesuchterParameter")}");
            Console.WriteLine($"ÜbergebenerParameter: {Program._provider.ReadParameter("ÜbergebenerParameter")}");
        }

        private static void HandleMessages(object? sender, InfoArgs msgArgs)
        {
            Console.WriteLine(msgArgs.MessageObject.ToString());
        }


    }
}