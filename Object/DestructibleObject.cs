using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Swift_Blade
{
    public class DestructibleObject : MonoBehaviour, IHealth
    {
        [SerializeField] private int _cutCascades = 3;
        [SerializeField] private float _explodeForce = 16f;
        [SerializeField] private Material _capMaterial;

        private bool _edgeSet = false;
        private Vector3 _edgeVertex = Vector3.zero;
        private Plane _edgePlane = new Plane();
        
        public bool IsDead => false;

        private NavMeshObstacle navMeshObstacle;

        private void Start()
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }
        
        public void TakeDamage(ActionData actionData)
        {
            DestroyMesh();
        }

        public void TakeHeal(float amount) { }

        public void Dead() { }

        public void DestroyMesh()
        {
            if (navMeshObstacle != null)
               Destroy(navMeshObstacle);
            
            var originalMesh = GetComponent<MeshFilter>().mesh;
            originalMesh.RecalculateBounds();
            
            var parts = new List<PartMesh>();
            var subParts = new List<PartMesh>();

            var mainPart = new PartMesh
            {
                uv = originalMesh.uv,
                vertices = originalMesh.vertices,
                normals = originalMesh.normals,
                triangles = new int[originalMesh.subMeshCount][],
                bounds = originalMesh.bounds
            };

            for (int i = 0; i < originalMesh.subMeshCount; i++)
                mainPart.triangles[i] = originalMesh.GetTriangles(i);

            parts.Add(mainPart);

            for (int c = 0; c < _cutCascades; c++)
            {
                for (int i = 0; i < parts.Count; ++i)
                {
                    var bounds = parts[i].bounds;
                    bounds.Expand(0.5f);

                    Vector3[] verts = parts[i].vertices;
                    Vector3 avg = Vector3.zero;
                    foreach (var v in verts) avg += v;
                    avg /= verts.Length;

                    Vector3 randomDir = Random.onUnitSphere;
                    Plane plane = new Plane(randomDir, avg);

                    var left = GenerateMesh(parts[i], plane, true);
                    var right = GenerateMesh(parts[i], plane, false);

                    if (left.vertices.Length > 0)
                        subParts.Add(left);
                    if (right.vertices.Length > 0)
                        subParts.Add(right);
                }
                
                parts = new List<PartMesh>(subParts);
                subParts.Clear();
            }

            var parent = new GameObject(name);
            parent.transform.SetPositionAndRotation(transform.position, transform.rotation);
            
            for (int i = 0; i < parts.Count; ++i)
            {
                PartMesh part = parts[i];
                part.MakeGameobject(this);

                var direction = new Vector3(part.bounds.center.x, Mathf.Max(part.bounds.center.y, 0f), part.bounds.center.z);
                part.createdObject.GetComponent<Rigidbody>().AddForceAtPosition(direction * _explodeForce, transform.position, ForceMode.Impulse);
                part.createdObject.transform.SetParent(parent.transform);
            }
            
            Destroy(gameObject);
        }

        private PartMesh GenerateMesh(PartMesh original, Plane plane, bool left)
        {
            var partMesh = new PartMesh();
            var ray1 = new Ray();
            var ray2 = new Ray();

            for (int i = 0; i < original.triangles.Length; i++)
            {
                var triangles = original.triangles[i];
                _edgeSet = false;

                for (int j = 0; j < triangles.Length; j += 3)
                {
                    var sideA = plane.GetSide(original.vertices[triangles[j]]) == left;
                    var sideB = plane.GetSide(original.vertices[triangles[j + 1]]) == left;
                    var sideC = plane.GetSide(original.vertices[triangles[j + 2]]) == left;

                    int sideCount = (sideA ? 1 : 0) + (sideB ? 1 : 0) + (sideC ? 1 : 0);
                    if (sideCount == 0) continue;

                    if (sideCount == 3)
                    {
                        partMesh.AddTriangle(i,
                            original.vertices[triangles[j]],
                            original.vertices[triangles[j + 1]],
                            original.vertices[triangles[j + 2]],
                            original.normals[triangles[j]],
                            original.normals[triangles[j + 1]],
                            original.normals[triangles[j + 2]],
                            original.uv[triangles[j]],
                            original.uv[triangles[j + 1]],
                            original.uv[triangles[j + 2]]);
                        continue;
                    }

                    int singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                    ray1.origin = original.vertices[triangles[j + singleIndex]];
                    Vector3 dir1 = original.vertices[triangles[j + ((singleIndex + 1) % 3)]] - ray1.origin;
                    ray1.direction = dir1;
                    plane.Raycast(ray1, out float enter1);
                    float lerp1 = enter1 / dir1.magnitude;

                    ray2.origin = original.vertices[triangles[j + singleIndex]];
                    Vector3 dir2 = original.vertices[triangles[j + ((singleIndex + 2) % 3)]] - ray2.origin;
                    ray2.direction = dir2;
                    plane.Raycast(ray2, out float enter2);
                    float lerp2 = enter2 / dir2.magnitude;

                    AddEdge(partMesh,
                        left ? -plane.normal : plane.normal,
                        ray1.origin + ray1.direction * enter1,
                        ray2.origin + ray2.direction * enter2);

                    if (sideCount == 1)
                    {
                        partMesh.AddTriangle(i,
                            original.vertices[triangles[j + singleIndex]],
                            ray1.origin + ray1.direction * enter1,
                            ray2.origin + ray2.direction * enter2,
                            original.normals[triangles[j + singleIndex]],
                            Vector3.Lerp(original.normals[triangles[j + singleIndex]], original.normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector3.Lerp(original.normals[triangles[j + singleIndex]], original.normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                            original.uv[triangles[j + singleIndex]],
                            Vector2.Lerp(original.uv[triangles[j + singleIndex]], original.uv[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector2.Lerp(original.uv[triangles[j + singleIndex]], original.uv[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    }

                    if (sideCount == 2)
                    {
                        partMesh.AddTriangle(i,
                            ray1.origin + ray1.direction * enter1,
                            original.vertices[triangles[j + ((singleIndex + 1) % 3)]],
                            original.vertices[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector3.Lerp(original.normals[triangles[j + singleIndex]], original.normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.normals[triangles[j + ((singleIndex + 1) % 3)]],
                            original.normals[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector2.Lerp(original.uv[triangles[j + singleIndex]], original.uv[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.uv[triangles[j + ((singleIndex + 1) % 3)]],
                            original.uv[triangles[j + ((singleIndex + 2) % 3)]]);

                        partMesh.AddTriangle(i,
                            ray1.origin + ray1.direction * enter1,
                            original.vertices[triangles[j + ((singleIndex + 2) % 3)]],
                            ray2.origin + ray2.direction * enter2,
                            Vector3.Lerp(original.normals[triangles[j + singleIndex]], original.normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.normals[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector3.Lerp(original.normals[triangles[j + singleIndex]], original.normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                            Vector2.Lerp(original.uv[triangles[j + singleIndex]], original.uv[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.uv[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector2.Lerp(original.uv[triangles[j + singleIndex]], original.uv[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    }
                }
            }

            partMesh.FillArrays();
            return partMesh;
        }

        private void AddEdge(PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2)
        {
            if (!_edgeSet)
            {
                _edgeSet = true;
                _edgeVertex = vertex1;
            }
            else
            {
                _edgePlane.Set3Points(_edgeVertex, vertex1, vertex2);

                Vector2 uvA = CalculatePlanarUV(_edgeVertex, normal);
                Vector2 uvB = CalculatePlanarUV(vertex1, normal);
                Vector2 uvC = CalculatePlanarUV(vertex2, normal);

                partMesh.AddCapTriangle(_edgeVertex, vertex1, vertex2, normal, uvA, uvB, uvC);
            }
        }

        private Vector2 CalculatePlanarUV(Vector3 vertex, Vector3 normal)
        {
            var abs = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
            if (abs.z >= abs.x && abs.z >= abs.y)
                return new Vector2(vertex.x, vertex.y);
            else if (abs.y >= abs.x)
                return new Vector2(vertex.x, vertex.z);
            else
                return new Vector2(vertex.y, vertex.z);
        }

        public class PartMesh
        {
            private List<Vector3> _vertices = new();
            private List<Vector3> _normals = new();
            private List<List<int>> _triangles = new();
            private List<int> _capTriangles = new();
            private List<Vector2> _uvs = new();

            public Vector3[] vertices;
            public Vector3[] normals;
            public int[][] triangles;
            public Vector2[] uv;
            public GameObject createdObject;
            public Bounds bounds = new();

            public PartMesh()
            {
                bounds.min = Vector3.one * float.MaxValue;
                bounds.max = Vector3.one * float.MinValue;
            }

            public void AddTriangle(int submesh, Vector3 v1, Vector3 v2, Vector3 v3,
                Vector3 n1, Vector3 n2, Vector3 n3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                while (_triangles.Count <= submesh)
                    _triangles.Add(new List<int>());

                _triangles[submesh].Add(_vertices.Count); _vertices.Add(v1);
                _triangles[submesh].Add(_vertices.Count); _vertices.Add(v2);
                _triangles[submesh].Add(_vertices.Count); _vertices.Add(v3);

                _normals.Add(n1); _normals.Add(n2); _normals.Add(n3);
                _uvs.Add(uv1); _uvs.Add(uv2); _uvs.Add(uv3);

                bounds.min = Vector3.Min(bounds.min, Vector3.Min(v1, Vector3.Min(v2, v3)));
                bounds.max = Vector3.Max(bounds.max, Vector3.Max(v1, Vector3.Max(v2, v3)));
            }

            public void AddCapTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal,
                Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                _capTriangles.Add(_vertices.Count); _vertices.Add(v1);
                _capTriangles.Add(_vertices.Count); _vertices.Add(v2);
                _capTriangles.Add(_vertices.Count); _vertices.Add(v3);

                _normals.Add(normal); _normals.Add(normal); _normals.Add(normal);
                _uvs.Add(uv1); _uvs.Add(uv2); _uvs.Add(uv3);
            }

            public void FillArrays()
            {
                vertices = _vertices.ToArray();
                normals = _normals.ToArray();
                uv = _uvs.ToArray();

                int extra = _capTriangles.Count > 0 ? 1 : 0;
                triangles = new int[_triangles.Count + extra][];

                for (int i = 0; i < _triangles.Count; i++)
                    triangles[i] = _triangles[i].ToArray();

                if (_capTriangles.Count > 0)
                    triangles[^1] = _capTriangles.ToArray();
            }

            public void MakeGameobject(DestructibleObject original)
            {
                createdObject = new GameObject(original.name); //����
                createdObject.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);
                createdObject.transform.localScale = original.transform.localScale;

                var mesh = new Mesh
                {
                    name = original.GetComponent<MeshFilter>().mesh.name,
                    vertices = vertices,
                    normals = normals,
                    uv = uv,
                    subMeshCount = triangles.Length
                };

                for (int i = 0; i < triangles.Length; i++)
                    mesh.SetTriangles(triangles[i], i);

                bounds = mesh.bounds;

                var renderer = createdObject.AddComponent<MeshRenderer>();
                var originalMaterial = original.GetComponent<MeshRenderer>().material;
                var finalMaterials = new Material[triangles.Length];

                finalMaterials[0] = originalMaterial;

                for (int i = 1; i < triangles.Length; i++)
                    finalMaterials[i] = original._capMaterial;

                renderer.materials = finalMaterials;

                var filter = createdObject.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                var collider = createdObject.AddComponent<MeshCollider>();
                collider.convex = true;

                var rigidbody = createdObject.AddComponent<Rigidbody>();
                rigidbody.mass = 15f;

                original.TutorialDestroyed(createdObject);
            }

            
        }

        protected virtual void TutorialDestroyed(GameObject createdObject)
        {
            
        }
    }
}
