using Microsoft.MixedReality.WorldLocking.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Diagnostics;

public class EditorMode : MonoBehaviour
{
    public bool isEditorMode;

    [SerializeField] private GameObject dashboard;
    [SerializeField] private DashboardCommand dashboardCommand;
    private MixedRealityToolkitVisualProfiler profiler;
    private MixedRealityToolkitVisualProfiler Profiler
    {
        get
        {
            if(profiler == null)
            {
                profiler = FindObjectOfType<MixedRealityToolkitVisualProfiler>();
            }
            return profiler;
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        if (isEditorMode)
        {
            EnableEditorMode();
        }
        else
        {
            DisableEditorMode();
        }
    }

    public void EnableEditorMode()
    {
        isEditorMode = true;
        dashboard.SetActive(true);
        dashboardCommand.ManagerEnabled = true;
        Profiler.IsVisible = true;
    }

    public void DisableEditorMode()
    {
        isEditorMode = false;
        dashboard.SetActive(false);
        dashboardCommand.ManagerEnabled = false;
        Profiler.IsVisible = false;
    }
}
