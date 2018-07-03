using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MultiDialogsBot.Database;

using System.Text;
     
namespace  MultiDialogsBot.Dialogs
{
    [Serializable]
    public abstract class CommonDialog : IDialog<object>     
    {
        public static bool debugMessages ;
     
        protected static HandSets handSets;
        MongoDBAccess mongoDBAccess = MongoDBAccess.Instance; 


        abstract public Task StartAsync(IDialogContext context);
        
        protected async Task PlanPricesButtonHandlerAsync(IDialogContext context, string modelPicked)
        {
            HandSetFeatures handSetFeatures;

            if (handSets == null)
                InitializeDataStruct();
            handSetFeatures = handSets.GetModelFeatures(modelPicked);

            var heroCard = new HeroCard()
            {
                Images = new List<CardImage> { new CardImage(handSetFeatures.MadCalmPicUrl + ".png", "img/jpeg") }
            };

            var message = context.MakeMessage();
            message.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(message);
        }

        protected string GetEquipmentImageURL(string model,bool madCalmPic)
        {
            if (handSets == null)
                InitializeDataStruct();

            return handSets.GetImageURL(model,madCalmPic);
        }

        protected List<string> GetTop5Sellers()
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.GetTop5Sales();
        }

        protected bool IsOldestOrNewest(string model)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.IsOldestOrNewest(model);
        }

        protected List<string> GetAllModels()
        {
            Dictionary<string, bool> bagOfAllModels;
            List<string> returnVal = new List<string>();

            if (handSets == null)
                InitializeDataStruct();
            bagOfAllModels = handSets.GetAllModels();
            foreach (var model in bagOfAllModels.Keys)
                returnVal.Add(model);
            return returnVal;
        }

        protected int GetModelCount()
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.GetHandSetCount();
        }

        protected Dictionary<string,bool> GetAllBrands()
        {
            Dictionary<string, bool> brandSet;

            if (handSets == null)
                 InitializeDataStruct();
            brandSet = handSets.GetAllBrands();

            return brandSet;
        }

        protected string GetModelBrand(string model)
        {
            if (handSets == null)
                InitializeDataStruct();

            return handSets.GetModelBrand(model);
        }

        protected DateTime GetModelReleaseDate(string model)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.GetModelReleaseDate(model);
        }

        protected string GetModelSpecsUrl(string model)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.GetSpecsUrl(model);
        }

        protected string GetModelReviewsUrl(string model)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.GetReviewsUrl(model);
        }

        protected Dictionary<string,bool> GetBrandModels(string brand)   // null to grab everything
        {
            if (handSets == null)
                InitializeDataStruct();
            if (brand == null)
                return handSets.GetAllModels();
            else
                return handSets.GetAllModelsForBrand(brand);
        }

        protected string TestMethod(int number)
        {
            string error = null;
            Dictionary<string, object> hash;
            StringBuilder dictContents = new StringBuilder();

            try
            {
                hash = mongoDBAccess.GetOfferData(number);
                foreach (string key in hash.Keys)
                    dictContents.Append($"'{key}' ==> '{hash[key]}'\r\n");
            }
            catch (Exception xception)
            {
                error = xception.Message;
            }
            return error ?? dictContents.ToString();
        }

        protected List<string> SelectWithFilter(List<string> regex_filters)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.SelectWithFilter(regex_filters);
        }

        protected List<string> GetColors(string model)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.GetModelColors(model);
        }

        protected bool IsBrandUnavailable(string brand)
        {
            if (handSets == null)
                InitializeDataStruct();
            return handSets.IsBrandUnavailable(brand);
        }

        private void InitializeDataStruct()
        {
            handSets = mongoDBAccess.GetCompleteHandSetData();
        }





        /*
        protected async Task StoreDocumentValues(IDialogContext context, BsonDocument document)
        {
            string resType = (string)document.GetElement("Result_Type").Value;

            context.ConversationData.SetValue("HandsetMakerKey", document.GetElement("HSet_Brand").Value);
            context.ConversationData.SetValue("HandsetModelKey", document.GetElement("HSet_Model").Value);
            context.ConversationData.SetValue("SubsNumber", document.GetElement("Anon_Subsno").Value);
            context.ConversationData.SetValue("CustNumber", document.GetElement("Cust_No").Value);
            context.ConversationData.SetValue("CurrentPlan", document.GetElement("Current_Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferPlan", document.GetElement("Offer_Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferVoice", document.GetElement("Offer_Voice_AddOn").Value);
            context.ConversationData.SetValue(resType + "_OfferText", document.GetElement("Offer_Text_AddOn").Value);
            context.ConversationData.SetValue(resType + "_OfferData", document.GetElement("Offer_Data_AddOn").Value);
            context.ConversationData.SetValue("CurrentRev", document.GetElement("Current_Revenue").Value);
            context.ConversationData.SetValue("CurrentMAF", document.GetElement("Current_MAF").Value);
            context.ConversationData.SetValue("CurrentRent", document.GetElement("Current_Rental").Value);
            context.ConversationData.SetValue("CurrentOvg", document.GetElement("Current_Overage").Value);
            context.ConversationData.SetValue(resType + "_OfferRev", document.GetElement("Offer_Revenue").Value);
            context.ConversationData.SetValue(resType + "_OfferMAF", document.GetElement("Offer_MAF").Value);
            context.ConversationData.SetValue(resType + "_OfferRent", document.GetElement("Offer_Rental").Value);
            context.ConversationData.SetValue(resType + "_OfferOvg", document.GetElement("Offer_Overage").Value);
            context.ConversationData.SetValue("Month1", document.GetElement("Analysed_Month_1").Value);
            context.ConversationData.SetValue("Month2", document.GetElement("Analysed_Month_2").Value);
            context.ConversationData.SetValue("Month3", document.GetElement("Analysed_Month_3").Value);
            context.ConversationData.SetValue(resType + "_Message1", document.GetElement("Sales_Message__1").Value);
            context.ConversationData.SetValue(resType + "_Message2", document.GetElement("Sales_Message__2").Value);
            context.ConversationData.SetValue(resType + "_Message3", document.GetElement("Sales_Message__3").Value);
            context.ConversationData.SetValue(resType + "_Message4", document.GetElement("Sales_Message__4").Value);
            context.ConversationData.SetValue(resType + "_Message5", document.GetElement("Sales_Message__5").Value);
            context.ConversationData.SetValue(resType + "_Message6", document.GetElement("Sales_Message__6").Value);
            context.ConversationData.SetValue(resType + "_Message7", document.GetElement("Sales_Message__7").Value);
            context.ConversationData.SetValue(resType + "_Message8", document.GetElement("Sales_Message__8").Value);
            context.ConversationData.SetValue(resType + "_Message9", document.GetElement("Sales_Message__9").Value);
            context.ConversationData.SetValue(resType + "_Message10", document.GetElement("Sales_Message__10").Value);
            context.ConversationData.SetValue(resType + "_Value1", document.GetElement("Reserved_Column_16").Value);
            context.ConversationData.SetValue(resType + "_Value2", document.GetElement("Reserved_Column_17").Value);
            context.ConversationData.SetValue(resType + "_Value3", document.GetElement("Reserved_Column_18").Value);
            context.ConversationData.SetValue(resType + "_Value4", document.GetElement("Reserved_Column_19").Value);
            context.ConversationData.SetValue(resType + "_Value5", document.GetElement("Reserved_Column_20").Value);
            context.ConversationData.SetValue(resType + "_Value6", document.GetElement("Reserved_Column_21").Value);
            context.ConversationData.SetValue(resType + "_Value7", document.GetElement("Reserved_Column_22").Value);
            context.ConversationData.SetValue(resType + "_Value8", document.GetElement("Reserved_Column_23").Value);
            context.ConversationData.SetValue(resType + "_Value9", document.GetElement("Reserved_Column_24").Value);
            context.ConversationData.SetValue(resType + "_Value10", document.GetElement("Reserved_Column_25").Value);
            context.ConversationData.SetValue(resType + "_BirthDate", document.GetElement("DATE_OF_BIRTH").Value);

            string Handset = document.GetElement("HSet_Brand").Value + " " + document.GetElement("HSet_Model").Value;
            context.ConversationData.SetValue("Handset", Handset);


        }*/

    }
}