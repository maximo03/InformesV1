using Dapper;
using INFORMES.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace INFORMES.Services
{
    public interface IRepositoryTask
    {
        Task sp_UpdateTask(VMTask model);
        Task<VMTask> sp_GetTaskId(int idTask, int IdUser);
        Task<IEnumerable<VMTask>> sp_GetTasks(int IdUser);
        Task sp_CreateTask(VMTask model);
        Task sp_DeleteTask(int IdTask);
        Task<DataTable> sp_GetInfoExel(int IdUser);
    }
    public class RepositoryTask: IRepositoryTask
    {
        private readonly string connectionString;
        public RepositoryTask(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task sp_CreateTask(VMTask model)
        {
            DateTime date = DateTime.Today;
            model.MonthTask = date.Month;
            model.YearTask = date.Year;
            decimal ht = Convert.ToDecimal(model.HoursTask);

            SqlConnection connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync("SP_InsertTask", new
            {
                IdTask = model.IdTask,
                TitleTask = model.TitleTask,
                IdUser = model.IdUser,
                DescriptionTask = model.DescriptionTask,
                HoursTask = ht,
                StartDate = model.StartDate,
                CloseDate = model.CloseDate,
                Journey = model.Journey,
                TeamProject = model.TeamProject,
                MonthTask = model.MonthTask,
                YearTask = model.YearTask
            }, commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task<IEnumerable<VMTask>> sp_GetTasks(int IdUser)
        {
            DateTime date = DateTime.Today;
            int monthNow = date.Month;
            int yearNow = date.Year;

            SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<VMTask>(@"SP_ListTaskDateNow", 
                new { IdUser = IdUser, MonthTask = monthNow, YearTask = yearNow },
                commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task<VMTask> sp_GetTaskId(int idTask, int IdUser)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<VMTask>(@"SELECT * FROM Task WHERE IdTask = @IdTask AND IdUser=@IdUser", new { idTask, IdUser });

        }

        public async Task sp_UpdateTask(VMTask model)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            decimal ht = Convert.ToDecimal(model.HoursTask);

            await connection.ExecuteAsync("SP_UpdateTask", new
            {
                IdTask = model.IdTask,
                TitleTask = model.TitleTask,
                IdUser = model.IdUser,
                DescriptionTask = model.DescriptionTask,
                HoursTask = ht,
                StartDate = model.StartDate,
                CloseDate = model.CloseDate,
                Journey = model.Journey,
                TeamProject = model.TeamProject,
                MonthTask = model.MonthTask,
                YearTask = model.YearTask
            }, commandType: System.Data.CommandType.StoredProcedure);


        }



        public async Task sp_DeleteTask(int IdTask)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Task WHERE IdTask=@IdTask", new { IdTask });
        }



       public async Task<DataTable> sp_GetInfoExel(int IdUser)
        {
            DateTime date = DateTime.Today;
            int monthNow = date.Month;
            int yearNow = date.Year;
            DataTable table_Info = new DataTable();
           await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using ( var adapter = new SqlDataAdapter())
                {
                    adapter.SelectCommand = new SqlCommand("SP_PrintTaskMonth", connection);
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@IdUser",IdUser);
                    adapter.SelectCommand.Parameters.AddWithValue("@MonthTask",monthNow);
                    adapter.SelectCommand.Parameters.AddWithValue("@YearTask",yearNow);
                    adapter.Fill(table_Info);
                }

            }

            return table_Info;
        }




        //End Class
    }
}
