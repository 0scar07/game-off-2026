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

    /// <summary>La Gran Sombra: Sombra enorme con estatua de piedra, ondas y punto débil.</summary>
    static void BuildBoss(Level level, Vector3 cell, Transform parent)
    {
        string path = Quat + "Enemy.fbx";
        var boss = new GameObject("GranSombra");
        boss.transform.SetParent(parent);
        boss.transform.position = cell;
        var size = new Vector3(4.6f, 4.4f, 3.6f);
        var night = Fit(path, cell, size, Night, boss.transform, recolor: ShadowNight);
        var statue = Fit(path, cell, size, Day, boss.transform, recolor: ShadowStone);
        night.transform.rotation = Quaternion.Euler(0f, 90f, 0f) * night.transform.rotation;
        statue.transform.rotation = Quaternion.Euler(0f, 90f, 0f) * statue.transform.rotation;
        var animator = night.GetComponentInChildren<Animator>();
        if (animator == null) animator = night.AddComponent<Animator>();
        animator.runtimeAnimatorController = ClipController(path, "Assets/Art/GranSombra.controller", new[] { "Idle", "Bite_InPlace", "HitRecieve" });
        var statueAnimator = statue.GetComponentInChildren<Animator>();
        if (statueAnimator != null) Object.DestroyImmediate(statueAnimator);
        Glow("Aura", cell + new Vector3(0f, 2.2f, -1.5f), 9f, new Color(0.6f, 0.25f, 1f, 0.5f), Night, night.transform);
        // Cuerpo sólido: se puede subir encima cuando es de piedra.
        Solid("CuerpoJefe", new Vector2(cell.x, cell.y + 1.6f), new Vector2(2.8f, 3.2f), null, boss.transform);

        // Punto débil: fragmento de luna en la cabeza.
        var weak = new GameObject("PuntoDebil").transform;
        weak.SetParent(boss.transform);
        weak.position = cell + new Vector3(0f, 4.2f, 0f);
        Fit(Quat + "Star.fbx", weak.position + new Vector3(0f, -0.35f, 0f), new Vector3(0.9f, 0.86f, 0.4f), Both, weak,
            recolor: new Dictionary<string, (Color, Color)> { ["Gold"] = (new Color(1f, 0.92f, 0.6f), new Color(0.9f, 0.7f, 0.2f)) });
        Glow("Brillo", weak.position + new Vector3(0f, 0f, -0.5f), 3f, new Color(1.5f, 1.1f, 0.5f, 0.8f), Both, weak).GetComponent<GlowSprite>().dayFactor = 1f;

        // Onda de muestra: un arco morado que corre por el suelo.
        var wave = new GameObject("Onda");
        wave.transform.SetParent(boss.transform);
        wave.transform.position = cell + new Vector3(-2f, 0.5f, -0.2f);
        Glow("Arco", wave.transform.position, 2.2f, new Color(0.8f, 0.35f, 1.6f, 0.9f), Night, wave.transform).transform.localScale = new Vector3(1.2f, 1.8f, 1f);
        wave.AddComponent<BossWave>();
        wave.SetActive(false);

        var b = boss.AddComponent<Boss>();
        b.visual = night.transform;
        b.statue = statue.transform;
        b.animator = animator;
        b.weakPoint = weak;
        b.waveTemplate = wave;
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
