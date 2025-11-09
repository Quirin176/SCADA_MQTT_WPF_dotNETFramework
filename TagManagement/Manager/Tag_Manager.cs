using MQTT_Protocol.DataTypes;
using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Driver_Tool.Manager
{
    public class Tag_Manager
    {
        public const string TAG = "Tag";
        public const string TAG_ID = "TagId";
        public const string TAG_NAME = "TagName";
        public const string TOPIC = "Topic";
        public const string QOS = "QoS";
        public const string RETAIN = "Retain";

        public static void Add(Device device, Tag tg)
        {
            try
            {
                if (tg == null) throw new NullReferenceException("The Tag is null reference exception");
                IsExisted(device, tg);
                device.Tags.Add(tg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Device device, Tag tg)
        {
            try
            {
                if (tg == null) throw new NullReferenceException("The Tag is null reference exception");

                foreach (Tag item in device.Tags)
                {
                    if (item.TagId == tg.TagId)
                    {
                        item.TagId = tg.TagId;
                        item.TagName = tg.TagName;
                        item.Topic = tg.Topic;
                        item.QoS = tg.QoS;
                        item.Retain = tg.Retain;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device gr, int tgId)
        {
            try
            {
                Tag result = GetByTagId(gr, tgId);
                if (result == null) throw new KeyNotFoundException("Tag Id is not found exception");
                gr.Tags.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device gr, string grName)
        {
            try
            {
                Tag result = GetByTagName(gr, grName);
                if (result == null) throw new KeyNotFoundException("Tag name is not found exception");
                gr.Tags.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device gr, Tag tg)
        {
            try
            {
                if (tg == null) throw new NullReferenceException("The Tag is null reference exception");
                foreach (Tag item in gr.Tags)
                {
                    if (item.TagId == tg.TagId)
                    {
                        gr.Tags.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Tag IsExisted(Device gr, Tag tg)
        {
            Tag result = null;
            try
            {
                foreach (Tag item in gr.Tags)
                {
                    if (item.TagId != tg.TagId && item.TagName.Equals(tg.TagName))
                    {
                        throw new InvalidOperationException(string.Format("Tag name: '{0}' is existed", tg.TagName));
                    }
                    if (item.TagId != tg.TagId && item.Topic.Equals(tg.Topic))
                    {
                        throw new InvalidOperationException(string.Format("Tag address: '{0}' is existed", tg.Topic));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Tag GetByTagId(Device gr, int tgId)
        {
            Tag result = null;
            try
            {
                foreach (Tag item in gr.Tags)
                {
                    if (item.TagId == tgId)
                    {
                        result = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Tag GetByTagName(Device gr, string tgName)
        {
            Tag result = null;
            try
            {
                foreach (Tag item in gr.Tags)
                {
                    if (item.TagName.Equals(tgName))
                    {
                        result = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Tag GetByAddress(Device gr, string topic)
        {
            Tag result = null;
            try
            {
                foreach (Tag item in gr.Tags)
                {
                    if (item.Topic.Equals(topic))
                    {
                        result = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static List<Tag> GetTags(XmlNode grNote)
        {
            List<Tag> grList = new List<Tag>();
            try
            {
                foreach (XmlNode item in grNote)
                {
                    Tag tg = new Tag();
                    tg.TagId = int.Parse(item.Attributes[TAG_ID].Value);
                    tg.TagName = item.Attributes[TAG_NAME].Value;
                    tg.Topic = item.Attributes[TOPIC].Value;
                    tg.QoS = byte.Parse(item.Attributes[QOS].Value);
                    tg.Retain = bool.Parse(item.Attributes[RETAIN].Value);
                    tg.Description = item.Attributes[Building_Manager.DESCRIPTION].Value;

                    grList.Add(tg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return grList;
        }
    }
}
