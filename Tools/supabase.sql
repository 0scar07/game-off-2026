-- FASE · tablas de Supabase para los récords en línea (80) y la analítica anónima (140).
-- Pégalo entero en Supabase → SQL Editor → New query → Run.
-- El juego solo usa la clave pública ("anon"): las reglas de abajo deciden qué puede hacer.

-- ───────── Récords en línea ─────────
create table if not exists public.records (
  id         bigint generated always as identity primary key,
  level      int         not null check (level between 1 and 200),
  name       text        not null check (char_length(name) between 1 and 12),
  time_ms    int         not null check (time_ms between 3000 and 3600000),
  created_at timestamptz not null default now()
);
create index if not exists records_level_time on public.records (level, time_ms);

alter table public.records enable row level security;

-- Cualquiera puede leer la tabla y añadir un récord (con los límites de arriba); nadie puede
-- cambiar ni borrar los de otros.
drop policy if exists "leer records" on public.records;
create policy "leer records" on public.records for select to anon using (true);
drop policy if exists "subir records" on public.records;
create policy "subir records" on public.records for insert to anon with check (true);

grant select, insert on public.records to anon;

-- Los 10 mejores de un nivel, un tiempo por nombre (su mejor marca).
create or replace function public.top_records(lvl int)
returns table (name text, time_ms int)
language sql stable security definer set search_path = public as $$
  select name, min(time_ms) as time_ms
  from public.records
  where level = lvl
  group by name
  order by min(time_ms)
  limit 10;
$$;
grant execute on function public.top_records(int) to anon;

-- ───────── Analítica anónima ─────────
-- Sin nombres ni identificadores: solo qué pasó, en qué nivel y dónde (x).
create table if not exists public.events (
  id         bigint generated always as identity primary key,
  level      int         not null check (level between 0 and 200),
  kind       text        not null check (kind in ('start', 'death', 'finish', 'quit')),
  x          real,
  seconds    real,
  created_at timestamptz not null default now()
);
alter table public.events enable row level security;

-- El juego solo puede añadir eventos; leerlos solo se puede desde el panel de Supabase.
drop policy if exists "subir eventos" on public.events;
create policy "subir eventos" on public.events for insert to anon with check (true);
grant insert on public.events to anon;

-- Resumen para ajustar niveles: dónde muere más gente en cada nivel (verlo en el SQL Editor).
-- select level, round(x) as x, count(*) as muertes from events where kind = 'death'
--   group by level, round(x) order by level, muertes desc;
