using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IV.BaseUnity
{
    public static class PivotUtils
    {
        public static string RemoveExtension(this string path)
        {
            int removeCount = 0;

            int startingIndex = -1;

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] != '.')
                    removeCount++;
                else
                {
                    startingIndex = i;
                    break;
                }
            }

            return path.Remove(startingIndex, removeCount + 1);
        }

        public static bool IsAPrefab(this GameObject gameObject)
        {
#if UNITY_2018_1_OR_NEWER
            return PrefabUtility.GetPrefabInstanceHandle(gameObject) != null && PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null;
#else
            return  PrefabUtility.GetPrefabObject(gameObject) != null;
#endif
        }

        public static bool Freeze(this Transform transform, Vector3 position, Quaternion rotation, Vector3 scale,
            bool saveToFBX)
        {
            MeshFilter filter = transform.GetComponent<MeshFilter>();

            Mesh mesh = filter != null ? filter.sharedMesh : null;


            Collider collider = transform.GetComponent<Collider>();

            PropertyInfo colliderCenterProperty = collider.GetType().GetProperty("center");

            Vector3 initialColliderCenter = Vector3.zero;

            Vector3 initialPos = Vector3.zero;

            if (colliderCenterProperty != null)
            {
                initialColliderCenter = (Vector3)colliderCenterProperty.GetValue(collider, null);
                initialPos = transform.position;
            }

            Transform[] children = new Transform[transform.childCount];

            // remove children so they don't move along
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = transform.GetChild(i);
            }

            for (int i = 0; i < children.Length; i++)
            {
                children[i].parent = null;
            }

            Vector3[] worldVertices = null;
            Vector3[] worldNormals = null;

            if (mesh)
            {
                worldVertices = mesh.vertices.Select(v => transform.TransformPoint(v)).ToArray();
                worldNormals = mesh.normals.Select(v => transform.TransformDirection(v)).ToArray();

                string path = AssetDatabase.GetAssetPath(mesh);

                if (!path.EndsWith(".asset") && !string.IsNullOrEmpty(path))
                {
                    bool cloned = filter.CloneAndSaveMesh(saveToFBX);

                    mesh = filter.sharedMesh;

                    if (!cloned)
                        return false;
                }
            }

            transform.position = position;

            transform.rotation = rotation;

            transform.localScale = scale;

            if (mesh)
            {
                Vector3[] vertices = worldVertices.Select(v => transform.InverseTransformPoint(v)).ToArray();
                Vector3[] normals = worldNormals.Select(v => transform.InverseTransformDirection(v)).ToArray();

                mesh.vertices = vertices;
                mesh.normals = normals;

                mesh.RecalculateBounds();
            }

            if (collider)
            {
                if (collider is MeshCollider)
                {
                    MeshCollider m = collider as MeshCollider;
                    m.sharedMesh = mesh;
                    m.convex = !m.convex;
                    m.convex = !m.convex;
                }

                if (colliderCenterProperty != null)
                {
                    colliderCenterProperty.SetValue(collider,
                        initialColliderCenter + (initialPos - transform.position), null);
                }
            }

            // re add children
            for (int i = 0; i < children.Length; i++)
            {
                children[i].parent = transform;
            }


            return true;
        }

        public static bool FreezeMesh(this MeshFilter filter, Vector3 position, Quaternion rotation, Vector3 scale,
            bool saveToFBX)
        {
            Mesh mesh = filter.sharedMesh;

            Transform transform = filter.transform;

            Collider collider = filter.GetComponent<Collider>();

            PropertyInfo colliderCenterProperty = collider.GetType().GetProperty("center");

            Vector3 initialColliderCenter = Vector3.zero;

            Vector3 initialPos = Vector3.zero;

            if (colliderCenterProperty != null)
            {
                initialColliderCenter = (Vector3)colliderCenterProperty.GetValue(collider, null);
                initialPos = transform.position;
            }

            //            Vector3[] normals = mesh.normals;
            //
            //            for (var i = 0; i < normals.Length; i++)
            //            {
            //                normals[i] *= -1;
            //            }

            Vector3[] worldVertices = mesh.vertices.Select(v => transform.TransformPoint(v)).ToArray();

            Transform[] children = new Transform[transform.childCount];

            // remove children so they don't move along
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = transform.GetChild(i);

                children[i].parent = null;
            }

            string path = AssetDatabase.GetAssetPath(mesh);

            if (!path.EndsWith(".asset") && !path.EndsWith(".prefab") && !string.IsNullOrEmpty(path))
            {
                bool cloned = true;

                mesh = GameObject.Instantiate(mesh);

                filter.sharedMesh = mesh;

                if (!cloned)
                    return false;
            }

            transform.position = position;

            transform.rotation = rotation;

            transform.localScale = scale;

            //            Vector3 [] normals = 
            Vector3[] vertices = worldVertices.Select(v => transform.InverseTransformPoint(v)).ToArray();
            mesh.vertices = vertices;
            //            mesh.FlipNormals();
            //            mesh.normals = normals;

            //            mesh.RecalculateBounds();
            //                        mesh.RecalculateNormals();


            //            for (var i = 0; i < mesh.normals.Length; i++)
            //            {
            //                if(mesh.normals[i] != normals[i])
            //                    Debug.LogError("normal");
            //            }
            //
            //            for (var i = 0; i < mesh.tangents.Length; i++)
            //            {
            //                if (mesh.tangents[i] != tangents[i])
            //                    Debug.LogError("tangent");
            //            }
            if (collider)
            {
                if (collider is MeshCollider)
                {
                    MeshCollider m = collider as MeshCollider;
                    m.sharedMesh = mesh;
                    m.convex = !m.convex;
                    m.convex = !m.convex;
                }
                //
                if (colliderCenterProperty != null)
                {
                    colliderCenterProperty.SetValue(collider,
                        initialColliderCenter + (initialPos - transform.position), null);
                }
            }

            // re add children
            for (int i = 0; i < children.Length; i++)
            {
                children[i].parent = transform;
            }


            return true;
        }


        public static bool CloneAndSaveMesh(this MeshFilter meshFilter, bool saveMeshToFbxFolder)
        {
            string path = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);

            if (!saveMeshToFbxFolder || path.StartsWith("Library"))
            {
                path = EditorUtility.SaveFilePanelInProject("Save Cloned Mesh", meshFilter.sharedMesh.name, "asset",
                    "");
            }
            else
            {
                path = path.RemoveExtension() + ".asset";
            }

            if (string.IsNullOrEmpty(path))
                return false;

            // clone mesh
            Mesh cloneMesh = GameObject.Instantiate(meshFilter.sharedMesh);

            // save mesh asset
            AssetDatabase.CreateAsset(cloneMesh, AssetDatabase.GenerateUniqueAssetPath(path));

            meshFilter.sharedMesh = cloneMesh;

            MeshCollider meshColl = meshFilter.GetComponent<MeshCollider>();

            if (meshColl)
            {
                meshColl.sharedMesh = cloneMesh;
            }

            return true;
        }
    }
}