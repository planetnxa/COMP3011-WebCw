using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppComp3011.Models;

namespace WebAppComp3011.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileService : ControllerBase
    {
        private string connectString = "Data Source=fragranceDB.db";

        /// <summary>
        ///  Get all user profiles from db
        /// </summary>
        // GET: api/UserProfile
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfile>>> GetUserProfiles()
        {
            var list = new List<UserProfile>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userProfiles";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var profile = new UserProfile()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            Password = reader["password"]?.ToString(),
                            Name = reader["name"]?.ToString(),
                            FirstLogin = reader["firstLogin"] != DBNull.Value ? Convert.ToInt32(reader["firstLogin"]) != 0 : false
                        };
                        list.Add(profile);
                    }
                }
                await cn.CloseAsync();
            }
            return list;
        }

        /// <summary>
        ///  Get user profile by id
        /// </summary>
        // GET: api/UserProfile/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserProfile>> GetUserProfileById([FromRoute] int id)
        {
            UserProfile profile = null;

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userProfiles WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        profile = new UserProfile()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            Password = reader["password"]?.ToString(),
                            Name = reader["name"]?.ToString(),
                            FirstLogin = reader["firstLogin"] != DBNull.Value ? Convert.ToInt32(reader["firstLogin"]) != 0 : false
                        };
                    }
                }
                await cn.CloseAsync();
            }

            if (profile == null)
                return NotFound();

            return profile;
        }

        /// <summary>
        ///  Get user profile by username
        /// </summary>
        // GET: api/UserProfile/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserProfile>> GetUserProfileByUsername([FromRoute] string username)
        {
            UserProfile profile = null;

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userProfiles WHERE username = @username LIMIT 1";
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        profile = new UserProfile()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"]?.ToString(),
                            Password = reader["password"]?.ToString(),
                            Name = reader["name"]?.ToString(),
                            FirstLogin = reader["firstLogin"] != DBNull.Value ? Convert.ToInt32(reader["firstLogin"]) != 0 : false
                        };
                    }
                }
                await cn.CloseAsync();
            }

            if (profile == null)
                return NotFound();

            return profile;
        }
        /// <summary>
        ///  For creating new user profiles
        /// </summary>
        // POST: api/UserProfile
        [HttpPost]
        public async Task<ActionResult<UserProfile>> PostUserProfile([FromBody] UserProfile profile)
        {
            int newId = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"INSERT INTO userProfiles (username, password, name, firstLogin) 
                    VALUES (@username, @password, @name, @firstLogin); 
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@username", profile.Username ?? string.Empty);
                // Hash password before storing if it's not already hashed (check length > 20 suggests it's hashed)
                string passwordToStore = profile.Password;
                if (passwordToStore != null && passwordToStore.Length < 20)
                {
                    passwordToStore = PasswordHashingService.HashPassword(passwordToStore);
                }
                cmd.Parameters.AddWithValue("@password", passwordToStore ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", profile.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@firstLogin", profile.FirstLogin ? 1 : 0);

                var result = await cmd.ExecuteScalarAsync();
                newId = Convert.ToInt32(result);
                await cn.CloseAsync();
            }

            profile.Id = newId;
            return CreatedAtAction(nameof(GetUserProfileById), new { id = profile.Id }, profile);
        }

        /// <summary>
        ///  Update user profile by id
        /// </summary>
        // PUT: api/UserProfile/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUserProfile([FromRoute] int id, [FromBody] UserProfile profile)
        {
            if (id != profile.Id)
                return BadRequest();

            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"UPDATE userProfiles SET
                    username = @username,
                    password = @password,
                    name = @name,
                    firstLogin = @firstLogin
                    WHERE id = @id";

                cmd.Parameters.AddWithValue("@username", profile.Username ?? string.Empty);
                // Hash password before storing if it's not already hashed (check length > 20 suggests it's hashed)
                string passwordToStore = profile.Password;
                if (passwordToStore != null && passwordToStore.Length < 20)
                {
                    passwordToStore = PasswordHashingService.HashPassword(passwordToStore);
                }
                cmd.Parameters.AddWithValue("@password", passwordToStore ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", profile.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@firstLogin", profile.FirstLogin ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", id);

                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
            {
                if (!UserProfileExists(id))
                    return NotFound();
            }

            return NoContent();
        }
        /// <summary>
        ///  Delete user profile from db
        /// </summary>
        // DELETE: api/UserProfile/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUserProfile([FromRoute] int id)
        {
            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM userProfiles WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }

        private bool UserProfileExists(int id)
        {
            var exists = false;
            using (var cn = new SqliteConnection(connectString))
            {
                cn.Open();
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM userProfiles WHERE id = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                exists = result != null;
            }

            return exists;
        }
    }
}
