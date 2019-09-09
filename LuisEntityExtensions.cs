using System;
using System.Collections.Generic;
using System.Linq;

namespace MCSC
{
    internal static class LuisEntityExtensions
    {
        public static LuisV2Entity SelectTopScore(this IEnumerable<LuisV2Entity> entities, string type)
        {
            LuisV2Entity result = null;
            double? topScore = null;
            foreach(var entity in entities.Where(item => item.Type == type))
            {
                if(entity.Score > topScore || topScore == null)
                {
                    topScore = entity.Score;
                    result = entity;
                }
            }
            return result;
        }

        public static int? SelectTopScoreInt(this IEnumerable<LuisV2Entity> entities, string type)
        {
            int? result = null;
            double? topScore = null;
            foreach(var entity in entities.Where(item => item.Type == type))
            {
                if(int.TryParse(entity.Entity, out var temp) && (entity.Score > topScore || topScore == null))
                {
                    topScore = entity.Score;
                    result = temp;
                }
            }
            return result;
        }

        public static DateTime? SelectTopScoreDateTime(this IEnumerable<LuisV2Entity> entities, string type)
        {
            DateTime? result = null;
            double? topScore = null;
            foreach(var entity in entities.Where(item => item.Type == type))
            {
                DateTime? temp;
                try
                {
                    var span = new Chronic.Core.Parser().Parse(entity.Entity);
                    temp = span.Start;
                }
                catch(Exception)
                {
                    temp = null;
                }
                
                if(temp != null && (entity.Score > topScore || topScore == null))
                {
                    topScore = entity.Score;
                    result = temp;
                }
            }
            return result;
        }
    }
}