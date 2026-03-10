using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppComp3011.Data;
using WebAppComp3011.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace WebAppComp3011.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class FragrancesController : ControllerBase
    {

        private readonly FragranceContext _context;
        // Updated connection string to use LocalDB SQL Server instance. Adjust as needed for your environment.
        private readonly string connectString = "Server=(localdb)\\mssqllocaldb;Database=FragranceDB;Trusted_Connection=True;MultipleActiveResultSets=true";

        public FragrancesController(FragranceContext context)
        {
            _context = context;
        }

        // GET: api/Fragrances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fragrance>>> GetFragrances()
        {
            var list = new List<Fragrance>();

            await using (var conn = new SqlConnection(connectString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Fragrance";
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(ReadFragranceFromReader(reader));
                }
            }

            return list;
        }

        // GET: api/Fragrances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fragrance>> GetFragrance(int id)
        {
            Fragrance frag = null;

            await using (var conn = new SqlConnection(connectString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Fragrance WHERE Id = @id";
                cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    frag = ReadFragranceFromReader(reader);
                }
            }

            if (frag == null)
                return NotFound();

            return frag;
        }

        // PUT: api/Fragrance/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFrag(int id, Fragrance frag)
        {
            if (id != frag.Id)
                return BadRequest();

            // Build UPDATE statement dynamically based on model properties (excluding Id)
            var props = typeof(Fragrance).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)).ToArray();

            var setClauses = props.Select(p => $"[{p.Name}] = @{p.Name}");
            var sql = $"UPDATE Fragrance SET {string.Join(", ", setClauses)} WHERE Id = @Id";

            int rowsAffected;
            await using (var conn = new SqlConnection(connectString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                foreach (var p in props)
                {
                    var val = p.GetValue(frag) ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@" + p.Name, val);
                }
                cmd.Parameters.AddWithValue("@Id", frag.Id);
                rowsAffected = await cmd.ExecuteNonQueryAsync();
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
            // Insert all properties except Id and return the generated Id
            var props = typeof(Fragrance).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)).ToArray();

            var columns = props.Select(p => $"[{p.Name}]").ToArray();
            var parameters = props.Select(p => "@" + p.Name).ToArray();

            var sql = $"INSERT INTO Fragrance ({string.Join(", ", columns)}) OUTPUT INSERTED.Id VALUES ({string.Join(", ", parameters)})";

            int newId;
            await using (var conn = new SqlConnection(connectString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                foreach (var p in props)
                {
                    var val = p.GetValue(fragrance) ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@" + p.Name, val);
                }
                var result = await cmd.ExecuteScalarAsync();
                newId = Convert.ToInt32(result);
            }

            fragrance.Id = newId;
            return CreatedAtAction(nameof(GetFragrance), new { id = fragrance.Id }, fragrance);
        }

        // DELETE: api/Fragrances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFragrance(int id)
        {
            int rowsAffected;
            await using (var conn = new SqlConnection(connectString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Fragrance WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                rowsAffected = await cmd.ExecuteNonQueryAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }

        private bool FragranceExists(int id)
        {
            var exists = false;
            using (var conn = new SqlConnection(connectString))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(1) FROM Fragrance WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                exists = Convert.ToInt32(result) > 0;
            }

            return exists;
        }

        private Fragrance ReadFragranceFromReader(SqlDataReader reader)
        {
            var frag = new Fragrance();
            var props = typeof(Fragrance).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var prop = props.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (prop == null) continue;
                var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                try
                {
                    if (val == null)
                    {
                        prop.SetValue(frag, null);
                    }
                    else
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        prop.SetValue(frag, Convert.ChangeType(val, targetType));
                    }
                }
                catch
                {
                    // ignore conversion errors for unsupported types
                }
            }

            return frag;
        }
    }

}
