using INFORMES.Models;
using INFORMES.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using ClosedXML.Excel;

namespace INFORMES.Controllers
{
    public class TaskController: Controller
    {
        private readonly IGetUser getUser;
        private readonly IRepositoryTask repositoryTask;

        public TaskController(IGetUser getUser, IRepositoryTask repositoryTask)
        {
            this.getUser = getUser;
            this.repositoryTask = repositoryTask;
        }
        public async Task<IActionResult> Index()
        {
            int IdUser = getUser.GetUserId();
            var Tasks = await repositoryTask.sp_GetTasks(IdUser);
            return View(Tasks);
        }

        public IActionResult CreateTask()
        {
            int id = getUser.GetUserId();
            VMTask model = new VMTask() { IdUser = id };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(VMTask model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await repositoryTask.sp_CreateTask(model);

            return RedirectToAction("Index","Task");
 
        }


        [HttpGet]
        public async Task<IActionResult> EditTask(int Id)
        {
            int IdUser = getUser.GetUserId();
            var Task = await repositoryTask.sp_GetTaskId(Id,IdUser);

            if (Task is null)
            {
                return RedirectToAction("Forbiden", "Home");
            }

            return View(Task);
        }


        [HttpPost]
        public async Task<IActionResult> EditTask(VMTask model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int IdUser = getUser.GetUserId();
            var Task = await repositoryTask.sp_GetTaskId(model.IdTask, IdUser);

            if (Task is null)
            {
                return RedirectToAction("Forbiden", "Home");
            }

            model.IdUser = IdUser;
            await repositoryTask.sp_UpdateTask(model);

            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> DeleteTask(int Id)
        {
            int IdUser = getUser.GetUserId();
            var Task = await repositoryTask.sp_GetTaskId(Id, IdUser);

            if (Task is null)
            {
                return RedirectToAction("Forbiden", "Home");
            }

            return View(Task);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteTk(int IdTask)
        {
            int IdUser = getUser.GetUserId();
            var Task = await repositoryTask.sp_GetTaskId(IdTask, IdUser);

            if (Task is null)
            {
                return RedirectToAction("Forbiden", "Home");
            }


            await repositoryTask.sp_DeleteTask(IdTask);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PrintExel(int IdTask)
        {
            int IdUser = getUser.GetUserId();
            DataTable tableTask = await repositoryTask.sp_GetInfoExel(IdUser);

            using (XLWorkbook book = new XLWorkbook())
            {
                tableTask.TableName = "Tareas";
                var sheet = book.Worksheets.Add(tableTask);
                sheet.ColumnsUsed().AdjustToContents();

                using (MemoryStream memory = new MemoryStream())
                {
                    book.SaveAs(memory);
                    var nameExel = string.Concat("Volcado_de_Tareas_",DateTime.Now.ToString(),".xlsx");
                    return File(memory.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",nameExel);
                }
            }
                      
        }




        //End class
    }
}
