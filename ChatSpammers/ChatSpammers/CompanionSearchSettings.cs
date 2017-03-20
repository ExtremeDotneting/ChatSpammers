using System;
using System.Collections.Generic;

namespace ChatSpammers
{
    public class CompanionSearchSettings: IFormattable
    {
        public CompanionSearchSettings(bool isUserMan, PeoplesAge userAge)
        {
            IsUserMan = isUserMan;
            IsCompanionMan = null;
            UserAge = userAge;
            CompanionAge = new List<PeoplesAge>() { PeoplesAge.From18To21, PeoplesAge.From22To25,
                PeoplesAge.From25To35, PeoplesAge.Older36, PeoplesAge.Under17 };
        }

        public CompanionSearchSettings(bool isUserMan, bool? isCompanionMan, PeoplesAge userAge, List<PeoplesAge> companionAge)
        {
            IsUserMan = isUserMan;
            IsCompanionMan = isCompanionMan;
            UserAge = userAge;
            CompanionAge = companionAge;
        }

        public bool IsUserMan
        {
            get;
            private set;
        }

        public bool? IsCompanionMan
        {
            get;
            private set;
        }

        public PeoplesAge UserAge
        {
            get;
            private set;
        }

        public List<PeoplesAge> CompanionAge
        {
            get;
            private set;
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            string res = "";
            res += "Current user age:" + Convert.ToString(UserAge)+";\n";
            res += "Current user sex: ";
            if (IsUserMan)
                res += "man;\n";
            else
                res += "woman;\n";

            res += "Another user age:";
            foreach (PeoplesAge item in CompanionAge)
                res += " " + Convert.ToString(item);
            res += ";\n";

            res += "Another user sex: ";
            if (IsCompanionMan == null)
            {
                res += "man or woman;\n";
            }
            else
            {
                if ((bool)IsCompanionMan)
                    res += "man;\n";
                else
                    res += "woman;\n";
            }

            return res;
            
        }
    }
}
