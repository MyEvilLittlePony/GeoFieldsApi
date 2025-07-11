using GeoFieldsApi.Models;
using NetTopologySuite.Geometries;

namespace GeoFieldsApi.Services
{
    public class FieldService
    {
        private readonly List<FieldModel> _fields;

        public FieldService(string fieldsPath, string centroidsPath)
        {
            _fields = KmlParser.Parse(fieldsPath, centroidsPath);
        }

        public List<FieldModel> GetAllFields() => _fields;

        public double GetSizeById(string id)
        {
            var field = _fields.FirstOrDefault(f => f.Id == id);
            return field?.Size ?? throw new KeyNotFoundException("Field not found");
        }

        public double GetDistanceToCenter(string id, double lat, double lng)
        {
            var field = _fields.FirstOrDefault(f => f.Id == id);
            if (field == null) throw new KeyNotFoundException();

            var center = field.Locations.Center;
            return HaversineDistance(center[0], center[1], lat, lng);
        }

        public object CheckPointBelongs(double lat, double lng)
        {
            foreach (var field in _fields)
            {
                var poly = new Polygon(new LinearRing(field.Locations.Polygon
                    .Select(p => new Coordinate(p[1], p[0])).ToArray()));

                var point = new Point(lng, lat); // lng, lat
                if (poly.Covers(point))
                {
                    return new { field.Id, field.Name };
                }
            }
            return false;
        }

        private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000; // meters
            double dLat = Math.PI / 180 * (lat2 - lat1);
            double dLon = Math.PI / 180 * (lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(Math.PI / 180 * lat1) * Math.Cos(Math.PI / 180 * lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
