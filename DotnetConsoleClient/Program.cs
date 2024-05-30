﻿using System.Text.Json;
using DittoSDK;
using Program;

public class App
{
    static Ditto ditto;
    static IDisposable dittoAuthObserver;
    static List<DittoTask> tasks = default!;
    static bool isAskedToExit = false;

    public static async Task Main(params string[] args)
    {
        InitializeDitto();

        dittoAuthObserver = ObserveDittoAuth();

        Console.WriteLine("\nWelcome to Ditto's Task App");

        string query = "SELECT * FROM tasks WHERE isDeleted == false";

        ditto.Sync.RegisterSubscription(query);

        ditto.Store.RegisterObserver(query, (result) =>
        {
            tasks = result.Items.ConvertAll(item => JsonSerializer.Deserialize<DittoTask>(item.JsonString()));
        });

        await ditto.Store.ExecuteAsync("EVICT FROM tasks WHERE isDeleted == true");

        ListCommands();

        await HandleUserInput();
    }

    private static void InitializeDitto()
    {
        var appId = "f87f6d8c-1b51-46e2-83d6-d97825ebab71";
        var identity = DittoIdentity.OnlineWithAuthentication(
            appId,
            new AuthDelegate(),
            false,
            "http://127.0.0.1:45002");

        ditto = new Ditto(identity);
        ditto.DisableSyncWithV3();
        ditto.Auth.Logout();

        var transportConfig = new DittoTransportConfig();
        transportConfig.Connect.WebsocketUrls.Add("ws://127.0.0.1:45002");
        ditto.TransportConfig = transportConfig;
        ditto.StartSync();
    }

    private static IDisposable ObserveDittoAuth()
    {
        return ditto.Auth.ObserveStatus((status) =>
        {
            if (status.IsAuthenticated)
            {
                Console.WriteLine("\nAuth success!");
            }
            else
            {
                Console.WriteLine("\nAuth required!! → try \"--login jellybeans\"");
            }
        });
    }

    private static async Task HandleUserInput()
    {
        while (!isAskedToExit)
        {
            Console.Write("\nYour command: ");
            string command = Console.ReadLine() ?? string.Empty;

            if (command.StartsWith("--login"))
            {
                await HandleLoginCommand(command);
            }
            else if (command.StartsWith("--logout"))
            {
                HandleLogoutCommand();
            }
            else if (command.StartsWith("--insert"))
            {
                await HandleInsertCommand(command);
            }
            else if (command.StartsWith("--toggle"))
            {
                await HandleToggleCommand(command);
            }
            else if (command.StartsWith("--delete"))
            {
                await HandleDeleteCommand(command);
            }
            else if (command.StartsWith("--list"))
            {
                HandleListTasksCommand();
            }
            else if (command.StartsWith("--menu"))
            {
                ListCommands();
            }
            else if (command.StartsWith("--exit"))
            {
                HandleExitCommand();
            }
            else
            {
                Console.WriteLine("Unknown command");
                ListCommands();
            }
        }
    }

    private static async Task HandleLoginCommand(string command)
    {
        string password = command.Replace("--login ", "");
        await ditto.Auth.LoginWithToken(password, "provider");
    }

    private static void HandleLogoutCommand()
    {
        ditto.Auth.Logout();
    }

    private static async Task HandleInsertCommand(string command)
    {
        string taskBody = command.Replace("--insert ", "");
        var task = new DittoTask(taskBody, false).ToDictionary();
        await ditto.Store.ExecuteAsync("INSERT INTO tasks DOCUMENTS (:task)", new Dictionary<string, object> { { "task", task } });
    }

    private static async Task HandleToggleCommand(string command)
    {
        string idToToggle = command.Replace("--toggle ", "");
        var isCompleted = tasks.First(t => t.Id == idToToggle).IsCompleted;
        await ditto.Store.ExecuteAsync(
            "UPDATE tasks " +
            "SET isCompleted = :newValue " +
            "WHERE _id == :id",
            new Dictionary<string, object> { { "newValue", !isCompleted }, { "id", idToToggle } });
    }

    private static async Task HandleDeleteCommand(string command)
    {
        string idToDelete = command.Replace("--delete ", "");
        var isDeleted = tasks.First(t => t.Id == idToDelete).IsDeleted;
        await ditto.Store.ExecuteAsync(
            "UPDATE tasks " +
            "SET isDeleted = :newValue " +
            "WHERE _id == :id",
            new Dictionary<string, object> { { "newValue", !isDeleted }, { "id", idToDelete } });
    }

    private static void HandleListTasksCommand()
    {
        tasks.ForEach(task =>
        {
            Console.WriteLine(task.ToString());
        });
    }

    private static void HandleExitCommand()
    {
        Console.WriteLine("Good bye!");
        ditto.Auth.Logout();
        isAskedToExit = true;
    }

    public static void ListCommands()
    {
        Console.WriteLine("************* Commands *************");

        Console.WriteLine("--login jellybeans");
        Console.WriteLine("   Logs in with a given password");
        Console.WriteLine("   Example: \"--login jellybeans\"");

        Console.WriteLine("--logout");
        Console.WriteLine("   Logout from Ditto auth");

        Console.WriteLine("--insert my new task");
        Console.WriteLine("   Inserts a task");
        Console.WriteLine("   Example: \"--insert Get Milk\"");
        
        Console.WriteLine("--toggle myTaskId");
        Console.WriteLine("   Toggles the isComplete property to the opposite value");
        Console.WriteLine("   Example: \"--toggle 1234abc\"");
        
        Console.WriteLine("--delete myTaskId");
        Console.WriteLine("   Deletes a task");
        Console.WriteLine("   Example: \"--delete 1234abc\"");
        
        Console.WriteLine("--list");
        Console.WriteLine("   List the current tasks");

        Console.WriteLine("--menu");
        Console.WriteLine("   List the menu options");
        
        Console.WriteLine("--exit");
        Console.WriteLine("   Exits the program");
        
        Console.WriteLine("************* Commands *************");
    }
}
