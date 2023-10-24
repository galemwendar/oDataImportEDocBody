using NLog;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oDataImportEDocBody.EDoc
{
    public class EDocBusinessLogic
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        ODataEDocService service;
        IDictionary<string, object> eDoc;
        int id;


        //private ODataEDocService service;
        //private IDictionary<string, object> eDoc;

        private EDocBusinessLogic(ODataEDocService service)
        {
            this.service = service;
        }

        public static async Task<EDocBusinessLogic> CreateAsync(ODataClient client, int id)
        {
            ODataEDocService service = new ODataEDocService(client);
            var eDoc = await service.FindEdocAsync(id);

            var businessLogic = new EDocBusinessLogic(service);
            businessLogic.eDoc = eDoc;
            return businessLogic;
        }


        public async Task SetBody(string filePath, bool ForceUpdateBody = false)
        {
            var extension = Path.GetExtension(filePath).Replace(".", "");
            int docId = (int)eDoc["Id"];
            var version = GetLastVersionAsync(extension);
            var targetApp = await service.FindAssociatedApplication(extension);
            if (version  == null)
            {
                version = await service.CreateNewVersion(docId, "Первоначальная версия",targetApp);
            }
            else if(NeedNewVersion(extension, version) || ForceUpdateBody == true)
            {
                version = await service.CreateNewVersion(docId, Path.GetFileName(filePath), targetApp);
            }
     
            var body = await File.ReadAllBytesAsync(filePath);
            await service.FillBodyAsync(docId, body, version);
        }


        private bool NeedNewVersion(string extension, IDictionary<string, object> version)
        {

            if (!version.TryGetValue("AssociatedApplication", out var application))
            { return true; }

            if (application is IDictionary<string, object> currentApp && currentApp.TryGetValue("Extension", out var currentExtension))
            { return currentExtension.ToString() != extension; }

            return true;

        }

        private IDictionary<string, object>? GetLastVersionAsync(string extension)
        {
            if (eDoc.TryGetValue("Versions", out var versions))
            {
                if (versions == null)
                {
                    return null;
                }

                if (versions is IEnumerable<IDictionary<string, object>> versionList)
                {
                    return versionList
                        .Select(version => new
                        {
                            Version = version,
                            Number = version["Number"] as int? ?? 0 // Преобразование "Number" в int
                        })
                        .OrderByDescending(version => version.Number)
                        .First()
                        .Version;
                }
                else if (versions is IDictionary<string, object> versionDict)
                {
                    return versionDict;
                }
            }
            return null;
        }

    }
}
