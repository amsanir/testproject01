using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
 
namespace testproject01
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
         
        static async Task Main(string[] args)
        {
           
                string content =await  GetDataFromAPI("http://test-demo.aemenersol.com/api/PlatformWell/GetPlatformWellActual");
                
                using (var context = new MyDbContext())
                { 
                        foreach (var platform in JsonSerializer.Deserialize<List<PlatformDto>>(content))
                    {

                        try
                        {
                            var existingItem = await context.Platform.FindAsync(platform.id);
                            if (existingItem == null)
                            {

                                context.Database.EnsureCreated();
                                context.Platform.Add(platform);
                                context.SaveChanges(); 

                               
                            }
                            else
                            {

                                existingItem.uniqueName = platform.uniqueName;
                                existingItem.latitude = platform.latitude;
                                existingItem.longitude = platform.longitude;
                                existingItem.createdAt = platform.createdAt;
                                existingItem.updatedAt = platform.updatedAt;
                            existingItem.wells = platform.wells;
                            context.SaveChanges();
                            }
                        }
                        catch
                        {
                            context.Database.EnsureCreated();
                            context.Platform.Add(platform);
                            context.SaveChanges();
                        }

                       
                       
                    } 
                   
                }

            } 

        public static  async Task<string> GetDataFromAPI(string apiUrl)
        {
            string bearerToken= await LoginToWebAPI("http://test-demo.aemenersol.com/api/Account/Login", "user@aemenersol.com", "Test@123");
            
            client.DefaultRequestHeaders.Authorization = null;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.Replace('"', ' ').Trim());
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        

        public static   async  Task<string> LoginToWebAPI(string EndPoint,string user,string pass)
        {
            using (var client = new HttpClient())
            {
                var loginData = new { username = user, password = pass };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(loginData), System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync(EndPoint , content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var token =   response.Content.ReadAsStringAsync().Result;
                    return token; 
                }
                else
                {
                    return $"Login failed. Status code: {response.StatusCode}";
                     
                }
            }
        }
       
    } 
    public class MyDbContext: DbContext
    {
        public DbSet<PlatformDto > Platform { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Server=(localdb)\mydatabase;Database=mydatabase;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }


    public class PlatformDto
    {
        [Key] // Marks it as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }
        public string uniqueName { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; } 
        public List<WellDto> wells { get; set; } 
    }

    public class WellDto
    {
        public int id { get; set; }
        public int platformId { get; set; }
        public string uniqueName { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

}