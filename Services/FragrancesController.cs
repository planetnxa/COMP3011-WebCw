using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebAppComp3011.Data;
using WebAppComp3011.Models;

namespace WebAppComp3011.Controllers
{
    // https://cwdeploy01.azurewebsites.net/api/fragrances site now available

    [Route("api/[controller]")]
    [ApiController]
    public class FragrancesController : ControllerBase
    {

        // Updated connection string to use LocalDB SQL Server instance. Adjust as needed for your environment.
        // private readonly string connectString = "Data Source= C:\\Users\\love\\Desktop\\ctrl\\web\\cw01\\fragranceDB.db";
        private string connectString = "Data Source=fragranceDB.db";
        //var connection = new SqliteConnection(connectString);


        // GET: api/Fragrances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fragrance>>> GetFragrances()
        {
            var list = new List<Fragrance>();
            using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                // read from perf100 table (schema provided by user)
                cmd.CommandText = "SELECT * FROM perf100";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    int p = 0;
                    while (await reader.ReadAsync())
                    {
                        p += 1;
                        var f = new Fragrance()
                        {
                            Id = Convert.ToInt32(reader["perfumeId"]),
                            FragUrl = reader["url"]?.ToString(),
                            FragName = reader["Perfume"]?.ToString(),
                            Brand = reader["Brand"]?.ToString(),
                            Country = reader["Country"]?.ToString(),
                            Gender = reader["Gender"]?.ToString(),
                            Rating = float.TryParse(reader["Rating"]?.ToString(), out var r) ? r : 0f,
                            Year = reader["Year"]?.ToString(),
                            Accords = reader["Accords"]?.ToString()?.Split('\'').ToList() ?? new List<string>(),
                            Perfumers = reader["Perfumers"]?.ToString(),

                            Notes = new Notes() { Top = new List<string>(), Middle = new List<string>(), Base = new List<string>() }
                        };

                        // try to populate notes from perfumeNotes table
                        if (f.Id != 0)
                        {
                            var notes = await ReadNotes(f.Id);
                            if (notes != null)
                                f.Notes = notes;
                        }

                        list.Add(f);
                    }
                    // Console.WriteLine(reader);
                } // await for async
                await cn.CloseAsync();
            }
            return list;


        }

        // Helper function
        private async Task<List<Fragrance>> GetFragrance(string sql, Dictionary<string, object> parameters)
        {
            var results = new List<Fragrance>();

            await using var cn = new SqliteConnection(connectString);
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = sql;

            // Add parameters safely
            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value);
                }
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var frag = new Fragrance
                {
                    Id = Convert.ToInt32(reader["perfumeId"]),
                    FragUrl = reader["url"]?.ToString(),
                    FragName = reader["Perfume"]?.ToString(),
                    Brand = reader["Brand"]?.ToString(),
                    Country = reader["Country"]?.ToString(),
                    Gender = reader["Gender"]?.ToString(),
                    Rating = float.TryParse(reader["Rating"]?.ToString(), out var r) ? r : 0f,
                    Year = reader["Year"]?.ToString(),
                    Accords = reader["Accords"]?.ToString()?.Split(',').ToList() ?? new List<string>(),
                    Perfumers = reader["Perfumers"]?.ToString(),
                    Notes = await ReadNotes(Convert.ToInt32(reader["perfumeId"]))
                };

                results.Add(frag);
            }

            return results;
        }

        // Get all perfumes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fragrance>>> GetAllFragrances()
        {
            var sql = "SELECT * FROM perf100";
            var results = await GetFragrance(sql, null);
            return results.Count == 0 ? NotFound() : results;
        }

        // Get by brand
        [HttpGet("brand/{brandName}")]
        public async Task<ActionResult<IEnumerable<Fragrance>>> GetByBrand(string brandName)
        {
            var sql = "SELECT * FROM perf100 WHERE Brand LIKE @brand";
            var parameters = new Dictionary<string, object> { { "@brand", $"%{brandName}%" } };
            var results = await GetFragrance(sql, parameters);
            return results.Count == 0 ? NotFound() : results;
        }

        // Get by accord
        [HttpGet("accord/{accord}")]
        public async Task<ActionResult<IEnumerable<Fragrance>>> GetByAccord(string accord)
        {
            var sql = "SELECT * FROM perf100 WHERE Accords LIKE @accord";
            var parameters = new Dictionary<string, object> { { "@accord", $"%{accord}%" } };
            var results = await GetFragrance(sql, parameters);
            return results.Count == 0 ? NotFound() : results;
        }

        // Get by notes (any top/middle/base)
        [HttpGet("note/{note}")]
        public async Task<ActionResult<IEnumerable<Fragrance>>> GetByNote(string note)
        {
            // Join PerfumeNotes to filter by note
            var sql = @"
        SELECT p.* 
        FROM perf100 p
        JOIN PerfumeNotes n ON p.perfumeId = n.PerfumeId
        WHERE n.Note LIKE @note
    ";
            var parameters = new Dictionary<string, object> { { "@note", $"%{note}%" } };
            var results = await GetFragrance(sql, parameters);
            return results.Count == 0 ? NotFound() : results;
        }

        private async Task<Notes> ReadNotes(int perfumeId)
        {
            var notes = new Notes() { 
                Top = new List<string>(), 
                Middle = new List<string>(), 
                Base = new List<string>()
            };

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT Note, Type FROM perfumeNotes WHERE perfumeId = @id";
                cmd.Parameters.AddWithValue("@id", perfumeId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var note = reader["Note"]?.ToString();
                        var type = reader["Type"]?.ToString();

                        if (string.IsNullOrEmpty(note) || string.IsNullOrEmpty(type)) continue;
                        switch (type.ToLowerInvariant())
                        {
                            case "top":
                                notes.Top.Add(note);
                                break;
                            case "middle":
                                notes.Middle.Add(note);
                                break;
                            case "base":
                                notes.Base.Add(note);
                                break;
                        }
                    }
                }
                await cn.CloseAsync();
            }

            return notes;
        }

      
       
        // PUT: api/Fragrance/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFrag(int id, Fragrance frag)
        {
            if (id != frag.Id)
                return BadRequest();

            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"UPDATE perf100 SET
                    url = @FragUrl,
                    Perfume = @FragName,
                    Brand = @Brand,
                    Country = @Country,
                    Gender = @Gender,
                    Rating = @Rating,
                    Year = @Year,
                    Accords = @Accords,
                    Perfumers = @Perfumers
                    WHERE perfumeId = @Id";

                cmd.Parameters.AddWithValue("@FragUrl", frag.FragUrl ?? string.Empty);
                cmd.Parameters.AddWithValue("@FragName", frag.FragName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Brand", frag.Brand ?? string.Empty);
                cmd.Parameters.AddWithValue("@Country", frag.Country ?? string.Empty);
                cmd.Parameters.AddWithValue("@Gender", frag.Gender ?? string.Empty);
                cmd.Parameters.AddWithValue("@Rating", frag.Rating);
                cmd.Parameters.AddWithValue("@Year", frag.Year ?? string.Empty);
                cmd.Parameters.AddWithValue("@Accords", frag.Accords != null ? string.Join("'", frag.Accords) : string.Empty);
                cmd.Parameters.AddWithValue("@Perfumers", frag.Perfumers ?? string.Empty);
                cmd.Parameters.AddWithValue("@Id", id);

                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            // update perfumeNotes rows for this perfume
            if (rowsAffected > 0)
            {
                await SaveNotesForPerfumeAsync(id, frag.Notes);
            }

            if (rowsAffected == 0)
            {
                if (!FragranceExists(id))
                    return NotFound();
            }

            return NoContent();
        }

        // POST: api/Fragrances
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Fragrance>> PostFragrance(Fragrance fragrance)
        {
            int newId = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"INSERT INTO perf100 (
                    url, Perfume, Brand, Country, Gender, Rating, Year, Accords, Perfumers
                    ) VALUES (
                    @FragUrl, @FragName, @Brand, @Country, @Gender, @Rating, @Year, @Accords, @Perfumers
                    ); SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@FragUrl", fragrance.FragUrl ?? string.Empty);
                cmd.Parameters.AddWithValue("@FragName", fragrance.FragName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Brand", fragrance.Brand ?? string.Empty);
                cmd.Parameters.AddWithValue("@Country", fragrance.Country ?? string.Empty);
                cmd.Parameters.AddWithValue("@Gender", fragrance.Gender ?? string.Empty);
                cmd.Parameters.AddWithValue("@Rating", fragrance.Rating);
                cmd.Parameters.AddWithValue("@Year", fragrance.Year ?? string.Empty);
                cmd.Parameters.AddWithValue("@Accords", fragrance.Accords != null ? string.Join("'", fragrance.Accords) : string.Empty);
                cmd.Parameters.AddWithValue("@Perfumers", fragrance.Perfumers ?? string.Empty);

                var result = await cmd.ExecuteScalarAsync();
                newId = Convert.ToInt32(result);
                await cn.CloseAsync();
            }

            fragrance.Id = newId;

            // save notes for newly created perfume
            if (fragrance.Notes != null)
            {
                await SaveNotesForPerfumeAsync(newId, fragrance.Notes);
            }

            return CreatedAtAction(nameof(GetFragrance), new { id = fragrance.Id }, fragrance);
        }

        // DELETE: api/Fragrances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFragrance(int id)
        {
            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM perf100 WHERE perfumeId = @id";
                cmd.Parameters.AddWithValue("@id", id);
                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }

        private bool FragranceExists(int id)
        {
            var exists = false;
            using (var cn = new SqliteConnection(connectString))
            {
                cn.Open();
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM perf100 WHERE perfumeId = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                exists = result != null;
            }

            return exists;
        }

        private int? TryGetInt(SqliteDataReader reader, string name)
        {
            try
            {
                if (reader.GetOrdinal(name) >= 0 && !reader.IsDBNull(reader.GetOrdinal(name)))
                {
                    return Convert.ToInt32(reader[name]);
                }
            }
            catch { }
            return null;
        }

        private async Task SaveNotesForPerfumeAsync(int perfumeId, Notes notes)
        {
            if (notes == null) return;

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                // delete existing notes
                var del = cn.CreateCommand();
                del.CommandText = "DELETE FROM perfumeNotes WHERE perfumeId = @id";
                del.Parameters.AddWithValue("@id", perfumeId);
                await del.ExecuteNonQueryAsync();

                // insert new notes
                if (notes.Top != null)
                {
                    foreach (var n in notes.Top)
                    {
                        var icmd = cn.CreateCommand();
                        icmd.CommandText = "INSERT INTO perfumeNotes (Note, Type, perfumeId) VALUES (@note, 'top', @id)";
                        icmd.Parameters.AddWithValue("@note", n ?? string.Empty);
                        icmd.Parameters.AddWithValue("@id", perfumeId);
                        await icmd.ExecuteNonQueryAsync();
                    }
                }
                if (notes.Middle != null)
                {
                    foreach (var n in notes.Middle)
                    {
                        var icmd = cn.CreateCommand();
                        icmd.CommandText = "INSERT INTO perfumeNotes (Note, Type, perfumeId) VALUES (@note, 'middle', @id)";
                        icmd.Parameters.AddWithValue("@note", n ?? string.Empty);
                        icmd.Parameters.AddWithValue("@id", perfumeId);
                        await icmd.ExecuteNonQueryAsync();
                    }
                }
                if (notes.Base != null)
                {
                    foreach (var n in notes.Base)
                    {
                        var icmd = cn.CreateCommand();
                        icmd.CommandText = "INSERT INTO perfumeNotes (Note, Type, perfumeId) VALUES (@note, 'base', @id)";
                        icmd.Parameters.AddWithValue("@note", n ?? string.Empty);
                        icmd.Parameters.AddWithValue("@id", perfumeId);
                        await icmd.ExecuteNonQueryAsync();
                    }
                }

                await cn.CloseAsync();
            }
        }

    }
}


