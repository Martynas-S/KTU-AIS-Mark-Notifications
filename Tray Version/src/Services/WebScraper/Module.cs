using AngleSharp.Dom;

namespace Tray_Version.Services.WebScraper
{
    sealed class Module
    {
        public string Semester { get; private set; }
        public string SemesterNumber { get; private set; }
        public string ModuleName { get; private set; }

        private string JSArgument { get; set; } // P1 and P2 in KTU-ICE
        public string P1 { get; private set; }
        public string P2 { get; private set; }

        public Module(IElement element)
        {
            Semester = GetSemester(element);
            SemesterNumber = GetSemesterNumber(element);
            ModuleName = GetModuleName(element);

            JSArgument = GetJSArgument(element);
            P1 = GetP1();
            P2 = GetP2();
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
