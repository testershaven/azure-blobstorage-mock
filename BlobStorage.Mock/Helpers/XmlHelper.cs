using System.Xml;
using System.Xml.Linq;

namespace Blobstorage.Mock.Helpers;

public static class XmlHelper
{
    public static bool DeepCompare(XElement? fullObject, XElement partialObject)
    {
        var isEqual = false;

        foreach (XElement item in partialObject.Elements())
        {
            if (fullObject.Elements().Any(e => e.Value == item.Value))
            {
                isEqual = !item.HasElements || DeepCompare(fullObject.Element(item.Name), item);
            }
            else
            {
                isEqual = false;
            }

            if (!isEqual)
            {
                break;
            }
        }
        return isEqual;
    }

    public static string ConvertXmlNodesToString(XmlNode[] nodes)
    {
        var xml = "";
        foreach (XmlNode node in nodes)
        {
            xml += node.OuterXml;
        }
        return xml;
    }
}
