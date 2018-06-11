using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using MultiDialogsBot.Database;
using MultiDialogsBot.Dialogs;
 


namespace MultiDialogsBot.Helper
{
    [Serializable]
    public class IntentDecoder : IComparer<HandSetFeatures>
    {
        readonly Dictionary<NodeLUISPhoneDialog.EIntents, Predicate<HandSetFeatures>> intentFilters;
        readonly Dictionary<NodeLUISPhoneDialog.EIntents, sgn> comparingFunctions;
        readonly Dictionary<NodeLUISPhoneDialog.EIntents, HandSets.accessor> getters;
        readonly Dictionary<NodeLUISPhoneDialog.EIntents, Predicate<HandSetFeatures>> booleanFilters;

        delegate int sgn(Tuple<HandSetFeatures,HandSetFeatures> x);
        delegate int NumberOfDifferent();

        struct KnockOutIntent : IComparable
        {
            public NodeLUISPhoneDialog.EIntents intent;
            public int numberOfKnockOuts;

            public KnockOutIntent(NodeLUISPhoneDialog.EIntents a,int b)
            {
                intent = a;
                numberOfKnockOuts = b;
            }

            public int CompareTo(object obj)
            {
                KnockOutIntent theOther = (KnockOutIntent)obj;
                return -numberOfKnockOuts.CompareTo(theOther.numberOfKnockOuts);
            }
        }

        public HandSets PhonesLeft { get { return handSets; } }
        public List<NodeLUISPhoneDialog.EIntents> Exclude
        {
            get
            {
                return intents2Exclude;
            }
        }

        List<NodeLUISPhoneDialog.EIntents> intents2Exclude;
        HandSets handSets;
        NodeLUISPhoneDialog.EIntents intent;
        double Threshold { get;  set; }
        
        bool desc;   // Order by DESC/ASC
        List<string> StrKeyWords { get; set; }
        DateTime DateThreshold { get; set; }




        public IntentDecoder(HandSets hand_sets,string brand, DateTime? releaseDate,List<string> identifiedMatches)
        {
            handSets = hand_sets;
            intents2Exclude = new List<NodeLUISPhoneDialog.EIntents>();
            if (identifiedMatches == null)
                handSets.InitializeBag(brand, releaseDate);
            else
                handSets.InitializeBag(identifiedMatches);
            intentFilters = new Dictionary<NodeLUISPhoneDialog.EIntents, Predicate<HandSetFeatures>>() {
                { NodeLUISPhoneDialog.EIntents.BatteryLife,x => x.BatteryLife < Threshold},
                { NodeLUISPhoneDialog.EIntents.Camera,y => y.Camera < Threshold },
                { NodeLUISPhoneDialog.EIntents.HighResDisplay,y => Prod(y.DisplayResolution) < Threshold },
                { NodeLUISPhoneDialog.EIntents.LargeStorage,y => y.MemoryMB < Threshold  },
                { NodeLUISPhoneDialog.EIntents.ScreenSize, y => !desc ^ (y.ScreenSize < Threshold) },
                { NodeLUISPhoneDialog.EIntents.Cheap, y => y.Price["Be You 60"] > Threshold },
                { NodeLUISPhoneDialog.EIntents.Small, y => desc ^ (Prod(y.BodySize) > Threshold) },
                { NodeLUISPhoneDialog.EIntents.Weight, y => y.Weight > Threshold },
                { NodeLUISPhoneDialog.EIntents.Color, y => !y.Colors.Exists(x => StrKeyWords.Contains(x.ToLower())) },     
                { NodeLUISPhoneDialog.EIntents.OS, y =>  !StrKeyWords.Contains(y.OS.ToLower())  },
                { NodeLUISPhoneDialog.EIntents.Brand, y => !StrKeyWords.Contains(y.Brand.ToLower())   },
                { NodeLUISPhoneDialog.EIntents.Newest, y => y.ReleaseDate < DateThreshold }
            };  
            comparingFunctions = new Dictionary<NodeLUISPhoneDialog.EIntents, sgn>()
            {
                { NodeLUISPhoneDialog.EIntents.BatteryLife, x => -Math.Sign(x.Item1.BatteryLife - x.Item2.BatteryLife)},
                { NodeLUISPhoneDialog.EIntents.Camera,x =>  -Math.Sign(x.Item1.Camera - x.Item2.Camera)},
                { NodeLUISPhoneDialog.EIntents.HighResDisplay,x => -Math.Sign(Prod(x.Item1.DisplayResolution) - Prod(x.Item2.DisplayResolution)) },
                { NodeLUISPhoneDialog.EIntents.LargeStorage,y => -Math.Sign(y.Item1.MemoryMB - y.Item2.MemoryMB)   },
                { NodeLUISPhoneDialog.EIntents.ScreenSize, y => (desc ? -1 : 1) * Math.Sign(y.Item1.ScreenSize - y.Item2.ScreenSize) },
                { NodeLUISPhoneDialog.EIntents.Cheap, y => Math.Sign(y.Item1.Price["Be You 60"] - y.Item2.Price["Be You 60"]) },
                { NodeLUISPhoneDialog.EIntents.Small, y => (desc ? -1 : 1) * Math.Sign(Prod(y.Item1.BodySize) - Prod(y.Item2.BodySize)) },
                { NodeLUISPhoneDialog.EIntents.Weight, y => Math.Sign(y.Item1.Weight - y.Item2.Weight)  },
                { NodeLUISPhoneDialog.EIntents.Newest, y => (y.Item2.ReleaseDate > y.Item1.ReleaseDate) ? 1 : -1 },
            };

            getters = new Dictionary<NodeLUISPhoneDialog.EIntents, HandSets.accessor>()
            {
                { NodeLUISPhoneDialog.EIntents.BatteryLife, x => x.BatteryLife},    
                { NodeLUISPhoneDialog.EIntents.Camera,x =>  x.Camera},
                { NodeLUISPhoneDialog.EIntents.HighResDisplay,x => Prod(x.DisplayResolution ) },
                { NodeLUISPhoneDialog.EIntents.LargeStorage,y => y.MemoryMB    },
                { NodeLUISPhoneDialog.EIntents.ScreenSize, y => y.ScreenSize  },
                { NodeLUISPhoneDialog.EIntents.Cheap, y => y.Price["Be You 60"]  },
                { NodeLUISPhoneDialog.EIntents.Small, y => Prod(y.BodySize)  },
                { NodeLUISPhoneDialog.EIntents.Weight, y => y.Weight  },
                { NodeLUISPhoneDialog.EIntents.Newest, y => y.ReleaseDate.Ticks  },
            };

            booleanFilters = new Dictionary<NodeLUISPhoneDialog.EIntents, Predicate<HandSetFeatures>>()
            {
                { NodeLUISPhoneDialog.EIntents.BandWidth,x => !x.Connectivity_4G },
                { NodeLUISPhoneDialog.EIntents.FMRadio, x => !x.HasFMRadio },
                { NodeLUISPhoneDialog.EIntents.DualCamera, x => !x.DualCamera },
                { NodeLUISPhoneDialog.EIntents.DualSIM, x => !x.DualSIM}, 
                { NodeLUISPhoneDialog.EIntents.ExpandableMemory, x => !x.ExpandableMemory },
                { NodeLUISPhoneDialog.EIntents.FaceID, x => !x.FaceId },
                { NodeLUISPhoneDialog.EIntents.GPS, x => !x.GPS },
                { NodeLUISPhoneDialog.EIntents.WiFi, x => !x.WiFi }, 
                { NodeLUISPhoneDialog.EIntents.HDVoice, x => ! x.HDVoice },
                { NodeLUISPhoneDialog.EIntents.SecondaryCamera, y => !y.SecondaryCamera },
                { NodeLUISPhoneDialog.EIntents.WaterResist, x => !x.WaterResist },
            };
        }

        int IComparer<HandSetFeatures>.Compare(HandSetFeatures x, HandSetFeatures y)
        {
            Tuple<HandSetFeatures, HandSetFeatures> tuple;

            tuple = new Tuple<HandSetFeatures, HandSetFeatures>(x, y);
            return comparingFunctions[intent](tuple);
        }

        public int CurrentNumberofHandsetsLeft()
        {
            return handSets.BagCount();
        }

        /* For debugging purposes */
        public string GetBagStrRep()
        {
            return handSets.BuildStrRep();
        }

        public bool KnocksSomeButNotAll(NodeLUISPhoneDialog.EIntents desiredFeature)
        {
            Dictionary<NodeLUISPhoneDialog.EIntents,NumberOfDifferent> enumerated = new Dictionary<NodeLUISPhoneDialog.EIntents, NumberOfDifferent>()
            {
                { NodeLUISPhoneDialog.EIntents.Color , () => handSets.GetBagColors().Count },
                { NodeLUISPhoneDialog.EIntents.Brand , () => handSets.GetBagBrands().Count},
                {  NodeLUISPhoneDialog.EIntents.OS, () => handSets.GetBagOSes().Count }
            };
            Predicate<HandSetFeatures> predicate;
            int count = handSets.BagCount();
            int knockOutNumber;

            if (booleanFilters.TryGetValue(desiredFeature,out predicate))  // It's boolean
            {
                knockOutNumber = handSets.KnockOutNumber(predicate);
                return (knockOutNumber != count) && (knockOutNumber != 0);
            }
            else if (intentFilters.TryGetValue(desiredFeature,out predicate))
            {
                if (!enumerated.ContainsKey(desiredFeature))
                {
                    double highStandardThreshold; 

                    intent = desiredFeature;
                    highStandardThreshold = handSets.GetHighStandardThreshold(this, getters[desiredFeature]);
                    if (desiredFeature == NodeLUISPhoneDialog.EIntents.Newest)
                        DateThreshold = new DateTime((long)highStandardThreshold);
                    else
                        Threshold = highStandardThreshold;
                    knockOutNumber = handSets.KnockOutNumber(predicate);
                    return (knockOutNumber != count) && (knockOutNumber != 0);
                }
                else
                {
                    return (enumerated[desiredFeature])() != 1;
                }

            }
            throw new Exception("Error...received a feature I don't know about:" + desiredFeature.ToString());
        }

        public int DecodeIntent(NodeLUISPhoneDialog.EIntents intent2Decode, List<string> keywords,bool orderByDesc)
        {
            intent = intent2Decode;
            if (booleanFilters.ContainsKey(intent2Decode))
                return DecodeIntent(intent2Decode, null);
            if ((intent2Decode == NodeLUISPhoneDialog.EIntents.Brand) ||
                (intent2Decode == NodeLUISPhoneDialog.EIntents.OS)    ||
                (intent2Decode == NodeLUISPhoneDialog.EIntents.Color))    // enumerated
            {
                StrKeyWords = keywords;
                return DecodeIntent(intent2Decode, null);
            }
            else  // it can only be numeric
            {
                if (intent2Decode == NodeLUISPhoneDialog.EIntents.Small)  // This situation needs a special case
                    desc = orderByDesc;
                if (intent2Decode == NodeLUISPhoneDialog.EIntents.Newest) // Convert it back to DateTime
                    DateThreshold = new DateTime((long)handSets.GetHighStandardThreshold(this, getters[intent2Decode]));
                else
                    Threshold = handSets.GetHighStandardThreshold(this, getters[intent2Decode]);
                DecodeIntent(intent2Decode, null);  // Eliminates the medium and low standard
                Threshold = -1;
                DateThreshold = new DateTime(1980, 1, 1);
                return DecodeIntent(intent2Decode, null);
            }
        }
        public int DecodeIntent(NodeLUISPhoneDialog.EIntents intent2Decode,LuisResult result, System.Text.StringBuilder  stringDeDebug = null )
        {
            int handSetsLeft;   
            Predicate<HandSetFeatures> predicate;

            intent = intent2Decode;
            if (result != null)
            {
                Threshold = -1;
                DateThreshold = new DateTime(1980, 1, 1);
                ExtractEntityInfo(intent2Decode, result);
            }
            intents2Exclude.Add(intent2Decode);
            if (intentFilters.TryGetValue(intent2Decode,out predicate))   // it is numeric or date or string
            {
                if ((intent2Decode == NodeLUISPhoneDialog.EIntents.Color) || (intent2Decode == NodeLUISPhoneDialog.EIntents.OS) || (intent2Decode == NodeLUISPhoneDialog.EIntents.Brand))  
                    // strings, enumerated
                {
                    if ((StrKeyWords == null) ||(StrKeyWords.Count == 0))
                        throw new Exception("Error...Either color, brand or OS selected and no string was supplied");
                    if (handSets.KnockOutNumber(predicate) == handSets.BagCount())
                        return 0;
                    handSets.EliminateFromBag(predicate);
                    return handSets.BagCount();
                }
                else
                    if ((Threshold != -1) || (DateThreshold != new DateTime(1980,1,1)))
                    {
                        if (stringDeDebug != null)
                            stringDeDebug.Append($" The threshold extracted is : {Threshold}");
                        if (handSets.KnockOutNumber(predicate) == handSets.BagCount())
                            return 0;
                        handSets.EliminateFromBag(predicate);
                        return handSets.BagCount();
                    }
                    else   // Subs said he wants the best of a certain feature without specifying a value
                    {
                        if (result != null)
                        {
                            if (NodeLUISPhoneDialog.EIntents.Newest == intent2Decode)
                                DateThreshold = new DateTime((long)handSets.GetHighStandardThreshold(this, getters[intent2Decode]));
                            else
                                Threshold = handSets.GetHighStandardThreshold(this, getters[intent2Decode]);
                            handSets.EliminateFromBag(predicate);
                        }
                        handSetsLeft = handSets.SortAndGetTop(this, getters[intent2Decode]);
                        if (handSetsLeft == 0)
                            return 0;
                        handSets.removeAllButTop(handSetsLeft);
                        return handSetsLeft;
                    }
            }
            /*** still here ? Boolean situation ****/
            if (booleanFilters.TryGetValue(intent2Decode, out predicate))
            {
                if (handSets.BagCount() == handSets.KnockOutNumber(predicate))
                    return 0;
                handSets.EliminateFromBag(predicate);
                return handSets.BagCount();
            }
            throw new Exception("IntentDecoder : Error...Intent not recongized");
        }
        
        private double Prod(IEnumerable<double> vector)
        {
            double returnValue = 1;

            foreach (var factor in vector)
                returnValue *= factor;
            return returnValue;
        }

        private List<string> GetSpecificBrands(LuisResult res)
        {
            List<string> returnVal = new List<string>();
            string ent;

            foreach (var entity in res.Entities)
                if ((entity.Type == "Brand") && (entity.Entity.ToUpper() != "BRAND"))
                {
                    ent = entity.Entity.ToLower();
                    returnVal.Add(ent == "iphone" ? "apple" : ent);
                }
            return returnVal;
        }

        private void GetDateThreshold (LuisResult result)
        {
            DateTime dateTime;

            foreach (var entity in result.Entities)
                if (entity.Type.StartsWith("builtin.datetimeV2") && DateTime.TryParse(entity.Entity,out dateTime))
                {
                    DateThreshold = dateTime;
                    break;
                }
        }

        private void GetBatteryLifeComposedEntityData(LuisResult result )
        {
            bool ret = false;
            int hours = 0;

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "BatteryLifeComposite")
                {
                    foreach (var child in cEntity.Children)
                        if ((child.Type == "builtin.number") && (int.TryParse(child.Value, out hours)))
                        {
                            ret = true;
                            break;
                        }
                    if (ret)
                        break;
                }
            if (hours != 0)
                Threshold = (double)hours;
        }

        private void GetWeightCompositeEntity(LuisResult res )
        {
            string aux;
            int unitIndex;
            bool found = false;
            double weight = 0;

            foreach (var cEntity in res.CompositeEntities)
                if (cEntity.ParentType == "WeightComposite")
                {
                    foreach (var child in cEntity.Children)
                    {
                        aux = child.Value.ToLower();
                        unitIndex = aux.IndexOf('g');
                        if ((child.Type == "builtin.dimension") && double.TryParse(aux.Substring(0,unitIndex), out weight))
                        {
                            found = true;
                            Threshold = weight;
                            break;
                        }
                    }
                    if (found)
                        return ;
                }
            return  ;
        }

        private void GetHighResDisplayCompositeData(LuisResult result)
        {
            string[] tokens;
            int nFactors = 0;
            double[] factors = new double[2];

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "DisplayComposite")
                {
                    foreach (var child in cEntity.Children)
                        if ((child.Type == "builtin.number"))
                        {
                            if (double.TryParse(child.Value,out factors[nFactors]))
                            {
                                if (++nFactors == 2)
                                {
                                    Threshold = factors[0] * factors[1];
                                    return;
                                }
                            }
                        }
                }
            foreach (var entity in result.Entities)
                if (entity.Type == "ScreenResolutionRegEx")
                {
                    tokens = entity.Entity.Split('x');
                    if (tokens.Length != 2) return;
                    if (double.TryParse(tokens[0],out factors[0]) && double.TryParse(tokens[1],out factors[1]))
                        Threshold = factors[0] * factors[1];
                }
        }

        private void GetScreenSizeCompositeEntityData(LuisResult result)
        {
            string entityValue;
            double inches;
            int unitsIndex;
            desc = true;  // If s/he says nothing, assume s/he wants the biggest possible screen size

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "ScreenSizeComposite")
                {
                    foreach (var child in cEntity.Children)
                        if (child.Type == "builtin.dimension")
                        {
                            entityValue = child.Value;
                            unitsIndex = entityValue.IndexOf('i'); 
                            if (-1 == unitsIndex)
                                continue;
                            if (double.TryParse(entityValue.Substring(0, unitsIndex), out inches))
                            {
                                Threshold = inches;
                            }
                            else
                                continue;
                        }
                        else if (child.Type == "OrderByWay")
                        {
                            desc = "small" != child.Value.ToLower();
                        }
                    return;
                }
        }
        private void GetCameraCompositeEntityData(LuisResult result)
        {
            double megaPixels = 0;

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "cameracomposite")
                {
                    foreach (var child in cEntity.Children)
                        if ((child.Type == "builtin.number") && double.TryParse(child.Value, out megaPixels))
                        {
                            Threshold = megaPixels;
                            return  ;
                        }
                }
            return ;
        }

        private void GetMemoryCompositeEntityData(LuisResult result)
        {
            string entityContents;
            int memory,gbIndex = -1,mbIndex = -1;

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "MemoryComposite")
                    foreach (var child in cEntity.Children)
                        if (child.Type == "builtin.dimension")
                        {
                            entityContents = child.Value.ToLower();
                            if ((-1 == (gbIndex = entityContents.IndexOf("gb"))) && (-1 == (mbIndex = entityContents.IndexOf("mb")))) return;
                            if ((mbIndex == -1)  // Gigs
                                && int.TryParse(entityContents.Substring(0, gbIndex), out memory))
                            {
                                Threshold = memory * 1024;
                                return;
                            }
                            else   // Megs
                            {
                                if (int.TryParse(entityContents.Substring(0,mbIndex),out memory))
                                {
                                    Threshold = memory;
                                    return;
                                }
                            }
                        }
        }

        private void ExtractPhoneSizeInfo(LuisResult result)
        {
            desc = false;  // By default, ascending
            string[] tokens;
            int index = 0;
            double[] volume = new double[3];

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "SizeComposite")
                {
                    foreach (var child in cEntity.Children)
                        switch (child.Type) 
                        {
                            case "OrderByWay":
                                desc = ("small" != child.Value.ToLower() && ("smallest" != child.Value.ToLower()));
                                break;
                            case "buildin.number":
                                if ((index < 3) && double.TryParse(child.Value, out volume[index]))
                                {
                                    ++index;  
                                }
                                break;     
                            case "DimensionsRegEx":
                                if (index >= 3)
                                    continue; // We already have the info we need about the desired volume threshold
                                tokens = child.Value.ToLower().Split('x');
                                if (double.TryParse(tokens[0], out volume[0]) && double.TryParse(tokens[1], out volume[1]) && double.TryParse(tokens[2], out volume[2]))
                                    index = 3;
                                break;
                            default:
                                break;
                        }
                    if (index == 3)  // OK, we have valid data
                    {
                        Threshold = Prod(volume);
                    }
                }
        }
        
        private bool GetOSData(LuisResult result)
        {
            List<string> subsOSChoices = new List<string>();

            foreach (var entity in result.Entities)
                if (entity.Type == "OperatingSystem")
                    subsOSChoices.Add(entity.Entity.ToLower());

            StrKeyWords = subsOSChoices;
            return StrKeyWords.Count != 0;
        }
             
        private void GetPreferredColors(LuisResult result)
        {
            List<string> colorVector = new List<string>();

            foreach (var entity in result.Entities)
                if (entity.Type == "Color")
                    colorVector.Add(entity.Entity.ToLower());
            StrKeyWords = colorVector;
        }

        private void ExtractEntityInfo (NodeLUISPhoneDialog.EIntents intent,LuisResult result)
        {
            switch (intent)
            {
                case NodeLUISPhoneDialog.EIntents.BatteryLife:
                    GetBatteryLifeComposedEntityData(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.Brand:
                    StrKeyWords = GetSpecificBrands(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.Camera:
                    GetCameraCompositeEntityData(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.HighResDisplay:
                    GetHighResDisplayCompositeData(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.LargeStorage:
                    GetMemoryCompositeEntityData(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.OS:
                    GetOSData(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.ScreenSize:
                    GetScreenSizeCompositeEntityData(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.Small:
                    ExtractPhoneSizeInfo(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.Weight:
                    GetWeightCompositeEntity(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.Color:
                    GetPreferredColors(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.Newest:
                    GetDateThreshold(result);
                    break;
                case NodeLUISPhoneDialog.EIntents.BandWidth:
                case NodeLUISPhoneDialog.EIntents.DualCamera:
                case NodeLUISPhoneDialog.EIntents.DualSIM:
                case NodeLUISPhoneDialog.EIntents.ExpandableMemory:
                case NodeLUISPhoneDialog.EIntents.FMRadio:
                case NodeLUISPhoneDialog.EIntents.FaceID:
                case NodeLUISPhoneDialog.EIntents.GPS:
                case NodeLUISPhoneDialog.EIntents.HDVoice:
                case NodeLUISPhoneDialog.EIntents.SecondaryCamera:
                case NodeLUISPhoneDialog.EIntents.WaterResist:
                case NodeLUISPhoneDialog.EIntents.WiFi:
                    break;
                default:
                    break;
            }
        }

        public List<NodeLUISPhoneDialog.EIntents> IntentsRanking(System.Text.StringBuilder sb )
        {
            List<KnockOutIntent> vector = new List<KnockOutIntent>();
            List<NodeLUISPhoneDialog.EIntents> returnValue = new List<NodeLUISPhoneDialog.EIntents>();
            int knockOutNumber, top4,index ;

            /*** Boolean processing ***/
            foreach (var intent in booleanFilters.Keys)
            {
                if (intents2Exclude.Contains(intent))
                    continue;
                knockOutNumber = handSets.KnockOutNumber(booleanFilters[intent]);
                sb.Append($"{intent.ToString()} knocks out {knockOutNumber}\r\n");
                vector.Add(new KnockOutIntent(intent,knockOutNumber));
            }

            /**** numeric processing ****/
            foreach (var intent in intentFilters.Keys)
            {
                double middle;
                List<string> colorsOnBag;
                List<List<string>> listOfCombinations;
                int maxWeight,weight,parcels,acc,x;

                if (  intents2Exclude.Contains(intent))
                    continue;
                switch (intent)
                {
                    case NodeLUISPhoneDialog.EIntents.Small:
                    case NodeLUISPhoneDialog.EIntents.OS:
                    case NodeLUISPhoneDialog.EIntents.Brand:
                        vector.Add(new KnockOutIntent(intent, handSets.BagCount() / 2));
                        sb.Append($"{intent.ToString()} knocks out {handSets.BagCount() / 2}\r\n");
                        break;
                    case NodeLUISPhoneDialog.EIntents.Color: // Special treatment
                        colorsOnBag = handSets.GetBagColors();
                        maxWeight = colorsOnBag.Count;
                        listOfCombinations = GetAllCombinations(colorsOnBag);
                        sb.Append("Number of colors : " + colorsOnBag.Count);
                        sb.Append("Number of combinations : " + listOfCombinations.Count);
                        sb.Append("Combination:");
                        acc = parcels = 0;
                        foreach (var combination in listOfCombinations)
                        {
                            StrKeyWords = combination;
                            weight = (maxWeight + 1 - combination.Count);
                            parcels += weight;
                            x = handSets.KnockOutNumber(intentFilters[NodeLUISPhoneDialog.EIntents.Color]);
                            acc += x * weight;
                        }
                        vector.Add(new KnockOutIntent(NodeLUISPhoneDialog.EIntents.Color,acc / parcels));
                        sb.Append("Knocks out " + (acc / parcels));
                        break;
                    default:
                        middle = handSets.ComputeMiddle(getters[intent]);
                        Threshold = middle;
                        knockOutNumber = handSets.KnockOutNumber(intentFilters[intent]);  
                        sb.Append($"{intent.ToString()} knocks out {knockOutNumber}\r\n");
                        vector.Add(new KnockOutIntent(intent, knockOutNumber));    
                        break;
                }
            }
            vector.Sort();
            /*
             * We need to remove the ones
             * that are at the head of the vector and 
             * will knock out the full bag. We also need
             * to remove the ones that won't touch the bag...if they
             * are before the fourth...
             *
             */
            index = 0; // head
            while ((index < vector.Count) && (vector[index].numberOfKnockOuts == handSets.BagCount()))
                ++index;
            top4 = Math.Min(vector.Count, index + 4);
            for (int i = index; i < top4; ++i)
            {
                if (vector[i].numberOfKnockOuts == 0)
                    break;
                returnValue.Add(vector[i].intent);
            }
            return returnValue;
        }

        private List<List<string>> GetAllCombinations(List<string> colors)
        {
            List<string> temp;
            string first = colors[0];
            List<List<string>> aux,returnValue = new List<List<string>>
            {
                new List<string>{ first },
            };
            int len = colors.Count;

            if (len != 1)
            {
                aux = GetAllCombinations(colors.GetRange(1, len - 1));
                foreach (var list in aux)
                {
                    temp = new List<string>(list);
                    temp.Insert(0, first);
                    returnValue.Add(temp);
                }
                returnValue = new List<List<string>>(returnValue.Concat(aux));
            }
            return returnValue;
        }
    }
}