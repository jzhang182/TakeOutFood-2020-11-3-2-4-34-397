namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            string ret = "============= Order details =============\n";
            var inputDict = new Dictionary<string, int>();
            var savingList = new List<string>();
            bool isUsingPromo = false;
            double total = 0.0;
            var itemDict = itemRepository.FindAll().ToDictionary(x=>x.Id, x=>x);
            var promoList = salesPromotionRepository.FindAll();
            foreach (var item in inputs)
            {
                var parsedStrings = item.Split(" ");
                inputDict.Add(parsedStrings[0], Convert.ToInt32(parsedStrings[2]));
                if (promoList[0].RelatedItems.Contains(parsedStrings[0])) 
                { 
                    isUsingPromo = true;
                    savingList.Add(parsedStrings[0]);
                }
            }

            foreach (var pair in inputDict)
            {
                double subtotal = itemDict[pair.Key].Price * pair.Value;
                total += subtotal;
                ret += $"{itemDict[pair.Key].Name} x {pair.Value} = {(int)subtotal} yuan\n";
            }
            ret += "-----------------------------------\n";

            if (isUsingPromo)
            {
                ret += "Promotion used:\n";

                double saving = 0.0;
                string savedItems = "";
                foreach (var id in savingList)
                {
                    saving += itemDict[id].Price * inputDict[id] * 0.5;
                    if(savingList.IndexOf(id) == 0)
                    {
                        savedItems += itemDict[id].Name;
                    }
                    else
                    {
                        savedItems += ", " + itemDict[id].Name;
                    }
                }

                total -= saving;
                ret += $"{promoList[0].DisplayName} ({savedItems}), saving {(int)saving} yuan\n";

                ret += "-----------------------------------\n";
            }

            ret += $"Total：{(int)total} yuan\n";
            ret += "===================================";
            return ret;
        }
    }
}
