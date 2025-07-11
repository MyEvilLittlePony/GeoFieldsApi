using GeoFieldsApi.Models;
using System.Globalization;
using System.Xml;

namespace GeoFieldsApi
{
    public static class KmlParser
    {
        private const string Namespace = "http://www.opengis.net/kml/2.2";

        public static List<FieldModel> Parse(string fieldsPath, string centroidsPath)
        {
            var centers = ParseCenters(centroidsPath);
            return ParseFields(fieldsPath, centers);
        }

        private static Dictionary<string, double[]> ParseCenters(string path)
        {
            var centers = new Dictionary<string, double[]>();
            var doc = LoadXml(path, out var nsmgr);
            var placemarks = doc.SelectNodes("//k:Placemark", nsmgr);

            foreach (XmlNode placemark in placemarks)
            {
                string? id = GetSimpleDataValue(placemark, "fid", nsmgr);
                if (string.IsNullOrEmpty(id)) continue;

                var coordText = placemark.SelectSingleNode("k:Point/k:coordinates", nsmgr)?.InnerText.Trim();
                if (coordText == null) continue;

                var coords = ParseCoordinate(coordText);
                centers[id] = coords;
            }

            return centers;
        }

        private static List<FieldModel> ParseFields(string path, Dictionary<string, double[]> centers)
        {
            var fields = new List<FieldModel>();
            var doc = LoadXml(path, out var nsmgr);
            var placemarks = doc.SelectNodes("//k:Placemark", nsmgr);

            foreach (XmlNode placemark in placemarks)
            {
                string? id = GetSimpleDataValue(placemark, "fid", nsmgr);
                if (string.IsNullOrEmpty(id)) continue;

                string name = placemark.SelectSingleNode("k:name", nsmgr)?.InnerText ?? "Unknown";

                var sizeStr = GetSimpleDataValue(placemark, "size", nsmgr);
                if (!double.TryParse(sizeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                    size = 0;

                var polygonNode = placemark.SelectSingleNode(".//k:Polygon/k:outerBoundaryIs/k:LinearRing/k:coordinates", nsmgr);
                if (polygonNode == null) continue;

                var points = ParsePolygon(polygonNode.InnerText);
                var center = centers.TryGetValue(id, out var c) ? c : [0.0, 0.0];

                fields.Add(new FieldModel
                {
                    Id = id,
                    Name = name,
                    Size = size,
                    Locations = new LocationData
                    {
                        Center = center,
                        Polygon = points
                    }
                });
            }

            return fields;
        }

        private static double[] ParseCoordinate(string coordText)
        {
            var parts = coordText.Split(',');
            return
            [
            double.Parse(parts[1], CultureInfo.InvariantCulture),
            double.Parse(parts[0], CultureInfo.InvariantCulture)
            ];
        }

        private static List<double[]> ParsePolygon(string coordText)
        {
            return coordText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var parts = s.Split(',');
                    return new double[]
                    {
                    double.Parse(parts[1], CultureInfo.InvariantCulture), // lat
                    double.Parse(parts[0], CultureInfo.InvariantCulture)  // lng
                    };
                }).ToList();
        }

        private static string? GetSimpleDataValue(XmlNode placemark, string fieldName, XmlNamespaceManager nsmgr)
        {
            return placemark.SelectSingleNode($".//k:SimpleData[@name='{fieldName}']", nsmgr)?.InnerText?.Trim();
        }

        private static XmlDocument LoadXml(string path, out XmlNamespaceManager nsmgr)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("k", Namespace);
            return doc;
        }
    }
}
