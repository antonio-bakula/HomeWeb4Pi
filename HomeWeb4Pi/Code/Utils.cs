using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace HomeWeb4Pi.Code
{
  public static class Utils
  {
    public static T ReadWebConfigAppSettings<T>(string key)
    {
      var value = ConfigurationManager.AppSettings[key];

      if (!string.IsNullOrEmpty(value))
      {
        return (T)Convert.ChangeType(value, typeof(T));
      }
      else
      {
        return default(T);
      }
    }

    public static bool WebConfigAppSettingExists(string key)
    {
      return ConfigurationManager.AppSettings.Keys.Cast<string>().Any(kk => kk == key);
    }

    public static bool IsDebugMode()
    {
#if DEBUG
      return true;
#else
      return false;
#endif
    }

  }
}