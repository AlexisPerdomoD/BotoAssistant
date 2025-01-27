using Microsoft.Extensions.Logging;
using Spectre.Console;
using static Spectre.Console.AnsiConsole;

namespace Boto.Utils;

public class SpectreConsole(LogLevel ll) : MainIOMannager(ll)
{
    public static bool YesOrNo(string? prompt = null)
    {
        var result = Prompt(
            new SelectionPrompt<string>()
                .Title(prompt ?? "[/]Want to try again?[/]")
                .AddChoices(["yes", "no"])
        );

        return result == "yes";
    }

    public override string? GetInput(
        string prompt,
        Func<string?, bool>? validator,
        string? customTryAgainMessage
    )
    {
        validator ??= static i => true;
        var question = new TextPrompt<string?>(prompt);
        var input = Prompt(question);
        var tries = 0;
        if (string.IsNullOrWhiteSpace(input))
            return null;
        while (true)
        {
            input = input.Trim().ToLowerInvariant();

            if (validator(input))
                break;

            if (++tries > 2)
            {
                WriteLine("Too many tries...");
                return null;
            }

            this.LogWarning(customTryAgainMessage ?? "Invalid input.");
            var tryAgain = YesOrNo();
            if (!tryAgain)
                return null;

            input = Prompt(question);

            if (string.IsNullOrWhiteSpace(input))
                return null;
        }

        LastInput = input;
        History.Add(input);
        return input;
    }

    public override string? GetInputSelector(
        string prompt,
        string[] options,
        Func<string?, bool>? validator = null,
        string? customTryAgainMessage = null
    ) => throw new NotImplementedException();
}
