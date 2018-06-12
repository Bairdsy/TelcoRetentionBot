using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MongoDB.Bson;
using MongoDB.Driver;

using System.Diagnostics;

namespace MultiDialogsBot.Database
{
    /* This class is meant to be a singleton */

    [Serializable]
    public class MongoDBAccess
    {
        const int MAX_TIMES_SEEN = 3;

        static MongoDBAccess thisInstance = new MongoDBAccess();
        static IMongoClient mongoClient;
        static IMongoDatabase madCalmDB;

        /* ctor */
        private MongoDBAccess()
        {
            mongoClient = new MongoClient("mongodb://telcoretentiondb:HsQmjXjc0FBMrWYbJr8eUsGdWoTuaYXvdO2PRj13sxoPYijxxcxG5oSDfhFtVFWAFeWxFbuyf1NbxnFREFssAw==@telcoretentiondb.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            madCalmDB = mongoClient.GetDatabase("madcalm");
        }

        public static MongoDBAccess Instance
        {
            get
            {
                return thisInstance;
            }
        }


        /*
        public Dictionary<string,object> GetSubsNoImages(int anonSubsno)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            var imagesCollection = madCalmDB.GetCollection<BsonDocument>("images");
            var filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", anonSubsno);
            var cursor = imagesCollection.Find(filter).ToCursor();

            foreach(var doc in cursor.ToEnumerable())
            {
                string imageName = doc.GetElement("name").Value.ToString();
                string imageURL = doc.GetElement("Image").Value.ToString();
                   
                ret.Add(imageName, imageURL);
            }
            return ret;
        }*/

        

        public Dictionary<string,bool> GetAllBrands()
        {
            Dictionary<string, bool> ret = new Dictionary<string, bool>();
            var handSetCollection = madCalmDB.GetCollection<BsonDocument>("features_new2");    
        
            var cursor = handSetCollection.Find(new BsonDocument()).ToCursor();

            foreach (var doc in cursor.ToEnumerable())
            {
                string brand = doc.GetElement("Brand").Value.ToString();           

                if (!ret.Keys.Contains(brand))
                    ret[brand] = false;
            }
            return ret;
        }

        public HandSets GetCompleteHandSetData()
        {
            string brand, model;
            HandSets returnVal = new HandSets();
            HandSetFeatures currentNode = null;

            var featuresCollection = madCalmDB.GetCollection<BsonDocument>("features_new2");
            var brandsCollection = madCalmDB.GetCollection<BsonDocument>("brands");

            var brandCursor = brandsCollection.Find(new BsonDocument()).ToCursor();   
            var orderBy = Builders<BsonDocument>.Sort.Ascending("Brand");
            var orderBy2 = Builders<BsonDocument>.Sort.Ascending("Model");
            var featureCur = featuresCollection.Find(new BsonDocument()).Sort(orderBy).Sort(orderBy2).ToCursor();
            string currentBrand, currentModel;
            
            currentBrand = currentModel = "";

            foreach (var doc in featureCur.ToEnumerable())     
            {
                if (doc == null)
                    throw new Exception("Null exception!");
                brand = doc.GetElement("Brand").Value.ToString();
                
                model = doc.GetElement("Model").Value.ToString();
                
                if ((currentBrand != brand) || (currentModel != model))
                {
                    currentNode = new HandSetFeatures(doc);
                   
                    currentBrand = brand;
                    currentModel = model;
                    returnVal.Add(currentNode);
                }
                else
                    currentNode.Colors.Add(doc.GetElement("Color").Value.ToString());
            }
             

            foreach (var doc in brandCursor.ToEnumerable())
                returnVal.SetBrandLogo(doc.GetElement("brand").Value.ToString().ToLower(), doc.GetElement("imageURL").Value.ToString());

            return returnVal;
        }

        public string GetBrandOfModel(string model)
        {
            var handSetCollection = madCalmDB.GetCollection<BsonDocument>("features_new2");    
            var filter = Builders<BsonDocument>.Filter.Eq("Model", model);
            var firstDoc = handSetCollection.Find(filter).First();

            if (firstDoc.Count() == 0)
                return null;
            else
                return firstDoc.GetElement("Brand").Value.ToString();     
        }



        public Dictionary<string,bool> GetModels(string brand)   // null = to obtain all
        {
            Dictionary<string, bool> ret = new Dictionary<string, bool>();
            var handSetCollection = madCalmDB.GetCollection<BsonDocument>("features_new2");                   // handsets b4
            FilterDefinition<BsonDocument> filter;
            IAsyncCursor<BsonDocument> cursor;

            if (null == brand)
            {
                cursor = handSetCollection.Find(new BsonDocument()).ToCursor();     
            }
            else
            {
                filter = Builders<BsonDocument>.Filter.Eq("Brand", brand);             // Maker b4 
                cursor = handSetCollection.Find(filter).ToCursor();
            }
            foreach (var doc in cursor.ToEnumerable())
            {
                string model = doc.GetElement("Model").Value.ToString();

                ret.Add(model, true);
            }
            return ret;
        }

        public string GetHandSetImageURL(string manufacturer,string model)
        {
            var handSetCollection = madCalmDB.GetCollection<BsonDocument>("features_new2");                       // handsets b4
            var filterBuilder = Builders<BsonDocument>.Filter;
            var projectionFilterBuilder = Builders<BsonDocument>.Projection;
            var filter = filterBuilder.Eq("Brand", manufacturer) & filterBuilder.Eq("Model", model);            // Maker b4 
            /* var proj = projectionFilterBuilder.Include("Image").Exclude("_id");*/
            var proj = projectionFilterBuilder.Include("Image").Exclude("_id");
            var firstDoc = handSetCollection.Find(filter).Project(proj).First();

            if (firstDoc.Count() == 0)
                return null;  // Such combination of model and brand doesn't exist..
            else
                return firstDoc.GetElement(0).Value.ToString();   // I'm assuming we should never have more than one 
        }

        public Dictionary<string,object> GetOfferData(int anonSubscriber)
        {
            Dictionary<string, object> returnValue = new Dictionary<string, object>();
            var collection = madCalmDB.GetCollection<BsonDocument>("offers");
            var filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", anonSubscriber);
            var cursor = collection.Find(filter).ToCursor();
           
            int counter = 0;

            foreach (var doc in cursor.ToEnumerable())
            {
                Debug.WriteLine($"Doc #{counter}");
                foreach (var element in doc)
                {
                    Debug.WriteLine($"chave = {element.Name}, value = {element.Value}");
                    returnValue[element.Name] = element.Value;
                }
            }
            return returnValue;
        }

        /*
         * To Handle LUIS
         * utterances upgrade
         **/

        public bool WasSeen(string intent,string utterance2Add)
        {
            var utterancesCollection = madCalmDB.GetCollection<BsonDocument>("InitialLUISUtterances");
            var filter = Builders<BsonDocument>.Filter.Eq("utterance", utterance2Add);
            var filter2 = Builders<BsonDocument>.Filter.Eq("intent", intent);
            var cur = utterancesCollection.Find(filter & filter2).ToCursor();
            int numberOfTimesSeen = 0;

            foreach (var doc in cur.ToEnumerable())
            {
                try
                {
                    numberOfTimesSeen = int.Parse(doc.GetElement("count").Value.ToString());
                }
                catch (Exception xception)
                {
                    throw new Exception("Error...Could not parse the mumeric value in the document, something is very wrong! Error Message : " +xception.Message);
                }
            }
            if (numberOfTimesSeen++ == 0)
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
                {
                    {"intent" ,intent},
                    { "utterance",utterance2Add},
                    { "count",1.ToString() }
                };

                utterancesCollection.InsertOne(new BsonDocument(keyValuePairs));
            }
            else
            {
                var updateDb = Builders<BsonDocument>.Update.Set("count", numberOfTimesSeen);

                utterancesCollection.UpdateOne(filter & filter2,updateDb);
            }
            return numberOfTimesSeen >= MAX_TIMES_SEEN;
        }

        public List<Tuple<string,string>> GetFeatureRanking()
        {
            List<Tuple<string, string>> returnVal = new List<Tuple<string, string>>();
            string intent;
            string utterance;
            var featureFreqCollection = madCalmDB.GetCollection<BsonDocument>("FeatureFrequency");
            var orderBy = Builders<BsonDocument>.Sort.Descending("Frequency");
            var cur = featureFreqCollection.Find(new BsonDocument()).Sort(orderBy).ToCursor();

            foreach(var doc in cur.ToEnumerable())
            {
                intent = doc.GetElement("FeatureName").Value.ToString();
                utterance = doc.GetElement("Utterance").Value.ToString();
                returnVal.Add(new Tuple<string, string>(intent, utterance));
            }
            return returnVal;
        }

        private List<BsonDocument> GetFeaturesDocument(IMongoCollection<BsonDocument> featuresCollection, BsonDocument elements)
        {
            string brand = elements.GetElement("Maker").Value.ToString().ToLower(),
                   model = elements.GetElement("Model").Value.ToString().ToLower();
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("Brand", brand) & filterBuilder.Eq("Model", model);
            var cur = featuresCollection.Find(filter).ToCursor();
            List<BsonDocument> returnValue = new List<BsonDocument>();

            try
            {
                foreach (var doc in cur.ToEnumerable())
                    returnValue.Add( doc);
                return returnValue;
            }
            catch
            { return null; }
        }


        
    }
}