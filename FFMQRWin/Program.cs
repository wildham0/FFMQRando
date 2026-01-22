using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFMQRWin
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }

	public struct FFMQResult
	{
		public bool Success;
		public string Message;
		public object Product;

		public FFMQResult(bool success, string message, object product)
		{
			Success = success;
			Message = message;
			Product = product;
		}
	}
}
