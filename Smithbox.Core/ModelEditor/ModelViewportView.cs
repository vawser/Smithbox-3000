using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.Mathematics;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
using Smithbox_Core;
using System.Numerics;

namespace Smithbox.Core.ModelEditorNS;

public class ModelViewportView
{
    public Project Project;
    public ModelEditor Editor;

    public CameraTransform orbitCamera = new();

    public Vector3 sc = new(10, 0.5f, 0.5f);

    public const float speed = 2;
    public bool first = true;

    public string[] operationNames = Enum.GetNames<ImGuizmoOperation>();
    public ImGuizmoOperation[] operations = Enum.GetValues<ImGuizmoOperation>();

    public string[] modeNames = Enum.GetNames<ImGuizmoMode>();
    public ImGuizmoMode[] modes = Enum.GetValues<ImGuizmoMode>();

    public ImGuizmoOperation operation = ImGuizmoOperation.Universal;
    public ImGuizmoMode mode = ImGuizmoMode.Local;

    public Viewport SourceViewport = new(1920, 1080);
    public Viewport Viewport;
    public bool gimbalGrabbed;
    public Matrix4x4 cube = Matrix4x4.Identity;
    public bool overGimbal;

    public ModelViewportView(Project curProject, ModelEditor editor)
    {
        Project = curProject;
        Editor = editor;

        App.MainWindow.Resized += Resized;
        SourceViewport = new(App.MainWindow.Width, App.MainWindow.Height);
    }
    private void Resized(object? sender, ResizedEventArgs e)
    {
        SourceViewport = new(e.Width, e.Height);
    }

    public void Draw()
    {
        /*
        if (!ImGui.IsWindowDocked())
        {
            uint dockspaceID = ImGui.GetID($"ModelEditorDockspace{ID}");
            var node = ImGuiP.DockBuilderGetCentralNode(dockspaceID);
            ImGuiP.DockBuilderDockWindow("Demo ImGuizmo", node.ID);
        }
        */

        HandleInput();

        // IMPORTANT: Calculate the viewport, so the gizmo is always in the center of the window, to avoid misalignment
        var position = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        float ratioX = size.X / SourceViewport.Width;
        float ratioY = size.Y / SourceViewport.Height;
        var s = Math.Min(ratioX, ratioY);
        var w = SourceViewport.Width * s;
        var h = SourceViewport.Height * s;
        var x = position.X + (size.X - w) / 2;
        var y = position.Y + (size.Y - h) / 2;
        Viewport = new Viewport(x, y, w, h);

        // Get the view and projection matrix from the camera
        var view = orbitCamera.View;
        var proj = orbitCamera.Projection;

        // IMPORTANT: Set the drawlist and enable ImGuizmo and set the rect, before using any ImGuizmo functions
        ImGuizmo.SetDrawlist();
        ImGuizmo.Enable(true);
        ImGuizmo.SetOrthographic(false);
        ImGuizmo.SetRect(position.X, position.Y, size.X, size.Y);

        DisplayEditor(view, proj);

        // User-Interface for the gizmo operation modes.
        ImGui.PushItemWidth(100);
        int opIndex = Array.IndexOf(operations, operation);
        if (ImGui.Combo("##Operation", ref opIndex, operationNames, operationNames.Length))
        {
            operation = operations[opIndex];
        }
        int modeIndex = Array.IndexOf(modes, mode);
        if (ImGui.Combo("##Mode", ref modeIndex, modeNames, modeNames.Length))
        {
            mode = modes[modeIndex];
        }
        ImGui.PopItemWidth();

        // Display the gizmo state
        ImGui.Text($"IsOver: {overGimbal}");
        ImGui.Text($"IsUsed: {gimbalGrabbed}");
    }


    public void DisplayEditor(Matrix4x4 view, Matrix4x4 proj)
    {
        var transform = cube;

        // Draw the grid and the cube
        Matrix4x4 matrix = Matrix4x4.Identity;
        ImGuizmo.DrawGrid(ref view, ref proj, ref matrix, 10);

        // VAW: This is the 'render' part of the cube, we will need to work out a different method for our models
        ImGuizmo.DrawCubes(ref view, ref proj, ref transform, 1);

        // IMPORTANT: If you use multiple gizmos, you need to set the ID for each gizmo
        ImGuizmo.PushID(0);

        // Call the Manipulate function to manipulate the cube
        if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
        {
            gimbalGrabbed = true;
            cube = transform;
        }

        // Query gizmo state and update local variables
        if (!ImGuizmo.IsUsing() && gimbalGrabbed)
        {
            gimbalGrabbed = false;
        }
        overGimbal = ImGuizmo.IsOver();

        ImGuizmo.PopID();
    }

    private float movementSpeed = 10.0f;
    private bool freeLookMode = false;
    private Vector2 freeLookRotation = Vector2.Zero;
    private float lookSensitivity = 0.005f;
    private bool invertMouseY = false;

    /// <summary>
    /// Handles the camera controls
    /// </summary>
    private void HandleInput()
    {
        // Use CFG values for these
        movementSpeed = CFG.Current.FreeLookBaseSpeed;
        invertMouseY = CFG.Current.UseInvertedControls;
        lookSensitivity = CFG.Current.LookSensitivity;

        if (!ImGui.IsWindowHovered())
            return;

        // === Orbit camera mode ===
        bool mouseMiddlePressed = ImGui.IsMouseDown(ImGuiMouseButton.Middle);
        bool lCtrlPressed = ImGui.IsKeyPressed(ImGuiKey.LeftCtrl);

        if ((mouseMiddlePressed || lCtrlPressed) && !ImGui.IsMouseDown(ImGuiMouseButton.Right))
        {
            Vector2 delta = Vector2.Zero;
            if (mouseMiddlePressed)
            {
                delta = Mouse.Delta;
            }

            float wheel = 0;
            if (lCtrlPressed)
            {
                wheel = Mouse.DeltaWheel.Y;
            }

            if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
            {
                sc.X += sc.X / 2 * -wheel;

                sc.Y += -delta.X * 0.004f * speed;
                sc.Z = Math.Clamp(sc.Z + delta.Y * 0.004f * speed, -MathF.PI / 2, MathF.PI / 2);

                first = false;

                Vector3 pos = SphereHelper.GetCartesianCoordinates(sc);
                var orientation = Quaternion.CreateFromYawPitchRoll(-sc.Y, sc.Z, 0);
                orbitCamera.PositionRotation = (pos, orientation);
                orbitCamera.Recalculate();
            }
        }

        // === Free look fly mode (WASD + Mouse Look) ===
        if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
        {
            freeLookMode = true;

            // Update look direction
            Vector2 mouseDelta = Mouse.Delta;
            freeLookRotation.X += mouseDelta.X * lookSensitivity;

            float pitchDelta = mouseDelta.Y * lookSensitivity * (invertMouseY ? -1 : 1);
            freeLookRotation.Y += pitchDelta;

            freeLookRotation.Y = Math.Clamp(freeLookRotation.Y, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);

            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(freeLookRotation.X, freeLookRotation.Y, 0);
            Vector3 forward = Vector3.Transform(-Vector3.UnitZ, rotation);
            Vector3 right = Vector3.Transform(Vector3.UnitX, rotation);
            Vector3 up = Vector3.Transform(Vector3.UnitY, rotation);

            // WASD Movement
            Vector3 move = Vector3.Zero;

            float speedMultiplier = 1f;

            if (Keyboard.IsDown(Key.LShift))
                speedMultiplier = CFG.Current.FreeLookFastSpeed;

            else if (Keyboard.IsDown(Key.LCtrl))
                speedMultiplier = CFG.Current.FreeLookSlowSpeed;

            float moveSpeed = movementSpeed * speedMultiplier * Time.Delta;

            if (Keyboard.IsDown(Key.W))
                move += orbitCamera.Forward;

            if (Keyboard.IsDown(Key.S))
                move -= orbitCamera.Forward;

            if (Keyboard.IsDown(Key.A))
                move -= orbitCamera.Right;

            if (Keyboard.IsDown(Key.D))
                move += orbitCamera.Right;

            if (Keyboard.IsDown(Key.Q))
                move -= orbitCamera.Up;

            if (Keyboard.IsDown(Key.E))
                move += orbitCamera.Up;

            if (move != Vector3.Zero)
            {
                move = Vector3.Normalize(move) * moveSpeed;
                orbitCamera.PositionRotation = (
                    orbitCamera.PositionRotation.Item1 + move,
                    rotation
                );
            }
            else
            {
                orbitCamera.PositionRotation = (
                    orbitCamera.PositionRotation.Item1,
                    rotation
                );
            }

            orbitCamera.Recalculate();
        }
        else
        {
            freeLookMode = false;
        }
    }
}
