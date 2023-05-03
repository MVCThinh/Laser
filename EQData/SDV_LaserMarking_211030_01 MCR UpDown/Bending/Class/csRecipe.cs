using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Bending
{
    public class csRecipe
    {
        public ConcurrentDictionary<eRecipe, double> Param = new ConcurrentDictionary<eRecipe, double>();
        //public Dictionary<eRecipe, double> Param = new Dictionary<eRecipe, double>();
        public string RecipeName;
        public string OldRecipeName;

        public csRecipe()
        {
            //디폴트 생성
            foreach (eRecipe s in Enum.GetValues(typeof(eRecipe)))
            {
                Param.TryAdd(s, 0); //초기화
                //Param.Add(s, 0); //초기화
            }
        }
    }
}
