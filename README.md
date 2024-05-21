# Flux.Hotkeys

A fluent way to define hotkeys  

```cs
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

// Can even have multiple hotkeys with unique callbacks
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
// TODO: add more object type support
var zValue = Ahk.Var.Get<int>("z");
Console.WriteLine(zValue); // "Value of z is 5"

//Load a library or execute scripts in a file
Ahk.LoadFile("functions.ahk");

// Execute a specific function (found in functions.ahk), with 2 parameters
Ahk.Func.Call("MyFunction", "Hello", "World");

// Execute a label
Ahk.Label.Call("DOSTUFF");

// Execute a function (in functions.ahk) that adds 5 and return results
Console.WriteLine(Ahk.Evaluate("Add5(5)"));

// You can also return results with Ahk.Func.Call
Console.WriteLine(Ahk.Func.Call("Add5", "5"));

// Make sure to call this if you're adding callbacks to hotkeys
Ahk.RegisterCallbacks();

Console.WriteLine("Press enter to exit...");
Console.ReadLine();
```
