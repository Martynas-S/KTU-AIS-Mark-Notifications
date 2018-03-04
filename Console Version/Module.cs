using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTU_AIS_Scraper
{
    sealed class Module
    {
        public string Semester { get; private set; }
        public string SemesterNumber { get; private set; }
        public string ModuleName { get; private set; }

        private string JSArgument { get; set; } // P1 and P2 in KTU-ICE
        public string p1 { get; private set; }
        public string p2 { get; private set; }

        public Module(IElement element)
        {
            Semester = GetSemester(element);
            SemesterNumber = GetSemesterNumber(element);
            ModuleName = GetModuleName(element);

            JSArgument = GetJSArgument(element);
            p1 = GetP1();
            p2 = GetP2();
        }

        private string GetSemester(IElement e)
            => e.ParentElement.ParentElement.FirstElementChild.FirstElementChild
                .TextContent.Split('(')[0].Trim();

        private string GetSemesterNumber(IElement e)
            => e.ParentElement.ParentElement.FirstElementChild.FirstElementChild
                .TextContent.Split('(')[1].Split(')')[0].Trim();

        private string GetModuleName(IElement e)
            => e.Children[1].TextContent;

        private string GetJSArgument(IElement e)
            => e.Children[5].FirstElementChild.GetAttribute("onclick")
               .Split('(')[1].Split(')')[0].Trim();

        private string GetP1()
            => JSArgument.Split(',')[0];

        private string GetP2()
            => JSArgument.Split(',')[1].Trim('\'');
    }
}
