-- FASE · Endurecimiento de Supabase (récords y analítica). Se aplica DESPUÉS de Tools/supabase.sql.
-- Pégalo entero en Supabase → SQL Editor → Run. Es seguro repetirlo.
-- Qué hace:
--   A. Quita los permisos que Supabase da por defecto (anon solo puede insertar en las columnas
--      necesarias; los récords solo se leen con top_records).
--   B. Récords: tiempo mínimo por nivel (60 % de la medalla Luna), nombres con letras normales,
--      como mucho 20 envíos por nombre y nivel y 120 por minuto en total; la hora la pone el servidor.
--   C. Analítica: valores en rango y como mucho 600 eventos por minuto.
-- Interruptor de emergencia si alguien llena la base: revoke insert on public.events from anon;

-- ───────── A. Permisos mínimos ─────────
revoke all on table public.records from anon, authenticated;
revoke all on table public.events  from anon, authenticated;
grant insert (level, name, time_ms)    on table public.records to anon;
grant insert (level, kind, x, seconds) on table public.events  to anon;
drop policy if exists "leer records" on public.records;
alter function public.top_records(int) set search_path = public;

-- ───────── B. Récords ─────────
create table if not exists public.level_limits (
  level  int primary key check (level between 1 and 200),
  min_ms int not null check (min_ms >= 3000)
);
alter table public.level_limits enable row level security;
revoke all on table public.level_limits from anon, authenticated;
insert into public.level_limits (level, min_ms) values
  (1, 15600),  -- 01-amanecer
  (2, 15600),  -- 02-noche-estrellada
  (3, 15600),  -- 03-las-dos-mitades
  (4, 15000),  -- 04-lago-helado
  (5, 16800),  -- 04b-relojes-de-sol
  (6, 10800),  -- 04c-garra-del-alba
  (7, 16800),  -- 05-criaturas
  (8, 16800),  -- 06-alas-en-la-noche
  (9, 15600),  -- 07-al-reves
  (10, 16200),  -- 08-sierras
  (11, 17400),  -- 08b-luciernagas
  (12, 8400),  -- 08c-reina-de-escarcha
  (13, 7800),  -- 09-la-torre
  (14, 15000),  -- 10-rio-nocturno
  (15, 18000),  -- 10b-dos-lineas
  (16, 6000),  -- 10c-la-subida
  (17, 21600),  -- 11-eclipse
  (18, 17400),  -- 11b-cuesta-de-la-luna
  (19, 8400),  -- 12-la-gran-sombra
  (20, 7800),  -- 100-extra-marea
  (21, 9600),  -- 101-extra-llaves
  (22, 10800),  -- 102-extra-oscuro
  (23, 10200),  -- 103-extra-ritmo
  (24, 8400),  -- 104-extra-curva
  (25, 8400),  -- 105-extra-sombras
  (26, 6000),  -- 106-extra-escalada
  (27, 9000),  -- 107-extra-ciudad-aurora
  (28, 9600),  -- 108-extra-tejados
  (29, 9000),  -- 109-extra-avenida
  (30, 7800),  -- 110-extra-arrecife
  (31, 7800),  -- 111-extra-corrientes
  (32, 7200),  -- 112-extra-marea-profunda
  (33, 12000),  -- 90-secreto-jardin-del-viento
  (34, 12000),  -- 91-secreto-bosque-oscuro
  (35, 15600),  -- 92-secreto-aurora
  (36, 13200),  -- 95-trucos-de-la-linea
  (37, 9000),  -- 96-extra-atardecer
  (38, 8400),  -- 97-extra-espejos
  (39, 8400),  -- 98-extra-peso
  (40, 10200)  -- 99-extra-persecucion
on conflict (level) do update set min_ms = excluded.min_ms;

create index if not exists records_level_name on public.records (level, name);
create index if not exists records_created    on public.records (created_at);

create or replace function public.records_guard() returns trigger
language plpgsql security definer set search_path = public as $$
declare n int;
begin
  new.name := btrim(new.name);
  new.created_at := now();
  if new.name !~ '^[A-Za-z0-9ÁÉÍÓÚÜÑÇáéíóúüñçÀÈÌÒÙàèìòùÂÊÎÔÛâêîôûÃÕãõ _.-]{1,12}$' then
    raise exception 'nombre no permitido' using errcode = '23514';
  end if;
  if new.time_ms < coalesce((select l.min_ms from public.level_limits l where l.level = new.level), 3000) then
    raise exception 'tiempo no permitido' using errcode = '23514';
  end if;
  select count(*) into n from public.records r where r.level = new.level and r.name = new.name;
  if n >= 20 then raise exception 'demasiados envios' using errcode = '23514'; end if;
  select count(*) into n from public.records r where r.created_at > now() - interval '1 minute';
  if n >= 120 then raise exception 'demasiados envios' using errcode = '23514'; end if;
  return new;
end $$;
revoke all on function public.records_guard() from public, anon, authenticated;
drop trigger if exists records_guard on public.records;
create trigger records_guard before insert on public.records
  for each row execute function public.records_guard();

-- ───────── C. Analítica ─────────
alter table public.events drop constraint if exists events_x_rango;
alter table public.events drop constraint if exists events_seconds_rango;
alter table public.events
  add constraint events_x_rango       check (x is null or x between -1000 and 100000),
  add constraint events_seconds_rango check (seconds is null or seconds between 0 and 86400);
create index if not exists events_created on public.events (created_at);

create or replace function public.events_guard() returns trigger
language plpgsql security definer set search_path = public as $$
begin
  new.created_at := now();
  if (select count(*) from public.events e where e.created_at > now() - interval '1 minute') >= 600 then
    raise exception 'demasiados eventos' using errcode = '23514';
  end if;
  return new;
end $$;
revoke all on function public.events_guard() from public, anon, authenticated;
drop trigger if exists events_guard on public.events;
create trigger events_guard before insert on public.events
  for each row execute function public.events_guard();
