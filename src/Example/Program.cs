using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Collections.Generic;

using Cysharp.Text;

using Flux.Hotkeys;
using Flux.Hotkeys.Actions;
using Flux.Hotkeys.Util;
using Flux.Hotkeys.Pipes;

// ReSharper disable RedundantLambdaParameterType

namespace Example;

[SupportedOSPlatform("windows")]
internal static class Program
{
    private static void Main()
    {
        // Add to script, Run immediately, or Run and Wait for code execution to finish 
        // or execute immediately
        var success = Ahk.CreateBlock() 
            .Action(Keyboard.Down(Key.Alt))
            .Action(Keyboard.Press(Key.Tab))
            .Action(Keyboard.Press(Key.LeftArrow))
            .Action(Ahk.Sleep(1.Seconds()))
            .Action(Keyboard.Press(Key.LeftArrow))
            .Action(Ahk.Sleep(1.Seconds()))
            .Execute(out var code);

        if (success)
        {
            Console.WriteLine("Successfully executed block:");
            Console.WriteLine(code);
        }
        
        var hotkey = Ahk.Hotkey(Key.A, [Key.Control])
            // Define the hotkeys method body
            .Actions(
                Keyboard.Press(Key.Num3),
                Ahk.Sleep(500.Milliseconds()),
                Keyboard.Press(Key.Space),
                Ahk.Sleep(75.Milliseconds()),
                Keyboard.Press(Key.Num5),
                (RawAction) $"v := {"You can even cast a string to become an action".Quote()}",
                (RawAction)AhkFmt.MsgBox("v", true))
            // blocks/intercepts input from all other sources or allows it to pass through and be seen
            .Block(false)
            // You can only register OnUp OR OnDown
            // signals that this should only activate when key(s) are released
            .OnUp(string (string hotkeyA) =>
            {
                Console.WriteLine($"{hotkeyA} was invoked");
                return "Success";
            });
        
        var hotkey2 = Ahk.Hotkey(Key.B, [Key.Control])
            // Define the hotkeys method body
            .Actions(
                Keyboard.Press(Key.Num3),
                Ahk.Sleep(500.Milliseconds()),
                Keyboard.Press(Key.Space),
                Ahk.Sleep(75.Milliseconds()),
                Keyboard.Press(Key.Num5),
                (RawAction) $"v := {"You can even cast a string to become an action".Quote()}",
                (RawAction)AhkFmt.MsgBox("v", true))
            // blocks/intercepts input from all other sources or allows it to pass through and be seen
            .Block(false)
            // You can only register OnUp OR OnDown
            // signals that this should only activate when key(s) are released
            .OnUp(string (string hotkeyA) =>
            {
                Console.WriteLine($"{hotkeyA} was invoked");
                return "Success";
            }, CallbackLocation.End);
      
        // Get success/failure state of newly added/executed code
        success = Ahk.CreateBlock()
            .Def(hotkey) // defines a hotkey, 
            .Def(hotkey2) // defines a hotkey, 
            .Complete(out code, ExecuteOption.Add); // and even spit out the raw text itself for debugging purposes

        if (success)
        {
            Console.WriteLine("Successfully added hotkey:");
            Console.WriteLine(code);
        }
        
        // Handful of methods inside 'AhkFmt' to allow easy convert to raw AHK 
        Ahk.Execute(AhkFmt.MsgBox("Title", "Message"));
        
        // Programmatically set variables, and ensure they actually got set  
        Ahk.Var.Set("x", 1);
        if (!Ahk.Var.Set("y", 4))
        {
            Console.WriteLine("y was not Set");
        }

        // Set multiple vars are the same time
        var setVars = Ahk.SetVars(new Dictionary<string, string> {
            {"varX", "12"},
            {"varY", "32"},
        });
        
        // Iterate over variables that were set and check which were and weren't 
        foreach (var v in setVars.Where(pair => !pair.Value))
        {
            Console.WriteLine($"{v.Key} was not set");
        }

        // Executes statements in place, and ensure it actually ran
        if (!Ahk.Execute(AhkFmt.Set("z", "x+y")))
        {
            Console.WriteLine("Unable to execute code");
        }

        // Return variables back from AHK
        var zValue = Ahk.Var.Get<int>("z");
        Console.WriteLine("Value of z is {0}", zValue); // "Value of z is 5"

        //Load a library or exec scripts in a file
        Ahk.LoadFile("functions.ahk");

        //execute a specific function (found in functions.ahk), with 2 parameters
        Ahk.Func.Call("MyFunction", "Hello", "World");

        //execute a label
        Ahk.Label.Call("DOSTUFF");

        //create a new function
        // string sayHelloFunction = "SayHello(name) \r\n { \r\n MsgBox, Hello %name% \r\n return \r\n }";
        // Ahk.Execute(sayHelloFunction);


        //execute a function (in functions.ahk) that adds 5 and return results
        var add5Results = Ahk.Evaluate("Add5( 5 )");
        Console.WriteLine("Eval: Result of 5 with Add5 func is {0}", add5Results);

        //you can also return results with the ExecFunction
        add5Results = Ahk.Func.Call("Add5", "5");
        Console.WriteLine("ExecFunction: Result of 5 with Add5 func is {0}", add5Results);

        //you can have AutoHotkey communicate with the hosting environment
        // 1 - Create Handler for your ahk code
        // 2 - Initalize Pipes Module, passing in your handler
        // 3 - Use 'SendPipeMessage(string)' from your AHK code

        /*
        var pipeHandler = new PipeMessageHandler(incomingMessage => {
            Console.WriteLine($"AHK Message: {incomingMessage}");

            return "Funk yeah concrete";
        });

        Ahk.AddPipeHandler(pipeHandler);

        success = Ahk.CreateBlock()
            .Action("serverResponse := SendPipeMessage(\"Yeeeeeea boiiiiiiii\")\n")
            .Action($"{AhkFmt.MsgBox($".NET Message: {AhkFmt.Expression("serverResponse")}")}")
            .Execute(out code);

        */
        // Console.WriteLine(!success ? "Unable to execute script" : code);
        Ahk.RegisterCallbacks();
        
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
    }
}