using HomeWeb4Pi.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeWeb4Pi.Models.Parts
{
  public class WeatherModel
  {
    private WeatherData data;

    public string City { get; set; }
    public string CurrentTemperature { get; private set; }
    public string CurrentCondition { get; private set; }
    public string CurrentConditionImage { get; private set; }
    public List<WeatherDayData> Forecasts { get; private set; }

    public WeatherModel()
    {
      this.City = Utils.ReadWebConfigAppSettings<string>("WeatherForecastCity");
      int woeId = Utils.ReadWebConfigAppSettings<int>("WeatherForecastWoeid");
      this.data = new WeatherData(woeId, 5);

      this.CurrentTemperature = this.data.Temperature.ToString() + "°";
      this.CurrentCondition = EnglishTranslator.TranslateToHr(this.data.Text);
      this.CurrentConditionImage = "/Content/Images/Weather/" + this.data.Code.ToString("00") + ".png";
      this.Forecasts = this.data.Forecasts.Skip(1).Select(fc => new WeatherDayData(fc)).ToList();
    }
  }

  public class WeatherDayData
  {
    /// <summary>
    /// Dan, npr.:
    /// Pon
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Prognoza temperature najniža npr.:
    /// 12°
    /// </summary>
    public string TemperatureMin { get; set; }

    /// <summary>
    /// Prognoza temperature najniža npr.:
    /// 22°
    /// </summary>
    public string TemperatureMax { get; set; }

    /// <summary>
    ///  prognoza, npr:
    ///  Sunčano
    /// </summary>
    public string Forecast { get; set; }

    public string ForecastImage { get; set; }

    public WeatherDayData(WeatherForecast forecast)
    {
      this.Day = EnglishTranslator.TranslateToHr(forecast.Day);
      this.TemperatureMin = $"{forecast.TempMin}°";
      this.TemperatureMax = $"{forecast.TempMax}°";
      this.Forecast = EnglishTranslator.TranslateToHr(forecast.Text);
      this.ForecastImage = "/Content/Images/Weather/" + forecast.Code.ToString("00") + ".png";
    }
  }

}