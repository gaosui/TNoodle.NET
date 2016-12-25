using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    public static class ExtensionMethods
    {
        public static Face oppositeFace(this Face f)
        {
            return (Face)(((int)f + 3) % 6);
        }
    }
}
