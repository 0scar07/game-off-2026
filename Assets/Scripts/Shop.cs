using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Tienda de cosméticos: se paga con los fragmentos de luna conseguidos (no se pierden de los
    /// niveles; es un saldo aparte). Lo comprado se pone y se quita desde la tienda.
    /// </summary>
    public static class Shop
    {
        public class Item
        {
            public string id, es, en;
            public int price;
            public Color color;
            public string Name => Loc.Pick(es, en);
        }

        public static readonly Item[] Items =
        {
            new Item { id = "gorro", es = "Gorro de lana", en = "Wool hat", price = 8, color = new Color(1f, 0.48f, 0.66f) },
            new Item { id = "estela", es = "Estela de estrellas", en = "Star trail", price = 12, color = new Color(1f, 0.83f, 0.35f) },
            new Item { id = "lineadorada", es = "Línea dorada", en = "Golden line", price = 15, color = new Color(1f, 0.88f, 0.55f) },
            new Item { id = "cristal", es = "Farol de cristal", en = "Crystal lantern", price = 10, color = new Color(0.62f, 0.9f, 1f) },
        };

        static int Earned
        {
            get
            {
                int total = 0;
                for (int level = 1; level <= LevelCatalog.Get.levels.Length; level++) total += Progress.Fragments(level);
                return total;
            }
        }

        static int Spent => PlayerPrefs.GetInt("fase_gastado", 0);
        public static int Balance => Earned - Spent;

        public static bool Owned(string id) => PlayerPrefs.GetInt("fase_compra_" + id, 0) == 1;
        public static bool Worn(string id) => Owned(id) && PlayerPrefs.GetInt("fase_puesto_" + id, 1) == 1;

        public static bool Buy(Item item)
        {
            if (Owned(item.id) || Balance < item.price) return false;
            PlayerPrefs.SetInt("fase_gastado", Spent + item.price);
            PlayerPrefs.SetInt("fase_compra_" + item.id, 1);
            PlayerPrefs.Save();
            Changed();
            return true;
        }

        public static void Toggle(Item item)
        {
            if (!Owned(item.id)) return;
            PlayerPrefs.SetInt("fase_puesto_" + item.id, Worn(item.id) ? 0 : 1);
            PlayerPrefs.Save();
            Changed();
        }

        static void Changed()
        {
            foreach (var style in Object.FindObjectsByType<LumiStyle>()) style.Apply();
        }
    }
}
