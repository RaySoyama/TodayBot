using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Win32;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

public class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;

    #region TodayBot File Settings
    private static string ConfigFolderName = "TodayBot Config";

    public static string TodayBot_Key;
    private string KeyFileName = "TodayBotKey.json";

    public static TodayBotConfigs TodayBot_Configs;
    private string ConfigFileName = "TodayBotConfig.json";
    #endregion

    #region TodayBot User Data
    private static string UserDataFolderName = "TodayBot User Data";
    public static UserWaterData TodayBot_UserWaterData;
    private static string UserWaterDataFileName = "TodayBotUserWaterData.json";
    #endregion
    public static int latency = 69;


    static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private async Task MainAsync()
    {

        InitializeKey(); //Load Key from File

        InitializeConfigs(); //Load Configuration from File

        LoadUserData();

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info
        });

        _commands = new CommandService(new CommandServiceConfig
        {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Info
        });


        _client.Ready += ClientIsReady;
        _client.Log += _client_Log;

        await _client.LoginAsync(TokenType.Bot, TodayBot_Key);
        await _client.StartAsync();

        //If user joins
        //_client.UserJoined += AnnounceJoinedUser;

        _client.MessageReceived += _client_MessageReceived;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

        _services = new ServiceCollection().BuildServiceProvider();

        await Task.Delay(-1);
    }

    private async Task _client_MessageReceived(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        var context = new SocketCommandContext(_client, message);

        latency = _client.Latency;


        if (context.IsPrivate == true && context.User.IsBot == false) //If they send a DM
        {

        }
        else if (context.User.IsBot == false) //Log Input from non bot
        {

        }


        if (context.Message == null || context.Message.Content.ToString() == "" || context.User.IsBot == true) //Checks if the msg is from a bot, and if the msg is empty
        {
            return;
        }


        int argPos = 0;

        if (!(message.HasCharPrefix(TodayBot_Configs.TodayBot_Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) //Checks if there is a prefix, or mentions @bot
        {
            return;
        }

        var result = await _commands.ExecuteAsync(context, argPos, _services); //Send the arguments and see if there is a  associated command

        //if bot command is good
        if (result.IsSuccess == true)
        {

        }
        else if (result.IsSuccess == false) //If the command failed, run this
        {

        }
    }

    private Task _client_Log(LogMessage arg) //Logging Errors
    {
        Console.WriteLine($"{DateTime.Now} {arg.Message}");
        return Task.CompletedTask;
    }

    private async Task ClientIsReady() //When the Bot first runs, and is ready
    {
        await _client.SetGameAsync($"{TodayBot_Configs.TodayBot_Prefix}Help");
    }


    #region SERVER SIDE ACTIONS
    /// <summary>
    /// Called when new member joins server
    /// </summary>
    /// <param name="user">User that joined</param>
    /// <returns></returns>
    public async Task AnnounceJoinedUser(SocketGuildUser user) //Welcomes the new user
    {
        await Task.CompletedTask;
    }

    private void InitializeKey()
    {
        ValidateFolder($"{AppDomain.CurrentDomain.BaseDirectory}\\{ConfigFolderName}");

        string keyPath = $"{AppDomain.CurrentDomain.BaseDirectory}{ConfigFolderName}" + $"\\{KeyFileName}";

        if (ValidateFile(keyPath) == false)
        {
            TodayBot_Key = "Token";

            var myFile = File.OpenText(keyPath);
            string contents = JsonConvert.SerializeObject(TodayBot_Key, Formatting.Indented);

            myFile.Close();

            File.WriteAllText(keyPath, contents);

            Console.WriteLine("No Key Found. Please check your application directory for the Bot Key");
            Console.ReadKey();

            System.Environment.Exit(1);
            return;
        }
        else
        {
            string keyContents = File.ReadAllText(keyPath);
            TodayBot_Key = JsonConvert.DeserializeObject<string>(keyContents);

            if (TodayBot_Key == "Token") //If Token is Default
            {
                Console.WriteLine("No Key Found. Please check your application directory for the Bot Key");
                Console.ReadKey();

                System.Environment.Exit(1);
                return;
            }
        }
    }
    private void InitializeConfigs()
    {
        ValidateFolder($"{AppDomain.CurrentDomain.BaseDirectory}\\{ConfigFolderName}");

        string configPath = $"{AppDomain.CurrentDomain.BaseDirectory}{ConfigFolderName}" + $"\\{ConfigFileName}";

        //if no file exist, create
        if (ValidateFile(configPath) == false)
        {
            TodayBot_Configs = new TodayBotConfigs()
            {
                TodayBot_Prefix = '?'
            };

            var myFile = File.OpenText(configPath);
            string contents = JsonConvert.SerializeObject(TodayBot_Configs, Formatting.Indented);

            myFile.Close();

            File.WriteAllText(configPath, contents);

            Console.WriteLine("No Configs found... Initializing");
            return;
        }
        else
        {
            string configContents = File.ReadAllText(configPath);
            TodayBot_Configs = JsonConvert.DeserializeObject<TodayBotConfigs>(configContents);
        }
    }
    public static void LoadUserData()
    {
        LoadUserWaterData();
    }
    public static void SaveUserData()
    {
        SaveUserWaterData();
    }

    private static void LoadUserWaterData()
    {
        ValidateFolder($"{AppDomain.CurrentDomain.BaseDirectory}\\{UserDataFolderName}");

        string path = $"{AppDomain.CurrentDomain.BaseDirectory}{UserDataFolderName}" + $"\\{UserWaterDataFileName}";

        //if no file exist, create
        if (ValidateFile(path) == false)
        {
            TodayBot_UserWaterData = new UserWaterData();
            Console.WriteLine("Initializing User Water Data");
            return;
        }
        else
        {
            string contents = File.ReadAllText(path);
            TodayBot_UserWaterData = JsonConvert.DeserializeObject<UserWaterData>(contents);

            if (TodayBot_UserWaterData == null)
            {
                TodayBot_UserWaterData = new UserWaterData();
            }
        }
    }
    private static void SaveUserWaterData()
    {
        ValidateFolder($"{AppDomain.CurrentDomain.BaseDirectory}\\{UserDataFolderName}");

        string path = $"{AppDomain.CurrentDomain.BaseDirectory}{UserDataFolderName}" + $"\\{UserWaterDataFileName}";

        //if no file exist, create
        if (ValidateFile(path) == false)
        {
            Console.WriteLine("Initializing User Water Data");
            return;
        }
        else
        {
            string contents = JsonConvert.SerializeObject(TodayBot_UserWaterData, Formatting.Indented);
            File.WriteAllText(path, contents);
        }
    }

    #endregion

    private static void ValidateFolder(string folderPath)
    {
        if (Directory.Exists($"{folderPath}") == false)
        {
            Directory.CreateDirectory($"{folderPath}");
        }
    }
    private static bool ValidateFile(string filePath)
    {
        if (File.Exists(filePath) == false)
        {
            var myFile = File.Create(filePath);
            myFile.Close();
            return false;
        }
        return true;
    }
}