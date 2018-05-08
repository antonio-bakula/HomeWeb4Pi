using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

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


  public static class RazorExtensionHelpers
  {

    /// <summary>
    /// Return Partial View.
    /// The element naming convention is maintained in the partial view by setting the prefix name from the expression.
    /// The name of the view (by default) is the class name of the Property or a UIHint("partial name").
    /// @Html.PartialFor(m => m.Address)  - partial view name is the class name of the Address property.
    /// </summary>
    /// <param name="expression">Model expression for the prefix name (m => m.Address)</param>
    /// <returns>Partial View as Mvc string</returns>
    public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
    {
      return html.PartialFor(expression, null);
    }

    /// <summary>
    /// Return Partial View.
    /// The element naming convention is maintained in the partial view by setting the prefix name from the expression.
    /// </summary>
    /// <param name="partialName">Partial View Name</param>
    /// <param name="expression">Model expression for the prefix name (m => m.Group[2])</param>
    /// <returns>Partial View as Mvc string</returns>
    public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string partialName)
    {
      string name = ExpressionHelper.GetExpressionText(expression);
      string modelName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
      ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
      object model = metaData.Model;


      if (partialName == null)
      {
        partialName = metaData.TemplateHint == null
            ? typeof(TProperty).Name    // Class name
            : metaData.TemplateHint;    // UIHint("template name")
      }

      // Use a ViewData copy with a new TemplateInfo with the prefix set
      ViewDataDictionary viewData = new ViewDataDictionary(html.ViewData)
      {
        TemplateInfo = new TemplateInfo { HtmlFieldPrefix = modelName }
      };

      // Call standard MVC Partial
      return html.Partial(partialName, model, viewData);
    }

    public static string RenderViewToString(ControllerContext context, string viewName, object model)
    {
      if (string.IsNullOrEmpty(viewName))
        viewName = context.RouteData.GetRequiredString("action");

      ViewDataDictionary viewData = new ViewDataDictionary(model);

      using (var sw = new StringWriter())
      {
        ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
        ViewContext viewContext = new ViewContext(context, viewResult.View, viewData, new TempDataDictionary(), sw);
        viewResult.View.Render(viewContext, sw);

        return sw.GetStringBuilder().ToString();
      }
    }
  }


}