using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GymManagement.DAL.Data.DbContexts;
using GymManagement.DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymManagement.DAL.Data.DataSeeding
{
    public static class GymDataSeeding
    {
        public static async Task SeedAsync(GymDbContext dbContext, string seedFilesPath, ILogger logger, CancellationToken ct = default)
        {
            try
            {
                if(!await dbContext.Plans.AnyAsync())
                {
                    var plans = LoadDataFromJsonFile<Plan>(seedFilesPath, "plans.json");
                    if(plans.Any())
                    {
                        dbContext.Plans.AddRange(plans);
                        //var result = await dbContext.SaveChangesAsync(ct);
                        logger.LogInformation($"Plans Seeded With Count: {plans.Count()}");
                    }
                    if (dbContext.ChangeTracker.HasChanges())
                        await dbContext.SaveChangesAsync(ct);
                    else
                        logger.LogInformation("Plans Already Seeded");
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Gym Data Seeding Failed");
                throw;
            }
        }
        private static List<T> LoadDataFromJsonFile<T>(string folderPath, string fileName)
        {
            var filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Seed Data File Not Found: {filePath}");

            var data = File.ReadAllText(filePath); // returns data as string
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<List<T>>(data) ?? [];

        }
    }
}
