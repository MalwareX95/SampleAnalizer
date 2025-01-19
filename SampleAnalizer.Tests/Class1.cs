using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleAnalizer;

namespace SampleAnalizer.Tests
{

    static class Db
    {
        public static void Select([StaticField] object selector)
        {
        }
    }

    class A
    {
        void Do()
        {
            Db.Select((object x) => x);
        }
    }
}
