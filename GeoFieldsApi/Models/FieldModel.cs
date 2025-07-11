namespace GeoFieldsApi.Models
{
    public class FieldModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; } // Площадь поля в м²
        public LocationData Locations { get; set; }
    }

    public class LocationData
    {
        public double[] Center { get; set; } // [lat, lng]
        public List<double[]> Polygon { get; set; } // [[lat, lng], [lat, lng], ...]
    }
}
