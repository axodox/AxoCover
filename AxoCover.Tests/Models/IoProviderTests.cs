using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxoCover.Models;
using Moq;

namespace AxoCover.Tests
{
  [TestClass]
  public class UnitTest1
  {
    private Mock<IOptions> _optionsMock;
    private IIoProvider _ioProvider;

    [TestInitialize]
    public void Initialize()
    {
      _optionsMock = new Mock<IOptions>();
      _ioProvider = new IoProvider(_optionsMock.Object);
    }

    [TestMethod]
    [DataRow(@"C:\dev\Global.runSettings", @"..\..\Global.runSettings")]
    [DataRow(@"C:\dev\TheApplication\TheApplication.runSettings", @"..\TheApplication.runSettings")]
    [DataRow(@"C:\dev\TheApplication\TheProject\TheProject.runSettings", @"..\TheProject\TheProject.runSettings")]
    [DataRow(@"C:\dev\TheApplication\TheProject\TheFolder\TheFolder.runSettings", @"..\TheProject\TheFolder\TheFolder.runSettings")]
    [DataRow(@"C:\dev\TheApplication\.AxoCover\AxoCover.runSettings", @"AxoCover.runSettings")]
    [DataRow(@"C:\dev\TheApplication\.AxoCover\TheFolder\TheFolder.runSettings", @"TheFolder\TheFolder.runSettings")]
    [DataRow(@"D:\dev\TheApplication\TheApplication.runSettings", @"D:\dev\TheApplication\TheApplication.runSettings")]
    [DataRow(@"", @"")]
    public void TestGetRelativePath(string input, string output)
    {
      _optionsMock
        .Setup(p => p.SolutionSettingsPath)
        .Returns(@"C:\dev\TheApplication\.AxoCover\settings.json");
      var relativePath = _ioProvider
        .GetRelativePath(input);
      Assert.AreEqual(output, relativePath);
    }

    [TestMethod]
    public void TestGetRelativePathNoSource()
    {
      _optionsMock
        .Setup(p => p.SolutionSettingsPath)
        .Returns("");
      var relativePath = IoProvider
        .GetRelativePath("", @"C:\dev\TheApplication\TheApplication.runSettings");
      Assert.AreEqual(@"C:\dev\TheApplication\TheApplication.runSettings", relativePath);
    }

    [TestMethod]
    [DataRow(@"..\..\Global.runSettings", @"C:\dev\Global.runSettings")]
    [DataRow(@"..\TheApplication.runSettings", @"C:\dev\TheApplication\TheApplication.runSettings")]
    [DataRow(@"..\TheProject\TheProject.runSettings", @"C:\dev\TheApplication\TheProject\TheProject.runSettings")]
    [DataRow(@"..\TheProject\TheFolder\TheFolder.runSettings", @"C:\dev\TheApplication\TheProject\TheFolder\TheFolder.runSettings")]
    [DataRow(@"AxoCover.runSettings", @"C:\dev\TheApplication\.AxoCover\AxoCover.runSettings")]
    [DataRow(@"TheFolder\TheFolder.runSettings", @"C:\dev\TheApplication\.AxoCover\TheFolder\TheFolder.runSettings")]
    [DataRow(@"D:\dev\TheApplication\TheApplication.runSettings", @"D:\dev\TheApplication\TheApplication.runSettings")]
    [DataRow(@"", @"C:\dev\TheApplication\.AxoCover")]
    public void TestGetAboslutePath(string input, string output)
    {
      _optionsMock
        .Setup(p => p.SolutionSettingsPath)
        .Returns(@"C:\dev\TheApplication\.AxoCover\settings.json");
      var absolutePath = _ioProvider
        .GetAbsolutePath(input);
      Assert.AreEqual(output, absolutePath);
    }
  }
}
