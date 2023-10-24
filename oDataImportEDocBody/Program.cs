using NLog;
using NLog.Config;
using oDataImportEDocBody;
using oDataImportEDocBody.EDoc;
using Simple.OData.Client;
using System;
using System.Text;

Console.OutputEncoding = System.Text.Encoding.UTF8;
long maxFileSizeInBytes = 2L * 1024 * 1024 * 1024; // 2 GB in bytes

string serviceUrl = "http://localhost/Integration/odata/";
string login = "Administrator";
string password = "11111";

int eDocId = 3;
string filePath = @"C:\Users\galemwendar\Downloads\Задачи по архиву_текущие.xlsx";

//Check file exists and lower then 2 Gb
if (!File.Exists(filePath))
{
    Console.WriteLine("file not exist");
    Console.ReadLine();
    return;
}
if( new FileInfo(filePath).Length >= maxFileSizeInBytes)
{
    Console.WriteLine("file then or equals 2Gb!");
    Console.ReadLine();
    return;
}
var client = new ODataClientService(serviceUrl, login, password).CreateClient();
var eDocLogic = await EDocBusinessLogic.CreateAsync(client, eDocId);
await eDocLogic.SetBody(filePath);



Console.ReadLine();


