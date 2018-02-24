using System.Xml.Serialization;

namespace AxoCover.Models.Testing.Data.CoverageReport
{
  public class Summary
  {
    [XmlAttribute("numClasses")]
    public int Classes { get; set; }

    [XmlAttribute("visitedClasses")]
    public int VisitedClasses { get; set; }

    [XmlAttribute("numMethods")]
    public int Methods { get; set; }

    [XmlAttribute("visitedMethods")]
    public int VisitedMethods { get; set; }

    [XmlAttribute("numSequencePoints")]
    public int SequencePoints { get; set; }

    [XmlAttribute("visitedSequencePoints")]
    public int VisitedSequencePoints { get; set; }

    [XmlAttribute("numBranchPoints")]
    public int BranchPoints { get; set; }

    [XmlAttribute("visitedBranchPoints")]
    public int VisitedBranchPoints { get; set; }

    [XmlAttribute("sequenceCoverage")]
    public double SequenceCoverage { get; set; }

    [XmlAttribute("branchCoverage")]
    public double BranchCoverage { get; set; }

    [XmlAttribute("maxCyclomaticComplexity")]
    public int MaxCyclomaticComplexity { get; set; }

    [XmlAttribute("minCyclomaticComplexity")]
    public int MinCyclomaticComplexity { get; set; }
  }
}
