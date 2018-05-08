using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace HomeWeb4Pi.Code
{
  public static class GoogleCalendar
  {
    static string[] _Scopes = { CalendarService.Scope.CalendarReadonly };

    public static Events GetUpcomingEvents(int maxResults, int maxDays)
    {
      var service = GetCalendarService();
      EventsResource.ListRequest request = service.Events.List("primary");
      request.TimeMin = DateTime.Now;
      request.TimeMax = DateTime.Now.AddDays(maxDays);
      request.ShowDeleted = false;
      request.SingleEvents = true;
      request.MaxResults = maxResults;
      request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
      return request.Execute();
    }

    private static CalendarService GetCalendarService()
    {
      UserCredential credential;

      string googleClientIdJsonFile = HttpContext.Current.Server.MapPath("~/App_Data/google_calendar_client_id.json");
      using (var stream = new FileStream(googleClientIdJsonFile, FileMode.Open, FileAccess.Read))
      {
        string storeFile = HttpContext.Current.Server.MapPath("~/App_Data/google_calendar_client_store.json");
        var secrets = GoogleClientSecrets.Load(stream).Secrets;
        var store = new FileDataStore(storeFile, true);
        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, _Scopes, "user", CancellationToken.None, store).Result;
      }

      return new CalendarService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = "HomeWebAppClient",
      });

    }
  }
}