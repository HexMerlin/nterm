using System.Globalization;
using Nterm.Core;
using Nterm.Core.Controls;
using AutosuggestOptions = Nterm.Core.Controls.AutosuggestOptions<string>;
using AutosuggestResult = Nterm.Core.Controls.AutosuggestResult<string>;

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
string userInput = Terminal.ReadLine();
Terminal.Write("Now select an option: ");

List<TextItem<Action>> items =
[
    new()
    {
        Text = "Option A with long text",
        Value = () => Terminal.WriteLine("Doing stuff with Option A")
    },
    new() { Text = "Option B", Value = () => Terminal.WriteLine("Doing stuff with Option B") },
    new()
    {
        Text = "Option C with long text",
        Value = () => Terminal.WriteLine("Doing stuff with Option C")
    },
    new()
    {
        Text = "Option D with long text",
        Value = () => Terminal.WriteLine("Doing stuff with Option D")
    },
    new()
    {
        Text = "Option E with long text",
        Value = () => Terminal.WriteLine("Doing stuff with Option E")
    },
    new() { Text = "Option F", Value = () => Terminal.WriteLine("Doing stuff with Option F") },
    new()
    {
        Text = "Option G with very long text that goes on and on and on",
        Value = () => Terminal.WriteLine("Doing stuff with Option G")
    },
    new()
    {
        Text = "Option H with very long text that goes on and on and on and off",
        Value = () => Terminal.WriteLine("Doing stuff with Option H")
    },
    new() { Text = "Option I", Value = () => Terminal.WriteLine("Doing stuff with Option I") },
    new() { Text = "Option J", Value = () => Terminal.WriteLine("Doing stuff with Option J") },
    new() { Text = "Option K", Value = () => Terminal.WriteLine("Doing stuff with Option K") },
    new() { Text = "Option L", Value = () => Terminal.WriteLine("Doing stuff with Option L") },
    new() { Text = "Option M", Value = () => Terminal.WriteLine("Doing stuff with Option M") },
];

TextItem<Action> selectedItem1 = SelectMenu.Show(items);
if (selectedItem1.IsEmpty())
{
    Terminal.WriteLine("Selection cancelled");
    return;
}

Terminal.Write(" and now select another option: ");
TextItem<Action> selectedItem2 = SelectMenu.Show(items, 1);

if (!selectedItem1.IsEmpty() && !selectedItem2.IsEmpty())
{
    Terminal.WriteLine($"\nSelected: {selectedItem1.Text} and {selectedItem2.Text}");
    selectedItem1.Value.Invoke();
    selectedItem2.Value.Invoke();
}
else
{
    Terminal.WriteLine("Selection cancelled");
    return;
}

Terminal.Write("Type Console...: ");

string[] candidates = ["Console.WriteLine", "Console.ReadKey", "Console.ReadLine"];
AutosuggestResult result = Autosuggest.Read(
    (current, _) =>
    {
        string text =
            candidates.FirstOrDefault(c => c.Contains(current, StringComparison.OrdinalIgnoreCase))
            ?? string.Empty;
        return new TextItem<string>
        {
            Text = text,
            Value = candidates.IndexOf(text).ToString(CultureInfo.InvariantCulture)
        };
    },
    options: new AutosuggestOptions
    {
        GetNextSuggestion = (current, previous) =>
        {
            string previousText = previous?.Text.ToString() ?? string.Empty;
            string text =
                candidates
                    .Except([previousText])
                    .LastOrDefault(c => c.Contains(current, StringComparison.OrdinalIgnoreCase))
                ?? string.Empty;
            return new TextItem<string> { Text = text, Value = text };
        },
        GetPreviousSuggestion = (current, previous) =>
        {
            string previousText = previous?.Text.ToString() ?? string.Empty;
            string text =
                candidates
                    .Except([previousText])
                    .FirstOrDefault(c => c.Contains(current, StringComparison.OrdinalIgnoreCase))
                ?? string.Empty;
            return new TextItem<string> { Text = text, Value = text.ToString() };
        },
    }
);

Terminal.WriteLine();
Terminal.WriteLine($"Selected suggestion: {result.LastSuggestion?.Value}");
Terminal.WriteLine($"Typed text: {result.TypedText}");

TextItem<FileSystemInfo> picked = FilePicker.Show(
    options: new()
    {
        FileExtensions = [],
        ShowOnlyDirectories = false,
        ShowHiddenFilesAndDirectories = true,
        AllowNavigationAboveStartDirectory = false
    }
);
if (!picked.IsEmpty())
{
    FileSystemInfo fs = picked.Value; // FileInfo or DirectoryInfo
    Terminal.WriteLine($"Picked: {fs.FullName}");
    // handle result
}

// TextInputController textInput =
//     new(state =>
//     {
//         //Terminal.SetCursorPosition();
//         Terminal.WriteLine($"Typed text: {state.Text}");
//     });
// TextInputState state = textInput.Read();

// Terminal.WriteLine();
// Terminal.WriteLine($"Typed text: {state.Text}");

Terminal.WriteLine("\nPress any key to exit...");
ConsoleKeyInfo key = Terminal.ReadKey();

Terminal.WriteLine($"Key pressed: {key.Key}, Modifiers: {key.Modifiers}");
