using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IV.BaseUnity
{
    public class SetPivotWindow : EditorWindow
    {
        private Vector3 _pivot;

        private GameObject _obj; //Selected object in the Hierarchy
        private MeshFilter _meshFilter; //Mesh Filter of the selected object
        private Mesh _mesh; //Mesh of the selected object

        private bool SetOnScene
        {
            get { return EditorPrefs.GetBool("Pivot_UseHandles", true); }
            set { EditorPrefs.SetBool("Pivot_UseHandles", value); }
        }

        private bool SnapToVertex
        {
            get { return EditorPrefs.GetBool("Pivot_SnapToVertex", false); }
            set { EditorPrefs.SetBool("Pivot_SnapToVertex", value); }
        }

        private bool SaveMeshToFbxFolder
        {
            get { return EditorPrefs.GetBool("Pivot_UseFBXFolder", true); }
            set { EditorPrefs.SetBool("Pivot_UseFBXFolder", value); }
        }

        private static bool DrawBoundingBox
        {
            get { return EditorPrefs.GetBool("Pivot_BoundingBox", false); }
            set { EditorPrefs.SetBool("Pivot_BoundingBox", value); }
        }

        private bool UseCustom
        {
            get { return EditorPrefs.GetBool("Pivot_CustomTransform", false); }
            set { EditorPrefs.SetBool("Pivot_CustomTransform", value); }
        }

        private bool UseCustomPosition
        {
            get { return EditorPrefs.GetBool("Pivot_CustomPos", false); }
            set { EditorPrefs.SetBool("Pivot_CustomPos", value); }
        }

        private bool UseCustomRotation
        {
            get { return EditorPrefs.GetBool("Pivot_CustomRot", false); }
            set { EditorPrefs.SetBool("Pivot_CustomRot", value); }
        }

        private Transform _transform;

        private Transform _trPivot;

        private Vector3 _customPosition;

        private Vector3 _customRotation;

        private bool _editPivot;

        private static SetPivotWindow _window;

        private float _x = .5f;
        private float _y = .5f;
        private float _z = .5f;

        private int _sectionSeparators = 4;

        private PropertyInfo _colliderCenterProperty;

        private Vector3 Min
        {
            get { return _transform.TransformPoint(_mesh.bounds.min); }
        }

        private Vector3 Max
        {
            get { return _transform.TransformPoint(_mesh.bounds.max); }
        }

        private UnityEditor.Tool _previousTool;
        private Vector3 _initialPos;

        private Vector3 _lastPos;
        private Quaternion _lastRotation;
        

        [MenuItem("GameObject/Set Pivot")] //Place the Set Pivot menu item in the GameObject menu
        [MenuItem("Window/Set Pivot")] //Place the Set Pivot menu item in the GameObject menu
        static void Init()
        {
            _window = GetWindow<SetPivotWindow>();
            _window.titleContent = new GUIContent("Set Pivot");
            _window.Show();
            _window.SelectionChanged();
        }

        void OnEnable()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            _window = this;
            _previousTool = Tools.current;
            _window.minSize = new Vector2(400, 675);
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            Tools.current = _previousTool;
        }

        void OnGUI()
        {
            if (_transform)
            {
                EditorGUILayout.Separator();

                if (GUILayout.Button(new GUIContent(_editPivot ? "Stop editing" : "Start editing",
                    "Starts/Stops editing pivot")))
                {
                    _editPivot = !_editPivot;

                    if (_editPivot)
                    {
                        SelectMesh();
                    }

                    SceneView.RepaintAll();

                    Tools.pivotMode = PivotMode.Pivot;

                    Tools.pivotRotation = PivotRotation.Local;

                    if (_editPivot)
                        _previousTool = Tools.current;
                    else
                        Tools.current = _previousTool;
                }

                EditorGUI.BeginDisabledGroup(!_editPivot); // Edit Pivot

                MeshClonningGUI();

                SetOnSceneGUI();

                UseBoundingBoxGUI();

                CustomPivotGUI();

                EditorGUI.EndDisabledGroup(); // Edit Pivot

                UtilitiesGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("No Object is Selected!", MessageType.Error);
            }
        }

        void SelectionChanged()
        {
            if (_obj == null || _obj.IsAPrefab())
            {
                _transform = null;
                _meshFilter = null;
                _mesh = null;
                Repaint();
                return;
            }

            _editPivot = false;
            Tools.current = _previousTool;

            _meshFilter = _obj.GetComponent<MeshFilter>();

            _transform = _obj.transform;

            if (_meshFilter != null && SetOnScene)
                SceneView.currentDrawingSceneView.FrameSelected();

            SelectMesh();

            Repaint();
        }

        bool CloneAndSaveMesh()
        {
            bool cloned = _meshFilter.CloneAndSaveMesh(SaveMeshToFbxFolder);

            SelectMesh();

            return cloned;
        }

        void AddSectionSeparators()
        {
            for (int i = 0; i < _sectionSeparators; i++)
            {
                EditorGUILayout.Separator();
            }
        }

        void MeshClonningGUI()
        {
            AddSectionSeparators();

            EditorGUILayout.LabelField("Mesh Clonning", EditorStyles.toolbarButton);

            EditorGUI.BeginDisabledGroup(!_mesh); // there's not mesh

            EditorGUILayout.BeginHorizontal();

            SaveMeshToFbxFolder =
                EditorGUILayout.Toggle(
                    new GUIContent("Save Mesh to FBX folder?", "Saves cloned mesh to original model folder"),
                    SaveMeshToFbxFolder);

            if (GUILayout.Button(new GUIContent("Clone mesh", "Clones the mesh so you won't lose the original one.")))
            {
                CloneAndSaveMesh();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup(); //there's no mesh
        }

        void CustomPivotGUI()
        {
            AddSectionSeparators();

            EditorGUILayout.LabelField("Custom Pivot", EditorStyles.toolbarButton);

            UseCustom = EditorGUILayout.Toggle(
                new GUIContent("Use Custom", "Set pivot position and rotation to a custom transform or vector"),
                UseCustom);

            EditorGUI.BeginDisabledGroup(!UseCustom);

            _trPivot = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Custom transform", "The custom transform to set the pivot to"), _trPivot,
                typeof(Transform), true);

            /* if (UseCustom)
             {
                 SetOnScene = false;
                 DrawBoundingBox = false;
             }*/

            EditorGUI.BeginDisabledGroup(_trPivot);


            UseCustomPosition = EditorGUILayout.ToggleLeft(new GUIContent("Position", "Use this custom pivot position"),
                UseCustomPosition);

            EditorGUI.BeginDisabledGroup(!UseCustomPosition);

            _customPosition = EditorGUILayout.Vector3Field("", _customPosition);

            EditorGUI.EndDisabledGroup();

            UseCustomRotation = EditorGUILayout.ToggleLeft(new GUIContent("Rotation", "Use this custom pivot rotation"),
                UseCustomRotation);

            EditorGUI.BeginDisabledGroup(!UseCustomRotation);

            _customRotation = EditorGUILayout.Vector3Field("", _customRotation);

            EditorGUI.EndDisabledGroup();

            if (UseCustom)
                SceneView.RepaintAll();

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(
                new GUIContent("Set Pivot", "sets the pivot to custom position, rotation or transform")))
            {
                if (_trPivot)
                {
                    _pivot = _trPivot.position;

                    SetPivotToCustom(_trPivot.position, _trPivot.rotation);
                }
                else
                {
                    SetPivotToCustom(UseCustomPosition ? _customPosition : _transform.position,
                        UseCustomRotation ? Quaternion.Euler(_customRotation) : _transform.rotation);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        void SetOnSceneGUI()
        {
            AddSectionSeparators();

            EditorGUILayout.LabelField("Set Pivot On Scene", EditorStyles.toolbarButton);

            if (UseCustom)
            {
                EditorGUILayout.HelpBox("Cannot use this option while you're using a custom pivot!", MessageType.Info);
            }

            EditorGUI.BeginDisabledGroup(UseCustom);

            bool setOnScene =
                EditorGUILayout.Toggle(new GUIContent("Set On Scene", "Use a gizmo to set the pivot on scene view."),
                    SetOnScene);

            if (!SetOnScene && setOnScene)
            {
                _pivot = _transform.position;
                _lastGizmoPivot = _pivot;
            }

            SetOnScene = setOnScene;

            EditorGUI.BeginDisabledGroup(!_mesh);

            SnapToVertex =
                EditorGUILayout.Toggle(new GUIContent("Snap to vertex", "Snap pivot position handle to mesh vertices."),
                    SnapToVertex);


            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }

        void UseBoundingBoxGUI()
        {
            AddSectionSeparators();

            EditorGUILayout.LabelField("Boundind Box", EditorStyles.toolbarButton);

            if (UseCustom)
                EditorGUILayout.HelpBox("Cannot use this option while you're using a custom pivot!", MessageType.Info);

            EditorGUI.BeginDisabledGroup(UseCustom || !_mesh);

            bool draw = EditorGUILayout.Toggle(
                new GUIContent("Draw bounding box", "Choose whether to draw bounding box gizmo or not."),
                DrawBoundingBox);

            if (draw != DrawBoundingBox)
            {
                ((SceneView)SceneView.sceneViews[0]).Repaint();
            }

            DrawBoundingBox = draw;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("X", GUILayout.MaxWidth(20));

            float x = EditorGUILayout.Slider(_x, 0, 1);

            if (!Mathf.Approximately(x, _x))
            {
                _pivot.x = Mathf.Lerp(Min.x, Max.x, x);

                UpdatePivot();
            }

            _x = x;


            if (GUILayout.Button(new GUIContent("Center", "Center pivot on X axis."), GUILayout.MinWidth(75)))
            {
                _x = .5f;
                _pivot.x = Mathf.Lerp(Min.x, Max.x, _x);
                UpdatePivot();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(20));

            float y = EditorGUILayout.Slider(_y, 0, 1);

            if (!Mathf.Approximately(y, _y))
            {
                _pivot.y = Mathf.Lerp(Min.y, Max.y, y);

                UpdatePivot();
            }

            _y = y;

            if (GUILayout.Button(new GUIContent("Center", "Center pivot on Y axis."), GUILayout.MinWidth(75)))
            {
                _y = .5f;
                _pivot.y = Mathf.Lerp(Min.y, Max.y, _y);
                UpdatePivot();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(20));

            float z = EditorGUILayout.Slider(_z, 0, 1);

            if (!Mathf.Approximately(z, _z))
            {
                _pivot.z = Mathf.Lerp(Min.z, Max.z, z);

                UpdatePivot();
            }

            _z = z;

            if (GUILayout.Button(new GUIContent("Center", "Center pivot on Z axis."), GUILayout.MinWidth(75)))
            {
                _z = .5f;
                _pivot.z = Mathf.Lerp(Min.z, Max.z, _z);
                UpdatePivot();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        void UtilitiesGUI()
        {
            AddSectionSeparators();

            EditorGUILayout.LabelField("Utilities", EditorStyles.toolbarButton);

            EditorGUILayout.Separator();

            EditorGUI.BeginDisabledGroup(UseCustom);

            EditorGUI.BeginDisabledGroup(!_mesh);

            if (GUILayout.Button(new GUIContent("Center Pivot", "Center the pivot to the mesh bounding box center")))
            {
                _x = _y = _z = .5f;
                _pivot = _transform.TransformPoint(_mesh.bounds.center);

                UpdatePivot();
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Freeze Rotation",
                "Freezing the rotation will allow you to change the pivot orientation.")))
            {
                FreezeRotation();
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Freeze scale",
                "Sets transform scale to one without changing object's size")))
            {
                FreezeScale();
            }
        }


        void FreezeRotation()
        {
            if (_transform)
            {
                bool updated = _transform.Freeze(_transform.position, Quaternion.identity, _transform.localScale,
                    SaveMeshToFbxFolder);

                if (updated)
                {
                    SelectMesh();
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        void FreezeScale()
        {
            if (_transform)
            {
                bool updated = _transform.Freeze(_transform.position, _transform.rotation, Vector3.one,
                    SaveMeshToFbxFolder);

                if (updated)
                {
                    SelectMesh();
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        bool UpdatePivot()
        {
            if (_transform)
            {
                bool updated = _transform.Freeze(_pivot, _transform.localRotation, _transform.localScale,
                    SaveMeshToFbxFolder);

                if (updated)
                {
                    _pivot = _transform.position;
                    _lastGizmoPivot = _pivot;
                    
                    SelectMesh();
                    
                    EditorSceneManager.MarkAllScenesDirty();
                }

                return updated;
            }
            else
            {
                return false;
            }
        }

        void SetPivotToCustom(Vector3 pos, Quaternion rot)
        {
            if (_transform)
            {
                bool updated = _transform.Freeze(pos, rot, _transform.localScale,
                    SaveMeshToFbxFolder);

                if (updated)
                {
                    _pivot = pos;
                    _lastGizmoPivot = _pivot;

                    SelectMesh();

                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        void SelectMesh()
        {
            if (_transform)
            {
                _meshFilter = _transform.GetComponent<MeshFilter>();
                _mesh = _meshFilter ? _meshFilter.sharedMesh : null;
                _pivot = _transform.position;
                _lastGizmoPivot = _pivot;

                if (_mesh != null)
                {
                    UpdateCustomValues();
                }
            }
            else
            {
                _mesh = null;
            }
        }

        void UpdateCustomValues()
        {
            _x = Mathf.InverseLerp(Min.x, Max.x, _pivot.x);
            _y = Mathf.InverseLerp(Min.y, Max.y, _pivot.y);
            _z = Mathf.InverseLerp(Min.z, Max.z, _pivot.z);

            _customPosition = _pivot;
            _customRotation = _transform.eulerAngles;
        }

        [DrawGizmo(GizmoType.Selected)]
        private static void DrawGizmos(MeshFilter filter, GizmoType gizmoType)
        {
            if (_window == null)
                return;

            if (_window._mesh == null)
                return;

            if (!DrawBoundingBox)
                return;

            if (!_window._editPivot)
                return;

            if (filter.sharedMesh == _window._mesh)
            {
                Gizmos.color = Color.green;

                Gizmos.matrix = Matrix4x4.TRS(filter.transform.position, filter.transform.rotation,
                    filter.transform.localScale);

                Vector3 center = _window._mesh.bounds.center;

                Vector3 size = _window._mesh.bounds.size;

                Gizmos.DrawWireCube(center, size);
            }
        }

        private Vector3 _lastGizmoPivot;

        void OnSceneGUI(SceneView sceneView)
        {
            GameObject obj = Selection.activeGameObject;

            if (obj != _obj || _transform == null)
            {
                _obj = obj;

                SelectionChanged();

                return;
            }

            if (_transform == null || !_editPivot)
                return;

            if (!SetOnScene || UseCustom)
            {
                if (UseCustom)
                {
                    Tools.current = UnityEditor.Tool.None;

                    if (_trPivot)
                    {
                        Handles.DoPositionHandle(_trPivot.position, _trPivot.rotation);
                    }

                    else
                    {
                        Vector3 pos = UseCustomPosition ? _customPosition : _transform.position;

                        Quaternion rot = UseCustomRotation ? Quaternion.Euler(_customRotation) : _transform.rotation;

                        Handles.DoPositionHandle(pos, rot);
                    }
                }
                else
                {
                    if (Tools.current == UnityEditor.Tool.None)
                        Tools.current = UnityEditor.Tool.Move;
                }

                return;
            }

            Handles.color = Color.red;
            //
            //            Handles.FreeMoveHandle(_transform.position, _transform.rotation, .1f, Vector3.zero,
            //                Handles.SphereHandleCap);

            Tools.current = UnityEditor.Tool.None;

            Vector3 pivot = Handles.DoPositionHandle(_lastGizmoPivot, _transform.rotation);

            bool movedPivot = pivot != _lastGizmoPivot;

            _lastGizmoPivot = pivot;

            // only change pivot when the current gizmo pivot is different from player position and the user stoped moving the handle
            if (pivot != _transform.position && !movedPivot)
            {
                _lastGizmoPivot = _pivot;

                if (SnapToVertex && _mesh)
                {
                    Vector3[] worldVertices = _mesh.vertices.Select(v => _transform.TransformPoint(v)).ToArray();

                    Vector3 snapPivot = ClosestVertex(worldVertices, pivot);

                    Vector3 oldPivot = _transform.position;

                    if (snapPivot != oldPivot)
                    {
                        if (!Mathf.Approximately(_pivot.x, pivot.x))
                        {
                            _pivot.x = snapPivot.x;
                        }
                        if (!Mathf.Approximately(_pivot.y, pivot.y))
                        {
                            _pivot.y = snapPivot.y;
                        }
                        if (!Mathf.Approximately(_pivot.z, pivot.z))
                        {
                            _pivot.z = snapPivot.z;
                        }

                        bool update = UpdatePivot();

                        if (!update)
                            _lastGizmoPivot = _transform.position;
                    }
                }
                else
                {
                    _pivot = pivot;

                    bool update = UpdatePivot();

                    if (!update)
                    {
                        _lastGizmoPivot = _transform.position;
                    }
                }
            }
        }

        private Vector3 ClosestVertex(Vector3[] vertices, Vector3 pivot)
        {
            int closest = 0;

            for (int i = 1; i < vertices.Length; i++)
            {
                if (Vector3.Distance(vertices[i], pivot) < Vector3.Distance(vertices[closest], pivot))
                {
                    closest = i;
                }
            }

            return vertices[closest];
        }
    }
}