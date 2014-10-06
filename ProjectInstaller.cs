using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;

namespace Foobalator.IpTracker
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller m_ServiceProcessInstaller;
        private ServiceInstaller m_ServiceInstaller;
        private Container m_Components = null;

        public ProjectInstaller()
        {
            m_ServiceProcessInstaller = new ServiceProcessInstaller();
            m_ServiceProcessInstaller.Account = ServiceAccount.LocalSystem;

            m_ServiceInstaller = new ServiceInstaller();

            m_ServiceInstaller.ServiceName = IpTracker.MyServiceName;
            m_ServiceInstaller.DisplayName = IpTracker.MyServiceName;
            m_ServiceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.AddRange(new Installer[]
			{
				m_ServiceProcessInstaller,
				m_ServiceInstaller
			});
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_Components != null)
                    m_Components.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
