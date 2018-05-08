using HomeWeb4Pi.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeWeb4Pi.Models.Parts
{
  public class CalendarModel
  {
    public List<CalendarDayModel> Days { get; private set; }

    public CalendarModel()
    {
      this.Days = new List<CalendarDayModel>();
      var calendarEvents = GoogleCalendar.GetUpcomingEvents(10, 60);
      foreach (var evt in calendarEvents.Items)
      {
        DateTime eventStart = DateTime.MinValue;
        if (evt.Start.DateTime.HasValue)
        {
          eventStart = evt.Start.DateTime.Value;
        }
        else
        {
          eventStart = DateTime.Parse(evt.Start.Date);
        }

        var eventDay = this.Days.FirstOrDefault(day => day.Date.Date == eventStart.Date);
        if (eventDay == null)
        {         
          eventDay = new CalendarDayModel(eventStart);
          this.Days.Add(eventDay);
        }

        var item = new CalendarItemModel();
        item.Title = evt.Summary;
        item.Start = eventStart;
        item.End = eventStart;
        if (evt.End.DateTime.HasValue)
        {
          item.End = evt.End.DateTime.Value;
        }
        eventDay.Items.Add(item);
      }
    }

  }

  public class CalendarDayModel
  {
    public DateTime Date { get; private set; }
    public List<CalendarItemModel> Items { get; set; }

    public string GoogleDate
    {
      get
      {
        return this.Date.ToString("yyyy-MM-dd");
      }
    }

    public string Title
    {
      get
      {
        string title = "";
        if (this.Date.Date == DateTime.Today)
        {
          title = "DANAS";
        }
        else if (this.Date.Date == DateTime.Today.AddDays(1))
        {
          title = "SUTRA";
        }
        else
        {
          title = EnglishTranslator.TranslateToHr(this.Date.DayOfWeek.ToString());
        }
        title += " - " + this.Date.ToShortDateString();
        return title;
      }
    }

    public CalendarDayModel(DateTime day)
    {
      this.Date = day;
      this.Items = new List<CalendarItemModel>();
    }
  }


  public class CalendarItemModel
  {
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public string Period
    {
      get
      {
        if (this.Start == this.End)
        {
          return "";
        }
        else
        {
          return this.Start.ToShortTimeString() + " - " + this.End.ToShortTimeString();
        }
      }
    }
  }
}