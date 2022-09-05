using System;
using System.Collections;
using System.IO;

namespace GameLauncher
{
    class Settings
    {
        private Hashtable keyPairs = new Hashtable();

        private struct SectionPair
        {
            public String Section;
            public String Key;
        }

        /// <summary>
        /// Opens the data at the given path and enumerates the values in the IniParser.
        /// </summary>
        public void Load(String data)
        {
            String strLine = null;
            String currentRoot = null;
            String[] keyPair = null;
            try
            {
                using (StringReader reader = new StringReader(data))
                {
                    //reader has access to the whole string.
                    while ((strLine = reader.ReadLine()) != null)
                    {
                        strLine = strLine.Trim();
                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentRoot = strLine.Substring(1, strLine.Length - 2);
                        }
                        else
                        {
                            keyPair = strLine.Split(new char[] { '=' }, 2);

                            SectionPair sectionPair;
                            String value = null;

                            if (currentRoot == null)
                                currentRoot = "ROOT";

                            sectionPair.Section = currentRoot.ToUpper();
                            sectionPair.Key = keyPair[0].ToUpper();

                            if (keyPair.Length > 1)
                                value = keyPair[1];

                            if (keyPair.Length > 0)
                                keyPairs.Add(sectionPair, value);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
                
        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
        public String GetSetting(String sectionName, String settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            sectionPair.Key = settingName.ToUpper();

            return (String)keyPairs[sectionPair];
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public String[] EnumSection(String sectionName)
        {
            ArrayList tmpArray = new ArrayList();

            foreach (SectionPair pair in keyPairs.Keys)
            {
                if (pair.Section == sectionName)
                    tmpArray.Add(pair.Key);
            }

            return (String[])tmpArray.ToArray(typeof(String));
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        /// <param name="settingValue">Value of key.</param>
        public void AddSetting(String sectionName, String settingName, String settingValue)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            sectionPair.Key = settingName.ToUpper();

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, settingValue);
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved with a null value.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void AddSetting(String sectionName, String settingName)
        {
            AddSetting(sectionName.ToUpper(), settingName.ToUpper(), null);
        }
    }
}
