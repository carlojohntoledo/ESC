﻿//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour {
  private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

  public enum Mode {
    OutlineAll,
    OutlineVisible,
    OutlineHidden,
    OutlineAndSilhouette,
    SilhouetteOnly
  }

  public Mode OutlineMode {
    get { return outlineMode; }
    set {
      outlineMode = value;
      needsUpdate = true;
    }
  }

  public Color OutlineColor {
    get { return outlineColor; }
    set {
      outlineColor = value;
      needsUpdate = true;
    }
  }

  public float OutlineWidth {
    get { return outlineWidth; }
    set {
      outlineWidth = value;
      needsUpdate = true;
    }
  }

  [Serializable]
  private class ListVector3 {
    public List<Vector3> data;
  }

  [SerializeField]
  private Mode outlineMode;

  [SerializeField]
  private Color outlineColor = Color.white;

  [SerializeField, Range(0f, 10f)]
  private float outlineWidth = 2f;

  [Header("Optional")]
  [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
  + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
  private bool precomputeOutline;

  [SerializeField, HideInInspector]
  private List<Mesh> bakeKeys = new List<Mesh>();

  [SerializeField, HideInInspector]
  private List<ListVector3> bakeValues = new List<ListVector3>();

  private Renderer[] renderers;
  private Material outlineMaskMaterial;
  private Material outlineFillMaterial;

  private bool needsUpdate;

  void Awake() {
    renderers = GetComponentsInChildren<Renderer>();

    outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
    outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

    outlineMaskMaterial.name = "OutlineMask (Instance)";
    outlineFillMaterial.name = "OutlineFill (Instance)";

    LoadSmoothNormals();
    needsUpdate = true;
  }

  void OnEnable() {
    DisableOutline(); // Ensure it starts disabled
  }

  void OnValidate() {
    needsUpdate = true;

    if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count) {
      bakeKeys.Clear();
      bakeValues.Clear();
    }

    if (precomputeOutline && bakeKeys.Count == 0) {
      Bake();
    }
  }

  void Update() {
    if (needsUpdate) {
      needsUpdate = false;
      UpdateMaterialProperties();
    }
  }

  void OnDisable() {
    DisableOutline();
  }

  void OnDestroy() {
    Destroy(outlineMaskMaterial);
    Destroy(outlineFillMaterial);
  }

  void Bake() {
    var bakedMeshes = new HashSet<Mesh>();

    foreach (var meshFilter in GetComponentsInChildren<MeshFilter>()) {
      if (!bakedMeshes.Add(meshFilter.sharedMesh)) continue;

      var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
      bakeKeys.Add(meshFilter.sharedMesh);
      bakeValues.Add(new ListVector3() { data = smoothNormals });
    }
  }

  void LoadSmoothNormals() {
    foreach (var meshFilter in GetComponentsInChildren<MeshFilter>()) {
      if (!registeredMeshes.Add(meshFilter.sharedMesh)) continue;

      var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
      var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

      meshFilter.sharedMesh.SetUVs(3, smoothNormals);

      var renderer = meshFilter.GetComponent<Renderer>();
      if (renderer != null) {
        CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
      }
    }

    foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>()) {
      if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) continue;

      skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
      CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
    }
  }

  List<Vector3> SmoothNormals(Mesh mesh) {
    var groups = mesh.vertices
      .Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
      .GroupBy(pair => pair.Key);

    var smoothNormals = new List<Vector3>(mesh.normals);

    foreach (var group in groups) {
      if (group.Count() == 1) continue;

      var smoothNormal = Vector3.zero;
      foreach (var pair in group) {
        smoothNormal += smoothNormals[pair.Value];
      }
      smoothNormal.Normalize();

      foreach (var pair in group) {
        smoothNormals[pair.Value] = smoothNormal;
      }
    }

    return smoothNormals;
  }

  void CombineSubmeshes(Mesh mesh, Material[] materials) {
    if (mesh.subMeshCount == 1 || mesh.subMeshCount > materials.Length) return;

    mesh.subMeshCount++;
    mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
  }

  void UpdateMaterialProperties() {
    outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

    switch (outlineMode) {
      case Mode.OutlineAll:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;
      case Mode.OutlineVisible:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;
      case Mode.OutlineHidden:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;
      case Mode.OutlineAndSilhouette:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;
      case Mode.SilhouetteOnly:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
        outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
        break;
    }
  }

  // ✅ NEW METHODS FOR RUNTIME CONTROL
  public void EnableOutline() {
    if (renderers == null || renderers.Length == 0)
      renderers = GetComponentsInChildren<Renderer>();

    foreach (var renderer in renderers) {
      var materials = renderer.sharedMaterials.ToList();
      if (!materials.Contains(outlineMaskMaterial)) materials.Add(outlineMaskMaterial);
      if (!materials.Contains(outlineFillMaterial)) materials.Add(outlineFillMaterial);
      renderer.materials = materials.ToArray();
    }
  }

  public void DisableOutline() {
    if (renderers == null || renderers.Length == 0)
      renderers = GetComponentsInChildren<Renderer>();

    foreach (var renderer in renderers) {
      var materials = renderer.sharedMaterials.ToList();
      if (materials.Contains(outlineMaskMaterial)) materials.Remove(outlineMaskMaterial);
      if (materials.Contains(outlineFillMaterial)) materials.Remove(outlineFillMaterial);
      renderer.materials = materials.ToArray();
    }
  }


}
