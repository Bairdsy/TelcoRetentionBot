using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MongoDB.Bson;
using MongoDB.Driver;


namespace MultiDialogsBot.Database
{
    [Serializable]
    public class HandSetFeatures
    {
         Dictionary<string, string> keyValuePairs;

        public string Brand { get; set; }
        public string Model { get; set; }
        public string ImageURL { get; set; }
        /**************************************************************************************************************************************/

        public bool Connectivity_4G { get; private set; }     // BandWidth

        public int BatteryLife { get; private set; }

        public double Camera { get; private set; }                // in MegaPixels

        public Dictionary<string, double> Price { get; private set; }

        public bool HasFMRadio { get; private set; }

        public double[] DisplayResolution { get; private set; }
        public string OS { get; private set; }

        public int MemoryMB { get; private set; }

        public List<string> Colors { get; set; }

        public bool DualCamera { get; private set; }

        public DateTime ReleaseDate { get; private set; }

        public bool DualSIM { get; private set; }

        public bool ExpandableMemory { get; private set; }

        public bool FaceId { get; private set; }

        public bool GPS { get; private set; }

        public bool WiFi { get; private set; }

        public bool HDVoice { get; private set; }

        public double ScreenSize { get; private set; }

        public bool SecondaryCamera { get; private set; }

        public double[] BodySize { get; private set; }

        public bool WaterResist { get; private set; }

        public int Weight {get;private set;}

        public bool IsSmartphone
        {
            get; private set;
        }

        public string MadCalmPicUrl
        {
            get;private set;
        }

        public string PhonePictureUrl { get; private set; }

        public void CopyTo(HandSetFeatures other)
        {
            other.Brand = Brand;
            other.Model = Model;
            other.ImageURL = ImageURL;    
        }
            

       // public HandSetFeatures(BsonDocument bsonElements,List<BsonDocument> features = null) : this()    
        public HandSetFeatures(BsonDocument bsonElements) : this()
        {
            string key,data;
            DateTime result;
            int batteryLifeInHours;
            double cameraResolutionMPixels;
            int weight,xRes,yRes,megs,gigs;
            double price,res;


        /*    Brand = bsonElements.GetElement("Brand").Value.ToString(); // bsonElements.GetElement("Maker").Value.ToString()  ;
            Model = bsonElements.GetElement("Model").Value.ToString()  ;*/
            ImageURL = "https://image.freepik.com/free-icon/not-available-abbreviation-inside-a-circle_318-33662.jpg"; // bsonElements.GetElement("Image").Value.ToString(); Vai mudar quando passarmos para a  nova
            keyValuePairs = new Dictionary<string, string>();

            /*  if ((features != null) && (features.Count != 0))
              {*/
            int counter = 0;
            foreach (var element in /*features[0]*/ bsonElements)
            {
                ++counter;
                if (element == null)
                    throw new Exception($"the element #{counter} is null");
                if (element.Name == null)
                    throw new Exception($"the element #{counter} has a null name");
                if (element.Value == null)
                    throw new Exception($"The eleement #{counter} has a null value");
                key = element.Name.ToString();
                data = element.Value.ToString();
                keyValuePairs.Add(key, element.Value.ToString());
                
                if (key == "Brand")
                {
                    Brand = data;
                }
                else if (key == "Model")
                {
                    Model = data;
                }
                else if ((key == "Release date") && DateTime.TryParse(data, out result))   // Release date
                {
                    ReleaseDate = result;
                }
                else if (key == "FM Radio")      // FM Radio
                    HasFMRadio = "No" != data;
                else if (key == "4G Connectivity")
                    Connectivity_4G = data == "Yes";
                else if (key.StartsWith("Battery life") && int.TryParse(data, out batteryLifeInHours))
                    BatteryLife = batteryLifeInHours;
               else if ((key.StartsWith("Camera") && double.TryParse(data, out cameraResolutionMPixels)))
                {
                    Camera = cameraResolutionMPixels;
                }
                else if (key.StartsWith("Be You") && double.TryParse(data, out price))
                {
                    Price.Add(key, price);
                }
                else if (key == "Color")
                {
                    Colors.Add(data.ToLower());
                }
                else if (key == "Dual Camara")
                {
                    DualCamera = "No" != data;
                }
                else if (key == "Dual SIM")
                    DualSIM = "No" != data;
                else if (key == "Expandable memory")
                    ExpandableMemory = "No" != data;
                else if (key == "Face Id")
                    FaceId = "Yes" == data;
                else if (key == "GPS / Wifi")
                {
                    GPS = "Yes" == data;
                    WiFi = "Yes" == data;
                }
                else if (key == "HD Voice")
                    HDVoice = "Yes" == data;
                else if (key == "Resolution")    // Screen resolution
                {
                    string[] tokens = data.Split(' ');
                    bool xCoordinateValid, yCoordinateValid;

                    if (tokens.Length >= 3)
                    {
                        xCoordinateValid = int.TryParse(tokens[0], out xRes);
                        yCoordinateValid = int.TryParse(tokens[2], out yRes);

                        if (xCoordinateValid && yCoordinateValid && (tokens[1].ToLower() == "x"))
                            DisplayResolution = new double[] { xRes, yRes };
                    }
                }
                else if (key == "Memory (GB)")
                {
                    int index;
                    if ((index = data.IndexOf("MB")) != -1)
                    {
                        if (int.TryParse(data.Substring(0, index), out megs))
                            MemoryMB = megs;
                    }
                    else if (int.TryParse(data, out gigs))
                    {
                        MemoryMB = 1024 * gigs;
                    }
                }
                else if (key == "OS")
                {
                    OS = data;
                }
                else if (key.StartsWith("Screen size "))
                {
                    if (double.TryParse(data, out res))
                    {
                        ScreenSize = res;
                    }
                }
                else if (key.StartsWith("Secondary Camera"))
                {
                    SecondaryCamera = "No" != data;
                }
                else if (key.StartsWith("Dimensions"))    // We expect it in the form a x b x c
                {
                    string[] tokens = data.ToLower().Split('x');

                    if (tokens.Length >= 3)
                    {
                        for (int i = 0; i < 3; ++i)
                            if (double.TryParse(tokens[i], out res))
                                BodySize[i] = res;
                            else
                                BodySize[i] = 1000;
                    }
                }
                else if (key.Equals("Water resistance"))
                {
                    WaterResist = (data.ToLower() == "yes");
                }
                else if (key.StartsWith("Weight"))
                {
                    if (int.TryParse(data, out weight))
                    {
                        Weight = weight;
                    }                                    
                }
                else if (key.Equals("Type"))
                {
                    IsSmartphone = ("Smartphone" == data);
                } 
                else if (key.Equals("MadCalm-Picture"))
                {
                    MadCalmPicUrl = data;
                }
                else if (key == "Picture URL")
                {
                    PhonePictureUrl = data;
                }
            }
        }

        HandSetFeatures()
        {
            ReleaseDate = new DateTime(2017, 1, 1);
            BatteryLife = 0;
            Camera = 0;
            Price = new Dictionary<string, double>();
            DisplayResolution = new double[] { 2, 2 };
            MemoryMB = 1;  // Just one MB
            ScreenSize = 1; // Just one inch
            BodySize = new double[]{ 1000,1000,1000};  // one cubic meter
            Colors = new List<string>();
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder($"Brand = {Brand} , Model = {Model}");



            if (this.keyValuePairs != null)
            {
                foreach (var k in keyValuePairs.Keys)
                    sb.Append($"{k} ==> {keyValuePairs[k]}" + "\r\n");
                sb.Append("\r\nBattery life = " + BatteryLife +"\r\n");
            }
            else
                sb.Append("No features\r\n");
            sb.Append("Available in the following colours:");
            foreach (var color in Colors)
                sb.Append(color + ";");
            sb.Append(string.Concat("Dimensions : ", (BodySize[0] * BodySize[1] * BodySize[2]), "\r\n"));
            return sb.ToString();
        }
    }
}