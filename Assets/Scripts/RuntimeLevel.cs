using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Monta un nivel mientras se juega a partir de un mapa de texto (las mismas letras que
    /// Assets/Levels), con las piezas de Resources/Piezas. Lo usan el editor de niveles y el modo
    /// sin fin (que va añadiendo tramos por la derecha).
    /// </summary>
    public class RuntimeLevel : MonoBehaviour
    {
        public const int Rows = 14;

        static readonly Dictionary<char, string> Names = new Dictionary<char, string>
        {
            ['#'] = "suelo", ['t'] = "tierra", ['D'] = "caja", ['N'] = "hielo", ['d'] = "tabla", ['n'] = "placa", ['~'] = "lago",
            ['^'] = "pinchos", ['F'] = "muelle", ['M'] = "sombra", ['B'] = "abeja", ['X'] = "sierra", ['*'] = "fragmento",
            ['K'] = "cofre", ['C'] = "control", ['G'] = "meta", ['O'] = "relojsol", ['Y'] = "luciernaga", ['Z'] = "cajacambia",
        };

        readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        readonly List<GameObject> spawned = new List<GameObject>();
        Transform root;

        /// <summary>Columnas montadas hasta ahora (el siguiente tramo empieza aquí).</summary>
        public int Width { get; private set; }
        public Vector3 Start { get; private set; } = new Vector3(2.5f, 3f, 0f);
        public int Fragments { get; private set; }

        GameObject Prefab(char code)
        {
            if (!Names.TryGetValue(code, out var name)) return null;
            if (!prefabs.TryGetValue(name, out var prefab)) prefabs[name] = prefab = Resources.Load<GameObject>("Piezas/" + name);
            return prefab;
        }

        public void Clear()
        {
            foreach (var go in spawned) if (go != null) Destroy(go);
            spawned.Clear();
            Width = 0;
            Fragments = 0;
        }

        /// <summary>Añade un mapa (filas de texto, la última es el suelo del mundo) a partir de la columna `offset`.</summary>
        public void Append(string[] rows, int offset)
        {
            if (root == null) root = new GameObject("Piezas").transform;
            int height = rows.Length;
            for (int row = 0; row < height; row++)
            {
                string line = rows[row];
                for (int col = 0; col < line.Length; col++)
                {
                    char c = line[col];
                    if (c == ' ') continue;
                    float bottom = height - 1 - row;
                    var cell = new Vector3(offset + col + 0.5f, bottom, 0f);
                    if (c == 'S') { Start = cell + Vector3.up * 0.95f; continue; }
                    // Suelo con hierba solo si arriba hay aire.
                    char code = c == '#' && row > 0 && col < rows[row - 1].Length && rows[row - 1][col] == '#' ? 't' : c;
                    var prefab = Prefab(code);
                    if (prefab == null) continue;
                    var piece = Instantiate(prefab, cell, Quaternion.identity, root);
                    spawned.Add(piece);
                    if (c == '*' || c == 'K') Fragments++;
                    foreach (var p in piece.GetComponentsInChildren<WorldPiece>()) p.Clip();
                }
                Width = Mathf.Max(Width, offset + line.Length);
            }
        }

        /// <summary>Coloca a Lumi en la salida y ajusta la línea y la cámara al ancho montado.</summary>
        public void PlacePlayer()
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.transform.position = Start;
                player.GetComponent<Rigidbody2D>().position = Start;
                player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                player.SetCheckpoint(Start);
            }
            Fit();
        }

        public void Fit()
        {
            var line = ShiftLine.Instance;
            if (line != null) { line.minX = 0.5f; line.maxX = Mathf.Max(10f, Width - 0.5f); }
            var follow = FindAnyObjectByType<CameraFollow>();
            if (follow != null) { follow.minX = Mathf.Min(9f, Width * 0.5f); follow.maxX = Mathf.Max(Width - 9f, Width * 0.5f); }
        }
    }
}
