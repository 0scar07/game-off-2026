using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Puerta de un mundo (casillas 3 = de día, 4 = de noche): un muro de dos casillas, sólido en
    /// los dos mundos, que se abre si Lumi llega con una llave del mismo mundo.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KeyDoor : Mechanic
    {
        public World world = World.Day;
        BoxCollider2D box;
        Transform door;
        float open;

        void Start()
        {
            box = GetComponent<BoxCollider2D>();
            var color = world == World.Day ? new Color(0.82f, 0.58f, 0.32f) : new Color(0.38f, 0.5f, 0.95f);
            door = Shapes.Block(transform, "Puerta", transform.position, new Vector3(0.95f, 2f, 1f), color).transform;
            Shapes.Glow(door, "Cerradura", transform.position + new Vector3(0f, 0f, -0.55f), new Vector2(0.55f, 0.55f), Color.white, 2f, false, KeyPickup.KeyTexture());
        }

        void Update()
        {
            var player = Player;
            if (box.enabled && player != null && Vector2.Distance(player.transform.position, transform.position) < 1.4f)
            {
                bool has = world == World.Day ? KeyPickup.DayKeys > 0 : KeyPickup.NightKeys > 0;
                if (has)
                {
                    if (world == World.Day) KeyPickup.DayKeys--; else KeyPickup.NightKeys--;
                    box.enabled = false;
                    Sfx.Play("boton", 0.9f, 0.6f);
                }
            }
            if (!box.enabled && open < 1f)
            {
                open = Mathf.MoveTowards(open, 1f, Time.deltaTime * 2.5f);
                door.position = transform.position + Vector3.up * open * 2.1f;
                door.localScale = new Vector3(0.95f, 2f * (1f - open * 0.8f), 1f);
            }
        }
    }
}
