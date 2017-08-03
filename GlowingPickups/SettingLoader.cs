using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace GlowingPickups
{
    public class SettingLoader<T> where T : class, new()
    {
        public T Load(string xmlPath)
        {
            var serializer = new XmlSerializer(typeof(T));

            T settings = null;

            if (File.Exists(xmlPath))
            {
                using (var stream = File.OpenRead(xmlPath)) settings = (T)serializer.Deserialize(stream);

                using (var stream = new FileStream(xmlPath, File.Exists(xmlPath) ? FileMode.Truncate : FileMode.Create, FileAccess.ReadWrite)) serializer.Serialize(stream, settings);
            }

            return settings;
        }
        public T Init(string xmlPath)
        {
            var ser = new XmlSerializer(typeof(T));
            T settings;

            using (var stream = File.OpenWrite(xmlPath))
            {
                ser.Serialize(stream, settings = new T());
            }

            return settings;
        }
    }
}
