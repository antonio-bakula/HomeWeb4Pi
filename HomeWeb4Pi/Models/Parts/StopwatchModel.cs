using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeWeb4Pi.Models.Parts
{
  public class StopwatchModel
  {
    public List<StopwatchPresetItemModel> Presets { get; private set; }

    public StopwatchModel()
    {
      var durations = new List<int> { 5, 10, 15, 30, 45, 1 };
      this.Presets = durations.Select(ii => new StopwatchPresetItemModel(ii)).ToList();
    }
  }

  public class StopwatchPresetItemModel
  {
    public string Minutes { get; private set; }
    public string Period { get; set; }

    public StopwatchPresetItemModel(int presetDuration)
    {
      this.Minutes = presetDuration.ToString();
      this.Period = presetDuration.ToString("00") + ":00";
    }
  }
}