using LicenseAPI.Contexts;
using LicenseAPI.Models;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.OpenApi.Validations;
using System.Globalization;
using System.Reflection.Emit;
using System.Text.Json;

namespace LicenseAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LicenseController : Controller
    {
        private readonly dbSCM _contextDbSCM;
        private readonly dbHRM _contextDbHRM;
        private readonly dbBCS _contextDbBCS;
        private readonly dbIoTFac2 _contextDbIoTFac2;

        public LicenseController(dbSCM contextDbSCM, dbHRM contextDbHRM, dbBCS contextDbBCS, dbIoTFac2 contextDbIoTFac2)
        {
            _contextDbSCM = contextDbSCM;
            _contextDbHRM = contextDbHRM;
            _contextDbBCS = contextDbBCS;
            _contextDbIoTFac2 = contextDbIoTFac2;
        }

        [HttpGet]
        [Route("/station/{code}")]
        public async Task<IActionResult> getStationDetail(string code)
        {
            var mstr = _contextDbSCM.SkcDictMstrs;
            var station = await (from x in mstr
                                 where x.DictType == "STATION" && x.Code == code
                                 select new
                                 {
                                     x.Code,
                                     x.RefCode,
                                     licenses = (from license in mstr
                                                 join mst in mstr
                                                 on license.RefCode equals mst.Code
                                                 where license.DictType == "STATION_LICENSE" && license.Code == x.Code
                                                 select new { DictMstr = mst.DictDesc, license.Code, license.RefCode }).ToList()
                                 }).FirstOrDefaultAsync();
            return Ok(station);
        }

        [HttpPost]
        [Route("/station/license/empcode")]
        public async Task<IActionResult> getStationLicenseEmpCode([FromBody] ModelDict obj)
        {
            var license = await (from mstr in _contextDbSCM.SkcDictMstrs
                                 where mstr.DictType == "STATION_LICENSE" && mstr.Code == obj.station
                                 select new
                                 {
                                     mstr.Code,
                                     mstr.RefCode,
                                     effectiveDate = (from x in _contextDbSCM.SkcLicenseTrainings where x.DictCode == mstr.RefCode && x.Empcode == obj.code select x).FirstOrDefault()
                                 }).ToListAsync();
            var user = await (from users in _contextDbHRM.Employees
                              where users.Code == obj.code
                              select new { users.Code, users.Name, users.Surn, users.Posit, users.Pren, users.Join }).FirstOrDefaultAsync();
            return Ok(new { licenses = license, user = user });
        }

        [HttpGet]
        [Route("employee/{code}")]
        public async Task<IActionResult> empGet(string code)
        {
            var user = await _contextDbHRM.Employees.SingleOrDefaultAsync(x => x.Code == code);
            return Ok(user);
        }

        [HttpGet]
        [Route("privilege/{empcode}")]
        public async Task<IActionResult> privGet(string empcode)
        {
            var priv = await (from userinrole in _contextDbSCM.SkcUserInRoles
                              join pri in _contextDbSCM.SkcPrivileges
                              on userinrole.PriRole equals pri.PriRole
                              where userinrole.PriEmpcode == empcode
                              select new
                              {
                                  userinrole.PriEmpcode,
                                  pri.PriRole,
                                  pri.PriProgram,
                                  pri.PriSearch,
                                  pri.PriAdd,
                                  pri.PriModify,
                                  pri.PriDelete
                              }
                              ).ToListAsync();
            return Ok(priv);
        }

        [HttpPost]
        [Route("/station/checkinout/")]
        public async Task<IActionResult> dictCheckInOut(ModelDict obj)
        {
            var item = new SkcCheckInOutLog();
            item.DictCode = obj.refCode;
            item.ChkEmpcode = obj.code;
            item.ChkState = obj.state;
            item.ChkDate = DateTime.Now;
            _contextDbSCM.SkcCheckInOutLogs.Add(item);
            int result = await _contextDbSCM.SaveChangesAsync();
            if (result >= 0)
            {
                return Ok(new { status = true });
            }
            return Ok(new { status = false });
        }

        [HttpPost]
        [Route("/line/station")]
        public async Task<IActionResult> stationOfLine([FromBody] ModelDict obj)
        {
            var line = obj.line;
            var msts = _contextDbSCM.SkcDictMstrs;
            var checkins = _contextDbSCM.SkcCheckInOutLogs;
            var trains = _contextDbSCM.SkcLicenseTrainings;
            var items = await (from a in msts
                               where a.RefCode == line && a.DictType == "STATION"
                               select new
                               {
                                   a.Note,
                                   code = a.Code,
                                   stName = a.DictDesc,
                                   emcode = (from b in checkins where b.DictCode == a.Code orderby b.ChkId descending select b.ChkEmpcode).FirstOrDefault(),
                                   checkinout = (from b in checkins where b.DictCode == a.Code orderby b.ChkId descending select b.ChkState).FirstOrDefault(),
                                   date = (from b in checkins where b.DictCode == a.Code && b.ChkState == "IN" orderby b.ChkId descending select b.ChkDate).FirstOrDefault(),
                                   licenses = (from l in msts.DefaultIfEmpty()
                                               join mst in msts
                                               on l.RefCode equals mst.Code
                                               where l.DictType == "STATION_LICENSE" && l.Code == a.Code
                                               select new { l.DictId, l.Code, mst.DictDesc, l.RefCode, expDate = (from train in trains where train.DictCode == l.RefCode && train.Empcode == ((from b in checkins where b.DictCode == a.Code orderby b.ChkId descending select b.ChkEmpcode).FirstOrDefault()) select train.ExpiredDate).FirstOrDefault() }
                                               ).ToList()
                               }).ToListAsync();
            var result = from x in items
                         select new
                         {
                             x.Note,
                             x.code,
                             x.stName,
                             x.emcode,
                             x.date,
                             x.licenses,
                             x.checkinout,
                             user = from user in _contextDbHRM.Employees
                                    where user.Code == x.emcode
                                    select user
                         };
            return Ok(result);
        }



        [HttpPost]
        [Route("/dict/licenseofstation/")]
        public async Task<IActionResult> dictLicenseOfStation([FromBody] ModelDict obj)
        {
            var context = await (from a in _contextDbSCM.SkcDictMstrs
                                 where a.DictType == obj.type && a.Code == obj.code && a.DictStatus == true
                                 select new
                                 {
                                     a.DictId,
                                     a.DictType,
                                     a.Code,
                                     a.RefCode,
                                     a.CreateDate,
                                     desc = (from b in _contextDbSCM.SkcDictMstrs where b.Code == a.RefCode select b.DictDesc).FirstOrDefault()
                                 }
                                 ).OrderByDescending(x => x.CreateDate).ToListAsync();
            return Ok(context);
        }

        [HttpPost]
        [Route("/dict/get")]
        public async Task<IActionResult> dictGet([FromBody] SkcDictMstr obj)
        {
            //if (obj.RefCode == "")
            //{
            //    return Ok();
            //}
            var context = await _contextDbSCM.SkcDictMstrs.ToListAsync();
            if (obj.DictId != 0 && obj.DictId != null && obj.DictId.ToString() != "")
            {
                return Ok(context.Where(x => x.DictId == obj.DictId).FirstOrDefault());
            }
            if (obj.DictType != null && obj.DictType != "")
            {
                context = context.Where(x => x.DictType == obj.DictType).ToList();
            }
            if (obj.Code != null && obj.Code != "")
            {
                context = context.Where(x => x.Code == obj.Code).ToList();
            }
            if (obj.RefCode != null && obj.RefCode != "")
            {
                context = context.Where(x => x.RefCode == obj.RefCode).ToList();
            }
            return Ok(new { data = context.OrderBy(x=>x.DictDesc) });
        }

        [HttpPost]
        [Route("/dict/license")]
        public IActionResult DictLicense()
        {
            var context =  _contextDbSCM.SkcDictMstrs.Where(x=>x.DictType == "LICENSE" && x.Code == x.RefCode && x.DictStatus == true).ToList();
            return Ok(new { data = context.OrderBy(x=>x.DictDesc)});
        }

        [HttpPost]
        [Route("/dict/all")]
        public async Task<IActionResult> dictAll()
        {
            var context = await _contextDbSCM.SkcDictMstrs.ToListAsync();
            return Ok(context);
        }


        [HttpPost]
        [Route("/dict/format/all")]
        public async Task<IActionResult> dictFormatAll()
        {
            List<SkcDictMstr> listFac = await _contextDbSCM.SkcDictMstrs.Where(x=>x.DictType == "FAC").ToListAsync();
            foreach(SkcDictMstr itemFac in listFac)
            {
                List<SkcDictMstr> listLine = await _contextDbSCM.SkcDictMstrs.Where(x => x.DictType == "LINE" && x.RefCode == itemFac.Code && x.RefItem == "FAC").ToListAsync();
                foreach(SkcDictMstr itemLine in listLine)
                {
                    List<SkcDictMstr> listSt = await _contextDbSCM.SkcDictMstrs.Where(x => x.DictType == "STATION" && x.RefCode == itemFac.Code && x.RefItem == "LINE").ToListAsync();
                }
            }
            return Ok(listFac);
        }

        [HttpPost]
        [Route("/license/user")]
        public async Task<IActionResult> getUserOfLicense([FromBody] ModelDict obj)
        {
            if (obj.code != null && obj.code != "null" && obj.code != "")
            {
                var context = await _contextDbSCM.SkcLicenseTrainings.Where(x => x.Empcode == obj.code && x.DictCode == obj.refCode).OrderByDescending(x => x.CreateDate).FirstOrDefaultAsync();
                return Ok(context);
            }
            else
            {
                var res = await _contextDbSCM.SkcLicenseTrainings.Where(x => x.DictCode == obj.refCode).ToListAsync();
                return Ok(res);
            }
        }


        [HttpGet]
        [Route("/dict/delete/{dictId}")]
        public async Task<IActionResult> delDictStation(int dictId)
        {
            //var context = _contextDbSCM.SkcDictMstrs.Where(x => x.DictId == dictId);
            //if (context != null)
            //{

            //    _contextDbSCM.SkcDictMstrs.RemoveRange(context);
            //    int state = await _contextDbSCM.SaveChangesAsync();
            //    if (state >= 0)
            //    {
            //        return Ok(new { status = true });
            //    }
            //}
            var item = await _contextDbSCM.SkcDictMstrs.FirstOrDefaultAsync(x => x.DictId == dictId);
            if (item != null)
            {
                item.DictStatus = false;
                int state = await _contextDbSCM.SaveChangesAsync();
                if (state >= 0)
                {
                    return Ok(new { status = true });
                }
            }
            return Ok(new { status = false });
        }

        [HttpPost]
        [Route("/dict/add/station")]
        public async Task<IActionResult> dictAddStation([FromBody] ModelDict obj)
        {
            string runnCode = "";
            SkcDictMstr lastCode = _contextDbSCM.SkcDictMstrs.Where(x => x.DictType == "STATION").OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastCode != null)
            {
                //runnCode = (Convert.ToInt32(lastCode.Code) + 1).ToString();
                string runCode = lastCode.Code.Substring(lastCode.Code.Length - 3);
                if (!runCode.All(char.IsDigit))
                {
                    runCode = lastCode.Code.Substring(lastCode.Code.Length - 2);
                }
                string charCode = lastCode.Code.Substring(0, lastCode.Code.Length - runCode.Length);
                runnCode = charCode + "" + (int.Parse(runCode) + 1).ToString().PadLeft(2, '0');
            }
            SkcDictMstr model = new SkcDictMstr();
            model.DictType = obj.type;
            model.Code = runnCode;
            model.DictDesc = obj.desc;
            model.RefCode = obj.refCode;
            model.CreateDate = DateTime.Now;
            model.RefItem = "LINE";
            model.DictStatus = true;
            _contextDbSCM.SkcDictMstrs.Add(model);
            var newItem = await _contextDbSCM.SaveChangesAsync();
            if (newItem > 0)
            {
                return Ok(new { status = true, code = runnCode });
            }
            else
            {
                return Ok(new { status = false, code = runnCode });
            }
        }



        [HttpPost]
        [Route("/dict/add/license")]
        public async Task<IActionResult> dictAddLicense([FromBody] ModelDict obj)
        {
            string runnCode = "";
            SkcDictMstr lastCode = _contextDbSCM.SkcDictMstrs.Where(x => x.DictType == "LICENSE").OrderByDescending(x => x.DictId).FirstOrDefault();
            if (lastCode != null)
            {
                string runCode = lastCode.Code.Substring(lastCode.Code.Length - 3);
                if (!runCode.All(char.IsDigit))
                {
                    runCode = lastCode.Code.Substring(lastCode.Code.Length - 2);
                }
                string charCode = lastCode.Code.Substring(0, lastCode.Code.Length - runCode.Length);
                runnCode = charCode + "" + (int.Parse(runCode) + 1).ToString().PadLeft(2, '0');
            }
            else
            {
                runnCode = "LIC01";
            }
            SkcDictMstr model = new SkcDictMstr();
            model.DictType = obj.type;
            model.Code = runnCode;
            model.DictDesc = obj.desc;
            model.RefCode = runnCode;
            model.CreateDate = DateTime.Now;
            model.DictStatus = true;
            _contextDbSCM.SkcDictMstrs.Add(model);
            var newItem = await _contextDbSCM.SaveChangesAsync();
            return Ok(new { status = (newItem > 0 ? true : false), code = runnCode });
        }

        [HttpPost]
        [Route("/dict/add")]
        public async Task<IActionResult> dictAdd([FromBody] ModelDict obj)
        {
            int runnCode = 1;
            if (obj.code == "" || obj.code == null)
            {
                var lastItem = _contextDbSCM.SkcDictMstrs.Where(x => x.DictType == obj.dictType).OrderByDescending(x => x.Code).Select(x=>x.Code).FirstOrDefault();
                if (lastItem.Count() > 0)
                {
                    runnCode = Convert.ToInt32(lastItem) + 1;
                }
                //if (lastCode != null)
                //{

                //string runCode = lastCode.Code.Substring(lastCode.Code.Length - 3);
                //if (!runCode.All(char.IsDigit))
                //{
                //    runCode = lastCode.Code.Substring(lastCode.Code.Length - 2);
                //}
                //string charCode = lastCode.Code.Substring(0, lastCode.Code.Length - runCode.Length);
                //runnCode = charCode + "" + (int.Parse(runCode) + 1).ToString().PadLeft(2, '0');
                //}
            }
            SkcDictMstr model = new SkcDictMstr();
            model.DictType = obj.dictType;
            model.Code = (obj.code == "" || obj.code == null) ? runnCode.ToString() : obj.code;
            model.DictDesc = obj.dictDesc;
            model.RefCode = obj.refCode;
            model.CreateDate = DateTime.Now;
            model.RefItem = obj.refItem;
            model.DictStatus = true;
            _contextDbSCM.SkcDictMstrs.Add(model);
            var newItem = await _contextDbSCM.SaveChangesAsync();
            if (newItem > 0)
            {
                return Ok(new { status = true, code = runnCode });
            }
            else
            {
                return Ok(new { status = false, code = runnCode });
            }
        }


        [HttpPost]
        [Route("/training/add")]
        public async Task<ActionResult> TrainingAdd([FromBody] SkcLicenseTraining item)
        {
            item.CreateDate = DateTime.Now;
            item.AlertDate = DateTime.Parse(item.ExpiredDate.ToString()).AddMonths(-3);
            _contextDbSCM.SkcLicenseTrainings.Add(item);
            int res = await _contextDbSCM.SaveChangesAsync();
            if (res >= 0)
            {
                return Ok(new { status = true });
            }
            return Ok(new { status = false });
        }

        [HttpPost]
        [Route("/training/user")]
        public async Task<IActionResult> TrainingUsers([FromBody] SkcLicenseTraining obj)
        {
            var items = await (from train in _contextDbSCM.SkcLicenseTrainings
                               where train.Empcode != null && train.DictCode == obj.DictCode
                               select new
                               {
                                   train.TrId,
                                   train.Empcode,
                                   train.EffectiveDate,
                                   train.ExpiredDate,
                                   train.CreateDate
                               }
                        ).ToListAsync();
            var result = (from x in items
                          select new
                          {
                              x.TrId,
                              x.Empcode,
                              name = (from user in _contextDbHRM.Employees where user.Code == x.Empcode select (user.Pren.ToUpper() + user.Name + " " + user.Surn)),
                              x.EffectiveDate,
                              x.ExpiredDate,
                              x.CreateDate
                          }).OrderByDescending(x => x.CreateDate);
            return Ok(result);
        }

        [HttpGet]
        [Route("/training/delete/{trId}")]
        public async Task<IActionResult> delTrainById(int trId)
        {
            var context = _contextDbSCM.SkcLicenseTrainings.Where(x => x.TrId == trId);
            if (context != null)
            {
                _contextDbSCM.SkcLicenseTrainings.RemoveRange(context);
                int state = await _contextDbSCM.SaveChangesAsync();
                if (state >= 0)
                {
                    return Ok(new { status = true });
                }
            }
            return Ok(new { status = false });
        }


        [HttpGet]
        //[Route("/accumulate/{line}/{st}/{stOld}")]
        //public IActionResult grafana(string line,string st,string stOld)
        [Route("/accumulate/{line}/{st}")]
        public IActionResult GetAccumulate(string line, string st)
        {
            List<ItemChart> itemChart = new List<ItemChart>();
            DateTime SDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 08:00:00"));
            DateTime EDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 20:00:00"));
            DateTime endDate = DateTime.Today;
            endDate = endDate.AddMonths(-5);
            var lastLogin = (from x in _contextDbSCM.SkcCheckInOutLogs where x.DictCode == st select new { x.ChkEmpcode ,x.ChkId}).OrderByDescending(x=>x.ChkId).FirstOrDefault();
            int Year = DateTime.Now.AddYears(DateTime.Now.Month <= 4 ? -1 : 0).Year;
            DateTime FirstDateOfYear = DateTime.Parse(Convert.ToString(Year) + "-04-01 08:00:00");
            //var LeakCheck = _contextDbIoTFac2.EtdLeakChecks.Where(x => (x.StampTime >= SDate && x.StampTime <= EDate) && x.LineName == line && x.Brazing == stOld).GroupBy(x => x.EmpCode).Select(g => new { CntSerialNo = g.Count(), EmpCode = g.Key });
            try
            {
                //if (LeakCheck.Count() > 0 && LeakCheck.FirstOrDefault().EmpCode != null && lastLogin.ChkEmpcode == LeakCheck.FirstOrDefault().EmpCode)
                //{
                if (lastLogin.ChkEmpcode != "") {
                    //var empCode = LeakCheck.FirstOrDefault().EmpCode;
                    var empCode = lastLogin.ChkEmpcode;
                    var daily = (from x in _contextDbIoTFac2.BrazingCertDataLogs
                                 where x.EmpCode == empCode && x.Pddate == DateTime.Now.Date
                                 select new { x.CountFg, x.CountNg }).FirstOrDefault();

                    //var accusFG = (from x in _contextDbIoTFac2.BrazingCertDataLogs
                    //               where x.EmpCode == empCode && x.Line == line
                    //               select new { CountFG = x.CountFg });

                    var accuFG = (from x in _contextDbIoTFac2.BrazingCertDataLogs
                                 where x.EmpCode == empCode
                                 select new { CountFG = x.CountFg });
                    var accuNG = (from x in _contextDbIoTFac2.BrazingCertDataLogs
                                   where x.EmpCode == empCode 
                                   && x.UpdateDate >= FirstDateOfYear && x.UpdateDate <= DateTime.Now
                                   select new { CountNG = x.CountNg }); // WHERE วันแรกที่ตัดรอบบิลวันที่ 1 เดือน 4

                    //var accus = (from x in _contextDbIoTFac2.BrazingCertDataLogs
                    //             where x.EmpCode == empCode && x.Pddate >= DateTime.Parse(DateTime.Now.ToString("yyyy-01-01")) && x.Pddate >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                    //             select new { CountNG = x.CountNg, CountFG = x.CountFg });

                    for (int i = 1; i <= 6; i++)
                    {
                        DateTime SDateNow = DateTime.Parse(endDate.ToString("yyyy-MM-01 08:00:00"));
                        DateTime EDateNow = DateTime.Parse(endDate.ToString("yyyy-MM-" + DateTime.DaysInMonth(endDate.Year, endDate.Month) + " 20:00:00"));
                        var dataMonth = _contextDbIoTFac2.BrazingCertDataLogs.Where(x => x.EmpCode == empCode && (x.UpdateDate >= SDateNow && x.UpdateDate <= EDateNow)).ToList().Sum(x => x.CountNg);
                        itemChart.Add(new ItemChart() { ng = Convert.ToInt32(dataMonth), month = (endDate.ToString("MMM") + "-" + (endDate.Year.ToString()).Substring(2, 2)) });
                        endDate = endDate.AddMonths(1);
                    }
                    //if (daily != null)
                    //{
                    //    return Ok(new { daily = new { ok = daily.CountFg, ng = daily.CountNg }, accu = new { ok = accuFG.Sum(x => x.CountFG), ng = accuNG.Sum(x => x.CountNG) }, chart = itemChart });
                    //}
                    //else
                    //{
                    //    return Ok(new { daily = new { ok = 0, ng = 0 }, accu = new { ok = 0, ng = 0 }, chart = itemChart });
                    //}
                    return Ok(new { daily = new { ok = (daily != null ? daily.CountFg : 0), ng = (daily != null ? daily.CountNg : 0)}, accu = new { ok = accuFG.Sum(x => x.CountFG), ng = accuNG.Sum(x => x.CountNG) }, chart = itemChart });
                }
                else
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        itemChart.Add(new ItemChart() { ng = 0, month = (endDate.ToString("MMM") + "-" + (endDate.Year.ToString()).Substring(2, 2)) });
                        endDate = endDate.AddMonths(1);
                    }
                    return Ok(new { daily = new { ok = 0, ng = 0 }, accu = new { ok = 0, ng = 0 }, chart = itemChart });
                }
            }
            catch(Exception e)
            {
                for (int i = 1; i <= 6; i++)
                {
                    itemChart.Add(new ItemChart() { ng = 0, month = (endDate.ToString("MMM") + "-" + (endDate.Year.ToString()).Substring(2, 2)) });
                    endDate = endDate.AddMonths(1);
                }
                return Ok(new { daily = new { ok = 0, ng = 0 }, accu = new { ok = 0, ng = 0 }, chart = itemChart });
            }
            //var Accu = _contextDbIoTFac2.EtdLeakChecks.Where(x => x.EmpCode != null && x.EmpCode != "" && x.EmpCode != "NULL").GroupBy(x => x.EmpCode).Select(g => new { Accumulate = g.Count(), EmpCode = g.Key });

            //var result = (from leak in LeakCheck.DefaultIfEmpty()
            //              join accu in Accu
            //              on leak.EmpCode equals accu.EmpCode
            //              select accu.Accumulate);
            //var okAccu = (from x in _contextDbIoTFac2.BrazingCertDataLogs
            //              where x.EmpCode == empCode
            //              select new { x.CountFg }
            //              ).Sum(x => x.CountFg);
        }

        [HttpPost]
        [Route("/dict/edit")]
        public async Task<IActionResult> editStation([FromBody] ModelDict obj)
        {
            var item = await _contextDbSCM.SkcDictMstrs.FirstOrDefaultAsync(x => x.DictId == obj.dictId);
            if (item != null)
            {
                item.DictDesc = obj.dictDesc;
            }
            int effUpdate = await _contextDbSCM.SaveChangesAsync();
            return Ok(new { status = effUpdate });
        }

        [HttpGet]
        [Route("/pstMstr")]
        public async Task<IActionResult> pstMstr()
        {
            var item = await _contextDbBCS.PositMstrs.ToListAsync();
            return Ok(item);
        }

        public class WeatherForecast
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string? Summary { get; set; }
        }

        [HttpGet]
        [Route("/station/checkin/{stcode}")]
        public IActionResult StationCheckIn(string stcode)
        {
            var checkin = _contextDbSCM.SkcCheckInOutLogs.Where(x => x.DictCode == stcode).OrderByDescending(x => x.ChkDate).Take(1).FirstOrDefault();
            return Ok(checkin);
        }


      
    }

    internal class ItemChart
    {
        public int ng { get; set; }
        public string month { get; set; }
    }
}
