using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Soap;

namespace Foobalator.IpTracker
{
    [Serializable]
    public class SystemState : PersistedBase<SoapFormatter>
    {
        #region Static Methods

        public static SystemState Load(string dir)
        {
            string file = Path.Combine(dir, "IpTracker.SystemState.xml");

            if(System.IO.File.Exists(file))
            {
                return (SystemState)PersistedBase<SoapFormatter>.LoadShared(file);
            }
            else
            {
                SystemState item = new SystemState(file);
                item.Save();
                item.Unlock();

                return item;
            }
        }

        #endregion

        #region Instance Data

        public IPAddress m_LastAddress;

        #endregion

        #region Instance Properties
        public IPAddress LastAddress
        {
            get
            {
                return m_LastAddress;
            }
            set
            {
                if (value.Equals(m_LastAddress))
                    return;

                m_LastAddress = value;
                Save();
            }
        }
        #endregion

        #region Instance Methods

        public SystemState()
        {
        }

        protected SystemState(string file)
            : base(file)
        {
        }

        #endregion
    }
}
