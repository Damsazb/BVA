using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace Bovision
{
    public class Sorter<T>
    {
        private static Dictionary<string, DynamicComparer<T>> cache = new Dictionary<string, DynamicComparer<T>>();
        public static List<T> Sort(List<T> list, List<SortProperty> props)
        {
            DynamicComparer<T> cmp;
            var sb = new StringBuilder();
            foreach (SortProperty p in props)
                sb.Append(p.ToString());
            var s = sb.ToString();
            if (!cache.ContainsKey(s))
            {
                cmp = new DynamicComparer<T>(props);
                cache[s] = cmp;
            }
            cmp = cache[s];
            list.Sort(cmp);
            return list;
        }
    }
    public enum DefaultValue { Smallest, Largest }
    public enum SortDirection { Ascend = 1, Descend = -1 }
    public struct SortProperty
    {
        public string Name;
        public SortDirection Direction;
        public DefaultValue Default;

        private static char[] delims = new char[] { ' ', ':' };
        public static List<SortProperty> Parse(string s)
        {
            string[] parts = s.Split(',');
            var props = new List<SortProperty>();
            for (int i = 0; i < parts.Length; i++)
            {
                props.Add(new SortProperty(parts[i]));
            }
            return props;
        }
        public SortProperty(string Name)
        {
            string[] ps = Name.Split(delims);
            this.Name = ps[0];
            if (ps.Length > 1)
            {
                string dir = ps[1];
                Direction = (dir.StartsWith("de", StringComparison.InvariantCultureIgnoreCase) || dir == "-1") ? SortDirection.Descend : SortDirection.Ascend;
            }
            else
                Direction = SortDirection.Ascend;
            if (ps.Length > 2)
            {
                string v = ps[2];
                Default = (string.Compare(v, "dl", true) == 0 ? DefaultValue.Largest : DefaultValue.Smallest);
            }
            else
                Default = DefaultValue.Smallest;
        }
        public SortProperty(string Name, SortDirection Direction)
        {
            this.Name = Name;
            this.Direction = Direction;
            this.Default = DefaultValue.Smallest;
        }
        public SortProperty(string Name, SortDirection Direction, DefaultValue Default)
        {
            this.Name = Name;
            this.Direction = Direction;
            this.Default = Default;
        }
        public override string ToString()
        {
            return ToString(null).ToString();
        }
        public StringBuilder ToString(StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder();
            sb.Append(Name);
            if (Direction == SortDirection.Descend || Default == DefaultValue.Largest)
                sb.Append(":").Append(Direction == SortDirection.Ascend ? "1" : "-1");
            if (Default == DefaultValue.Largest)
                sb.Append(":dl");
            return sb;
        }
    }
    
    public class DynamicComparer<T> : System.Collections.Generic.IComparer<T>
    {
        protected DynamicMethod method;
        protected CompareMethodInvoker comparer;
        protected delegate int CompareMethodInvoker(T x, T y);

        public DynamicComparer(List<SortProperty> sortProperties)
        {
            CheckSortProperties(sortProperties);
            method = CreateDynamicCompareMethod(sortProperties);
            comparer = (CompareMethodInvoker)method.CreateDelegate(typeof(CompareMethodInvoker));
        }

        public int Compare(T x, T y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;

            // OV har objekt med Municipality == null, detta smäller. Hantera detta.
            try
            {
                return comparer.Invoke(x, y);
            }
            catch
            {
                try
                {
                    return -comparer.Invoke(y, x);
                }
                catch { }
            }
            return 0;
        }
        public static int CompareStringLessIsMore(string a, string b)
        {
            bool ea = String.IsNullOrEmpty(a);
            bool eb = String.IsNullOrEmpty(b);
            if (ea || eb)
            {
                if (ea && eb) return 0;
                if (ea) return 1;
                if (eb) return -1;
            }
            return a.CompareTo(b);
        }
        Dictionary<Type, LocalBuilder> localVariables = new Dictionary<Type, LocalBuilder>();
        ILGenerator ilGen = null;
        protected DynamicMethod CreateDynamicCompareMethod(List<SortProperty> sortProperties)
        {
            DynamicMethod dm = new DynamicMethod("DynamicComparison", typeof(int), new Type[] { typeof(T), typeof(T) }, typeof(DynamicComparer<T>));
            ilGen = dm.GetILGenerator();

            Label lbl = ilGen.DefineLabel(); // Declare and define a label that we can jump to.
            ilGen.DeclareLocal(typeof(int)); // Declare a local variable for storing result.

            ilGen.Emit(OpCodes.Ldc_I4_0); // Push 0 onto the eval stack.
            ilGen.Emit(OpCodes.Stloc_0); // Store the eval stack item in the local variable @ position 0.

            foreach (SortProperty property in sortProperties) // For each of the properties we want to check inject the following il.
            {
                Label continueLabel = ilGen.DefineLabel();
                ilGen.Emit(OpCodes.Ldloc_0); // Load local variable at position 0.
                ilGen.Emit(OpCodes.Brtrue, lbl); // Is the local variable in the evaluation stack equal to 0. If not jump to the label we just defined.

                ilGen.Emit(OpCodes.Ldarg_0); // Load argument at position 0.

                var path = property.Name.Split('.');
                Type t = GenerateCall(path);

                if (t.IsValueType) // If the type is a valuetype then we need to inject the following IL.
                {
                    if (!localVariables.ContainsKey(t)) // Do we have a local variable for this type? If not, add one.
                        localVariables.Add(t, ilGen.DeclareLocal(t)); // Adds a local variable of type x.

                    int localIndex = localVariables[t].LocalIndex; // This local variable is for handling value types of type x.
                    ilGen.Emit(OpCodes.Stloc, localIndex); // Store the value in the local var at position x.
                    ilGen.Emit(OpCodes.Ldloca_S, localIndex); // Load the address of the value into the stack. 
                }
                else
                {   // Reference types could be null
                    Label leftNotNull = ilGen.DefineLabel();
                    Label rightNotNull = ilGen.DefineLabel();

                    ilGen.Emit(OpCodes.Dup);
                    ilGen.Emit(OpCodes.Brtrue_S, leftNotNull);
                    // left is null
                    ilGen.Emit(OpCodes.Pop);
                    ilGen.Emit(OpCodes.Ldarg_1);
                    GenerateCall(path);
                    ilGen.Emit(OpCodes.Brtrue_S, rightNotNull);
                    // both are null, so equal
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    ilGen.Emit(OpCodes.Br, continueLabel);

                    ilGen.MarkLabel(rightNotNull);
                    ilGen.Emit(OpCodes.Ldc_I4_M1);
                    ilGen.Emit(OpCodes.Br, continueLabel);
                    ilGen.MarkLabel(leftNotNull);
                }

                //_ilGen.Emit(OpCodes.Ldarga_S, (byte)argumentIndex);

                ilGen.Emit(OpCodes.Ldarg_1); // Load argument at position 0.
                GenerateCall(path);

                // Compare the top 2 items in the evaluation stack and push the return value into the eval stack.
                if (property.Default == DefaultValue.Largest && t == typeof(string))
                    ilGen.EmitCall(OpCodes.Call, this.GetType().GetMethod("CompareStringLessIsMore", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), null);
                else
                {
                    var m = t.GetMethod("CompareTo", new Type[] { t });
                    var itype = typeof(IComparable<>).MakeGenericType(t);
                    if (m != null)
                        ilGen.EmitCall(OpCodes.Call, m, null);
                    else if (itype.IsAssignableFrom(t))
                        ilGen.EmitCall(OpCodes.Callvirt, itype.GetMethod("CompareTo", new Type[] { t }), null);
                    else if (typeof(IComparable).IsAssignableFrom(t))
                        ilGen.EmitCall(OpCodes.Callvirt, typeof(IComparable).GetMethod("CompareTo", new Type[] { t }), null);
                    else
                        throw new Exception("Could not find a method to call CompareTo");
                }
                ilGen.MarkLabel(continueLabel);
                if (property.Direction == SortDirection.Descend) // If the sort should be descending we need to flip the result of the comparison.
                    ilGen.Emit(OpCodes.Neg); // Negates the item in the eval stack.
                ilGen.Emit(OpCodes.Stloc_0); // Store the result in the local variable.
            }

            ilGen.MarkLabel(lbl); // This is the spot where the label we created earlier should be added.
            ilGen.Emit(OpCodes.Ldloc_0); // Load the local var into the eval stack.
            ilGen.Emit(OpCodes.Ret); // Return the value.
            return dm;
        }
        private Type ResolveType(string[] path)
        {
            Type t = typeof(T);

            foreach (var part in path)
            {
                var f = t.GetField(part);
                var p = t.GetProperty(part);
                var m = t.GetMethod(part);
                if (f != null)
                    t = f.FieldType;
                else if (p != null)
                    t = p.PropertyType;
                else if (m != null)
                    t = m.ReturnType;
                else
                    return null;
            }
            return t;
        }
        private Type GenerateCall(string[] path)
        {
            Type t = typeof(T);
            Label continueLabel = ilGen.DefineLabel();
            for (int i = 0; i < path.Length; i++)
            {
                var f = t.GetField(path[i]);
                if (f != null)
                {
                    t = f.FieldType;
                    ilGen.Emit(f.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, f);
                }
                var p = t.GetProperty(path[i]);
                if (p != null)
                {
                    t = p.PropertyType;
                    ilGen.EmitCall(OpCodes.Callvirt, p.GetGetMethod(), null);
                }
                var m = t.GetMethod(path[i]);
                if (m != null && m.GetParameters().Length == 0)
                {
                    t = m.ReturnType;
                    ilGen.EmitCall((m.IsFinal || !m.IsVirtual) ? OpCodes.Call : OpCodes.Callvirt, m, null);
                }
                if (f == null && p == null && m == null)
                    return null;

                if (t.IsEnum)
                    ilGen.Emit(OpCodes.Box, t);
                else if (t.IsValueType) // If the type is a valuetype then we need to inject the following IL.
                {
                    if (i < path.Length - 1)
                        ilGen.Emit(OpCodes.Box, t);
                }
                else
                {
                    ilGen.Emit(OpCodes.Dup);
                    ilGen.Emit(OpCodes.Brfalse_S, continueLabel);
                }

            }
            ilGen.MarkLabel(continueLabel);
            return t;
        }
        protected void CheckSortProperties(List<SortProperty> sortProperties)
        {
            if (sortProperties == null)
                sortProperties = new List<SortProperty>();

            Type instanceType = typeof(T);
            //if (!instanceType.IsPublic)
            //    throw new ArgumentException(string.Format("Type \"{0}\" is not public.", typeof(T).FullName));

            foreach (SortProperty sProp in sortProperties)
            {
                Type t = ResolveType(sProp.Name.Split('.'));
                if (t == null)
                    throw new ArgumentException(string.Format("No public property or field named \"{0}\" was found.", sProp.Name));
                if (!(typeof(IComparable).IsAssignableFrom(t) || typeof(IComparable<>).MakeGenericType(t).IsAssignableFrom(t)))
                    throw new ArgumentException(string.Format("The type \"{1}\" of the property or field \"{0}\" does not implement IComparable.", sProp.Name, t.Name));
            }
        }
    }
}
