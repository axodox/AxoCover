using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
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

    public HockeyClient(IEditorContext editorContext, IOptions options)
      : base(editorContext, options)
    {
      var installationId = options.InstallationId;
      if (installationId == Guid.Empty)
      {
        installationId = Guid.NewGuid();
        options.InstallationId = installationId;
      }

      AppId = options.TelemetryKey;
      PackageName = AxoCoverPackage.Manifest.Name;
      Version = AxoCoverPackage.Manifest.Version;
      InstallationId = installationId;
      Model = editorContext.Version;

      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("User-Agent", "Hockey/WinWPF");

      _uriPrefix = $"https://rink.hockeyapp.net/api/2/apps/{AppId}/";
      _httpClient.BaseAddress = new Uri(_uriPrefix);
    }

    protected override async Task<bool> UploadExceptionAsync(Exception exception)
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
              var stackFrames = StackItem.FromStackTrace(stackTrace);

              if (stackFrames.Length > 0)
              {
                foreach (var stackItem in stackFrames)
                {
                  writer.Write($"  at {stackItem.MethodName}");
                  if (stackItem.SourceFile != null)
                  {
                    writer.WriteLine($"({Path.GetFileName(stackItem.SourceFile)}:{stackItem.Line})");
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
