using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class UConsole : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("This will enable the mouse to be used incase of other scripts locking it, when this is check it will also lock back the mouse when finished. Default = False")]
    public bool enableMouse = false;
    [Tooltip("Enable Hud shows a HUD in the top right of the screen showing the number of logs, warnings or erros. Default = True")]
    public bool enableHUD = true;
    [Tooltip("This is the key to open the console. Default = Back Quote \"`\" ")]
    public KeyCode consoleKey = KeyCode.BackQuote;
    [Tooltip("This will cause UConsole to start outputting Debug.Log messages. Default = False")]
    public bool debugUConsole = false;


    /// <summary>
    /// Is true when the main console window is open. Read Only!
    /// </summary>
    public bool isOpen { get; private set; }
    public int logCount { get; private set; }
    public int warningCount { get; private set; }
    public int errorCount { get; private set; }
    /// <summary>
    /// This is an array of all the last arguments passed thought, this is split by ' '. Remember the first one is always going to be your command. Read Only!
    /// </summary>
    public string[] lastArgs { get; private set; }

    public struct ConsoleMessage
    {
        public string condition;
        public string stackTrace;
        public LogType type;
    }

    /// <summary>
    /// This is a list of all the current messages in the console.
    /// This list gets cleared with ClearLogs() and is stored in the same format as normal unity logs.
    /// Read Only!
    /// </summary>
    public List<ConsoleMessage> messages  { get; private set; }

    private GameObject mainWindow;
    private GameObject consoleHUD;
    private TextMeshProUGUI logsCounter, warningsCounter, errosCounter;
    private TextMeshProUGUI output;
    private RectTransform outputTransform;
    private TMP_InputField inputfield;
    private List<Command> commands = new List<Command>();
    private static UConsole uConsole;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (uConsole == null)
            uConsole = this;
        else
            Destroy(gameObject);

        messages = new List<ConsoleMessage>();
        mainWindow = transform.Find("MainWindow").gameObject;
        consoleHUD = transform.Find("ConsoleHUD").gameObject;
        logsCounter = consoleHUD.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        warningsCounter = consoleHUD.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        errosCounter = consoleHUD.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        output = mainWindow.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        outputTransform = output.rectTransform;
        inputfield = mainWindow.GetComponentInChildren<TMP_InputField>();
        mainWindow.SetActive(false);
        consoleHUD.SetActive(enableHUD);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += LogReceived;
        Log("UConsole Started!");
    }

    private void Start()
    {
        AddCommand("test", "Just a test command display the different types of messages you can get", Test);
        AddCommand("ping", "says a message back", Ping);
        AddCommand("clear", "clears the console", ClearLogs);
        AddCommand("uconsole.hud", "Used to enable or disable the hud of UConsole at the top, requires 1 more arguemnt eg \"uconsole.hud true\"",SetHUD);
        AddCommand("uconsole.mouse", "If the mouse gets unlocked when opening UConsole, requires 1 more arguemnt eg \"uconsole.mouse false\"", SetMouse);
        AddCommand("uconsole.debug", "If you want UConsole to output its debug messages to the console, requires 1 more arguemnt eg \"uconsole.debug false\"", ToggleDebug);
        AddCommand("help", "Displays all the possiable commands including custom ones, Help can take 1 extra arg which is help and then a command to just display about that one command", Help);
    }

    private void Update()
    {
        CheckForKeys();
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= LogReceived;
    }

    private void CheckForKeys()
    {
        if (Input.GetKeyDown(consoleKey))
        {
            SetActive(!isOpen);
            ToggleMouse();
        }

        if (inputfield.text != "" && inputfield.isFocused && Input.GetKey(KeyCode.Return))
        {
            FindCommand();
        }
    }

    private void Focus()
    {
        inputfield.ActivateInputField();
        inputfield.Select();
    }

    private void ToggleMouse()
    {
        if (enableMouse)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Log("Cursor State set to None");
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Log("Cursor State set to Locked");
            }
        }
    }

    private void ToggleHud()
    {
        consoleHUD.SetActive(enableHUD);
        Log("Hud visibility has been set to " + enableHUD);
    }

    /// <summary>
    /// Used to open or close the Main console window
    /// </summary>
    public void SetActive(bool state)
    {
        Log("Setting UConsole sate to " + state);
        isOpen = state;
        mainWindow.SetActive(state);

        if (state)
            Focus();
    }

    /// <summary>
    /// Add custom commands into UConsole
    /// </summary>
    public void AddCommand(string command, string description, Action action)
    {
        Regex regex = new Regex("^[a-zA-Z0-9.]*$");
        if (command.Contains(" "))
        {
            Debug.LogError("Command " + command + " contains spaces!");
        }
        else if (!regex.IsMatch(command))
        {
            Debug.LogError("Command " + command + " contains invalids characters!");
        }
        else
        {
            foreach (Command c in commands)
            {
                if (c.command.ToLower().Equals(command.ToLower()))
                {
                    Debug.LogError("Command " + command + " is already registered!");
                    return;
                }
            }

            Command newCommand = new Command();
            newCommand.action = action;
            newCommand.command = command;
            newCommand.description = description;
            commands.Add(newCommand);
            Log(newCommand.command + " has been added");
        }

    }

    /// <summary>
    /// Removes any custom commands
    /// </summary>
    public void RemoveCommand(string command)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].command == command)
            {
                commands.RemoveAt(i);
                Log(command + " has been removed");
                return;
            }
        }

        Debug.LogError(command + " could not be found to be removed from UConsole");
    }

    private void Log(object obj)
    {
        if (debugUConsole)
            Debug.Log(obj);
    }

    private void LogReceived(string condition, string stackTrace, LogType type)
    {
        ConsoleMessage lastMessage = new ConsoleMessage();
        lastMessage.condition = condition;
        lastMessage.stackTrace = stackTrace;
        lastMessage.type = type;
        messages.Insert(0, lastMessage);
  
        output.text = output.text + LogToString(lastMessage);

        if (type == LogType.Error)
        {
            errorCount++;
        }
        else if (type == LogType.Warning)
        {
            warningCount++;
        }
        else if (type == LogType.Log)
        {
            logCount++;
        }

        UpdateCounters();
    }

    private string LogToString(ConsoleMessage log)
    {
        string returnMessage = @"
";

        if (log.type == LogType.Error)
        {
            returnMessage += "<color=\"red\">";
        }
        else if (log.type == LogType.Warning)
        {
            returnMessage += "<color=#FFFF00>";
        }
        else if (log.type == LogType.Log)
        {
            returnMessage += "<color=#FFFFFF>";
        }
        returnMessage += log.condition;
        return returnMessage;
    }

    private void UpdateCounters()
    {
        logsCounter.text = logCount.ToString();
        warningsCounter.text = warningCount.ToString();
        errosCounter.text = errorCount.ToString();
    }

    /// <summary>
    /// Clears the in game console logs by destorying all the gameobjects and reseting the counters
    /// </summary>
    public void ClearLogs()
    {
        output.text = "";
        messages.Clear();
        logCount = 0;
        warningCount = 0;
        errorCount = 0;
    }

    public void FindCommand()
    {
        string message = inputfield.text;
        inputfield.text = "";
        Focus();
        Log("Command Received: " + message);
        string[] args = message.Split(' ');

        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].command == args[0])
            {
                lastArgs = args;
                commands[i].action();
                return;
            }
        }

        UnknowCommand(message);
    }

    private void Test()
    {
        Debug.Log("This is an log message");
        Debug.LogWarning("This is an warning message");
        Debug.LogError("This is an error message");
    }

    private void Ping()
    {
        Debug.Log("Pong!");
    }

    private void Help()
    {
        string[] args = lastArgs;
        if (args.Length == 1)
        {
            LogReceived("<b> <align=\"center\"> Help </align> </b>", "", LogType.Log);
            LogReceived("", "", LogType.Log);
            for (int i = 0; i < commands.Count; i++)
            {
                LogReceived("<b>" + commands[i].command + "</b>", "", LogType.Log);
                LogReceived(commands[i].description, "", LogType.Log);
                LogReceived("", "", LogType.Log);
            }
        }
        else
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].command == args[1])
                {
                    LogReceived("<b> Help </b>", "", LogType.Log);
                    LogReceived("", "", LogType.Log);
                    LogReceived("<b>" + commands[i].command + "</b>", "", LogType.Log);
                    LogReceived(commands[i].description, "", LogType.Log);
                    return;
                }
            }

            LogReceived("Unknow Command", "", LogType.Warning);
        }
    }

    private void ToggleDebug()
    {
        string[] args = lastArgs;
        if (args.Length < 2)
        {
            Debug.LogError("uconsole.debug requires another argument eg \"uconsole.debug false\"");
        }
        else if (args[1] == "false" || args[1] == "False" || args[1] == "0" || args[1] == "f" || args[1] == "F")
        {
            debugUConsole = false;
            Debug.Log("UConsole Debug Mode is now " + debugUConsole);
        }
        else if (args[1] == "true" || args[1] == "True" || args[1] == "1" || args[1] == "t" || args[1] == "T")
        {
            debugUConsole = true;
            Debug.Log("UConsole Debug Mode is now " + debugUConsole);
        }
        else
        {
            Debug.LogWarning("Unknow argument of " + args[1]);
            Debug.Log("uconsole.debug requires another argument eg \"uconsole.debug false\"");
        }
    }

    private void SetHUD()
    {
        string[] args = lastArgs;
        if (args.Length < 2)
        {
            Debug.LogError("uconsole.hud requires another argument eg \"uconsole.hud false\"");
        }
        else if (args[1] == "false" || args[1] == "False" || args[1] == "0" || args[1] == "f" || args[1] == "F")
        {
            enableHUD = false;
            Debug.Log("UConsole's HUD now hidden");
        }
        else if (args[1] == "true" || args[1] == "True" || args[1] == "1" || args[1] == "t" || args[1] == "T")
        {
            enableHUD = true;
            Debug.Log("UConsole's HUD now visiable");
        }
        else
        {
            Debug.LogWarning("Unknow argument of " + args[1]);
            Debug.Log("uconsole.hud requires another argument eg \"uconsole.hud false\"");
        }
        ToggleHud();
    }

    private void SetMouse()
    {
        string[] args = lastArgs;
        if (args.Length < 2)
        {
            Debug.LogError("uconsole.mouse requires another argument eg \"uconsole.mouse false\"");
        }
        else if (args[1] == "false" || args[1] == "False" || args[1] == "0" || args[1] == "f" || args[1] == "F")
        {
            enableMouse = false;
            Debug.Log("Your mouse will now stay its current state when using UConsole");
        }
        else if (args[1] == "true" || args[1] == "True" || args[1] == "1" || args[1] == "t" || args[1] == "T")
        {
            enableMouse = true;
            Debug.Log("Your mouse will now show and hide when using UConsole");
        }
        else
        {
            Debug.LogWarning("Unknow argument of " + args[1]);
            Debug.Log("uconsole.mouse requires another argument eg \"uconsole.mouse false\"");
        }
    }

    private void UnknowCommand(string message)
    {
        Debug.LogWarning("Unknow Command: " + message);
    }
}

public class Command
{
    /// <summary>
    /// This is the description of the command, this description will be used in the help menu explain what this command does 
    /// </summary>
    public string description;
    /// <summary>
    /// This is the command which you type into the console to run it
    /// </summary>
    public string command;
    /// <summary>
    /// This is the action which UConsole will call. 
    /// </summary>
    public Action action;
}