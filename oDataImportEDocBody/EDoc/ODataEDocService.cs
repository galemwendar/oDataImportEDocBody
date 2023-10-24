using NLog;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oDataImportEDocBody.EDoc
{
    public class ODataEDocService
    {
        ODataClient client;
        Logger logger = LogManager.GetCurrentClassLogger();
        HttpResponseMessage response = null;


        public ODataEDocService(ODataClient client)
        {
            this.client = client;
        }

        public async Task<IDictionary<string, object>> FindEdocAsync(int eDocId)
        {
            return await client
                .For("IElectronicDocuments")
                .Key(eDocId)
                .Expand("Versions($expand=AssociatedApplication,Body)")
                .FindEntryAsync();
        }

        public async Task<IDictionary<string, object>> FindAssociatedApplication(string extension)
        {
            try
            {
                return await client.For("IAssociatedApplications").Filter($@"Extension eq '{extension}'").FindEntryAsync();
            }
            catch (Exception ex) { logger.Error(ex); return null; }
        }

        public async Task<IDictionary<string, object>> CreateNewVersion(int eDocId, string note, IDictionary<string, object> associatedApp)
        {
            try
            {
                return await client.For("IElectronicDocuments")
           .Key(eDocId)
           .NavigateTo("Versions")
           .Set(new { Note = note, Created = DateTime.Now, AssociatedApplication = associatedApp })
           .InsertEntryAsync();
            }
            catch (Exception ex) { logger.Error(ex); return null; }

        }

        public async Task FillBodyAsync(int eDocId, byte[] body, IDictionary<string, object> lastVersion)
         {
            try
            {
                lastVersion.TryGetValue("Id", out var lastVersionKey);
                int.TryParse(lastVersionKey.ToString(), out int lastVersionId);
                var eDocBody = await client.For("IElectronicDocuments")
                    .Key(eDocId)
                    .NavigateTo("Versions")
                    .Key(lastVersionId)
                    .NavigateTo("Body")
                    .Set(new { Value = body, })
                    .InsertEntryAsync();
            }
            catch (Exception ex) { logger.Error(ex); return; }

        }
    }
}
