using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text.RegularExpressions;

using MultiDialogsBot.Helper;

namespace MultiDialogsBot.Database
{

    [Serializable]
    public class HandSets
    {

        public delegate double accessor(HandSetFeatures handSetFeatures);

        [Serializable]
        class Models : IEnumerable
        {
            IEnumerator IEnumerable.GetEnumerator() { return models.Values.GetEnumerator();}

            Dictionary<string, HandSetFeatures> models;
            Dictionary<string, HandSetFeatures> spaceLess;

            public string BrandLogoURL { get; set; }

            public Models ()
            {
                models = new Dictionary<string, HandSetFeatures>();
                spaceLess = new Dictionary<string, HandSetFeatures>();
            }
            /*
            public Models(string brandImageURL) : this()
            {
                BrandLogoURL = brandImageURL;
            }
            */
            public void Add(HandSetFeatures newModel)
            {
                HandSetFeatures old;

                if (!models.TryGetValue(newModel.Model, out old))
                {
                    models.Add(newModel.Model, newModel);
                    spaceLess.Add(Miscellany.RemoveSpaces(newModel.Model), newModel);
                }
                else
                    newModel.CopyTo(old);
            }

            public Dictionary<string,bool> GetAllModels()
            {
                Dictionary<string, bool> returnVal = new Dictionary<string, bool>();

                foreach (var model in models.Keys)
                    if (models[model].OS != null)
                        returnVal.Add(model, false);
                return returnVal;
            }

            public List<string> GetTop5MostSoldModels()
            {
                List<string> returnVal = new List<string>();
                int maxSales,counter = 0,modelsCount ;
                HandSetFeatures  mostSold;
                string tail;

                while((returnVal.Count != models.Count) && (counter < 5))
                {
                    maxSales = -1;
                    mostSold = null;
                    foreach (HandSetFeatures handSet in models.Values)
                    {
                        if (!returnVal.Contains(handSet.Model) && handSet.SalesNumber >= maxSales)
                        {
                            mostSold = handSet;

                            maxSales = mostSold.SalesNumber;
                        }
                    }
                    returnVal.Add(mostSold.Model);
                    modelsCount = returnVal.Count;
                    if (modelsCount != 1)
                    {
                        tail = returnVal[modelsCount - 2];
                        if (models[tail].SalesNumber == maxSales)
                            continue; // Do not increment, we need five different
                    }
                    ++counter;
                }
                return returnVal;
            }

            public string GetSpecificModelBrand(string model)
            {
                HandSetFeatures features;

                if (models.TryGetValue(model, out features))
                    return features.Brand;
                //return models[model].Brand;
                if (spaceLess.TryGetValue(Miscellany.RemoveSpaces(model), out features))
                    return features.Brand;
                else
                    throw new Exception($"Error....I don't have the {model} model in my database");
            }

            public List<string> SelectWithRegEx(List<string> regex_filters)
            {
                Regex regex;
                List<string> returnVal = new List<string>();

                foreach (string filter in regex_filters)
                {
                    regex = new Regex(filter);
                    foreach (string model in models.Keys)
                    {
                        if (  regex.IsMatch(model.ToLower()))                 
                            returnVal.Add(model);
                    }
                }
                return  returnVal;
            }
             
            public HandSetFeatures GetEquipmentFeatures(string model)
            {
                HandSetFeatures features;

                if (models.TryGetValue(model, out features))
                    return features;
                //return models[model];
                if (models.TryGetValue(Miscellany.RemoveSpaces(model), out features))  
                    return features;
                else
                    throw new Exception($"Error...could not find the {model} model in database");
            }
        }

        Dictionary<string, Models> brands = new Dictionary<string, Models>();
        Models masterDict = new Models();
        List<string> unavailableBrands = new List<string>();
        List<HandSetFeatures> bag = new List<HandSetFeatures>();

        public void Add(HandSetFeatures newModel)
        {
            Models brandModels;


            if (!brands.TryGetValue(newModel.Brand, out brandModels))
            {
                brands.Add(newModel.Brand, new Models());
                brands[newModel.Brand].Add(newModel);
                masterDict.Add(newModel);
            }
            else
            {
                masterDict.Add(newModel);
                brandModels.Add(newModel);
            }
        }

        public bool IsBrandUnavailable(string unavailableBrand)
        {
            return unavailableBrands.Contains(unavailableBrand.ToLower());
        }

        public List<string> GetTop5Sales()
        {
            return masterDict.GetTop5MostSoldModels();
        }
        public int GetHandSetCount()
        {
            return GetAllModels().Count;
        }
        public Dictionary<string,bool> GetAllBrands()
        {
            Dictionary<string, bool> returnVal = new Dictionary<string, bool>();


            foreach (var brand in brands.Keys)
                returnVal.Add(brand, false);
            return returnVal;
        }

        public Dictionary<string,bool> GetAllModels()
        {
            return masterDict.GetAllModels();
        }

        public Dictionary<string,bool> GetAllModelsForBrand(string brand)
        {
            Models models; // = brands[brand];

            if (brands.TryGetValue(brand, out models))
            {
                return models.GetAllModels();
            }
            else
                return new Dictionary<string, bool>();
        }

        public bool IsOldestOrNewest(string model)
        {
            string brand = GetModelBrand(model);
            bool older, newer;
            Models brandModels = this.brands[brand];
            HandSetFeatures modelFeatures;

            newer = older = false;
            modelFeatures = brandModels.GetEquipmentFeatures(model);
            foreach (HandSetFeatures otherModelFeatures in brandModels)
            {
                if (otherModelFeatures.ReleaseDate < modelFeatures.ReleaseDate)
                    older = true;
                else if (otherModelFeatures.ReleaseDate > modelFeatures.ReleaseDate)
                    newer = true;
            }
            return !older || !newer;
        }

        public string GetModelBrand(string model)
        {
            // Search the master
            return masterDict.GetSpecificModelBrand(model);
        }

        public List<string> SelectWithFilter(List<string> filters)
        {
            return masterDict.SelectWithRegEx(filters);
        }

        public string GetImageURL(string model,bool madCalmImage)
        {
            HandSetFeatures handSetFeatures;

            handSetFeatures = masterDict.GetEquipmentFeatures(model);
            if (madCalmImage)
                return handSetFeatures.MadCalmPicUrl + "-1-recommended.png";
            else
                return handSetFeatures.PhonePictureUrl;
        }

        public string GetSpecsUrl(string model)
        {
            HandSetFeatures features;

            features = masterDict.GetEquipmentFeatures(model);
            return features.SpecsUrl;
        }

        public string GetReviewsUrl(string model)
        {
            HandSetFeatures features;

            features = masterDict.GetEquipmentFeatures(model);
            return features.ReviewsUrl;
        }

        public DateTime GetModelReleaseDate(string model)
        {
            HandSetFeatures handSetFeatures;

            handSetFeatures = masterDict.GetEquipmentFeatures(model);
            return handSetFeatures.ReleaseDate;
        }

        public double GetModelSize(string model)
        {
            HandSetFeatures features = masterDict.GetEquipmentFeatures(model);

            return Miscellany.Product(features.BodySize);
        }

        /***********************************************
         * To handle the bag of handsets               *
         * that we wannt to reduce more and more       *
         * via NodeLUISPhoneDialog                     *
         * (Branch 7 - Recommend a phone)              *
         *                                             *
         ***********************************************/
         
        public void InitializeBag(List<string> identifiedMatches)
        {
            HandSetFeatures handSetFeatures;

            bag.Clear();

            foreach (var model in identifiedMatches)
            {
                handSetFeatures = masterDict.GetEquipmentFeatures(model);
                if (handSetFeatures.OS != null)
                    bag.Add(handSetFeatures);
            }
        }
        public void InitializeBag(string brand2Filter, DateTime? releaseDate)
        {
            List<Models> listOfSetsOfModels = new List<Models>();
            DateTime release = releaseDate ?? new DateTime( 1980,1,1);
            string brand2Exclude ;

            bag.Clear();
            if (brand2Filter == null)
                listOfSetsOfModels.Add(masterDict);
            else if (brand2Filter[0] != '!')
                listOfSetsOfModels.Add(brands[brand2Filter]);
            else
            {
                brand2Exclude = brand2Filter.Substring(1);
                foreach (var brand in brands.Keys)
                    if (brand != brand2Exclude)
                        listOfSetsOfModels.Add(brands[brand]);
            }
            foreach (Models set in listOfSetsOfModels) 
                foreach (HandSetFeatures handset in set)
                    if ((handset.ReleaseDate > release) && (handset.OS != null))
                        bag.Add(handset);
        }



        public int BagCount()
        {
            return bag.Count;
        }

        public int EliminateFromBag(Predicate<HandSetFeatures> predicate)
        {
            return bag.RemoveAll(predicate);
        }

        public void removeAllButTop(int phones2Keep)
        {
            int total = bag.Count;

            if (total == phones2Keep)
                return;
            bag.RemoveRange(phones2Keep, total - phones2Keep );
            return;
        }

        public HandSetFeatures GetModelFeatures(string model)
        {
            return masterDict.GetEquipmentFeatures(model);
        }

        public void GetMaxAndMinLimits(accessor getter,out double min, out double max)
        {
            min = bag.Min(x => getter(x));
            max = bag.Max(x => getter(x));
        }

        public double ComputeMiddle(accessor getter)
        {
            return (bag.Max(x => getter(x)) + bag.Min(x => getter(x))) / 2;
        }
        public int KnockOutNumber(Predicate<HandSetFeatures> predicate)
        {
            return bag.Count(x =>  predicate(x));
        }

        public List<string> GetBagColors()
        {
            List<string> returnVal = new List<string>() ;

            foreach (var handset in bag)
                returnVal = new List<string>(returnVal.Union<string>(handset.Colors));

            return returnVal;
        }

        public List<string> GetBagOSes()
        {
            List<string> temp;
            IEnumerable<string> temp2;

            temp = new List<string>();
            foreach (var feature in bag)
                temp.Add(feature.OS);
            temp2 = temp.Distinct<string>();
            return new List<string>(temp2);
        }

        public List<string> GetBagBrands()
        {
            List<string> temp;
            IEnumerable<string> temp2;

            temp = new List<string>();
            foreach (var feature in bag)
                temp.Add(feature.Brand);
            temp2 = temp.Distinct();
            return new List<string>(temp2);
        }

        public double GetHighStandardThreshold(IComparer<HandSetFeatures> comparer,accessor getter)  
        {
            double min, max, temp;

            bag.Sort(comparer);   
            min = getter(bag[0]);
            max = getter(bag[bag.Count - 1]);
            if (min > max )
            {
                temp = min;
                min = max;
                max = temp;
                return (min + (max - min) / 100 * 70);
            }
            return (min + (max - min) / 100 * 30);
        }

        public string BuildStrRepFull()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (var brand in brands.Keys)
            {
                stringBuilder.Append("Brand : " + brand + "\r\nModels:\r\n");
                foreach (var model in brands[brand])
                    stringBuilder.Append("--> " + model);
            }

            foreach (var el in masterDict)
            {
                stringBuilder.Append(el.ToString());
                stringBuilder.Append("\r\n-----\\//------\r\n");
            }

            return stringBuilder.ToString();
        }

        public string BuildStrRep()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (var el in bag)
            {
                stringBuilder.Append(el.ToString());
                stringBuilder.Append("\r\n-----\\//------\r\n");
            }

            return  stringBuilder.ToString();
        }
        public int SortAndGetTop(IComparer<HandSetFeatures> comparer,accessor getter )
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("Contents");
            int counter = 0;
        
            bag.Sort(comparer);
 
            for (int i = 0; i < bag.Count; ++i)
            {
                double temp = getter(bag[i]);

                ++counter;
                if (counter >= BotConstants.MAX_CAROUSEL_CARDS)           // MAX+CAROUSEL_CARDS = minimum possible to show in caroussel but you've got to have at least 2 different
                {
                    if (((i + 1) >= bag.Count) || (getter(bag[i + 1]) != temp))
                    break;
                }
            }
            return counter;
        }

        public List<string> GetBagModels()
        {
            List<string> returnVal = new List<string>();

            foreach (var handset in bag)
                returnVal.Add(handset.Model);
            return returnVal;
        }

        public string GetBrandLogo(string brand)
        {
            string logo;

            logo = brands[brand].BrandLogoURL ?? "https://image.freepik.com/free-icon/not-available-abbreviation-inside-a-circle_318-33662.jpg";
            return logo;
        } 

        public List<string> GetModelColors(string model)
        {
            HandSetFeatures handSetFeatures;

            handSetFeatures = masterDict.GetEquipmentFeatures(model);
            return handSetFeatures.Colors;
        }

        public void SetBrandLogo(string brand,string url)
        {
            Models theBrandModels;

            if (brands.TryGetValue(brand, out theBrandModels))
                theBrandModels.BrandLogoURL = url;
            else
                unavailableBrands.Add(brand.ToLower());
        }
    }
}