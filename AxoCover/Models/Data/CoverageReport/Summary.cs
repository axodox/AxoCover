using System.Xml.Serialization;

namespace AxoCover.Models.Data.CoverageReport
{
  public class Summary
  {
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
