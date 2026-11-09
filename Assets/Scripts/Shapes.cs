using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Formas simples (plano, esfera, cilindro) sin colisionador. Sustituye a
    /// GameObject.CreatePrimitive, que en la web intenta añadir colisionadores 3D que el juego no
    /// usa y que se quitan al exportar (y llenaba la consola de errores).
    /// </summary>
    public static class Shapes
    {
        static readonly Dictionary<PrimitiveType, Mesh> meshes = new Dictionary<PrimitiveType, Mesh>();

        public static GameObject Create(PrimitiveType type, string name = null)
        {
            var go = new GameObject(name ?? type.ToString());
            go.AddComponent<MeshFilter>().sharedMesh = MeshFor(type);
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = DefaultMaterial;
            return go;
        }

        static Material defaultMaterial;
        static Material DefaultMaterial
        {
            get
            {
                if (defaultMaterial == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Fase/World");
                    if (shader != null) defaultMaterial = new Material(shader);
                }
                return defaultMaterial;
            }
        }

        static Mesh MeshFor(PrimitiveType type)
        {
            if (meshes.TryGetValue(type, out var mesh) && mesh != null) return mesh;
            mesh = type switch
            {
                PrimitiveType.Sphere => Sphere(16, 12),
                PrimitiveType.Cylinder => Cylinder(20),
                _ => Quad(),
            };
            mesh.name = type.ToString();
            meshes[type] = mesh;
            return mesh;
        }

        /// <summary>Plano de 1×1 mirando hacia -z (como el Quad de Unity).</summary>
        static Mesh Quad()
        {
            var mesh = new Mesh
            {
                vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) },
                uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) },
                triangles = new[] { 0, 3, 1, 3, 0, 2 },
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Esfera de diámetro 1.</summary>
        static Mesh Sphere(int segments, int rings)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int r = 0; r <= rings; r++)
            {
                float v = r / (float)rings, phi = v * Mathf.PI;
                for (int s = 0; s <= segments; s++)
                {
                    float u = s / (float)segments, theta = u * Mathf.PI * 2f;
                    vertices.Add(new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta)) * 0.5f);
                    uvs.Add(new Vector2(u, 1f - v));
                }
            }
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < segments; s++)
                {
                    int a = r * (segments + 1) + s, b = a + segments + 1;
                    triangles.AddRange(new[] { a, a + 1, b, b, a + 1, b + 1 });
                }
            return Build(vertices, uvs, triangles);
        }

        /// <summary>Cilindro de diámetro 1 y alto 2 (como el de Unity), con tapas.</summary>
        static Mesh Cylinder(int segments)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float u = s / (float)segments, theta = u * Mathf.PI * 2f;
                var ring = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * 0.5f;
                vertices.Add(ring + Vector3.down); uvs.Add(new Vector2(u, 0f));
                vertices.Add(ring + Vector3.up); uvs.Add(new Vector2(u, 1f));
            }
            for (int s = 0; s < segments; s++)
            {
                int a = s * 2;
                triangles.AddRange(new[] { a, a + 1, a + 2, a + 2, a + 1, a + 3 });
            }
            foreach (float y in new[] { -1f, 1f })
            {
                int center = vertices.Count;
                vertices.Add(new Vector3(0f, y, 0f)); uvs.Add(new Vector2(0.5f, 0.5f));
                for (int s = 0; s <= segments; s++)
                {
                    float theta = s / (float)segments * Mathf.PI * 2f;
                    vertices.Add(new Vector3(Mathf.Cos(theta) * 0.5f, y, Mathf.Sin(theta) * 0.5f));
                    uvs.Add(new Vector2(0.5f + Mathf.Cos(theta) * 0.5f, 0.5f + Mathf.Sin(theta) * 0.5f));
                }
                for (int s = 0; s < segments; s++)
                    if (y > 0f) triangles.AddRange(new[] { center, center + s + 2, center + s + 1 });
                    else triangles.AddRange(new[] { center, center + s + 1, center + s + 2 });
            }
            return Build(vertices, uvs, triangles);
        }

        static Mesh Build(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
