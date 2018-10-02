using Dapper;
using FS.Core.Models;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FS.Core.Services
{
    public class ApplicationRepository
    {
        private readonly string _connectionString;

        public ApplicationRepository()
        {
            _connectionString = Properties.Settings.Default.ConnectionString;
        }

        [Obsolete("For error fix", true)]
        internal IEnumerable<ApplicationInfo> GetApplicationsForMovingWithError()
        {
            string query = @"SELECT id, numer_wniosku
                                    FROM morp._fs_all
                                    WHERE move_status = 'ERR' AND is_for_move = true AND is_moved = false ORDER BY numer_wniosku LIMIT 1100;";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                return conn.Query(query).Select(x => new ApplicationInfo
                {
                    Id = x.id,
                    AppNumber = x.numer_wniosku
                });
            }
        }

        internal IEnumerable<ApplicationInfo> GetApplicationsForMoving(int recordLimit)
        {
            string query = @"SELECT id, numer_wniosku
                                    FROM morp._fs_all
                                    WHERE move_status = 'W8' AND is_for_move = true AND is_moved = false ORDER BY numer_wniosku LIMIT @limit;";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                return conn.Query(query, new { limit = recordLimit }).Select(x => new ApplicationInfo
                {
                    Id = x.id,
                    AppNumber = x.numer_wniosku
                });
            }
        }

        internal IEnumerable<ApplicationInfo> GetApplicationsForCopy()
        {
            string query = @"SELECT id, numer_wniosku
                                    FROM morp._fs_all
                                    WHERE move_status = 'W8' AND czy_stary_fs = true AND is_moved = false order by id limit 10;";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                return conn.Query(query).Select(x => new ApplicationInfo
                {
                    Id = x.id,
                    AppNumber = x.numer_wniosku
                });
            }
        }

        internal IEnumerable<ApplicationInfo> GetAppInfoList()
        {
            string query = @"SELECT id, numer_wniosku --, status, czy_stary_fs, czy_nowy_fs, message, add_date
                                    FROM morp._fs_all
                                    WHERE status = 'W8' order by id desc;";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                return conn.Query(query).Select(x => new ApplicationInfo
                {
                    Id = x.id,
                    AppNumber = x.numer_wniosku
                });
            }
        }

        internal int UpdateAnalyseInfo(ApplicationInfo applicationInfo)
        {
            int rowsAffected = -1;

            string query = @"UPDATE morp._fs_all
                               SET status = @status, czy_stary_fs = @czy_stary_fs, czy_nowy_fs = @czy_nowy_fs, 
                                    message = @message, stary_fs_size = @stary_fs_size, nowy_fs_size = @nowy_fs_size,
                                    add_date = now()
                             WHERE id = @id";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                rowsAffected = conn.Execute(query, new
                {
                    status = applicationInfo.AnalyseStatus,
                    czy_stary_fs = applicationInfo.IsOnOldFs,
                    czy_nowy_fs = applicationInfo.IsOnNewFs,
                    message = applicationInfo.AnalyseMessage,
                    stary_fs_size = applicationInfo.OldFsSize,
                    nowy_fs_size = applicationInfo.NewFsSize,
                    id = applicationInfo.Id
                });
            }

            return rowsAffected;
        }

        internal int UpdateMoveInfo(ApplicationInfo applicationInfo)
        {
            int rowsAffected = -1;

            string query = @"UPDATE morp._fs_all
                                   SET is_moved = @is_moved, move_msg = @move_msg, move_date = now(), move_status = @move_status, moved_mb = @moved_mb
                             WHERE id = @id";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                rowsAffected = conn.Execute(query, new
                {
                    is_moved = applicationInfo.IsMoved,
                    move_msg = applicationInfo.MoveMessage,
                    move_status = applicationInfo.MoveStatus,
                    moved_mb = applicationInfo.MovedMb,
                    id = applicationInfo.Id
                });
            }

            return rowsAffected;
        }
    }
}
