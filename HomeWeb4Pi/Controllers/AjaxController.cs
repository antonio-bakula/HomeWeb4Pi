using HomeWeb4Pi.Code;
using HomeWeb4Pi.Models.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HomeWeb4Pi.Controllers
{
  public class AjaxController : Controller
  {
    // GET: Ajax
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult RefreshWeatherForecast()
    {
      var model = new WeatherModel();
      string html = RazorExtensionHelpers.RenderViewToString(this.ControllerContext, "~/Views/Parts/Weather.cshtml", model);
      return Content(html);
    }

    public ActionResult RefreshCalendar()
    {
      var model = new CalendarModel();
      string html = RazorExtensionHelpers.RenderViewToString(this.ControllerContext, "~/Views/Parts/Calendar.cshtml", model);
      return Content(html);
    }

    public ActionResult RefreshClock()
    {
      var model = new ClockModel();
      string html = RazorExtensionHelpers.RenderViewToString(this.ControllerContext, "~/Views/Parts/Clock.cshtml", model);
      return Content(html);
    }
  }
}