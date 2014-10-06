using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Data;
using Microsoft.Win32;

namespace Foobalator.IpTracker
{
    public class IpTracker
    {
        public static string MyServiceName = "IpTracker";

        private Timer m_Timer = null;
        private ISettings m_Settings = new ConfigFileSettings();

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        public void Start()
        {
            Log.SetSettings(m_Settings);

            string value = m_Settings.GetValue("IntervalSeconds");
            TimeSpan interval = TimeSpan.FromSeconds(int.Parse(value));
            m_Timer = new Timer(new TimerCallback(TimerMethod), this, 0, (int)interval.TotalMilliseconds);
        }

        public static void TimerMethod(object state)
        {
            IpTracker service = (IpTracker)state;
            service.TimerMethod();
        }

        private void TimerMethod()
        {
            try
            {
                IPAddress ipAddress = GetIpAddress();
                SystemState state = SystemState.Load(m_Settings.GetValue("StateDir"));
                IPAddress oldIpAddress = state.LastAddress;

                if (!ipAddress.Equals(oldIpAddress))
                {
                    Log.WriteLine("Address has changed from " + oldIpAddress + " to " + ipAddress);

                    string result = "";

                    string value = m_Settings.GetValue("Dns Domains");
                    foreach(string domain in value.Split(','))
                    {
                        try
                        {
                            result = UpdateDns(ipAddress, domain, m_Settings.GetValue("Dns Username"), m_Settings.GetValue("Dns Password"));
                            Log.WriteLine(result);
                        }
                        catch (Exception e)
                        {
                            Log.Write(e);
                            result += "\n\n" + e.ToString();
                        }
                    }

                    state.LastAddress = ipAddress;

                    try
                    {
                        Notify(ipAddress.ToString(), result);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
        }

        private string UpdateDns(IPAddress address, string domain, string name, string password)
        {
            WebRequest request = WebRequest.Create(@"http://dyn.everydns.net/index.php?ver=0.1&ip=" + address.ToString() + @"&domain=" + domain);
            request.Credentials = new BasicCredentials(name, password);
            request.PreAuthenticate = true;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private IPAddress GetIpAddress()
        {
            WebRequest request = WebRequest.Create(@"http://www.whatismyip.com/automation/n09230945.asp");
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string value = reader.ReadToEnd();
            return IPAddress.Parse(value);
       }

        private void Notify(string subject, string textBody)
        {
            CDO.MessageClass message = new CDO.MessageClass();
            CDO.Configuration config = message.Configuration;

            config.Fields[CDO.CdoConfiguration.cdoAutoPromoteBodyParts].Value = true;
            config.Fields[CDO.CdoConfiguration.cdoURLGetLatestVersion].Value = true;

            string server = m_Settings.GetValue("Email Server");
            if (server.Length > 0)
            {
                config.Fields[CDO.CdoConfiguration.cdoSendUsingMethod].Value = CDO.CdoSendUsing.cdoSendUsingPort;
                config.Fields[CDO.CdoConfiguration.cdoSMTPServer].Value = server;
                config.Fields[CDO.CdoConfiguration.cdoSMTPServerPort].Value = int.Parse(m_Settings.GetValue("Email Port"));

                string value = m_Settings.GetValue("Email Auth");
                CDO.CdoProtocolsAuthentication auth = (CDO.CdoProtocolsAuthentication)Enum.Parse(typeof(CDO.CdoProtocolsAuthentication), value);
                if (auth != CDO.CdoProtocolsAuthentication.cdoAnonymous)
                {
                    config.Fields[CDO.CdoConfiguration.cdoSMTPAuthenticate].Value = auth;
                    config.Fields[CDO.CdoConfiguration.cdoSendUserName].Value = m_Settings.GetValue("Email Username");
                    config.Fields[CDO.CdoConfiguration.cdoSendPassword].Value = m_Settings.GetValue("Email Password");
                    config.Fields[CDO.CdoConfiguration.cdoSMTPUseSSL].Value = bool.Parse(m_Settings.GetValue("Email UseSSL"));
                }
            }
            else
            {
                config.Fields[CDO.CdoConfiguration.cdoSendUsingMethod].Value = CDO.CdoSendUsing.cdoSendUsingPickup;
            }

            config.Fields.Update();

            message.To = m_Settings.GetValue("Email To");
            message.From = m_Settings.GetValue("Email From");
            message.ReplyTo = m_Settings.GetValue("Email ReplyTo");
            message.Subject = subject;

            //string createUrl = m_Settings.GetValue("Email CreateUrl");
            //if (createUrl.Length > 0)
            //{
            //    message.CreateMHTMLBody(createUrl, CDO.CdoMHTMLFlags.cdoSuppressAll, null, null);
            //}
            message.TextBody = textBody;

            message.Send();

            Log.WriteLine("Notification sent to " + message.To);
        }

        public void Stop()
        {
            m_Timer.Dispose();
        }
    }
}
