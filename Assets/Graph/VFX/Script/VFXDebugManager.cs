using GameplayIngredients;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using System.Text;
using UnityEngine.InputSystem; // New Input System

[ManagerDefaultPrefab("VFXDebugManager")]
public class VFXDebugManager : Manager
{
    [Header("UI")]
    public GameObject uiRoot;
    public Text debugText;

    // Key bindings (New Input System)
    const Key Toggle = Key.F7;
    const Key PrevFX = Key.PageUp;
    const Key NextFX = Key.PageDown;
    const Key Play = Key.I;
    const Key Stop = Key.U;
    const Key Pause = Key.P;
    const Key Reinit = Key.J;
    const Key Step = Key.K;
    const Key Sort = Key.M;
    const Key ToggleVisibility = Key.L;

    bool visible = false;

    private void Update()
    {
        // Toggle debug UI visibility
        if (KeyDown(Toggle))
        {
            visible = !visible;
            if (uiRoot != null)
                uiRoot.SetActive(visible);
        }

        // Update debug text when visible
        if (visible && debugText != null)
        {
            debugText.text = UpdateVFXDebug();
        }
    }

    // Utility helpers for readability
    bool KeyDown(Key key) => Keyboard.current[key].wasPressedThisFrame;
    bool KeyHeld(Key key) => Keyboard.current[key].isPressed;

    int selectedVFX = -1;
    Sorting sorting = Sorting.None;

    enum Sorting
    {
        None = 0,
        DistanceToCamera = 1,
        ParticleCount = 2
    }

    string UpdateVFXDebug()
    {
        VisualEffect[] allEffects = VFXManager.GetComponents();

        // Cycle sorting mode
        if (KeyDown(Sort))
            sorting = (Sorting)(((int)sorting + 1) % 3);

        // Sort by distance to camera
        if (sorting == Sorting.DistanceToCamera)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                sorting = Sorting.ParticleCount;
            }
            else
            {
                allEffects = allEffects.OrderBy(o =>
                    Vector3.SqrMagnitude(o.transform.position - camera.transform.position)
                ).ToArray();
            }
        }

        // Sort by particle count
        if (sorting == Sorting.ParticleCount)
        {
            allEffects = allEffects.OrderBy(o => -o.aliveParticleCount).ToArray();
        }

        if (allEffects.Length == 0)
            return "No Active VFX Components in scene";

        // Navigate VFX list
        selectedVFX -= KeyDown(PrevFX) ? 1 : 0;
        selectedVFX += KeyDown(NextFX) ? 1 : 0;

        bool shift = KeyHeld(Key.LeftShift) || KeyHeld(Key.RightShift);

        selectedVFX = Mathf.Clamp(selectedVFX, 0, allEffects.Length - 1);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"{allEffects.Length} Visual Effect Component(s) active. Sorting : {sorting}");
        sb.AppendLine();

        sb.AppendLine($"{"Game Object Name",-24}| {"Visual Effect Asset",-24}| {"PlayState",-12}| {"Visibility",-12}| {"Particle Count",12}");
        sb.AppendLine($"===================================================================================================================================");

        int idx = 0;

        foreach (var vfx in allEffects)
        {
            // Highlight selected VFX
            if (idx == selectedVFX)
                sb.Append("<color=orange>");

            string gameObjectname = vfx.gameObject.name;
            string vfxName = (vfx.visualEffectAsset == null ? "(No VFX Asset)" : vfx.visualEffectAsset.name);
            string playState = (vfx.pause ? "Paused" : "Playing");
            var renderer = vfx.GetComponent<Renderer>();
            string visibility = renderer.enabled ? (vfx.culled ? "Culled" : "Visible") : "Disabled";
            string particleCount = vfx.aliveParticleCount.ToString();

            sb.Append($"{gameObjectname,-24}| {vfxName,-24}| {playState,-12}| {visibility,-12}| {particleCount,12}");

            if (idx == selectedVFX)
                sb.Append("</color>");
            sb.Append("\n");
            idx++;
        }

        var selected = allEffects[selectedVFX];

        // Blink selected VFX when CTRL is held
        if (KeyHeld(Key.LeftCtrl) || KeyHeld(Key.RightCtrl))
        {
            var selectedRenderer = selected.GetComponent<Renderer>();
            selectedRenderer.enabled = Time.unscaledTime % 0.5f < 0.25f;
        }

        // Apply actions to all VFX when SHIFT is held
        if (shift)
        {
            foreach (var vfx in allEffects)
            {
                if (KeyDown(Play)) vfx.Play();
                if (KeyDown(Stop)) vfx.Stop();
                if (KeyDown(Pause)) vfx.pause = !vfx.pause;
                if (KeyDown(Reinit)) vfx.Reinit();
                if (KeyDown(Step)) vfx.AdvanceOneFrame();
                if (KeyDown(ToggleVisibility))
                    vfx.GetComponent<Renderer>().enabled = !vfx.GetComponent<Renderer>().enabled;
            }
        }
        else
        {
            // Apply actions only to selected VFX
            if (KeyDown(Play)) selected.Play();
            if (KeyDown(Stop)) selected.Stop();
            if (KeyDown(Pause)) selected.pause = !selected.pause;
            if (KeyDown(Reinit)) selected.Reinit();
            if (KeyDown(Step)) selected.Simulate(Time.deltaTime);
            if (KeyDown(ToggleVisibility))
                selected.GetComponent<Renderer>().enabled = !selected.GetComponent<Renderer>().enabled;
        }

        return sb.ToString();
    }
}
