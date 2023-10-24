using NLog;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oDataImportEDocBody
{
    public class ODataClientService
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        HttpResponseMessage response = null;
        ODataClientSettings settings;
        public ODataClientService(string serviceUrl, string login, string password) 
        {
            settings = new ODataClientSettings(new Uri(serviceUrl));
            settings.IgnoreResourceNotFoundException = true;
            settings.OnTrace = (x, y) =>
            {
                logger.Trace(string.Format(x, y));
            };
            settings.RequestTimeout = new TimeSpan(0, 0, 600);
            settings.BeforeRequest += delegate (HttpRequestMessage message)
            {
                var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", login, password)));
                message.Headers.Add("Authorization", "Basic " + authHeaderValue);
            };
            settings.AfterResponse += httpResonse => { response = httpResonse; };
        }

        public ODataClient CreateClient()
        {
            try
            {
                return new ODataClient(settings);
            }
            catch (Exception ex)
            {
               logger.Error(ex);
                return null;
            }
        }
    }
}
