using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bovision.Import;

namespace Bovision
{   
    public class Search
    {
        public Query Query { get; private set; }
        public bool SortAndPage { get; private set; }
        public Pager<ImportEstate> Pager;

        public Search(Query q, bool SortAndPage = true)
        {
            this.Query = q;
            this.SortAndPage = SortAndPage;

            var res = PerformSearch(Data.Estates);
            if (SortAndPage)
            {
                List<SortProperty> props = new List<SortProperty>(Query.Sorting);
                props.Add(last);

                res = Sorter<ImportEstate>.Sort(res, props);
                Pager = new Pager<ImportEstate>(res, Query.Page, Query.ItemsPerPage == 0 ? 20 : Query.ItemsPerPage);
                res = Pager.Result;
            }
            result = res;
        }
        private static SortProperty last = new SortProperty("Id", SortDirection.Descend);
        private List<ImportEstate> result = null;
        public List<ImportEstate> Result
        {
            get
            {
                return result;
            }
        } 
        private List<ImportEstate> PerformSearch(IEnumerable<ImportEstate> list)
        {
            if (Query.AgentIds.Count > 0)
            {
                if (Query.AgentIds.Count == 1)
                {
                    var aid = Query.AgentIds[0];
                    list = list.Where(e => e.AgentId == aid);
                }
                else
                {
                    var aids = new HashSet<int>(Query.AgentIds);
                    list = list.Where(e => aids.Contains(e.AgentId));
                }
            }
            if(Query.Active != 0)
                list = list.Where(est => est.Active == (Query.Active == 1));
            foreach( var type in Query.EstateTypes)
            {
                var c = type.EstateType.Contract;
                var t = type.EstateType.Type;
                list = list.Where(est =>
                   (c == EstateType.ContractType.Unknown || c == est.Contract) &&
                   (t == EstateType.ObjectType.Unknown || t == est.Type));
                list = list.Where(est =>
                   (c == EstateType.ContractType.Unknown || c == est.Contract) &&
                   (t == EstateType.ObjectType.Unknown || t == est.Type));
            }
            if(Query.EstateAreas.Count > 0)
            {
                var areas = Query.EstateAreas.Select(id => EstateArea.Get(id)).Where(ea => ea != null);
                list = list.Where(delegate(ImportEstate e)
                {
                    foreach (var area in areas)
                    {
                        Type t = area.FullId.Type.Type;
                        if (t == typeof(Municipality))
                        {
                            if (area.Id == 899)
                            {
                                if (e.MunicipalityId == 840 || e.MunicipalityId == 885)
                                    return true;
                            }
                            else
                                if (e.MunicipalityId == area.Id)
                                    return true;
                        }
                        else if (t == typeof(Country))
                        {
                            var c = Country.ByISO2Code(e.CountryId);
                            if ((c != null && (c.Id == area.Id || area.FullId.Id == 0 && c.ISO2 != "SE")))
                                return true;
                        }
                        else if (t == typeof(County))
                        {
                            var m = Municipality.ById(e.MunicipalityId);
                            if( m != null && m.County.Id == area.Id)
                                return true;
                        }
                        else
                        {
                            if (area.BoundingBox.IsPointInShape(e.LatLong) && area.AreaPolygon.IsPointInShape(e.LatLong))
                                return true;
                        }
                    }
                    return false;
                });
            }
            if (!Query.Price.Empty)
                list = list.Where(e => Query.Price.InRange(e.Price));
            if (!Query.Rent.Empty)
                list = list.Where(e => Query.Rent.InRange(e.Rent));
            if (!Query.Rooms.Empty)
                list = list.Where(e => Query.Rooms.InRange(e.Rooms));
            if (!Query.LivingArea.Empty)
                list = list.Where(e => Query.LivingArea.InRange(e.UsableArea));
            if (!Query.FloorsInBuilding.Empty)
                list = list.Where(e => Query.FloorsInBuilding.InRange(e.FloorsInBuilding));
            if (!Query.Floor.Empty)
                list = list.Where(e => Query.Floor.InRange(e.Floor));
            if (Query.Elevator == 1)
                list = list.Where(e => e.HasElevator);
            else if(Query.Elevator == -1)
                list = list.Where(e => !e.HasElevator);

            return list.ToList();
        }
    }
}
