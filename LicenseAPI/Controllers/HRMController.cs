using LicenseAPI.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LicenseAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HRMController : Controller
    {
        private readonly dbHRM _contextHRM;

        public HRMController(dbHRM contextHRM)
        {
            _contextHRM = contextHRM;
        }


        [HttpGet]
        [Route("employee/{code}")]
        public async Task<IActionResult> getEmployee(string code)
        {
            var context = await _contextHRM.Employees.Where(x => x.Code == code).ToListAsync();
            return Ok(context);
        }
    }
}
