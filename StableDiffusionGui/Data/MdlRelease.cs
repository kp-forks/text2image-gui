﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusionGui.Data
{
    internal class MdlRelease
    {
        public string Version { get; set; } = "0.0.0";
        public string Channel { get; set; } = "";
        public DateTime ReleaseDate { get; set; }
        public string HashBasefiles { get; set; } = "";

        public MdlRelease () { }

        public MdlRelease (EasyDict<string, string> properties)
        {
            Version = properties.Get("version", "0.0.0");
            Channel = properties.Get("channel", "none");
            ReleaseDate = DateTime.ParseExact(properties.Get("date", "2000-01-01"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            HashBasefiles = properties.Get("hashBasefiles", "");
        }

        public override string ToString()
        {
            return $"{Version} ({CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Channel)} Branch) ({ReleaseDate.ToString("yyyy-MM-dd")})";
        }
    }
}
