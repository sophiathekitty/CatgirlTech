﻿/*
    This file is part of Station Science.

    Station Science is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Station Science is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Station Science.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * i have modified the namespace and class name so that i can use it for my resource manager module.
 * i'm new to c# development and don't know if this is necessary or not. but best be safe than sorry
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CatgirlTech // ~sophia edit~ namespace for my projecet
{
    class CgTResourceHelper // ~sophia edit~ CatgirlTech preface just in case. i'm guessing namespace should make this unneccessary?
    {
        public static PartResource getResource(Part part, string name)
        {
            PartResourceList resourceList = part.Resources;
            return resourceList.list.Find(delegate(PartResource cur)
            {
                return (cur.resourceName == name);
            });
        }

        public static double getResourceAmount(Part part, string name)
        {
            PartResource res = getResource(part, name);
            if (res == null)
                return 0;
            return res.amount;
        }

        public static PartResource setResourceMaxAmount(Part part, string name, double max)
        {
            PartResource res = getResource(part, name);
            if (res == null && max > 0)
            {
                ConfigNode node = new ConfigNode("RESOURCE");
                node.AddValue("name", name);
                node.AddValue("amount", 0);
                node.AddValue("maxAmount", max);
                res = part.AddResource(node);
            }
            else if (res != null && max > 0)
            {
                res.maxAmount = max;
            }
            else if (res != null && max <= 0)
            {
                part.Resources.list.Remove(res);
            }
            return res;
        }

        public static double getResourceDensity(string name)
        {
            var resDef = PartResourceLibrary.Instance.resourceDefinitions["Bioproducts"];
            if (resDef != null)
                return resDef.density;
            return 0;
        }
        private static double sumDemand(List<PartResource> list)
        {
            double ret = 0;
            foreach (PartResource pr in list)
            {
                if(pr.flowState)
                    ret += (pr.maxAmount - pr.amount);
            }
            return ret;
        }

        public static double getDemand(Part part, string name)
        {
            var res_set = new List<PartResource>();
            var res_def = PartResourceLibrary.Instance.GetDefinition(name);
            if (res_def == null) return 0;
            //part.GetConnectedResources(res_def.id, res_def.resourceFlowMode, res_set); // ~sophia edit~ compiler says it needs flowMode in the middle?
            //part.GetConnectedResources(res_def.id, res_set);
            if (res_set == null) return 0;
            return sumDemand(res_set);
        }

        private static double sumAvailable(List<PartResource> list)
        {
            double ret = 0;
            foreach (PartResource pr in list)
            {
                if(pr.flowState)
                    ret += pr.amount;
            }
            return ret;
        }

        public static double getAvailable(Part part, string name)
        {
            var res_set = new List<PartResource>();
            var res_def = PartResourceLibrary.Instance.GetDefinition(name);
            if (res_def == null) return 0;
            //part.GetConnectedResources(res_def.id, res_def.resourceFlowMode, res_set); // ~sophia edit~ compiler says it needs flowMode in the middle?
            //part.GetConnectedResources(res_def.id, res_set);
            if (res_set == null) return 0;
            return sumAvailable(res_set);
        }

        public static double requestResourcePartial(Part part, string name, double amount)
        {
            if (amount > 0)
            {
                //UnityEngine.MonoBehaviour.print(name + " request: " + amount);
                double taken = part.RequestResource(name, amount);
                //UnityEngine.MonoBehaviour.print(name + " request taken: " + taken);
                if (taken >= amount * .99999)
                    return taken;
                double available = getAvailable(part, name);
                //UnityEngine.MonoBehaviour.print(name + " request available: " + available);
                double new_amount = Math.Min(amount, available) * .99999;
                //UnityEngine.MonoBehaviour.print(name + " request new_amount: " + new_amount);
                if (new_amount > taken)
                    return taken + part.RequestResource(name, new_amount - taken);
                else
                    return taken;
            }
            else if (amount < 0)
            {
                //UnityEngine.MonoBehaviour.print(name + " request: " + amount);
                double taken = part.RequestResource(name, amount);
                //UnityEngine.MonoBehaviour.print(name+" request taken: " + taken);
                if (taken <= amount * .99999)
                    return taken;
                double available = getDemand(part, name);
                //UnityEngine.MonoBehaviour.print(name + " request available: " + available);
                double new_amount = Math.Max(amount, available) * .99999;
                //UnityEngine.MonoBehaviour.print(name + " request new_amount: " + new_amount);
                if (new_amount < taken)
                    return taken + part.RequestResource(name, new_amount - taken);
                else
                    return taken;
            }
            else
                return 0;
        }
    }
}