/* begin;

create table balloon_rounds (
  id                 uuid primary key default gen_random_uuid(),
  player_id          uuid not null references players(id) on delete restrict,
  operator_id        uuid not null references operators(id) on delete restrict,
  game_id            uuid references games(id),
  client_round_id    text not null,        -- idempotency key from Unity (BalloonColyseusManager's per-round GUID)
  bet_amount         numeric(12,2) not null check (bet_amount > 0),
  auto_target        numeric(10,2) not null default 0,
  client_seed        text not null default '',
  server_seed        text,                 -- populated on resolution (burst or cash-out)
  server_seed_hash   text,
  result_multiplier  numeric(10,2),        -- burst point (loss) or cash-out multiplier (win)
  payout             numeric(12,2),
  status             text not null default 'pending'
                      check (status in ('pending','placed','active','settling','cashed_out','burst','cancelled')),
  created_at         timestamptz not null default now(),
  resolved_at        timestamptz,
  unique (player_id, client_round_id)
);

create index idx_balloon_rounds_player_status on balloon_rounds(player_id, status);

create table balloon_bet_history (
  id          uuid primary key default gen_random_uuid(),
  player_id   uuid not null,
  bet_amount  numeric not null,
  win_amount  numeric not null default 0,
  created_at  timestamptz not null default now()
);

create index idx_balloon_bet_history_player_id on balloon_bet_history(player_id, created_at desc);

alter table balloon_rounds      enable row level security;
alter table balloon_bet_history enable row level security;

create policy "balloon_rounds_operator_read" on balloon_rounds
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);

insert into games (slug, name, game_url, is_active)
values ('balloon', 'Balloon', '<placeholder — set once the WebGL build is hosted>', true)
on conflict (slug) do nothing;

commit; */

-- Balloon persistence, modelled on Balloon's schema:
--   balloon_rounds       one row per balloon (single stake, own timeline)
--   balloon_bets         generic wallet contract (same column shape as
--                        roulette_bets / balloon_bets) so walletShared.ts
--                        handleDebit/handleCredit work unmodified
--   balloon_bet_history  player-facing history
-- Deliberately NOT using the generic game_rounds/game_bets tier (009): the
-- multiplier ticks live and cash-out timing is uncertain, so the round does
-- not resolve in one atomic step (same reasoning as Aviator).

begin;

create table balloon_rounds (
  id                 uuid primary key default gen_random_uuid(),
  player_id          uuid not null references platform_players(id) on delete restrict,
  operator_id        uuid not null references platform_operators(id) on delete restrict,
  game_id            uuid references platform_games(id),
  client_round_id    text not null,        -- idempotency key from Unity (BalloonColyseusManager's per-round GUID)
  bet_amount         numeric(12,2) not null check (bet_amount > 0),
  auto_target        numeric(10,2) not null default 0,
  nonce              integer,
  client_seed        text not null default '',
  server_seed        text,                 -- populated on resolution (burst or cash-out)
  server_seed_hash   text,
  result_multiplier  numeric(10,2),        -- burst point (loss) or cash-out multiplier (win)
  payout             numeric(12,2),
  status             text not null default 'pending'
                      check (status in ('pending','placed','active','settling','cashed_out','burst','cancelled')),
  created_at         timestamptz not null default now(),
  resolved_at        timestamptz,
  unique (player_id, client_round_id)
);

create table balloon_bets (
  id           uuid primary key default gen_random_uuid(),
  round_id     uuid not null references balloon_rounds(id) on delete restrict,
  player_id    uuid not null references platform_players(id) on delete restrict,
  operator_id  uuid not null references platform_operators(id) on delete restrict,
  game_id      uuid references platform_games(id),
  bet_type     text not null default 'balloon',
  selection    jsonb not null default '{}'::jsonb,   -- {autoTarget} at debit
  amount       numeric(12,2) not null check (amount > 0),
  multiplier   numeric(10,4) not null default 0,      -- 0 at debit, patched at credit (burst/cash-out point unknown until resolved)
  won          boolean,
  payout       numeric(12,2) not null default 0,
  outcome      jsonb,                                  -- {resultMultiplier, endedBy: 'cashout'|'burst'|'auto'} patched at credit
  created_at   timestamptz not null default now()
);

create table balloon_bet_history (
  id          uuid primary key default gen_random_uuid(),
  player_id   uuid not null,
  round_id    uuid references balloon_rounds(id),
  round_hash  text not null default '',
  bet_amount  numeric not null,
  win_amount  numeric not null default 0,
  created_at  timestamptz not null default now()
);

create index idx_balloon_bets_round on balloon_bets(round_id);
create index idx_balloon_rounds_player_status on balloon_rounds(player_id, status);
create index idx_balloon_bet_history_player_id on balloon_bet_history(player_id, created_at desc);
create index idx_balloon_bet_history_round on balloon_bet_history(round_id);

alter table balloon_rounds      enable row level security;
alter table balloon_bets        enable row level security;
alter table balloon_bet_history enable row level security;

create policy "balloon_rounds_operator_read" on balloon_rounds
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);
create policy "balloon_bets_operator_read" on balloon_bets
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);

insert into platform_games (slug, name, game_url, is_active)
values ('balloon', 'Balloon', '<placeholder — set once the WebGL build is hosted>', true)
on conflict (slug) do nothing;

commit;