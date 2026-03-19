using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppComp3011.Models;

namespace WebAppComp3011.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCabinetController : ControllerBase
    {
        private string connectString = "Data Source=fragranceDB.db";

        // GET: api/UserCabinet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserCabinet>>> GetUserCabinet()
        {
            var list = new List<UserCabinet>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userCabinet";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var entry = new UserCabinet()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            PerfumeId = reader["perfumeId"] != DBNull.Value ? Convert.ToInt32(reader["perfumeId"]) : (int?)null,
                            UserId = reader["userId"] != DBNull.Value ? Convert.ToInt32(reader["userId"]) : (int?)null,
                            Comments = reader["comments"]?.ToString()
                        };
                        list.Add(entry);
                    }
                }
                await cn.CloseAsync();
            }
            return list;
        }

        // GET: api/UserCabinet/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserCabinet>> GetUserCabinetById([FromRoute] int id)
        {
            UserCabinet entry = null;

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userCabinet WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        entry = new UserCabinet()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            PerfumeId = reader["perfumeId"] != DBNull.Value ? Convert.ToInt32(reader["perfumeId"]) : (int?)null,
                            UserId = reader["userId"] != DBNull.Value ? Convert.ToInt32(reader["userId"]) : (int?)null,
                            Comments = reader["comments"]?.ToString()
                        };
                    }
                }
                await cn.CloseAsync();
            }

            if (entry == null)
                return NotFound();

            return entry;
        }

        // GET: api/UserCabinet/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserCabinet>>> GetByUserId([FromRoute] int userId)
        {
            var list = new List<UserCabinet>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userCabinet WHERE userId = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var entry = new UserCabinet()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            PerfumeId = reader["perfumeId"] != DBNull.Value ? Convert.ToInt32(reader["perfumeId"]) : (int?)null,
                            UserId = reader["userId"] != DBNull.Value ? Convert.ToInt32(reader["userId"]) : (int?)null,
                            Comments = reader["comments"]?.ToString()
                        };
                        list.Add(entry);
                    }
                }
                await cn.CloseAsync();
            }
            return list.Count == 0 ? NotFound() : list;
        }

        // GET: api/UserCabinet/perfume/{perfumeId}
        [HttpGet("perfume/{perfumeId}")]
        public async Task<ActionResult<IEnumerable<UserCabinet>>> GetByPerfumeId([FromRoute] int perfumeId)
        {
            var list = new List<UserCabinet>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userCabinet WHERE perfumeId = @perfumeId";
                cmd.Parameters.AddWithValue("@perfumeId", perfumeId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var entry = new UserCabinet()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            PerfumeId = reader["perfumeId"] != DBNull.Value ? Convert.ToInt32(reader["perfumeId"]) : (int?)null,
                            UserId = reader["userId"] != DBNull.Value ? Convert.ToInt32(reader["userId"]) : (int?)null,
                            Comments = reader["comments"]?.ToString()
                        };
                        list.Add(entry);
                    }
                }
                await cn.CloseAsync();
            }
            return list.Count == 0 ? NotFound() : list;
        }

        // POST: api/UserCabinet
        [HttpPost]
        public async Task<ActionResult<UserCabinet>> PostUserCabinet([FromBody] UserCabinet entry)
        {
            int newId = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"INSERT INTO userCabinet (username, perfumeId, userId, comments) 
                    VALUES (@username, @perfumeId, @userId, @comments); 
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@username", entry.Username ?? string.Empty);
                cmd.Parameters.AddWithValue("@perfumeId", entry.PerfumeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@userId", entry.UserId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@comments", entry.Comments ?? string.Empty);

                var result = await cmd.ExecuteScalarAsync();
                newId = Convert.ToInt32(result);
                await cn.CloseAsync();
            }

            entry.Id = newId;
            return CreatedAtAction(nameof(GetUserCabinetById), new { id = entry.Id }, entry);
        }

        // PUT: api/UserCabinet/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserCabinet([FromRoute] int id, [FromBody] UserCabinet entry)
        {
            if (id != entry.Id)
                return BadRequest();

            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"UPDATE userCabinet SET
                    username = @username,
                    perfumeId = @perfumeId,
                    userId = @userId,
                    comments = @comments
                    WHERE id = @id";

                cmd.Parameters.AddWithValue("@username", entry.Username ?? string.Empty);
                cmd.Parameters.AddWithValue("@perfumeId", entry.PerfumeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@userId", entry.UserId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@comments", entry.Comments ?? string.Empty);
                cmd.Parameters.AddWithValue("@id", id);

                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
            {
                if (!UserCabinetExists(id))
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/UserCabinet/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserCabinet([FromRoute] int id)
        {
            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM userCabinet WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }

        private bool UserCabinetExists(int id)
        {
            var exists = false;
            using (var cn = new SqliteConnection(connectString))
            {
                cn.Open();
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM userCabinet WHERE id = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                exists = result != null;
            }

            return exists;
        }
    }
}
