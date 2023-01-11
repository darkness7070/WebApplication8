using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using Microsoft.VisualBasic;

string token = "0";
const long idUser = 0;
const long idUser1 = 0;
TelegramBotClient _client = new(token);
CancellationTokenSource _cts = new();
int OutTime = 15;
List<HttpsProxy> listProxy = new();
string[] file = System.IO.File.ReadAllLines("https_proxy");

string help = "/add login:password@host:port - добавить прокси\n /del {index} - удалить прокс\n /list - список прокси";
const string url = @"https://www.humblebundle.com/login?hmb_source=navbar&goto=%2F";
const int RequestTime = 3;
const int QueueSize = 100;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

_client.StartReceiving(HandleUpdateAsync,
                           HandleErrorAsync,
                           receiverOptions,
                           _cts.Token);
Console.WriteLine(_client.GetMeAsync().Result.FirstName + " OK!");



async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    string syntax = "Используйте данный синтаксис:" + help;
    var message = update.Message;
    if (message == null) return;
    string[] arg = message.Text.Split(' ');
    if (message.Chat.Id.ToString() != idUser.ToString())
    {
        await _client.SendTextMessageAsync(chatId: message.Chat.Id, text: "Чао, ты не мой босс");
        return;
    }

    if (arg.Count() != 2 || (arg[0].ToLower() != "/add" && arg[0] != "/del"))
    {
        if (arg[0].ToLower() == "/list" && arg.Count() == 1)
        {
            string text = string.Empty;
            foreach (var item in listProxy)
            {
                text += $"({item.id}) {item.login}:{item.password}@{item.ip}:{item.port}\n";
            }
            await _client.SendTextMessageAsync(chatId: idUser, text: text);
            return;
        }
        await _client.SendTextMessageAsync(chatId: idUser, text: help);
        return;
    }

    if (arg[0].ToLower() == "/add")
    {
        string[] proxy = arg[1].Split('@');
        if (proxy.Count() != 2)
        {
            await _client.SendTextMessageAsync(chatId: idUser, text: help);
            return;
        }
        if (proxy[0].Split(':').Count() + proxy[1].Split(':').Count() != 4)
        {
            await _client.SendTextMessageAsync(chatId: idUser, text: "Прокси удалён");
            return;
        }
        string text = string.Empty;
        try
        {
            var newproxy = new HttpsProxy()
            {
                id = listProxy.Last().id + 1,
                login = proxy[0].Split(':')[0],
                password = proxy[0].Split(':')[1],
                ip = proxy[1].Split(':')[0],
                port = Convert.ToInt32(proxy[1].Split(':')[1])
            };
            listProxy.Add(newproxy);
            text = $"{newproxy.login}:{newproxy.password}@{newproxy.ip}:{newproxy.port}";
        }
        catch
        {
            await _client.SendTextMessageAsync(chatId: idUser, text: "Не удалось добавить прокси");
            return;
        }
        using (FileStream str = new("https_proxy", FileMode.Append, FileAccess.Write))
        {
            byte[] buffer = Encoding.UTF8.GetBytes("\n" + text);
            await str.WriteAsync(buffer, 0, buffer.Length);
            await str.FlushAsync();
            str.Close();
        }
        await _client.SendTextMessageAsync(chatId: idUser, text: "Прокси добавлен");
        return;
    }
    if (arg[0].ToLower() == "/del")
    {
        int id;
        try
        {
            id = Convert.ToInt32(arg[1]);
        }
        catch
        {
            await _client.SendTextMessageAsync(chatId: idUser, text: help);
            return;
        }
        string text = "";
        HttpsProxy delProxy = listProxy.Where(x => x.id == id).FirstOrDefault();
        if (delProxy == null) return;
        var FileText = System.IO.File.ReadAllLines("https_proxy");
        for (int i = 0; i < FileText.Length; i++)
        {
            if (i != id)
            {
                text += FileText[i] + "\n";
            }
        }
        using (FileStream str = new("https_proxy", FileMode.Truncate, FileAccess.Write))
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            await str.WriteAsync(buffer, 0, buffer.Length);
            await str.FlushAsync();
            str.Close();
        }
        listProxy.Remove(delProxy);
        await _client.SendTextMessageAsync(chatId: idUser, text: "Прокси удалён");
    }
    else
    {
        await _client.SendTextMessageAsync(chatId: idUser, text: help);
    }
    return;
}
Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    return Task.CompletedTask;
}

app.Run();

record User()
{
    public string login { get; set; }
    public string password { get; set; }
}
class HttpsProxy
{
    public int id { get; set; }
    public string login { get; set; }
    public string password { get; set; }
    public string ip { get; set; }
    public int port { get; set; }
}
class RequestStatus
{
    public string Status { get; set; }
}
class RequestMethod
{

    public string Method { get; set; }
    public int id { get; set; }
}