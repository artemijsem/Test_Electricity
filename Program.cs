using System;
using System.Net.Http;
using System.Timers;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


namespace Electric_Prices
{
    public class Program
    {
        private static System.Timers.Timer APITimer;
        public async void Main()
        {

           
        }

        public async void RetrieveData()
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync("https://dashboard.elering.ee/api/nps/price/EE/current");

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            //Test API output
            var result = JsonConvert.DeserializeObject(responseBody);

            Console.WriteLine(result);

            try
            {
                SqlConnectionStringBuilder sql_builder = new SqlConnectionStringBuilder();

                sql_builder.ConnectionString = "Data Source=LAPTOP-TH8704MP\\ARTEMISJEM;Database=electricpriceDB;Integrated Security=sspi;Encrypt=False;";
                

                using (SqlConnection connection = new SqlConnection(sql_builder.ConnectionString))
                {

                    connection.Open();       

                    string sql = "InsertAPIData";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Add parameter that will be passed to stored procedure
                        command.Parameters.Add(new SqlParameter("@json", responseBody));

                        command.ExecuteReader();
                    }                   
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SetTimer()
        {
            APITimer = new System.Timers.Timer(20000);

            APITimer.Elapsed += OnTimedEvent;
            APITimer.AutoReset = true;
            APITimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source,ElapsedEventArgs e)
        {
            RetrieveData();
        }
    }
}


    
        