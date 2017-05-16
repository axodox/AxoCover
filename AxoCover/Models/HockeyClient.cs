using AxoCover.Models.Extensions;
using AxoCover.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AxoCover.Models
{
  public class HockeyClient : TelemetryManager, IDisposable
  {
    public string AppId { get; private set; }

    public string PackageName { get; private set; }

    public string Version { get; private set; }

    public Guid InstallationId { get; private set; }

    public string Manufacturer { get; private set; }

    public string Model { get; private set; }

    private bool _isDisposed;

    private readonly HttpClient _httpClient;

    private readonly string _uriPrefix;

    private readonly Regex _stackRegex;

    public HockeyClient(IEditorContext editorContext)
      : base(editorContext)
    {
      var installationId = Settings.Default.InstallationId;
      if (installationId == Guid.Empty)
      {
        installationId = Guid.NewGuid();
        Settings.Default.InstallationId = installationId;
      }

      AppId = Settings.Default.TelemetryKey;
      PackageName = AxoCoverPackage.Manifest.Name;
      Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      InstallationId = installationId;
      Model = editorContext.Version;

      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("User-Agent", "Hockey/WinWPF");

      _uriPrefix = $"https://rink.hockeyapp.net/api/2/apps/{AppId}/";
      _httpClient.BaseAddress = new Uri(_uriPrefix);

      _stackRegex = new Regex(@" *at (?<methodName>[^(]*)\([^\)]*\)( in (?<filePath>.*):line (?<line>\d+))?");
    }

    public override async Task<bool> UploadExceptionAsync(Exception exception)
    {
      if (IsTelemetryEnabled)
      {
        using (new InvariantCulture())
        {
          try
          {
            using (var writer = new StringWriter())
            {
              writer.WriteLine("Package: " + PackageName);
              writer.WriteLine("Version: " + Version);

              writer.WriteLine("OS: Windows");
              writer.WriteLine("Windows: " + Environment.OSVersion.Version.ToString());

              if (!string.IsNullOrEmpty(Manufacturer))
              {
                writer.WriteLine("Manufacturer: " + Manufacturer);
              }

              if (!string.IsNullOrEmpty(Model))
              {
                writer.WriteLine("Model: " + Model);
              }

              writer.WriteLine("Date: " + DateTime.Now.ToUniversalTime().ToString("O"));
              writer.WriteLine("CrashReporter Key: " + InstallationId);

              var exceptionItem = exception;
              while (exceptionItem != null)
              {
                writer.WriteLine();
                writer.WriteLine(exception.GetType().FullName + ": " + exception.Message);

                var stackTrace = exception.StackTrace ?? new StackTrace().ToString();
                var stackFrames = _stackRegex.Matches(stackTrace);

                if (stackFrames.Count > 0)
                {
                  foreach (Match match in stackFrames)
                  {
                    writer.Write($"  at {match.Groups["methodName"].Value}");
                    if (match.Groups["filePath"].Success && match.Groups["line"].Success)
                    {
                      writer.WriteLine($"({ Path.GetFileName(match.Groups["filePath"].Value)}:{ match.Groups["line"].Value})");
                    }
                    else
                    {
                      writer.WriteLine();
                    }
                  }
                }
                else
                {
                  //In case the regex fails somehow                  
                  writer.WriteLine(stackTrace);
                  writer.WriteLine("(could not parse stacktrace)");
                }

                exceptionItem = exceptionItem.InnerException;
              }

              writer.Flush();

              var text = $"raw={HttpUtility.UrlEncode(writer.ToString()).Replace("+", "%20")}" +
                "&sdk=HockeySDKWinWPF" +
                "&sdk_version=hockeysdk.wpf:4.1.6.1005" +
                "&userID=" + InstallationId;

              var form = new StringContent(text);
              form.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
              form.Headers.ContentType.CharSet = null;

              var response = await _httpClient.PostAsync("crashes", form);
              return response.StatusCode == HttpStatusCode.Created;
            }
          }
          catch
          {
            return false;
          }
        }
      }
      else
      {
        return true;
      }
    }

    public void Dispose()
    {
      if (!_isDisposed)
      {
        _isDisposed = true;
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
      }
    }
  }
}
