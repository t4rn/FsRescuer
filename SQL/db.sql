insert into morp._fs_all (numer_wniosku, status)
SELECT numer_wniosku, 'W8' FROM morp.wnioski order by id_wniosek

-- drop table morp._fs_all
CREATE TABLE morp._fs_all
(
  id serial primary key,
  numer_wniosku character varying(7) unique,
  status character varying(4),
  czy_stary_fs boolean default false,
  stary_fs_size numeric,
  czy_nowy_fs boolean default false,
  nowy_fs_size numeric,
  message text,
  add_date timestamp without time zone default now()
)
WITH (  OIDS=FALSE);
GRANT ALL ON TABLE morp._fs_all TO api_ws;

ALTER TABLE morp._fs_all ADD COLUMN move_status character varying(4) default 'W8';
ALTER TABLE morp._fs_all ADD COLUMN is_for_move boolean default false;
ALTER TABLE morp._fs_all ADD COLUMN is_moved boolean default false;
ALTER TABLE morp._fs_all ADD COLUMN move_msg text;
ALTER TABLE morp._fs_all ADD COLUMN move_date timestamp without time zone;
ALTER TABLE morp._fs_all ADD COLUMN moved_mb numeric;
