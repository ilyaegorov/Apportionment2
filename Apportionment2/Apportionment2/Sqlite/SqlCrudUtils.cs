using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using SQLite;

namespace Apportionment2.Sqlite
{
    public static class SqlCrudUtils
    {
        public static Costs GetNewCost(string tripId)
        {
            return new Costs
            {
                id = Guid.NewGuid().ToString(),
                DateCreate = DateTime.Now.ToString(App.DateFormat),
                Sync = Utils.SyncStatus(tripId),
                TripId = tripId
            }; 
        }

        public static UserCostShares GetNewUserCostShare(string userId, string costId, string tripId)
        {
            UserCostShares newUserCostShare = new UserCostShares
            {
                id = Guid.NewGuid().ToString(),
                TripId = tripId,
                CostId = costId,
                UserId = userId,
                Share = 1,
                Sync = Utils.SyncStatus(tripId),
            };

            return newUserCostShare;
        }


        public static CostValues GetNewUserCostValue(string userId, string costId, string tripId, string currencyId)
        {
            CostValues newCostValue = new CostValues
            {
                id = Guid.NewGuid().ToString(),
                TripId = tripId,
                CostId = costId,
                UserId = userId,
                CurrencyId = currencyId,
                Value = 0,
                Sync = Utils.SyncStatus(tripId),
            };

            return newCostValue;
        }

        public static void DeleteCost(Costs cost)
        {
            var costShares = App.Database.Table<UserCostShares>().Where(n => n.CostId == cost.id);

            foreach (var costShare in costShares)
                Delete(costShare);

            var costValues = App.Database.Table<CostValues>().Where(n => n.CostId == cost.id);

            foreach (var costValue in costValues)
                Delete(costValue);

            Delete(cost);
        }

        public static void Save(object obj)
        {
            if (Exists(obj))
                App.Database.Update(obj);
            else
                App.Database.Insert(obj);
        }

        public static void Delete(object obj)
        {
            if (Exists(obj))
                App.Database.Delete(obj);
        }

        public static bool Exists(object obj)
        {
            Type objType = Orm.GetType(obj);
            int rowsAffected = 0;

            if (obj == null || objType == null)
                return false;

            var map = App.Database.GetMapping(objType);

            var pk = map.PK;

            if (pk == null)
                throw new NotSupportedException("Cannot find " + map.TableName + ": it has no PK");

            var cmdString = $"update {map.TableName} set Sync = Sync where id = '" +  pk.GetValue(obj) + "' ";
            var cmd = App.Database.CreateCommand(cmdString);

            try
            {
               rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {

                if (ex.Result == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode(App.Database.Handle) 
                    == SQLite3.ExtendedResult.ConstraintNotNull)
                {
                    throw NotNullConstraintViolationException.New(ex, map, obj);
                }

                throw ex;
            }

            if (rowsAffected > 0)
                return true;

            return false;
        }
    }
}
