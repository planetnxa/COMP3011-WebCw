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
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace WebAppComp3011.Controllers
{


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
                // include sqlite internal rowid so we can use it as the Fragrance.Id
                cmd.CommandText = "SELECT rowid, * FROM perfume100"; // made smaller db table for testing
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    int p = 0;
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine(reader["Perfume"].ToString());
                        p += 1;
                        Fragrance f = new Fragrance() {
                            Id = Convert.ToInt32(reader["rowid"]),
                            FragUrl = reader["url"].ToString(),
                            FragName = reader["Perfume"].ToString(),
                            Brand = reader["Brand"].ToString(),
                            Country = reader["Country"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            RatingValue = float.Parse(reader["Rating Value"].ToString()), // fi fix with db in cw01
                            RatingCount = Convert.ToInt32(reader["Rating Count"]),
                            Year = reader["year"].ToString(),
                            Notes = new Notes() { Base = reader["Base"].ToString(),
                                Middle = reader["Middle"].ToString(),
                                Top = reader["Top"].ToString() },
                            Perfumer1 = reader["Perfumer1"].ToString(),
                            Perfumer2 = reader["Perfumer2"].ToString(),
                            MainAccords = new List<string> {reader["mainaccord1"].ToString(), reader["mainaccord2"].ToString() , reader["mainaccord3"].ToString() , reader["mainaccord4"].ToString() , reader["mainaccord5"].ToString() },
                        };

                        list.Add(f);                   
                    }
                   // Console.WriteLine(reader);
                } // await for async
                await cn.CloseAsync();
            }
            return list;


        }
        
        // GET: api/Fragrances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fragrance>> GetFragrance(int id)
        {
            Fragrance frag = null;

            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                // use sqlite internal rowid as identifier for scaffolding
                cmd.CommandText = "SELECT rowid, * FROM perfume100 WHERE rowid = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        frag = new Fragrance()
                        {
                            Id = Convert.ToInt32(reader["rowid"]),
                            FragUrl = reader["url"].ToString(),
                            FragName = reader["Perfume"].ToString(),
                            Brand = reader["Brand"].ToString(),
                            Country = reader["Country"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            RatingValue = float.TryParse(reader["Rating Value"].ToString(), out var rv) ? rv : 0f,
                            RatingCount = int.TryParse(reader["Rating Count"].ToString(), out var rc) ? rc : 0,
                            Year = reader["year"].ToString(),
                            Notes = new Notes()
                            {
                                Base = reader["Base"].ToString(),
                                Middle = reader["Middle"].ToString(),
                                Top = reader["Top"].ToString()
                            },
                            Perfumer1 = reader["Perfumer1"].ToString(),
                            Perfumer2 = reader["Perfumer2"].ToString(),
                            MainAccords = new List<string>
                            {
                                reader["mainaccord1"].ToString(),
                                reader["mainaccord2"].ToString(),
                                reader["mainaccord3"].ToString(),
                                reader["mainaccord4"].ToString(),
                                reader["mainaccord5"].ToString()
                            }
                        };
                    }
                }
                await cn.CloseAsync();
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

            int rowsAffected = 0;
            await using (var cn = new SqliteConnection(connectString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = @"UPDATE perfume100 SET
                    url = @FragUrl,
                    Perfume = @FragName,
                    Brand = @Brand,
                    Country = @Country,
                    Gender = @Gender,
                    ""Rating Value"" = @RatingValue,
                    ""Rating Count"" = @RatingCount,
                    year = @Year,
                    Top = @Top,
                    Middle = @Middle,
                    Base = @Base,
                    Perfumer1 = @Perfumer1,
                    Perfumer2 = @Perfumer2,
                    mainaccord1 = @ma1,
                    mainaccord2 = @ma2,
                    mainaccord3 = @ma3,
                    mainaccord4 = @ma4,
                    mainaccord5 = @ma5
                    WHERE rowid = @Id";

                cmd.Parameters.AddWithValue("@FragUrl", frag.FragUrl ?? string.Empty);
                cmd.Parameters.AddWithValue("@FragName", frag.FragName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Brand", frag.Brand ?? string.Empty);
                cmd.Parameters.AddWithValue("@Country", frag.Country ?? string.Empty);
                cmd.Parameters.AddWithValue("@Gender", frag.Gender ?? string.Empty);
                cmd.Parameters.AddWithValue("@RatingValue", frag.RatingValue);
                cmd.Parameters.AddWithValue("@RatingCount", frag.RatingCount);
                cmd.Parameters.AddWithValue("@Year", frag.Year ?? string.Empty);
                cmd.Parameters.AddWithValue("@Top", frag.Notes?.Top ?? string.Empty);
                cmd.Parameters.AddWithValue("@Middle", frag.Notes?.Middle ?? string.Empty);
                cmd.Parameters.AddWithValue("@Base", frag.Notes?.Base ?? string.Empty);
                cmd.Parameters.AddWithValue("@Perfumer1", frag.Perfumer1 ?? string.Empty);
                cmd.Parameters.AddWithValue("@Perfumer2", frag.Perfumer2 ?? string.Empty);
                var ma = frag.MainAccords ?? new List<string>();
                cmd.Parameters.AddWithValue("@ma1", ma.ElementAtOrDefault(0) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma2", ma.ElementAtOrDefault(1) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma3", ma.ElementAtOrDefault(2) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma4", ma.ElementAtOrDefault(3) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma5", ma.ElementAtOrDefault(4) ?? string.Empty);
                cmd.Parameters.AddWithValue("@Id", id);

                rowsAffected = await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
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
                cmd.CommandText = @"INSERT INTO perfume100 (
                    url, Perfume, Brand, Country, Gender, ""Rating Value"", ""Rating Count"", year,
                    Top, Middle, Base, Perfumer1, Perfumer2, mainaccord1, mainaccord2, mainaccord3, mainaccord4, mainaccord5
                    ) VALUES (
                    @FragUrl, @FragName, @Brand, @Country, @Gender, @RatingValue, @RatingCount, @Year,
                    @Top, @Middle, @Base, @Perfumer1, @Perfumer2, @ma1, @ma2, @ma3, @ma4, @ma5
                    ); SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@FragUrl", fragrance.FragUrl ?? string.Empty);
                cmd.Parameters.AddWithValue("@FragName", fragrance.FragName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Brand", fragrance.Brand ?? string.Empty);
                cmd.Parameters.AddWithValue("@Country", fragrance.Country ?? string.Empty);
                cmd.Parameters.AddWithValue("@Gender", fragrance.Gender ?? string.Empty);
                cmd.Parameters.AddWithValue("@RatingValue", fragrance.RatingValue);
                cmd.Parameters.AddWithValue("@RatingCount", fragrance.RatingCount);
                cmd.Parameters.AddWithValue("@Year", fragrance.Year ?? string.Empty);
                cmd.Parameters.AddWithValue("@Top", fragrance.Notes?.Top ?? string.Empty);
                cmd.Parameters.AddWithValue("@Middle", fragrance.Notes?.Middle ?? string.Empty);
                cmd.Parameters.AddWithValue("@Base", fragrance.Notes?.Base ?? string.Empty);
                cmd.Parameters.AddWithValue("@Perfumer1", fragrance.Perfumer1 ?? string.Empty);
                cmd.Parameters.AddWithValue("@Perfumer2", fragrance.Perfumer2 ?? string.Empty);
                var ma = fragrance.MainAccords ?? new List<string>();
                cmd.Parameters.AddWithValue("@ma1", ma.ElementAtOrDefault(0) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma2", ma.ElementAtOrDefault(1) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma3", ma.ElementAtOrDefault(2) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma4", ma.ElementAtOrDefault(3) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ma5", ma.ElementAtOrDefault(4) ?? string.Empty);

                var result = await cmd.ExecuteScalarAsync();
                newId = Convert.ToInt32(result);
                await cn.CloseAsync();
            }

            fragrance.Id = newId;
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
                cmd.CommandText = "DELETE FROM perfume100 WHERE rowid = @id";
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
                cmd.CommandText = "SELECT 1 FROM perfume100 WHERE rowid = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                exists = result != null;
            }

            return exists;
        }
    }
}


