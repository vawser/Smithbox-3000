using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

/// <summary>
/// This is the top-level editor that holds everything else
/// </summary>
public class SmithboxEditor
{
    private bool HasSetup = false;

    public SmithboxEditor()
    {
    }

    /// <summary>
    /// Setup the program, creating folders, initializing generic banks, etc.
    /// </summary>
    public void Setup()
    {
        if (HasSetup)
            return;

        HasSetup = true;

        // Create program data folder
        var folder = $"{AppContext.BaseDirectory}/{Consts.DataFolder}/";
        if(!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Load configuration variables
        CFG.Load();
    }

    /// <summary>
    /// Draw loop
    /// </summary>
    public void Draw()
    {
        MessageBox.Draw();

        if(ImGui.Begin("main"))
        {
            if(ImGui.Button("Press Me"))
            {
                MessageBox.Print("oops");
            }

            ImGui.End();
        }
    }
}
