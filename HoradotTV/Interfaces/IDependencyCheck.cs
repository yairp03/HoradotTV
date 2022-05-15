using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoradotTV.Models
{
    internal interface IDependencyCheck
    {
        public string LoadingText { get; }
        public string FixProblemUrl { get; }

        public Task<bool> RunCheckAsync();
    }
}
