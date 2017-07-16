using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace GlowingPickup
{
    public static class Util
    {
        public static Setting ReadSettings(string path)
        {
            var ser = new XmlSerializer(typeof(Setting));

            Setting settings = null;

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path)) settings = (Setting)ser.Deserialize(stream);

                using (var stream = new FileStream(path, File.Exists(path) ? FileMode.Truncate : FileMode.Create, FileAccess.ReadWrite)) ser.Serialize(stream, settings);
            }
            else
            {
                using (var stream = File.OpenWrite(path)) ser.Serialize(stream, settings = new Setting());
            }

            return settings;
        }
    }
}
