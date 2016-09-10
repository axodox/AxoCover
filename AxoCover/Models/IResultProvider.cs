using AxoCover.Models.Data;
using System;

namespace AxoCover.Models
{
  public interface IResultProvider
  {
    event EventHandler ResultsUpdated;

    TestResult GetTestResult(TestMethod testMethod);
  }
}