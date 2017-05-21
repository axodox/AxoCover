using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Common.Models
{
  [DataContract]
  public class TestResultMessage
  {
    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string Text { get; set; }
  }
}
