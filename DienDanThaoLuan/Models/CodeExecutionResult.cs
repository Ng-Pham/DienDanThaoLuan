using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DienDanThaoLuan.Models
{
    public class RunResult
    {
        public string stdout { get; set; }
        public string stderr { get; set; }
        public int code { get; set; }
        public string signal { get; set; }
        public string output { get; set; }
    }

    public class CodeExecutionResult
    {
        public string language { get; set; }
        public string version { get; set; }
        public RunResult run { get; set; }
    }

}