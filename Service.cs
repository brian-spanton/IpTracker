using System;
using System.ServiceProcess;
using System.ComponentModel;

namespace Foobalator.IpTracker
{
	public class Service : ServiceBase
	{
		private Container components = null;
		private IpTracker m_app = new IpTracker();

		public Service()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
		}

		// The main entry point for the process
		public static void Main(string[] args)
		{
			if(args.Length > 0)
			{
				Log.WriteLine("Starting up");
				IpTracker app = new IpTracker();
				app.Start();

				System.Console.Read();

				app.Stop();
				Log.WriteLine("Shutting down");
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] { new Service() };
				ServiceBase.Run(ServicesToRun);
			}
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			ServiceName = IpTracker.MyServiceName;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			m_app.Start();
		}

		protected override void OnStop()
		{
			m_app.Stop();
		}
	}
}
