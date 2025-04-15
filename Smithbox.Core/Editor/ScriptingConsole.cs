using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Smithbox.Core.Editor;

public class ScriptingConsole
{
    private ScriptState<object>? scriptState;
    private ScriptOptions options;
    private readonly List<string> logs = new();

    public IReadOnlyList<string> Logs => logs;

    public ScriptingConsole()
    {
        options = ScriptOptions.Default
            .WithReferences(typeof(Smithbox).Assembly)
            .WithImports("System", "System.Math");
    }

    public async Task InitializeAsync()
    {
        scriptState = await CSharpScript.RunAsync("", options);
    }

    public async Task EvaluateAsync(string code)
    {
        try
        {
            if (scriptState == null)
                await InitializeAsync();

            if (scriptState == null)
            {
                logs.Add("[ERROR] Script state not initialized.");
                return;
            }

            // Pass the program context to the script when evaluating it
            var result = await CSharpScript.EvaluateAsync<object>(
                code, options
            );

            logs.Add($"> {code}");
            if (result != null)
                logs.Add(result.ToString() ?? "null");
        }
        catch (Exception ex)
        {
            logs.Add($"[ERROR] {ex.Message}");
        }
    }
}