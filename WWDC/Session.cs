using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWDC
{
    public sealed class Session
    {
        public int year = 0;
        public int id = -1;
        public string date = "";
        public string title = "";
        public string track = "";
        public string description = "";
        public string download_hd = "";
        public string download_sd = "";
        public string slides = "";

        public override string ToString()
        {
            return title;
        }
    }
}
