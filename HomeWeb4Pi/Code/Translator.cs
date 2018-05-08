using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace HomeWeb4Pi.Code
{
  public static class EnglishTranslator
  {
    private static List<TranslationEntry> _translations = null;

    public static string TranslateToHr(string eng)
    {
      if (_translations == null)
      {
        string json = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/translations.json"));
        _translations = JsonConvert.DeserializeObject<List<TranslationEntry>>(json);
      }
      var entry = _translations.FirstOrDefault(en => en.English == eng);
      if (entry != null)
      {
        return entry.Croatian;
      }
      return eng;
    }
  }

  public class TranslationEntry
  {
    public string English { get; set; }
    public string Croatian { get; set; }
  }

}