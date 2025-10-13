using Nterm.Core;
using Nterm.Examples;

Terminal.ForegroundColor = Color.White;

Terminal.Title = "Nterm Controls Example";

Terminal.WriteLine("Nterm Controls Example");
Terminal.WriteLine("==============================");
Terminal.WriteLine();

// Test the Select control with cursor position functionality
Terminal.WriteLine("Testing Select Control - Cursor Position");
Terminal.WriteLine(
    "================================ very long text that is longer than the terminal width what will happen to the text now? ===================================="
);
Terminal.WriteLine();

Terminal.Write("Type something here: ");
_ = Terminal.ReadLine();

SelectMenuDemo.Run();
AutosuggestDemo.Run();
FilePickerDemo.Run();

Terminal.WriteLine("\nPress any key to exit...");
ConsoleKeyInfo key = Terminal.ReadKey();

Terminal.WriteLine($"Key pressed: {key.Key}, Modifiers: {key.Modifiers}");
