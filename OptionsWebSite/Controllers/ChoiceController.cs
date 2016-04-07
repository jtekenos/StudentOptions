using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DiplomaDataModel.Models;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts;
using System.Drawing;

namespace OptionsWebSite.Controllers
{
    public class ChoiceController : Controller
    {
        private DataContext db = new DataContext();

        // GET: Choice
        public ActionResult Index()
        {
            ViewBag.DefaultTerm = db.YearTerms
                .Where(yt => yt.IsDefault == true)
                .FirstOrDefault()
                .YearTermId;
            return View();
        }

        public ActionResult Modify()
        {
            return View(db.Choices.Include("Option1").Include("Option2").Include("Option3").Include("Option4").Include("YearTerm").ToList());
        }

        public PartialViewResult YearTermSelect()
        {
            ViewBag.YearTermList = new SelectList(
                db.YearTerms
                .GroupBy(yt => yt.YearTermId)
                .Select(y => y.FirstOrDefault())
                .OrderBy(y => !y.IsDefault),
                "YearTermId", "Description");
            return PartialView("_YearTermSelectPartial");

        }
        public PartialViewResult GenerateChartPartial(int yeartermid = 0)
        {
           
            ViewBag.DefaultTerm = db.YearTerms
                .Where(yt => yt.IsDefault == true)
                .FirstOrDefault()
                .YearTermId;

            int numOfOptions = db.Options
                .Where(o => o.IsActive == true)
                .Count();
            int numOfStudents = db.Choices
                .Where(c => c.YearTermId == yeartermid)
                .Count();

            int[] activeOptionIds = db.Options
                .Where(o => o.IsActive == true)
                .Select(ot => ot.OptionId)
                .ToArray();
            string[] optionsList = db.Options
                .Where(o => o.IsActive == true)
                .OrderBy(or => or.OptionId)
                .Select(ot => ot.Title)
                .ToArray();
            

            object[] firstOptionCount = new object[numOfOptions];
            object[] secondOptionCount = new object[numOfOptions];
            object[] thirdOptionCount = new object[numOfOptions];
            object[] fourthOptionCount = new object[numOfOptions];

            for (int i = 0; i < activeOptionIds.Length; i++)
            {
                var tempid = activeOptionIds[i];
                var temp = db.Choices
                    .Where(c => c.FirstChoiceOptionId == tempid
                            && c.YearTermId == yeartermid)
                    .Count();
                firstOptionCount[i] = temp;
            }

            for (int i = 0; i < activeOptionIds.Length; i++)
            {
                var tempid = activeOptionIds[i];
                var temp = db.Choices
                    .Where(c => c.SecondChoiceOptionId == tempid
                            && c.YearTermId == yeartermid)
                    .Count();
                secondOptionCount[i] = temp;
            }

            for (int i = 0; i < activeOptionIds.Length; i++)
            {
                var tempid = activeOptionIds[i];
                var temp = db.Choices
                    .Where(c => c.ThirdChoiceOptionId == tempid
                            && c.YearTermId == yeartermid)
                    .Count();
                thirdOptionCount[i] = temp;
            }

            for (int i = 0; i < activeOptionIds.Length; i++)
            {
                var tempid = activeOptionIds[i];
                var temp = db.Choices
                    .Where(c => c.FourthChoiceOptionId == tempid
                            && c.YearTermId == yeartermid)
                    .Count();
                fourthOptionCount[i] = temp;
            }

            Highcharts chart = new Highcharts("chart")
                .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column })
                .SetTitle(new Title { Text = "Student Option Selections " })
                .SetXAxis(new XAxis { Categories = optionsList })
                .SetYAxis(new YAxis
                {
                    Min = 0,
                    Title = new YAxisTitle { Text = "Students" }
                })
                .SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        PointPadding = 0.2,
                        BorderWidth = 0
                    }
                })
                .SetSeries(new[]
                {
                    new Series { Name = "First Choice", Data = new Data(firstOptionCount) },
                    new Series { Name = "Second Choice", Data = new Data(secondOptionCount) },
                    new Series { Name = "Third Choice", Data = new Data(thirdOptionCount) },
                    new Series { Name = "Fourth Choice", Data = new Data(fourthOptionCount) },
                });

            return PartialView("_ChartReportPartial", chart);
        }
        public PartialViewResult GenerateReportPartial(int yeartermid = 0)
        {

            ViewBag.Message = yeartermid;
            return PartialView("_DetailReportPartial", db.Choices
                .Include("Option1")
                .Include("Option2")
                .Include("Option3")
                .Include("Option4")
                .Include("YearTerm")
                .ToList());
        }

        public ActionResult Details(int? id)
        // GET: Choice/Details/5
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Choice choice = db.Choices.Find(id);

            Choice choice = db.Choices.Include("Option1").Include("Option2").Include("Option3").Include("Option4").Include("YearTerm").FirstOrDefault(c => c.ChoiceId == id);
            if (choice == null)
            {
                return HttpNotFound();
            }
            return View(choice);
        }

        // GET: Choice/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Choice/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ChoiceId,StudentId,StudentFirstName,StudentLastName,FirstChoiceOptionId,SecondChoiceOptionId,ThirdChoiceOptionId,FourthChoiceOptionId,SelectionDate,YearTerm")] Choice choice)
        {

            if (ModelState.IsValid)
            {
                db.Choices.Add(choice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(choice);
        }

        // GET: Choice/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Choice choice = db.Choices.Find(id);
            if (choice == null)
            {
                return HttpNotFound();
            }
            return View(choice);
        }

        // POST: Choice/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ChoiceId,StudentId,StudentFirstName,StudentLastName,FirstChoiceOptionId,SecondChoiceOptionId,ThirdChoiceOptionId,FourthChoiceOptionId,SelectionDate,YearTerm")] Choice choice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(choice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(choice);
        }

        // GET: Choice/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Choice choice = db.Choices.Find(id);
            if (choice == null)
            {
                return HttpNotFound();
            }
            return View(choice);
        }

        // POST: Choice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Choice choice = db.Choices.Find(id);
            db.Choices.Remove(choice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
