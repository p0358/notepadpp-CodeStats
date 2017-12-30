using System;
using System.Collections.Generic;

namespace CodeStats
{
    internal class XpObj
    {
        public string language { get; set; }
        public int xp { get; set; }

        public XpObj(string lang)
        {
            language = lang;
            xp = 0;
        }

        public XpObj(string lang, int initialXp)
        {
            language = lang;
            xp = initialXp;
        }

        public void addXp(int count)
        {
            xp += count;
        }
    }

    internal class Pulse
    {
        public string coded_at { get; set; }
        public List<XpObj> xps;

        public Pulse()
        {
            coded_at = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
            xps = new List<XpObj>();
        }

        public void addXpForLanguage(string lang, int xp)
        {
            bool foundExisting = false;

            foreach (var xpobj in xps)
            {
                if (xpobj.language == lang)
                {
                    foundExisting = true;
                    xpobj.addXp(xp);
                }
            }

            if (!foundExisting)
            {
                xps.Add(new XpObj(lang, xp));
            }

            coded_at = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
        }

        public bool isEmpty()
        {
            return xps.Count == 0;
        }

        /*internal Pulse(Heartbeat h)
        {
            entity = h.entity;
            timestamp = h.timestamp;
            project = h.project;
            is_write = h.is_write;
        }*/
    }
}
