using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System.Linq;

namespace Humica.Logic.CF
{

    public partial class CFNumberRank
    {
        private SMSystemEntity DB = new SMSystemEntity();
        HumicaDBContext DBX = new HumicaDBContext();
        public string POType { get; set; }
        public BSNumberRank Number { get; set; }
        public BSNumberRankItemN NumberItem { get; set; }
        public BSNumberRankNY NumberItemHasIdentity { get; set; }
        public BSNumberRankItem NumberItemHasIdentityYear { get; set; }
        public CFDocType DocConf { get; set; }
        public string Indentification { get; set; }
        public string NextNumberRank { get; set; }
        public int NextNumber { get; set; }

        public string CompanyCode { get; set; }

        public string Separated { get; set; }

        public CFNumberRank(string DocType)
        {
            try
            {
                DocConf = DB.CFDocTypes.Find(DocType);
                Number = DB.BSNumberRanks.Find(DocConf.NumberRank);
                NumberItem = DB.BSNumberRankItemNs.Find(DocConf.NumberRank, DocConf.NumberRankItem);

                if (NumberItem != null)
                {
                    if (NumberItem.Status == 0)
                    {
                        NumberItem.Status = NumberItem.FromNumber;
                    }
                    else
                    {
                        NumberItem.Status = NumberItem.Status + 1;
                    }
                    DB.Entry(NumberItem).State = System.Data.Entity.EntityState.Modified;
                    int row = DB.SaveChanges();
                    if (row == 1)
                    {
                        this.Separated = NumberItem.Separated;
                        this.NextNumber = NumberItem.Status;
                        NextNumberRank = this.getLenOfPrefix(Number.Length, NumberItem.Status);
                        NextNumberRank = NumberItem.Prefix + NumberItem.Separated + NextNumberRank;
                    }
                }
            }
            catch
            {
                NextNumberRank = null;
            }
        }
        public CFNumberRank(string DocType, bool IsRunNext = false)
        {

            var DocConf = DBX.HREmpCodes.Find(DocType);
            Number = DB.BSNumberRanks.Find(DocConf.NumberRank);
            NumberItem = DB.BSNumberRankItemNs.Find(DocConf.NumberRank, DocConf.NumberRankItem);

            if (NumberItem != null)
            {
                if (NumberItem.Status == 0)
                {
                    NumberItem.Status = NumberItem.FromNumber;
                }
                else
                {
                    NumberItem.Status = NumberItem.Status - 1;
                }
                DB.Entry(NumberItem).State = System.Data.Entity.EntityState.Modified;
                int row = DB.SaveChanges();
            }
        }
        public CFNumberRank(string DocType, DocConfType Doc, bool IsRunNext = false)
        {
            try
            {
                if (Doc == DocConfType.Normal)
                {
                    DocConf = DB.CFDocTypes.Find(DocType);
                    Number = DB.BSNumberRanks.Find(DocConf.NumberRank);
                    NumberItem = DB.BSNumberRankItemNs.Find(DocConf.NumberRank, DocConf.NumberRankItem);
                }
                if (Doc == DocConfType.EmpCode)
                {
                    var objDoc = DBX.HREmpCodes.Find(DocType);
                    Number = DB.BSNumberRanks.Find(objDoc.NumberRank);
                    NumberItem = DB.BSNumberRankItemNs.Find(objDoc.NumberRank, objDoc.NumberRankItem);
                }
                if (Doc == DocConfType.GL)
                {
                    var objDoc = DBX.HRJournalTypes.Find(DocType);
                    Number = DB.BSNumberRanks.Find(objDoc.NumberRank);
                    NumberItem = DB.BSNumberRankItemNs.Find(objDoc.NumberRank, objDoc.NumberRankItem);
                }
                if (IsRunNext == true)
                {
                    if (NumberItem != null)
                    {
                        if (NumberItem.Status == 0)
                        {
                            NumberItem.Status = NumberItem.FromNumber;
                        }
                        else
                        {
                            NumberItem.Status = NumberItem.Status + 1;
                        }
                        DB.Entry(NumberItem).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                        if (row == 1)
                        {
                            this.Separated = NumberItem.Separated;
                            this.NextNumber = NumberItem.Status;
                            NextNumberRank = this.getLenOfPrefix(Number.Length, NumberItem.Status);
                            NextNumberRank = NumberItem.Prefix + NumberItem.Separated + NextNumberRank;
                        }
                    }
                }
            }
            catch
            {
                NextNumberRank = null;
            }
        }
        public CFNumberRank(string DocType, string ScreenID)
        {
            try
            {
                var objDoc = DBX.ExCfWorkFlowItems.FirstOrDefault(w => w.ScreenID == ScreenID && w.DocType == DocType);
                Number = DB.BSNumberRanks.Find(objDoc.NumberRank);
                NumberItem = DB.BSNumberRankItemNs.Find(objDoc.NumberRank, objDoc.NumberRankItem);

                if (NumberItem != null)
                {
                    if (NumberItem.Status == 0)
                    {
                        NumberItem.Status = NumberItem.FromNumber;
                    }
                    else
                    {
                        NumberItem.Status = NumberItem.Status + 1;
                    }
                    DB.Entry(NumberItem).State = System.Data.Entity.EntityState.Modified;
                    int row = DB.SaveChanges();
                    if (row == 1)
                    {
                        this.Separated = NumberItem.Separated;
                        this.NextNumber = NumberItem.Status;
                        NextNumberRank = this.getLenOfPrefix(Number.Length, NumberItem.Status);
                        NextNumberRank = NumberItem.Prefix + NumberItem.Separated + NextNumberRank;
                    }
                }
            }
            catch
            {
                NextNumberRank = null;
            }
        }
        public CFNumberRank(string DocType, string ScreenID, bool IsRunNext = false)
        {

            var objDoc = DBX.ExCfWorkFlowItems.Find(ScreenID, DocType);
            Number = DB.BSNumberRanks.Find(objDoc.NumberRank);
            NumberItem = DB.BSNumberRankItemNs.Find(objDoc.NumberRank, objDoc.NumberRankItem);

            if (NumberItem != null)
            {
                if (NumberItem.Status == 0)
                {
                    NumberItem.Status = NumberItem.FromNumber;
                }
                else
                {
                    NumberItem.Status = NumberItem.Status - 1;
                }
                DB.Entry(NumberItem).State = System.Data.Entity.EntityState.Modified;
                int row = DB.SaveChanges();
            }
        }

        public CFNumberRank(string DocType, string CompanyCode, DocConfType Doc, int Year, bool IsRunNext = false)
        {
            DocConf = DB.CFDocTypes.Find(DocType);
            this.CompanyCode = CompanyCode;
            Number = DB.BSNumberRanks.Find(DocConf.NumberRank);
            NumberItemHasIdentityYear = DB.BSNumberRankItems.Find(DocConf.NumberRank, DocConf.NumberRankItem, CompanyCode, Year);
            if (IsRunNext == true)
            {
                if (NumberItemHasIdentityYear != null)
                {
                    if (NumberItemHasIdentityYear.Status == 0)
                    {
                        NumberItemHasIdentityYear.Status = NumberItemHasIdentityYear.FromNumber;
                    }
                    else
                    {
                        NumberItemHasIdentityYear.Status = NumberItemHasIdentityYear.Status + 1;
                    }
                    DB.Entry(NumberItemHasIdentityYear).State = System.Data.Entity.EntityState.Modified;
                    int row = DB.SaveChanges();
                    if (row == 1)
                    {
                        this.Separated = NumberItemHasIdentityYear.Separated;
                        this.NextNumber = NumberItemHasIdentityYear.Status;
                        NextNumberRank = this.getLenOfPrefix(Number.Length, NumberItemHasIdentityYear.Status);
                        NextNumberRank = NumberItemHasIdentityYear.Prefix + NumberItemHasIdentityYear.Separated + NextNumberRank;
                    }
                }
            }
        }
        public CFNumberRank(string DocType, DocConfType Doc, int Period, bool IsRunNext = false)
        {
            try
            {
                if (Doc == DocConfType.Normal)
                {
                    DocConf = DB.CFDocTypes.Find(DocType);
                    Number = DB.BSNumberRanks.Find(DocConf.NumberRank);
                    if (Number.IsFlagYear == false && Number.IsHasIndentification == false)
                    {
                        NumberItem = DB.BSNumberRankItemNs.Find(DocConf.NumberRank, DocConf.NumberRankItem);
                    }
                    else if (Number.IsFlagYear == false && Number.IsHasIndentification == true)
                    {
                        NumberItemHasIdentity = DB.BSNumberRankNies.Find(DocConf.NumberRank, DocConf.NumberRankItem, SYConstant.DEFAULT_PLANT);
                    }
                    else if (Number.IsFlagYear == true && Number.IsHasIndentification == true)
                    {
                        NumberItemHasIdentityYear = DB.BSNumberRankItems.Find(DocConf.NumberRank, DocConf.NumberRankItem, SYConstant.DEFAULT_PLANT, Period);
                    }
                }

                if (Doc == DocConfType.FixedAsset)
                {
                    Number = DB.BSNumberRanks.Find(DocType);
                    if (Number.IsFlagYear == false && Number.IsHasIndentification == false)
                    {
                        NumberItem = DB.BSNumberRankItemNs.Find(DocType, "10");
                    }
                    else if (Number.IsFlagYear == false && Number.IsHasIndentification == true)
                    {
                        NumberItemHasIdentity = DB.BSNumberRankNies.Find(DocType, "10", SYConstant.DEFAULT_PLANT);
                    }
                    else if (Number.IsFlagYear == true && Number.IsHasIndentification == true)
                    {
                        NumberItemHasIdentityYear = DB.BSNumberRankItems.Find(DocType, "10", SYConstant.DEFAULT_PLANT, Period);
                    }
                }
                if (IsRunNext == true)
                {
                    //no company & no year
                    if (NumberItem != null)
                    {
                        if (NumberItem.Status == 0)
                        {
                            NumberItem.Status = NumberItem.FromNumber;
                        }
                        else
                        {
                            NumberItem.Status = NumberItem.Status + 1;
                        }
                        DB.Entry(NumberItem).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                        if (row == 1)
                        {
                            Separated = NumberItem.Separated;
                            NextNumber = NumberItem.Status;
                            NextNumberRank = getLenOfPrefix(Number.Length, NumberItem.Status);
                            NextNumberRank = NumberItem.Prefix + NumberItem.Separated + NextNumberRank;
                        }
                    }
                    //has company & no year
                    if (NumberItemHasIdentity != null)
                    {
                        if (NumberItemHasIdentity.Status == 0)
                        {
                            NumberItemHasIdentity.Status = NumberItemHasIdentity.FromNumber;
                        }
                        else
                        {
                            NumberItemHasIdentity.Status = NumberItemHasIdentity.Status + 1;
                        }
                        DB.Entry(NumberItemHasIdentity).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                        if (row == 1)
                        {
                            Separated = NumberItemHasIdentity.Separated;
                            NextNumber = NumberItemHasIdentity.Status;
                            NextNumberRank = getLenOfPrefix(Number.Length, NumberItemHasIdentity.Status);
                            NextNumberRank = NumberItemHasIdentity.Prefix + NumberItemHasIdentity.Separated + NextNumberRank;
                        }
                    }
                    //has company & has year
                    if (NumberItemHasIdentityYear != null)
                    {
                        if (NumberItemHasIdentityYear.Status == 0)
                        {
                            NumberItemHasIdentityYear.Status = NumberItemHasIdentityYear.FromNumber;
                        }
                        else
                        {
                            NumberItemHasIdentityYear.Status = NumberItemHasIdentityYear.Status + 1;
                        }
                        DB.Entry(NumberItemHasIdentityYear).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                        if (row == 1)
                        {
                            Separated = NumberItemHasIdentityYear.Separated;
                            NextNumber = NumberItemHasIdentityYear.Status;
                            NextNumberRank = getLenOfPrefix(Number.Length, NumberItemHasIdentityYear.Status);
                            NextNumberRank = NumberItemHasIdentityYear.Prefix + NumberItemHasIdentityYear.Separated + NextNumberRank;
                        }
                    }
                }
            }
            catch
            {
                NextNumberRank = null;
            }
        }
        public string getLenOfPrefix(int len, int currentNumber)
        {
            string r = "";
            for (int i = currentNumber.ToString().Length; i < len; i++)
            {
                r = r + "0";
            }
            r = r + currentNumber.ToString();
            return r;
        }
    }
    public enum DocConfType
    {
        Normal, EmpCode, GL, FixedAsset
    }

}