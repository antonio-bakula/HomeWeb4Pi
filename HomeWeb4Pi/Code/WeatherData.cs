using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Xml;

namespace HomeWeb4Pi.Code
{
  public class WeatherData
  {
    public int Woeid { get; private set; }

    public int Code { get; set; }

    public int Temperature { get; set; }

    public string Text { get; set; }

    public List<WeatherForecast> Forecasts { get; private set; }

    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Kreira vremensku prognozu za trenutno stanje.
    /// </summary>
    /// <param name="key">Woeid lokacije.</param>
    public WeatherData(int wid)
      : this(wid, 0)
    {
    }

    /// <summary>
    /// Kreira vremensku prognozu za trenutno stanje i prognozira stanje za broj narednih dana (počevši sa današnjim)
    /// </summary>
    /// <param name="key">Woeid. Vidi: http://www.woeidlookup.com/ - npr. Zagreb je 851128</param>
    /// <param name="days">Broj dana.</param>
    public WeatherData(int wid, int days)
    {
      this.Forecasts = new List<WeatherForecast>();
      SetWoeidAndCollect(wid, days);
    }

    private void SetWoeidAndCollect(int wid, int days)
    {
      this.Woeid = wid;
      CollectData(days);
    }

    private void CollectData(int days)
    {
      // Popis kodova za vremenske uvjete: https://developer.yahoo.com/weather/documentation.html#codes

      string xmlString;

      // Cache za vremensku prognozu pojedinog grada
      string cacheID = "weatherForecast" + this.Woeid;
      bool dataFromCache = GetCache(cacheID, out xmlString);
      bool wasReadFromFileStorage = false;
      if (!dataFromCache)
      {
        string yql = String.Format("select * from weather.forecast where woeid={0} and u='c'", this.Woeid.ToString());
        string address = "https://query.yahooapis.com/v1/public/yql?q=" + HttpUtility.UrlEncode(yql) + "&format=xml&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
        try
        {
          xmlString = this.GetMessage(address);
        }
        catch
        {
          // Yahoo ne radi
          xmlString = "";
        }

        if (xmlString == "") // Servis ne radi ili je krivo postavljen grad. pokušavam čitanje iz file-a
        {
          xmlString = ReadFromFileStorage();
          wasReadFromFileStorage = true;
        }
      }

      if (!ParseXmlStringSafe(days, xmlString))
      {
        /// sa trenutnim sadržajem imam problema, probati ću uzeti iz file-a zadnji dobar
        xmlString = ReadFromFileStorage();
        wasReadFromFileStorage = true;

        if (!ParseXmlStringSafe(days, xmlString))
          xmlString = "";  /// ni to nije uspjelo, neželim da mi takav sadržaj spremi u cache
      }

      if (!string.IsNullOrEmpty(xmlString) && !dataFromCache && this.Forecasts.Count > 0)
      {
        SetCache(xmlString, cacheID);
        if (!wasReadFromFileStorage)
          WriteToFileStorage(xmlString);
      }
    }

    private bool ParseXmlStringSafe(int days, string xmlString)
    {
      if (string.IsNullOrEmpty(xmlString))
      {
        return false;
      }

      return ParseXmlString(days, xmlString);
    }

    private bool ParseXmlString(int days, string xmlString)
    {
      var doc = new XmlDocument();
      doc.LoadXml(xmlString);
      var man = new XmlNamespaceManager(doc.NameTable);
      man.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");

      XmlNodeList nodesCondition = doc.SelectNodes("/query/results/channel/item/yweather:condition", man);
      if (nodesCondition.Count == 0)
      {
        return false;
      }
      XmlNode nodeCondition = nodesCondition[0];
      this.Code = Convert.ToInt32(nodeCondition.Attributes["code"].Value);
      this.Temperature = Convert.ToInt32(nodeCondition.Attributes["temp"].Value);
      this.Text = nodeCondition.Attributes["text"].Value;

      XmlNodeList nodesForecast = doc.SelectNodes("/query/results/channel/item/yweather:forecast", man);

      if ((days > 0) && (nodesForecast.Count >= days))
      {
        this.Forecasts = new List<WeatherForecast>();
        for (int i = 0; i < days; i++)
        {
          XmlNode nodeForecast = nodesForecast[i];
          int code = Convert.ToInt32(nodeForecast.Attributes["code"].Value);
          int high = Convert.ToInt32(nodeForecast.Attributes["high"].Value);
          int low = Convert.ToInt32(nodeForecast.Attributes["low"].Value);
          string day = nodeForecast.Attributes["day"].Value;
          string text = nodeForecast.Attributes["text"].Value;

          this.Forecasts.Add(new WeatherForecast(day, code, low, high, text));
        }
      }

      return this.Forecasts.Count > 0;
    }

    private void SetCache(string xmlString, string cacheID)
    {
      DateTime absoluteExpiration = DateTime.Now.AddHours(1);
      TimeSpan slidingExpiration = System.Web.Caching.Cache.NoSlidingExpiration;
      HttpRuntime.Cache.Insert(cacheID, xmlString, null, absoluteExpiration, slidingExpiration);
      HttpRuntime.Cache.Insert(cacheID + "LastUpdated", DateTime.Now, null, absoluteExpiration, slidingExpiration);
      this.LastUpdated = DateTime.Now;
    }

    private bool GetCache(string cacheID, out string xmlContents)
    {
      string dateCacheID = cacheID + "LastUpdated";
      if (HttpRuntime.Cache[cacheID] != null && HttpRuntime.Cache[cacheID] is string &&
          HttpRuntime.Cache[dateCacheID] != null && HttpRuntime.Cache[dateCacheID] is DateTime)
      {
        xmlContents = (string)HttpRuntime.Cache[cacheID];
        this.LastUpdated = (DateTime)HttpRuntime.Cache[dateCacheID];
        return true;
      }
      else
      {
        xmlContents = "";
        return false;
      }
    }

    private string ReadFromFileStorage()
    {
      string fileName = PrepareStorageFileAndGetFullPath("LastForecastInfo", this.Woeid);
      return DataStorage.ReadFromFile<string>(fileName);
    }

    private void WriteToFileStorage(string xmlContents)
    {
      string fileName = PrepareStorageFileAndGetFullPath("LastForecastInfo", this.Woeid);
      DataStorage.WriteToFile(xmlContents, fileName);
    }

    private string GetMessage(string endPoint)
    {
      HttpWebRequest request = CreateWebRequest(endPoint);
      request.Timeout = 2000;

      using (var response = (HttpWebResponse)request.GetResponse())
      {
        var responseValue = string.Empty;

        if (response.StatusCode != HttpStatusCode.OK)
        {
          throw new HttpException("Error getting weatherd data. Status: " + response.StatusCode.ToString());
        }

        // grab the response   
        using (var responseStream = response.GetResponseStream())
        {
          using (var reader = new StreamReader(responseStream))
          {
            responseValue = reader.ReadToEnd();
          }
        }

        return responseValue;
      }
    }

    private HttpWebRequest CreateWebRequest(string endPoint)
    {
      var request = (HttpWebRequest)WebRequest.Create(endPoint);

      request.Method = "GET";
      request.ContentLength = 0;
      request.ContentType = "text/xml";

      return request;
    }

    private static string PrepareStorageFileAndGetFullPath(string storageSig, int locationCode)
    {
      string storageID = GetStorageID(storageSig, locationCode.ToString());
      string myDir = HttpContext.Current.Server.MapPath("/App_Data") + "\\" + storageSig;
      if (!Directory.Exists(myDir))
        Directory.CreateDirectory(myDir);

      return Path.Combine(myDir, storageSig + "-" + storageID + ".xml");
    }

    private static string GetStorageID(string storageSig, string locationCode)
    {
      string storageID;
      storageID = storageSig + locationCode;
      return storageID;
    }
  }

  public class WeatherForecast
  {
    public string Day { get; private set; }
    public int Code { get; private set; }
    public int TempMin { get; private set; }
    public int TempMax { get; private set; }
    public string Text { get; private set; }

    public WeatherForecast(string day, int code, int tempMin, int tempMax, string text)
    {
      this.Day = day;
      this.Code = code;
      this.TempMin = tempMin;
      this.TempMax = tempMax;
      this.Text = text;
    }

  }

  public static class DataStorage
  {
    /// <summary>
    /// over write file
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="filename"></param>
    public static void WriteToFile(object obj, string filename)
    {
      lock (typeof(DataStorage))
      {
        FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        try
        {
          fs.Lock(0, obj.ToString().Length);
          IFormatter fmt = new BinaryFormatter();
          fmt.Serialize(fs, obj);
          fs.Unlock(0, obj.ToString().Length);
        }
        finally
        {
          fs.Close();
        }
      }
    }

    public static void AppendToFile(object obj, string filename)
    {
      lock (typeof(DataStorage))
      {
        FileStream fs = File.Open(filename, FileMode.Append, FileAccess.Write);
        try
        {
          using (StreamWriter log = new StreamWriter(fs))
          {
            log.Write(obj);
          }
        }
        finally
        {
          fs.Close();
        }
      }
    }

    public static T ReadFromFile<T>(string fileName)
    {
      lock (typeof(DataStorage))
      {
        T result = default(T);


        if (File.Exists(fileName))
        {
          try
          {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try
            {
              IFormatter fmt = new BinaryFormatter();
              result = (T)fmt.Deserialize(fs);
            }
            finally
            {
              fs.Close();
            }
          }
          catch
          {
            File.Delete(fileName);
          }
        }
        return result;
      }
    }

    public static string ReadTextFromFile(string fileName)
    {
      string fileContents = "";
      lock (typeof(DataStorage))
      {
        if (File.Exists(fileName))
        {
          try
          {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try
            {
              using (StreamReader reader = new StreamReader(fs))
              {
                fileContents = reader.ReadToEnd();
              }
            }
            finally
            {
              fs.Close();
            }
          }
          catch
          {
            //
          }
        }
        return fileContents;
      }
    }

  }
}