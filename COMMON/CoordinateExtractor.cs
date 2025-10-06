using System.Globalization;
using System.Web;
using MODEL;
using Serilog;

namespace COMMON;

public class CoordinateExtractor
    {
        public static bool TryExtractCoordinates(string url, out double longitude, out double latitude)
        {
            longitude = 0;
            latitude = 0;

            if (!url.StartsWith("https://2gis.kz"))
            {
                return false;
            }

            try
            {
                Uri uri = new Uri(url);
                string path = uri.AbsolutePath;
                string query = uri.Query;

                // Check for coordinates in the path
                var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var segment in pathSegments)
                {
                    string decodedSegment = HttpUtility.UrlDecode(segment);
                    if (TryParseCoordinates(decodedSegment, out longitude, out latitude))
                    {
                        return true;
                    }
                }

                // Check for coordinates in the query string
                var queryParams = HttpUtility.ParseQueryString(query);
                string mParam = queryParams.Get("m");

                if (!string.IsNullOrEmpty(mParam))
                {
                    string decodedMParam = HttpUtility.UrlDecode(mParam);
                    int index = decodedMParam.IndexOf('/');
                    if (index != -1)
                    {
                        decodedMParam = decodedMParam.Substring(0, index);
                    }

                    if (TryParseCoordinates(decodedMParam, out longitude, out latitude))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, url);
            }

            return false;
        }


        private static bool TryParseCoordinates(string input, out double longitude, out double latitude)
        {
            longitude = 0;
            latitude = 0;

            var coordinates = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (coordinates.Length == 2 &&
                double.TryParse(coordinates[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out longitude) &&
                double.TryParse(coordinates[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out latitude))
            {
                return true;
            }

            return false;
        }


    public static int GetRegionForCoordinates(double longitude, double latitude, List<Region> regionList)
        {
            if (regionList == null || regionList.Count == 0)
            {
                throw new ArgumentException("Region list cannot be null or empty.");
            }

            double minDistance = double.MaxValue;
            int closestRegionId = -1;

            foreach (var region in regionList)
            {
                double distance = Math.Pow(region.Longitude - longitude, 2) + Math.Pow(region.Latitude - latitude, 2);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRegionId = region.Id;
                }
            }
            return closestRegionId;
        }
    }