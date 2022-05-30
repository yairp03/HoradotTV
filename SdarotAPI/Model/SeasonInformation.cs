using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdarotAPI.Model
{
    public class SeasonInformation
    {
        public int SeasonIndex { get; set; }
        public string SeasonName { get; set; }

        public SeasonInformation(int seasonIndex, string seasonName)
        {
            SeasonIndex = seasonIndex;
            SeasonName = seasonName;
        }
    }
}
