using System.Globalization;
using Nterm.Core.Controls;
using AutosuggestOptions = Nterm.Core.Controls.AutosuggestOptions<string>;
using AutosuggestResult = Nterm.Core.Controls.AutosuggestResult<string>;

namespace Nterm.Examples;

public class AutosuggestDemo
{
    public static void Run()
    {
        Terminal.Write("Type \"Console\": ");
        string[] candidates = ["Console.WriteLine", "Console.ReadKey", "Console.ReadLine"];
        AutosuggestResult result = Autosuggest.Read(
            (current, _) =>
            {
                string text =
                    candidates.FirstOrDefault(c =>
                        c.Contains(current, StringComparison.OrdinalIgnoreCase)
                    ) ?? string.Empty;
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
                            .LastOrDefault(c =>
                                c.Contains(current, StringComparison.OrdinalIgnoreCase)
                            ) ?? string.Empty;
                    return new TextItem<string> { Text = text, Value = text };
                },
                GetPreviousSuggestion = (current, previous) =>
                {
                    string previousText = previous?.Text.ToString() ?? string.Empty;
                    string text =
                        candidates
                            .Except([previousText])
                            .FirstOrDefault(c =>
                                c.Contains(current, StringComparison.OrdinalIgnoreCase)
                            ) ?? string.Empty;
                    return new TextItem<string> { Text = text, Value = text.ToString() };
                },
            }
        );

        Terminal.WriteLine();
        Terminal.WriteLine($"Selected suggestion: {result.LastSuggestion?.Value}");
        Terminal.WriteLine($"Typed text: {result.TypedText}");
    }
}
