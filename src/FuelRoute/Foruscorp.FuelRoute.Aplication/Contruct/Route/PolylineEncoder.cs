using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route
{
    public static class PolylineEncoder
    {
        /// <summary>
        /// Encodes a list of coordinates (latitude, longitude) into a Google Maps polyline string.
        /// </summary>
        /// <param name="coordinates">List of [lat, lng] pairs.</param>
        /// <returns>Encoded polyline string.</returns>
        public static string EncodePolyline(List<List<double>> coordinates)
        {
            if (coordinates == null || coordinates.Count == 0)
            {
                return string.Empty;
            }

            var encodedPoints = new StringBuilder();
            int prevLat = 0, prevLng = 0;

            foreach (var coord in coordinates)
            {
                if (coord.Count < 2)
                {
                    continue; // Skip invalid coordinate pairs
                }

                // Convert latitude and longitude to integers (multiply by 1e5 for precision)
                int lat = (int)(coord[0] * 1e5);
                int lng = (int)(coord[1] * 1e5);

                // Encode the difference from the previous point
                EncodeDifference(lat - prevLat, encodedPoints);
                EncodeDifference(lng - prevLng, encodedPoints);

                prevLat = lat;
                prevLng = lng;
            }

            return encodedPoints.ToString();
        }

        private static void EncodeDifference(int value, StringBuilder encoded)
        {
            // Step 1: Take the two's complement for negative values
            value = value < 0 ? ~(value << 1) : value << 1;

            // Step 2: Encode the value in chunks of 5 bits
            while (value >= 0x20)
            {
                int nextValue = (0x20 | (value & 0x1f)) + 63;
                encoded.Append((char)nextValue);
                value >>= 5;
            }

            // Step 3: Encode the final chunk
            value += 63;
            encoded.Append((char)value);
        }


        /// <summary>
        /// Decodes a Google Maps polyline string into a list of [lat, lng] coordinates.
        /// </summary>
        /// <param name="encodedPolyline">The encoded polyline string.</param>
        /// <returns>List of [lat, lng] pairs.</returns>
        public static List<List<double>> DecodePolyline(string encodedPolyline)
        {
            var coordinates = new List<List<double>>();
            if (string.IsNullOrEmpty(encodedPolyline))
            {
                return coordinates;
            }

            int index = 0;
            int lat = 0, lng = 0;

            while (index < encodedPolyline.Length)
            {
                // Decode latitude
                lat += DecodeValue(encodedPolyline, ref index);
                // Decode longitude
                lng += DecodeValue(encodedPolyline, ref index);

                // Convert back to double (divide by 1e5 for original precision)
                coordinates.Add(new List<double> { lat / 1e5, lng / 1e5 });
            }

            return coordinates;
        }

        private static int DecodeValue(string encoded, ref int index)
        {
            int value = 0;
            int shift = 0;
            int currentByte;

            do
            {
                if (index >= encoded.Length)
                {
                    return value; // Handle incomplete data gracefully
                }

                currentByte = encoded[index++] - 63;
                value |= (currentByte & 0x1f) << shift;
                shift += 5;
            } while (currentByte >= 0x20);

            // Reverse two's complement for negative values
            return ((value & 1) != 0 ? ~(value >> 1) : (value >> 1));
        }

        ///// <summary>
        ///// Converts encoded polyline to a Google Maps API Directions request URL.
        ///// </summary>
        ///// <param name="encodedPolyline">The encoded polyline string.</param>
        ///// <param name="apiKey">Google Maps API key.</param>
        ///// <returns>URL for Directions API with polyline as path.</returns>
        //public static string CreateDirectionsApiUrl(string encodedPolyline, string apiKey)
        //{
        //    if (string.IsNullOrEmpty(encodedPolyline) || string.IsNullOrEmpty(apiKey))
        //    {
        //        throw new ArgumentException("Encoded polyline and API key are required.");
        //    }

        //    // Example: Use polyline as part of a Directions API request
        //    // For simplicity, assume origin and destination are derived from polyline endpoints
        //    var url = $"https://maps.googleapis.com/maps/api/directions/json?" +
        //              $"path=enc:{Uri.EscapeDataString(encodedPolyline)}" +
        //              $"&key={Uri.EscapeDataString(apiKey)}";

        //    return url;
        //}
    }
}
