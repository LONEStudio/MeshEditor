using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityExtensions.MeshPro.MeshEditor.Editor.Scripts.Base;

public class MeshSimplifierPage : MeshEditorPage
{
    #region 网格压缩相关字段

    private static List<Mesh> SimplifiedMeshs = new List<Mesh>(); //压缩后的网格
    private static float mesh_SimplifierQuality = 25; //压缩比例
    private static float mesh_LastSimplifierQuality; //上一个赋值的压缩比例
    private static bool meshSimplifierFold; //是否点压缩选项开折页

    #endregion

    private void Awake()
    {
        PageName = "网格压缩";
        PageIcon= Resources.Load<Texture2D>("Textures/MeshSimplifier");
        PageToolTips = "网格模型压缩工具\n有效减少模型大小";
    }

    protected override void OnGUI()
    {
        MeshSimplifierMenu();
    }
    
    /// <summary>
    /// 压缩网格编辑菜单
    /// </summary>
    private void MeshSimplifierMenu()
    {
        if (CheckFields == null || CheckFields.Count <= 0) return;
        var editMeshRenderer = CheckFields[0].Renderer;
        var editMeshFilter = CheckFields[0].Filter;
        ///————————————————————————————————————————————————————————————————————————————————————————————————————网格压缩
        // meshSimplifierFold = EditorGUILayout.BeginFoldoutHeaderGroup(meshSimplifierFold, new GUIContent("网格压缩"));
        // if (meshSimplifierFold)
        // {
        mesh_SimplifierQuality = EditorGUILayout.Slider(new GUIContent("压缩比例%"), mesh_SimplifierQuality, 0f, 100f);
        mesh_SimplifierQuality = mesh_SimplifierQuality > 100 ? 100 :
            mesh_SimplifierQuality < 0 ? 0 : mesh_SimplifierQuality;
        if (mesh_SimplifierQuality <= 0)
        {
            EditorGUILayout.HelpBox("未压缩", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("压缩比例越高，顶点数量越少", MessageType.Info);
            if (SimplifiedMeshs != null || SimplifiedMeshs.Count >= 0) //如果已经操作过压缩并生成压缩网格
            {
                int index = 0;
                foreach (var mesh in SimplifiedMeshs)
                {
                    index++;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        string.Format("({0})  压缩后-->顶点数:{1}\n压缩后-->三角面数:{2}"
                            , index
                            , mesh.vertexCount
                            , mesh.triangles.Length)
                        , MessageType.Info, true);
                    EditorGUILayout.Space(5);
                }
            }


            if (mesh_LastSimplifierQuality != mesh_SimplifierQuality && GUILayout.Button("预览"))
            {
                SimplifiedMeshs.Clear();
                foreach (var field in CheckFields)
                {
                    editMeshFilter = field.Filter;
                    var smfMesh = SimplifierMesh(editMeshFilter.sharedMesh,
                        (100 - mesh_SimplifierQuality) * 0.01f);
                    SimplifiedMeshs.Add(smfMesh);
                }

                mesh_LastSimplifierQuality = mesh_SimplifierQuality;
                //ShowNotification(new GUIContent("压缩已更新"), 1f);
            }

            if (SimplifiedMeshs.Count >= 0 && GUILayout.Button("压缩"))
            {
                if (EditorUtility.DisplayDialog("确定要应用\n压缩后的网格吗?\n此操作不可逆哦", "", "确定", "取消"))
                {
                    for (int i = 0; i < CheckFields.Count; i++)
                    {
                        CheckFields[i].Filter.sharedMesh = SimplifiedMeshs[i];
                    }

                    //editMeshFilter.sharedMesh = SimplifiedMeshs;
                    ParentWindow.ShowNotification(new GUIContent("压缩完成"), 1f);
                }
            }
        }

        // }
        //
        // EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private Mesh SimplifierMesh(Mesh sourceMesh, float quality)
    {
        var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        meshSimplifier.Initialize(sourceMesh);
        meshSimplifier.SimplifyMesh(quality);
        var mesh = meshSimplifier.ToMesh();
        mesh.name = sourceMesh.name;
        return mesh;
    }
}