using HomeWeb4Pi.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeWeb4Pi.Models.Parts
{
  public class ClockModel
  {
    public string Time { get; set; }
    public string Date { get; set; }

    public ClockModel()
    {
      var now = DateTime.Now;
      this.Time = now.Hour.ToString("00") + ":" + now.Minute.ToString("00");
      this.Date = EnglishTranslator.TranslateToHr(now.DayOfWeek.ToString()) + " " + now.ToShortDateString();
    }
  }
}