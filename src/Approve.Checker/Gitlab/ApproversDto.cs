using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Approve.Checker.Gitlab;

public class ApproversDto
{
    public int CountApprovers { get; set; } = 1;
    public List<string> Users { get; set; }
}