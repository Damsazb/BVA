using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public class EstateType
    {
        public enum ContractType { Sale, TenantOwnership, TenancyRights, SubTenancyRights, StudentTenancy, CompanyLet, None, Unknown }
        public enum ObjectType { House, Townhouse, Cottage, Flat, Farm, Lot, Parking, Unknown }

        public ObjectType Type = ObjectType.Unknown;
        public ContractType Contract = ContractType.Unknown;


        private static OneToMany<ObjectType, ContractType> map = new OneToMany<ObjectType, ContractType>();
        private static Dictionary<string, ObjectType> otypes = new Dictionary<string, ObjectType>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, ContractType> ctypes = new Dictionary<string, ContractType>(StringComparer.OrdinalIgnoreCase);
        static EstateType()
        {
            map.Add(ObjectType.House, ContractType.Sale, ContractType.TenantOwnership, ContractType.TenancyRights, ContractType.SubTenancyRights, ContractType.StudentTenancy, ContractType.CompanyLet);
            map.Add(ObjectType.Townhouse, ContractType.Sale, ContractType.TenantOwnership, ContractType.TenancyRights, ContractType.SubTenancyRights, ContractType.StudentTenancy, ContractType.CompanyLet);
            map.Add(ObjectType.Cottage, ContractType.Sale, ContractType.TenantOwnership, ContractType.TenancyRights, ContractType.SubTenancyRights, ContractType.StudentTenancy, ContractType.CompanyLet);
            map.Add(ObjectType.Flat, ContractType.Sale, ContractType.TenantOwnership, ContractType.TenancyRights, ContractType.SubTenancyRights, ContractType.StudentTenancy, ContractType.CompanyLet);
            map.Add(ObjectType.Farm, ContractType.Sale, ContractType.TenancyRights, ContractType.SubTenancyRights, ContractType.StudentTenancy, ContractType.CompanyLet);
            map.Add(ObjectType.Lot, ContractType.Sale);
            map.Add(ObjectType.Parking, ContractType.TenancyRights);
            map.Keys.ForEach(k => otypes[k.ToString()] = k);
            map.AllValues.ForEach(v => ctypes[v.ToString()] = v);
        }
        public static ContractType GetContractType(string name)
        {
            ContractType c;
            if (ctypes.TryGetValue(name, out c))
                return c;
            return ContractType.Unknown;
        }
        public static ObjectType GetObjectType(string name)
        {
            ObjectType o;
            if (otypes.TryGetValue(name, out o))
                return o;
            return ObjectType.Unknown;
        }
        public static EstateType GetEstateType(string objecttype, string contract)
        {
            return new EstateType() { Type = GetObjectType(objecttype), Contract = GetContractType(contract) };
        }
        public static EstateType GetEstateType(ObjectType objecttype, ContractType contract)
        {
            return new EstateType() { Type = objecttype, Contract = contract };
        }
        public override string ToString()
        {
            return this.Type.ToString() + ":" + this.Contract.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var v = obj as EstateType;
            if ((object)v == null)
                return false;
            return Type == v.Type && Contract == v.Contract;
        }
        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Contract.GetHashCode();
        }
        public bool Equals(EstateType v)
        {
            if ((object)v == null)
                return false;
            return Type == v.Type && Contract == v.Contract;
        }
        public static bool operator ==(EstateType a, EstateType b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Type == b.Type && a.Contract == b.Contract;
        }
        public static bool operator !=(EstateType a, EstateType b)
        {
            return !(a == b);
        }

        public static bool ValidType(ObjectType ot, ContractType ct)
        {
            var contracts = map[ot];
            if (contracts == null)
                return false;
            return contracts.Exists(c => c == ct);
        }
        public static bool ValidType(string ot, string ct)
        {
            return ValidType(GetObjectType(ot), GetContractType(ct));
        }
        public static bool ValidObjectType(string t)
        {
            return GetObjectType(t) != ObjectType.Unknown;
        }
        public static bool ValidContractType(string t)
        {
            return GetContractType(t) != ContractType.Unknown;
        }


        public static int GetFullEstateType(ObjectType ot, ContractType ct)
        {
            if (ct == ContractType.StudentTenancy)
                return 71;
            switch (ot)
            {
                case ObjectType.Townhouse:
                case ObjectType.House:
                    switch (ct)
                    {
                        case ContractType.SubTenancyRights:
                        case ContractType.TenancyRights:
                        case ContractType.StudentTenancy:
                            return 31;
                        case ContractType.Sale: return 1;
                        case ContractType.TenantOwnership: return 6;
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case ObjectType.Cottage:
                    switch (ct)
                    {
                        case ContractType.SubTenancyRights:
                        case ContractType.TenancyRights:
                            // case ContractType.StudentTenancy:
                            return 34;
                        case ContractType.Sale: return 4;
                        case ContractType.TenantOwnership: return 6;
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case ObjectType.Flat:
                    switch (ct)
                    {
                        case ContractType.SubTenancyRights:
                        case ContractType.TenancyRights:
                            // case ContractType.StudentTenancy:
                            return 7;
                        case ContractType.Sale: return 26;
                        case ContractType.TenantOwnership: return 6;
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case ObjectType.Farm:
                    switch (ct)
                    {
                        case ContractType.SubTenancyRights:
                        case ContractType.TenancyRights:
                            //   case ContractType.StudentTenancy:
                            return 35;
                        case ContractType.Sale: return 5;
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case ObjectType.Lot:
                    switch (ct)
                    {
                        case ContractType.Sale: return 9;
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case ObjectType.Parking:
                    switch (ct)
                    {
                        case ContractType.TenancyRights: return 14;
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }

                default:
                    throw new Exception("Unknown Estate type:" + ot.ToString());
            }
        }
    }
}
