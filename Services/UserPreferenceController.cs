using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAppComp3011.Models;

namespace WebAppComp3011.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPreferenceController : ControllerBase
    {
        private string connectString = "Data Source=fragranceDB.db";

        /// <summary>
        ///  Get all user preferences
        /// </summary>
        // GET: api/UserPreference
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserPreference>>> GetUserPreferences()
        {
            var list = new List<UserPreference>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userPrefs";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var pref = new UserPreference()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            PrefVal = reader["PrefVal"]?.ToString() ?? string.Empty,
                            PrefType = reader["PrefType"]?.ToString() ?? string.Empty
                        };
                        list.Add(pref);
                    }
                }
                await cn.CloseAsync();
            }
            return list;
        }

        /// <summary>
        ///  Get a specific user preference by id
        /// </summary>
        // GET: api/UserPreference/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserPreference>> GetUserPreferenceById([FromRoute] int id)
        {
            UserPreference? pref = null;

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userPrefs WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        pref = new UserPreference()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            PrefVal = reader["PrefVal"]?.ToString() ?? string.Empty,
                            PrefType = reader["PrefType"]?.ToString() ?? string.Empty
                        };
                    }
                }
                await cn.CloseAsync();
            }

            if (pref == null)
                return NotFound();

            return pref;
        }

        /// <summary>
        ///  Get all preferences for a specific user
        /// </summary>
        // GET: api/UserPreference/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserPreference>>> GetByUserId([FromRoute] int userId)
        {
            var list = new List<UserPreference>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userPrefs WHERE UserId = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var pref = new UserPreference()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            PrefVal = reader["PrefVal"]?.ToString() ?? string.Empty,
                            PrefType = reader["PrefType"]?.ToString() ?? string.Empty
                        };
                        list.Add(pref);
                    }
                }
                await cn.CloseAsync();
            }
            return list.Count == 0 ? NotFound() : list;
        }

        /// <summary>
        ///  Add a new user preference
        /// </summary>
        // POST: api/UserPreference
        [HttpPost]
        public async Task<ActionResult<UserPreference>> PostUserPreference([FromBody] UserPreference pref)
        {
            int newId = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"INSERT INTO userPrefs (UserId, PrefVal, PrefType) 
                    VALUES (@userId, @prefVal, @prefType); 
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@userId", pref.UserId);
                cmd.Parameters.AddWithValue("@prefVal", pref.PrefVal ?? string.Empty);
                cmd.Parameters.AddWithValue("@prefType", pref.PrefType ?? string.Empty);

                var result = await cmd.ExecuteScalarAsync();
                newId = Convert.ToInt32(result);
                await cn.CloseAsync();
            }

            pref.Id = newId;
            return CreatedAtAction(nameof(GetUserPreferenceById), new { id = pref.Id }, pref);
        }

        /// <summary>
        ///  Update a user preference by id
        /// </summary>
        // PUT: api/UserPreference/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserPreference([FromRoute] int id, [FromBody] UserPreference pref)
        {
            if (id != pref.Id)
                return BadRequest();

            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"UPDATE userPrefs SET
                    UserId = @userId,
                    PrefVal = @prefVal,
                    PrefType = @prefType
                    WHERE Id = @id";

                cmd.Parameters.AddWithValue("@userId", pref.UserId);
                cmd.Parameters.AddWithValue("@prefVal", pref.PrefVal ?? string.Empty);
                cmd.Parameters.AddWithValue("@prefType", pref.PrefType ?? string.Empty);
                cmd.Parameters.AddWithValue("@id", id);

                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
            {
                if (!UserPreferenceExists(id))
                    return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        ///  Delete a user preference by id
        /// </summary>
        // DELETE: api/UserPreference/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserPreference([FromRoute] int id)
        {
            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM userPrefs WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }

        private bool UserPreferenceExists(int id)
        {
            var exists = false;
            using (var cn = new SqliteConnection(connectString))
            {
                cn.Open();
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM userPrefs WHERE Id = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                exists = result != null;
            }
            return exists;
        }
    }
}
