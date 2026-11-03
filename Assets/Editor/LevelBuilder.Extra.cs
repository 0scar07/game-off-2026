using System.Collections.Generic;
using Fase;
using UnityEngine;

/// <summary>
/// Mecánicas de la segunda tanda: plataformas de sol (O), corrientes de aire (W), camino de
/// luciérnagas (Y) y la Gran Sombra (J). Ver Assets/Levels/LEEME.md.
/// </summary>
public static partial class LevelBuilder
{
    /// <summary>Reloj de sol: plataforma de madera con un engranaje dorado que solo se mueve de día.</summary>
    static void BuildSunPlatform(Vector3 cell, Transform parent)
    {
        var platform = new GameObject("PlataformaSol");
        platform.transform.SetParent(parent);
        platform.transform.position = cell + new Vector3(0f, 0.85f, 0f);
        var body = platform.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        platform.AddComponent<BoxCollider2D>().size = new Vector2(3f, 0.3f);
        var sun = platform.AddComponent<SunPlatform>();
        for (int i = -1; i <= 1; i++)
            Fit(Quat + "Bridge_Modular_Center.fbx", cell + new Vector3(i, 0.55f, 0f), new Vector3(1.02f, 0.75f, 1.6f), Both, platform.transform);
        var gear = Primitive(PrimitiveType.Cylinder, "Engranaje", platform.transform.position + new Vector3(0f, -0.35f, -0.85f), new Vector3(0.7f, 0.06f, 0.7f),
            Lit("Engranaje", Both, new Color(1f, 0.78f, 0.3f), new Color(0.25f, 0.15f, 0f)));
        gear.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        gear.transform.SetParent(platform.transform, true);
        for (int k = 0; k < 6; k++)
        {
            var tooth = Primitive(PrimitiveType.Cube, "Diente", gear.transform.position, new Vector3(0.16f, 0.16f, 0.12f), Lit("Engranaje", Both, default, default));
            tooth.transform.SetParent(gear.transform, true);
            tooth.transform.localPosition = Quaternion.Euler(0f, k * 60f, 0f) * new Vector3(0.62f, 0f, 0f);
        }
        sun.gear = gear.transform;
        Glow("Sol", platform.transform.position + new Vector3(0f, -0.35f, -1f), 1.6f, new Color(1.3f, 0.9f, 0.4f, 0.5f), Day, platform.transform);
    }

    /// <summary>Corriente de aire: desde la W más baja hasta la última W de la columna.</summary>
    static void BuildAirCurrent(Level level, int col, int row, Transform parent)
    {
        int top = row;
        while (level.At(col, top - 1) == 'W') top--;
        float bottom = level.Bottom(row), height = row - top + 1;
        var center = new Vector3(col + 0.5f, bottom + height * 0.5f, 0f);
        var current = new GameObject("Corriente");
        current.transform.SetParent(parent);
        current.transform.position = center;
        current.AddComponent<AirCurrent>().size = new Vector2(1.6f, height);

        // Hojas y polvo que suben (solo se ven de día).
        var leaves = Particles("Hojas", current.transform, center + new Vector3(0f, -height * 0.5f, -0.4f),
            ParticleMaterial("Corriente", SoftDot(), Color.white, Day, false));
        var main = leaves.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(height / 5f, height / 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 5.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 0.9f, 0.8f), new Color(0.75f, 0.95f, 0.6f, 0.8f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = leaves.emission;
        emission.rateOverTime = 14f * height;
        var shape = leaves.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1.4f, 0.1f, 0.2f);
        shape.rotation = new Vector3(-90f, 0f, 0f);
        var noise = leaves.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        FadeOut(leaves, true);
        Glow("Remolino", center + new Vector3(0f, 0f, -0.3f), height * 0.6f, new Color(0.8f, 1f, 0.9f, 0.18f), Day, current.transform)
            .transform.localScale = new Vector3(1.8f, height, 1f);
    }

    /// <summary>Losa escondida del camino de luciérnagas (aparece de noche junto al farol).</summary>
    static void BuildGlowTile(Vector3 cell, Transform parent)
    {
        var tile = new GameObject("LosaLuciernaga");
        tile.transform.SetParent(parent);
        tile.transform.position = cell + new Vector3(0f, 0.85f, 0f);
        var box = tile.AddComponent<BoxCollider2D>();
        box.size = new Vector2(1f, 0.3f);
        var slab = Primitive(PrimitiveType.Cube, "Losa", tile.transform.position, new Vector3(1f, 0.25f, 1.2f),
            Lit("LosaLuciernaga", Night, new Color(0.35f, 0.55f, 0.35f), new Color(0.35f, 0.8f, 0.3f)));
        slab.transform.SetParent(tile.transform, true);
        var glow = Glow("Luz", tile.transform.position + new Vector3(0f, 0.1f, -0.4f), 1.6f, new Color(0.7f, 1.4f, 0.4f, 0.6f), Night, tile.transform);
        var path = tile.AddComponent<GlowPath>();
        path.solid = box;
        path.renderers = new[] { slab.GetComponent<Renderer>(), glow.GetComponent<Renderer>() };

        // Luciérnagas que siempre revolotean encima y delatan el camino.
        var flies = Particles("Luciernagas", parent, cell + new Vector3(0f, 1.6f, -0.3f), ParticleMaterial("Luciernaga", SoftDot(), Color.white, Night, true));
        var main = flies.main;
        main.startLifetime = 2.5f;
        main.startSpeed = 0.15f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        main.startColor = new Color(0.8f, 1.6f, 0.5f, 1f);
        var emission = flies.emission;
        emission.rateOverTime = 1.5f;
        var shape = flies.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        var noise = flies.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        FadeOut(flies, true);
    }

    static readonly Dictionary<string, (Color, Color)> DawnBird = new Dictionary<string, (Color, Color)>
    {
        ["Main"] = (new Color(1f, 0.55f, 0.25f), new Color(0.35f, 0.12f, 0f)),
        ["Main_2"] = (new Color(0.75f, 0.3f, 0.15f), default),
        ["Wings"] = (new Color(1f, 0.85f, 0.45f), new Color(0.6f, 0.4f, 0.05f)),
        ["Eyes"] = (new Color(0.1f, 0.08f, 0.1f), default),
    };

    static readonly Dictionary<string, (Color, Color)> FrostQueen = new Dictionary<string, (Color, Color)>
    {
        ["Main"] = (new Color(0.55f, 0.82f, 1f), new Color(0.1f, 0.35f, 0.7f)),
        ["Main_2"] = (new Color(0.35f, 0.6f, 0.95f), default),
        ["Main_Dark"] = (new Color(0.25f, 0.45f, 0.85f), default),
        ["Eyes"] = (new Color(1f, 0.3f, 0.55f), new Color(1.6f, 0.25f, 0.7f)),
    };

    /// <summary>
    /// Jefes: J = la Gran Sombra (de noche), P = la Garra del Alba (pájaro de día),
    /// R = la Reina de Escarcha (de noche). Todos: estatua en el otro mundo, ondas y punto débil.
    /// </summary>
    static void BuildBoss(Level level, Vector3 cell, Transform parent, char kind = 'J')
    {
        bool bird = kind == 'P', queen = kind == 'R';
        string path = Quat + (bird ? "Bee.fbx" : queen ? "Crab.fbx" : "Enemy.fbx");
        var palette = bird ? DawnBird : queen ? FrostQueen : ShadowNight;
        var boss = new GameObject(bird ? "GarraDelAlba" : queen ? "ReinaDeEscarcha" : "GranSombra");
        boss.transform.SetParent(parent);
        boss.transform.position = cell + (bird ? Vector3.up * 1.5f : Vector3.zero);
        cell = boss.transform.position;
        var size = bird ? new Vector3(4.6f, 3.2f, 3.4f) : queen ? new Vector3(4.8f, 3.4f, 3.6f) : new Vector3(4.6f, 4.4f, 3.6f);
        int awakeWorld = bird ? Day : Night, sleepWorld = bird ? Night : Day;
        var night = Fit(path, cell, size, awakeWorld, boss.transform, recolor: palette);
        var statue = Fit(path, cell, size, sleepWorld, boss.transform, recolor: ShadowStone);
        night.transform.rotation = Quaternion.Euler(0f, 90f, 0f) * night.transform.rotation;
        statue.transform.rotation = Quaternion.Euler(0f, 90f, 0f) * statue.transform.rotation;
        var animator = night.GetComponentInChildren<Animator>();
        if (animator == null) animator = night.AddComponent<Animator>();
        string idle = bird ? "Flying" : "Idle", attack = bird ? "Bite_Front" : "Bite_InPlace";
        animator.runtimeAnimatorController = ClipController(path, $"Assets/Art/Jefe_{kind}.controller", new[] { idle, attack, "HitRecieve" });
        var statueAnimator = statue.GetComponentInChildren<Animator>();
        if (statueAnimator != null) Object.DestroyImmediate(statueAnimator);
        Glow("Aura", cell + new Vector3(0f, 2.2f, -1.5f), 9f, bird ? new Color(1.4f, 0.8f, 0.3f, 0.45f) : queen ? new Color(0.4f, 0.8f, 1.6f, 0.5f) : new Color(0.6f, 0.25f, 1f, 0.5f), awakeWorld, night.transform);
        // Cuerpo sólido: se puede subir encima cuando es de piedra.
        float bodyHeight = size.y * 0.72f;
        Solid("CuerpoJefe", new Vector2(cell.x, cell.y + bodyHeight * 0.5f), new Vector2(2.8f, bodyHeight), null, boss.transform);

        // Punto débil: fragmento de luna en la cabeza.
        var weak = new GameObject("PuntoDebil").transform;
        weak.SetParent(boss.transform);
        weak.position = cell + new Vector3(0f, bodyHeight + 1f, 0f);
        Fit(Quat + "Star.fbx", weak.position + new Vector3(0f, -0.35f, 0f), new Vector3(0.9f, 0.86f, 0.4f), Both, weak,
            recolor: new Dictionary<string, (Color, Color)> { ["Gold"] = (new Color(1f, 0.92f, 0.6f), new Color(0.9f, 0.7f, 0.2f)) });
        Glow("Brillo", weak.position + new Vector3(0f, 0f, -0.5f), 3f, new Color(1.5f, 1.1f, 0.5f, 0.8f), Both, weak).GetComponent<GlowSprite>().dayFactor = 1f;

        // Onda de muestra: un arco morado que corre por el suelo.
        var wave = new GameObject("Onda");
        wave.transform.SetParent(boss.transform);
        wave.transform.position = new Vector3(cell.x - 2f, cell.y - (bird ? 1.5f : 0f) + 0.5f, -0.2f);
        Glow("Arco", wave.transform.position, 2.2f, bird ? new Color(1.6f, 0.9f, 0.3f, 0.9f) : new Color(0.8f, 0.35f, 1.6f, 0.9f), awakeWorld, wave.transform).transform.localScale = new Vector3(1.2f, 1.8f, 1f);
        wave.AddComponent<BossWave>();
        wave.SetActive(false);

        var b = boss.AddComponent<Boss>();
        b.visual = night.transform;
        b.statue = statue.transform;
        b.animator = animator;
        b.weakPoint = weak;
        b.waveTemplate = wave;
        b.dayCreature = bird;
        b.idleState = idle;
        b.attackState = attack;
        if (bird || queen)
        {
            b.health = 2;
            b.bossName = bird ? "Garra del Alba" : "Reina de Escarcha";
            b.bossNameEn = bird ? "Dawn Talon" : "Frost Queen";
        }
    }

    /// <summary>Caja que cambia (Z): madera de día, hielo de noche; se puede empujar.</summary>
    static void BuildChangingBox(Vector3 cell, Transform parent)
    {
        var box = new GameObject("CajaQueCambia");
        box.transform.SetParent(parent);
        box.transform.position = cell + new Vector3(0f, 0.5f, 0f);
        box.AddComponent<Rigidbody2D>();
        box.AddComponent<BoxCollider2D>().size = new Vector2(0.96f, 0.96f);
        box.AddComponent<ChangingBox>();
        Fit(Quat + "Cube_Crate.fbx", cell, new Vector3(0.98f, 0.98f, 1.2f), Day, box.transform);
        Fit(Platformer + "block-snow.fbx", cell, new Vector3(0.98f, 0.98f, 1.2f), Night, box.transform, new Color(0.7f, 0.85f, 1f), IceGlow);
    }

    /// <summary>Tras construir el nivel: la meta del jefe solo aparece al vencerlo.</summary>
    static void LinkBossGoal(Transform root, Transform moving)
    {
        var boss = moving.GetComponentInChildren<Boss>(true);
        if (boss == null) return;
        var goal = root.Find("Meta");
        if (goal != null) boss.goal = goal.gameObject;
    }
}
