using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drum_Machine.Data.Entities;

namespace Drum_Machine.Core
{
    public static class AppSession
    {
        public static User? CurrentUser { get; set; }
    }
}