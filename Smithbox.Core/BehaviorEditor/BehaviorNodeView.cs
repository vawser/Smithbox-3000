using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using Microsoft.AspNetCore.Components.Forms;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.NodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorNodeView
{
    private Project Project;
    private BehaviorEditor Editor;
    private NodeEditor NodeEditor;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar; //| ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.None;

    public BehaviorNodeView(Project projectOwner, BehaviorEditor behEditor)
    {
        Project = projectOwner;
        Editor = behEditor;
        NodeEditor = new();

        // EXAMPLE
        NodeEditor.Initialize();
        var node1 = NodeEditor.CreateNode("Node");
        node1.CreatePin(NodeEditor, "In", PinKind.Input, PinType.DontCare, ImNodesPinShape.Circle);
        var out1 = node1.CreatePin(NodeEditor, "Out", PinKind.Output, PinType.DontCare, ImNodesPinShape.Circle);
        var node2 = NodeEditor.CreateNode("Node");
        var in2 = node2.CreatePin(NodeEditor, "In", PinKind.Input, PinType.DontCare, ImNodesPinShape.Circle);
        var out2 = node2.CreatePin(NodeEditor, "Out", PinKind.Output, PinType.DontCare, ImNodesPinShape.Circle);
        var node3 = NodeEditor.CreateNode("Node");
        var in3 = node3.CreatePin(NodeEditor, "In", PinKind.Input, PinType.DontCare, ImNodesPinShape.Circle);
        node3.CreatePin(NodeEditor, "Out", PinKind.Output, PinType.DontCare, ImNodesPinShape.Circle);
        NodeEditor.CreateLink(out1, in2);
        NodeEditor.CreateLink(out1, in3);
        NodeEditor.CreateLink(out2, in3);
    }

    public void Draw()
    {
        NodeEditor.Draw();
    }
}
