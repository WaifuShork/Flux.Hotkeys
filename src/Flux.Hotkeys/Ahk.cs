using System;
using System.Numerics;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Flux.Hotkeys.Actions;
using Flux.Hotkeys.Util.Exceptions;
using Flux.Hotkeys.Pipes;
using Flux.Hotkeys.Util;

namespace Flux.Hotkeys;

/// <summary>
/// This class expects an AutoHotkey.dll to be available on the machine. (UNICODE) version.
/// </summary>
[PublicAPI]
public class Ahk
{
    internal const string DllPath = "AutoHotkey.dll";
    public static bool ThrowOnWarning { get; set; } = true;

    public static Dictionary<string, HotkeyCallback?> HotkeyCallbacks = [];
    
    static Ahk() 
    {
        LibraryLoader.EnsureDllIsLoaded();
        var threadId = Internals.LoadText("", "", "");
        if (threadId == 0 || !Internals.IsAhkReady())
        { 
            Console.WriteLine("Unable to initialize AutoHotkey.dll");
        }
        
        RegisterCallbacks();
    }

    public static void RegisterCallbacks()
    {
        var pipeHandler = new PipeMessageHandler(incomingMessage =>
        {
            if (HotkeyCallbacks.TryGetValue(incomingMessage, out var method))
            {
                return method?.Invoke(incomingMessage) ?? "";
            }

            return "";
        });
        
        AddPipeHandler(pipeHandler);
    }

    public static RawAction Snippet(string code) => RawAction.Snippet(code);
    public static SleepAction Sleep(TimeSpan duration) => new SleepAction().For(duration);
    public static HotkeyAction Hotkey((Key left, Key right) keyPair, IEnumerable<Key>? modifiers = default, bool isBlocking = false, SendMode sendMode = SendMode.Input) => new HotkeyAction().Combine(keyPair, modifiers, isBlocking, sendMode);
    public static HotkeyAction Hotkey(Key key, IEnumerable<Key>? modifiers = default, bool isBlocking = false, SendMode sendMode = SendMode.Input) => new HotkeyAction().Normal(key, modifiers, isBlocking, sendMode);

    public static AhkBlock CreateBlock(string initialSnippet = "")
    {
        return AhkBlock.Create(initialSnippet);
    }
    
    public static Dictionary<string, bool> SetVars(Dictionary<string, string> variables)
    {
        var setVars = new Dictionary<string, bool>();
        foreach (var (name, value) in variables)
        {
            setVars[name] = Var.Set(name, value);
        }

        return setVars;
    }

    /// <summary>
    /// Evaulates an expression or function and returns the results
    /// </summary>
    /// <param name="code">The code to execute</param>
    /// <returns>Returns the result of an expression</returns>
    public static string Evaluate(string code)
    {
        var codeToRun = "A__EVAL:=" + code;
        return Internals.AhkExec(codeToRun) ? Var.Get("A__EVAL") : "";
    }

    /// <summary>
    /// Loads a file into the running script
    /// </summary>
    /// <param name="filePath">The filepath of the script</param>
    /// <param name="option"></param>
    public static bool LoadFile(string filePath, ExecuteOption option = ExecuteOption.Run)
    {
        return Internals.AddFile(filePath, option);
    }

    public static bool AddSnippet(string scriptText, ExecuteOption option = ExecuteOption.Run)
    { 
        return Internals.AddScript(scriptText, option);
    }

    /// <summary>
    /// Loads a scripts contents into the running script
    /// </summary>
    /// <param name="scriptText">The contents of the scripts file</param>
    /// <param name="option"></param>
    public static bool LoadScript(string scriptText, ExecuteOption option = ExecuteOption.Run) 
    {
        return Internals.AddScript(scriptText, option);
    }
    
    /// <summary>
    /// Executes raw ahk code.
    /// </summary>
    /// <param name="code">The code to execute</param>
    public static bool Execute(string code)
    {
        return Internals.AhkExec(code);
    }

    /// <summary>
    /// Terminates the running scripts
    /// </summary>
    public static void Terminate(uint timeout = 1000)
    {
        Internals.AhkTerminate(timeout);
    }

    public static void Reset() 
    {
        Terminate();
        Internals.AhkReload();
        var threadId = Internals.LoadText("", "", "");
    }

    /// <summary>
    /// Suspends the scripts
    /// </summary>
    public static void Suspend()
    {
        Execute("Suspend, On");
    }

    /// <summary>
    /// Unsuspends the scripts
    /// </summary>
    public static void Unsuspend()
    {
        Execute("Suspend, Off");
    }

    /// <summary>
    /// Enables communication between AutoHotkey code and the hosting enviorment.
    /// This module imports an AHK function named SendPipeMessage that you can use
    /// call the specified handler.
    /// </summary>
    /// <param name="sendPipeMessageHandler">The handler that will receive the SendPipesMessage from AHK.</param>
    public static void AddPipeHandler(PipeMessageHandler sendPipeMessageHandler) 
    {
        AhkPipes.LoadPipesModule(sendPipeMessageHandler);
    }

    private static class Internals
    {
        /// <summary>
        /// Start new thread from ahk file.
        /// </summary>
        /// <param name="path">This parameter must be a path to existing ahk file.</param>
        /// <param name="options">Additional parameter passed to AutoHotkey.dll (not available in Version 2 alpha).</param>
        /// <param name="parameters">Parameters passed to dll.</param>
        /// <returns>	ahkdll returns a thread handle.</returns>
        /// <remarks>ahktextdll is available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkdll")]
        public static extern uint LoadFile([MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string options, [MarshalAs(UnmanagedType.LPWStr)] string parameters);

        /// <summary>
        /// ahktextdll is used to launch a script in a separate thread from text/variable.
        /// </summary>
        /// <param name="code">This parameter must be a string with ahk script.</param>
        /// <param name="options">Additional parameter passed to AutoHotkey.dll (not available in Version 2 alpha).</param>
        /// <param name="parameters">Parameters passed to dll.</param>
        /// <returns>ahkdll returns a thread handle.</returns>
        /// <remarks>ahktextdll is available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahktextdll")]
        public static extern uint LoadText([MarshalAs(UnmanagedType.LPWStr)] string code, [MarshalAs(UnmanagedType.LPWStr)] string options, [MarshalAs(UnmanagedType.LPWStr)] string parameters);
        
        /// <summary>
        /// ahkReady is used to check if a dll script is running or not.
        /// </summary>
        /// <returns>1 if a thread is running or 0 otherwise.</returns>
        /// <remarks>Available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkReady")]
        public static extern bool IsAhkReady();
        
        /// <summary>
        /// ahkTerminate is used to stop and exit a running script.
        /// </summary>
        /// <param name="timeout">Time in milliseconds to wait until thread exits.</param>
        /// <returns>Returns always 0.</returns>
        /// <remarks>Available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkTerminate")]
        public static extern void AhkTerminate(uint timeout);

        /// <summary>
        /// ahkReload is used to terminate and start a running script again.
        /// </summary>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkReload")]
        public static extern void AhkReload();

        /// <summary>
        /// ahkPause will pause/un-pause a thread and run traditional AutoHotkey Sleep internally.
        /// </summary>
        /// <param name="strState">Should be "On" or "Off" as a string</param>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkPause")]
        private static extern void AhkPause([MarshalAs(UnmanagedType.LPWStr)] string strState);

        public static void Pause()
        {
            AhkPause("On");
        } 
        
        public static void Resume()
        {
            AhkPause("Off");
        }

        /// <summary>
        /// addFile includes additional script from a file to the running script.
        /// </summary>
        /// <param name="filePath">Path to a file that will be added to a running script.</param>
        /// <param name="execute"></param>
        /// <returns>addFile returns a pointer to the first line of new created code.</returns>
        /// <remarks>pointerLine can be used in ahkExecuteLine to execute one line only or until a return is encountered.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "addFile")]
        private static extern uint AddFile([MarshalAs(UnmanagedType.LPWStr)]string filePath, byte execute);
        public static bool AddFile(string filePath, ExecuteOption options)
        {
            return AddFile(filePath, (byte)options) != 0;
        }

        /// <summary>
        /// addScript includes additional script from a string to the running script.
        /// </summary>
        /// <param name="code">cript that will be added to a running script.</param>
        /// <param name="execute">Determines whether the added script should be executed.</param>
        /// <returns>addScript returns a pointer to the first line of new created code.</returns>
        /// <remarks>pointerLine can be used in ahkExecuteLine to execute one line only or until a return is encountered.</remarks>
        [DllImport(Ahk.DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "addScript")]
        private static extern uint AddScript([MarshalAs(UnmanagedType.LPWStr)]string code, byte execute);
        public static bool AddScript(string code, ExecuteOption executeOption = ExecuteOption.Run)
        {
            return AddScript(code, (byte)executeOption) != 0;
        }

        /// <summary>
        /// Execute a script from a string that contains ahk script.
        /// </summary>
        /// <param name="code">Script as string/text or variable containing script that will be executed.</param>
        /// <returns>Returns true if script was executed and false if there was an error.</returns>
        /// <remarks>ahkExec will execute the code and delete it before it returns.</remarks>
        [DllImport(Ahk.DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkExec")]
        public static extern bool AhkExec([MarshalAs(UnmanagedType.LPWStr)] string code);
    } 

    [PublicAPI]
    public static class Label
    {
        /// <summary>
        /// Executes a label
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        public static void Call(string labelName)
        {
            AhkLabel(labelName, false);
        }

        /// <summary>
        /// Determines if the label exists.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        /// <returns>Returns true if the label exists, otherwise false</returns>
        public static bool Exists(string labelName)
        {
            var labelPtr = AhkFindLabel(labelName);
            return labelPtr != IntPtr.Zero;
        }
        
        /// <summary>
        /// ahkLabel is used to launch a Goto/GoSub routine in script.
        /// </summary>
        /// <param name="labelName">Name of label to execute.</param>
        /// <param name="noWait">Do not to wait until execution finished. </param>
        /// <returns>	1 if label exists 0 otherwise.</returns>
        /// <remarks>Default is 0 = wait for code to finish execution.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkLabel")]
        internal static extern bool AhkLabel([MarshalAs(UnmanagedType.LPWStr)] string labelName, bool noWait);
    
        /// <summary>
        /// ahkFindLabel is used to get a pointer to the label.
        /// </summary>
        /// <param name="labelName">Name of label.</param>
        /// <returns>ahkFindLabel returns a pointer to a line where label points to.</returns>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkFindLabel")]
        internal static extern IntPtr AhkFindLabel([MarshalAs(UnmanagedType.LPWStr)] string labelName);
    }
    
    [PublicAPI]
    public static class Func
    {
        /// <summary>
        /// Calls an already defined function.
        /// </summary>
        /// <param name="functionName">The name of the function to execute.</param>
        /// <param name="parameters">The 1st parameter</param>
        public static string Call(string functionName, params string[] parameters)
        {
            if (parameters.Length > 10 && ThrowOnWarning)
            {
                throw new AhkException($"Expected less than 10 parameters, got ({parameters.Length})");
            }

            var valuePtr = (IntPtr)(typeof(Func).GetMethod("AhkFunction")?.Invoke(null, [functionName, .. parameters]) ?? IntPtr.Zero);
            return (valuePtr == IntPtr.Zero ? null : Marshal.PtrToStringUni(valuePtr)) ?? "";
        }
        
        /// <summary>
        /// Determines if the function exists.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Returns true if the function exists, otherwise false.</returns>
        public static bool Exists(string functionName)
        {
            return Find(functionName) != IntPtr.Zero;
        }

        public static IntPtr Find(string functionName)
        {
            return AhkFindFunc(functionName);
        }
        
        /// <summary>
        /// ahkFunction is used to launch a function in script.
        /// </summary>
        /// <param name="functionName">Name of function to call.</param>
        /// <param name="parameter1">The 1st parameter, or null</param>
        /// <param name="parameter2">The 2nd parameter, or null</param>
        /// <param name="parameter3">The 3rd parameter, or null</param>
        /// <param name="parameter4">The 4th parameter, or null</param>
        /// <param name="parameter5">The 5th parameter, or null</param>
        /// <param name="parameter6">The 6th parameter, or null</param>
        /// <param name="parameter7">The 7th parameter, or null</param>
        /// <param name="parameter8">The 8th parameter, or null</param>
        /// <param name="parameter9">The 9th parameter, or null</param>
        /// <param name="parameter10">The 10th parameter, or null</param>
        /// <returns>	Return value is always a string/text, add 0 to make sure it resolves to digit if necessary.</returns>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkFunction")]
        internal static extern IntPtr AhkFunction(
            [MarshalAs(UnmanagedType.LPWStr)] string functionName,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter1,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter2,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter3,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter4,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter5,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter6,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter7,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter8,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter9,
            [MarshalAs(UnmanagedType.LPWStr)] string? parameter10);

        /// <summary>
        /// ahkFunction is used to launch a function in script.
        /// </summary>
        /// <param name="functionName">Name of function to call.</param>
        /// <param name="parameters">Parameters to pass to function.</param>
        /// <returns>0 if function exists else -1.</returns>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkPostFunction")]
        internal static extern bool AhkPostFunction([MarshalAs(UnmanagedType.LPWStr)] string functionName, [MarshalAs(UnmanagedType.LPWStr)] string parameters);
        
        /// <summary>
        /// ahkFundFunc is used to get function its pointer
        /// </summary>
        /// <param name="funcName">Name of function to call.</param>
        /// <returns>Function pointer.</returns>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkFindFunc")]
        internal static extern IntPtr AhkFindFunc([MarshalAs(UnmanagedType.LPWStr)] string funcName);
    }

    [PublicAPI]
    public static class Var
    {
        /// <summary>
        /// Gets the value for a variable or an empty string if the variable does not exist.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Returns the value of the variable, or an empty string if the variable does not exist.</returns>
        public static string Get(string variableName)
        {
            var p = GetInternal(variableName, false);
            return Marshal.PtrToStringUni(p) ?? "";
        }

        public static T Get<T>(string name) where T : struct, INumber<T>
        {
            var value = Get(name);
            return T.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : T.Zero;
        }

        public static IntPtr GetPtr(string variableName)
        {
            return GetInternal(variableName, true);
        }

        /// <summary>
        /// Sets the value of a variable.
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public static bool Set(string varName, string? value = null)
        {
            value ??= "";
            return SetInternal(varName, value);
        }

        public static bool Set<T>(string varName, T value) where T : struct, INumber<T>
        {
            if (typeof(T).GetMethods().Any(m => m.Name == "ToString"))
            {
                return Set(varName, value.ToString());
            }

            throw new InvalidOperationException($"Type {typeof(T)} does not contain ToString");
        }
        
        /// <summary>
        /// Build in function to get a pointer to the structure of a user-defined variable. 
        /// </summary>
        /// <param name="variable">the name of the variable</param>
        /// <returns>The pointer to the variable.</returns>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getVar")]
        internal static extern IntPtr AhkGetVar([MarshalAs(UnmanagedType.LPWStr)] string variable);
        
        /// <summary>
        /// ahkassign is used to assign a string to a variable in script.
        /// </summary>
        /// <param name="variableName">Name of a variable.</param>
        /// <param name="newValue">Value to assign to variable.</param>
        /// <returns>Returns value is 0 on success and -1 on failure.</returns>
        /// <remarks>ahkassign will create the variable if it does not exist.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkassign")]
        private static extern int Assign([MarshalAs(UnmanagedType.LPWStr)] string variableName, [MarshalAs(UnmanagedType.LPWStr)] string newValue);

        internal static bool SetInternal(string variableName, string value)
        {
            return Assign(variableName, value) == 0;
        }
        
        /// <summary>
        /// ahkgetvar is used to get a value from a variable in script. 
        /// </summary>
        /// <param name="variableName">Name of variable to get value from.</param>
        /// <param name="getPointer">Get value or pointer.</param>
        /// <returns>Returned value is always a string, add 0 to convert to integer if necessary, especially when using getPointer.</returns>
        /// <remarks>ahkgetvar returns empty string if variable does not exist or is empty.</remarks>
        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ahkgetvar")]
        private static extern IntPtr AhkGetVar([MarshalAs(UnmanagedType.LPWStr)] string variableName, [MarshalAs(UnmanagedType.I4)] int getPointer);

        internal static IntPtr GetInternal(string variableName, bool getPointer)
        {
            return AhkGetVar(variableName, getPointer ? 1 : 0);
        }
    }
}