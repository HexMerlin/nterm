using NTerm.Controls;
using NTerm.Core;

Terminal.ForegroundColor = Color.White;

Terminal.Title = "Nterm Controls Example";

Terminal.WriteLine("Nterm Controls Example");
Terminal.WriteLine("==============================");
Terminal.WriteLine();

// Test the Select control with cursor position functionality
Terminal.WriteLine("Testing Select Control - Cursor Position");
Terminal.WriteLine("=======================================");
Terminal.WriteLine();

Terminal.Write("Type something here: ");
string userInput = Terminal.ReadLine();
Terminal.Write("Now select an option: ");

List<SelectItem<Action>> items =
[
    new() { Text = "Option A", Value = () => Terminal.WriteLine("Doing stuff with Option A") },
    new() { Text = "Option B", Value = () => Terminal.WriteLine("Doing stuff with Option B") },
    new() { Text = "Option C", Value = () => Terminal.WriteLine("Doing stuff with Option C") },
    new() { Text = "Option D", Value = () => Terminal.WriteLine("Doing stuff with Option D") },
    new() { Text = "Option E", Value = () => Terminal.WriteLine("Doing stuff with Option E") },
    new() { Text = "Option F", Value = () => Terminal.WriteLine("Doing stuff with Option F") },
    new() { Text = "Option G", Value = () => Terminal.WriteLine("Doing stuff with Option G") },
    new() { Text = "Option H", Value = () => Terminal.WriteLine("Doing stuff with Option H") },
    new() { Text = "Option I", Value = () => Terminal.WriteLine("Doing stuff with Option I") },
    new() { Text = "Option J", Value = () => Terminal.WriteLine("Doing stuff with Option J") },
    new() { Text = "Option K", Value = () => Terminal.WriteLine("Doing stuff with Option K") },
    new() { Text = "Option L", Value = () => Terminal.WriteLine("Doing stuff with Option L") },
    new() { Text = "Option M", Value = () => Terminal.WriteLine("Doing stuff with Option M") },
];

SelectItem<Action> selectedItem1 = Select.Show(items);
if (selectedItem1.IsEmpty())
{
    Terminal.WriteLine("Selection cancelled");
    return;
}

Terminal.Write(" and now select another option: ");
SelectItem<Action> selectedItem2 = Select.Show(items, 1);

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

Terminal.Write("Type something else here: ");
string userInput2 = Terminal.ReadLine();

Terminal.WriteLine("\nPress any key to exit...");
ConsoleKeyInfo key = Terminal.ReadKey();

Terminal.WriteLine($"Key pressed: {key.Key}, Modifiers: {key.Modifiers}");
