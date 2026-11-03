# FASE · Documento de diseño

## La idea en una frase

Una línea parte el mundo en **día** y **noche**; cada cosa existe solo en su mundo, y el jugador
mueve la línea para decidir qué es real en cada momento.

## Encaje con el tema SHIFT

*Shift* es literal: el jugador desplaza (*shifts*) la frontera entre dos mundos, y con ella cambia
(*shifts*) el mundo entero. La tecla por defecto para moverla es **Shift**.

## Bucle de juego

1. Lumi avanza por un nivel en 2.5D.
2. Delante hay un obstáculo que solo se resuelve en un mundo (un hueco con puente de día, un
   lago que solo se pisa helado de noche, una Sombra que solo duerme de día...).
3. El jugador coloca la línea para que cada trozo esté en el mundo que necesita, a veces
   moviéndola mientras salta.
4. Fragmentos de luna, cofres secretos y medallas de tiempo invitan a repetir.

## Reglas de la línea

| Lado de **día** | Lado de **noche** |
|---|---|
| Madera: cajas, tablas, puentes | Hielo: bloques, placas, lagos helados |
| Flores-muelle (impulsan hacia arriba) | Sombras despiertas (hacen perder) |
| Relojes de sol en marcha | Abejas de la noche |
| Corrientes de aire que elevan | Caminos de luciérnagas (cerca del farol) |
| Sombras convertidas en piedra | Caja mágica de hielo (resbala) |

Variantes: línea **inclinada** (capítulo 3), **tumbada** (la torre vertical), **doble** (día ·
noche · día), con **energía** limitada, y la **burbuja de noche** del farol (poder del capítulo 3).

## Estructura

| Capítulo | Bioma | Niveles | Jefe | Secreto |
|---|---|---|---|---|
| I · La Pradera | verano, flauta y marimba | Amanecer, Noche estrellada, Las dos mitades, Lago helado, Relojes de sol | Garra del Alba (pájaro de día) | Jardín del viento |
| II · El Bosque Otoñal | otoño, guitarra y cuerdas | Criaturas, Alas en la noche, Al revés, Sierras del bosque, Luciérnagas | Reina de Escarcha | Bosque oscuro |
| III · La Cumbre Helada | nieve, campanas y coro | La torre, Río nocturno, Dos líneas, La subida, Eclipse, Cuesta de la luna | La Gran Sombra (final) | Aurora |

Los niveles secretos se abren con todos los fragmentos de su capítulo.

## Personajes

- **Lumi**: portadora de farol. Corre, salta (con margen de coyote y salto guardado) y mueve la línea.
- **Nox**: la Lumi de la noche (se desbloquea en el capítulo I): salta más alto y corre más despacio.
- **Sombras**: patrullan de noche; de día son estatuas.
- **Jefes**: duermen en el mundo contrario; en piedra muestran un fragmento en la cabeza que hay que tocar.

## Progresión y rejugabilidad

- Medallas de tiempo: Bronce, Plata, Oro y **Luna** (secreta).
- 22 logros (4 secretos), desafíos por nivel (sin caer, toda la luna, pocas líneas).
- Diario de Lumi: una página por nivel, en los cofres.
- Fantasma del récord, repeticiones a ×3, estadísticas totales.
- Armario (8 bufandas, 4 faroles) y tienda de cosméticos pagada con fragmentos.
- Taller de niveles con códigos, modo sin fin, desafío diario, modo espejo, cooperativo local.
- Dos finales: reconstruir la luna o dejar el mundo partido.

## Accesibilidad

Modo asistido (sin medallas), alto contraste (rayas en lo que solo existe de noche), teclas
configurables, velocidad de la línea, vibración opcional, textos en español e inglés, y una
luciérnaga que da pistas si te atascas.

## Dirección artística

Low poly suave (Quaternius, Kenney) con luz cálida de día y fría de noche, posprocesado con
bloom y viñeta, partículas (polen, nieve, luciérnagas), cielo con sol, luna y aurora, y una
línea brillante que corta la imagen en pantalla para que el corte sea limpio de cerca y de lejos.

## Sonido

Música propia por capítulo en dos versiones (día y noche) que se mezclan según cuánta pantalla
ocupa cada mundo; una capa de campanas cuando Lumi está sobre la línea; tambores con los jefes;
efectos sintetizados en la tonalidad de la música y la vocecita de Lumi.
