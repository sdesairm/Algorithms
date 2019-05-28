using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SDesaiRM.Algorithms
{
    //class IndexedTreeOfProperties
    public sealed class ObjectBinaryTree<T> where T : new()
    {
        private List<T> m_lstT;
        private List<T> m_NodeValues = new List<T>();
        private SortedList<IComparable, ObjectBinaryTree<T>> m_NodeTree = new SortedList<IComparable, ObjectBinaryTree<T>>();
        private List<string> m_IndexedColumns = new List<string>();
        private string m_IndexedColumnName { get; set; }

        private ObjectBinaryTree(List<string> IndexedColumns) { m_IndexedColumns = IndexedColumns; }

        SortedList<string, PropertyInfo> m_TProperties = new SortedList<string, PropertyInfo>();
        private SortedList<string, PropertyInfo> Properties
        {
            get
            {
                if (m_TProperties.Count == 0)
                    foreach (PropertyInfo pi in typeof(T).GetProperties())
                        if (!pi.PropertyType.IsAssignableFrom(typeof(IComparable)))
                            m_TProperties.Add(pi.Name, pi);
                return m_TProperties;
            }
        }

        public ObjectBinaryTree(List<T> dt)
        {
            m_lstT = dt;
        }

        public void SetPropertiesToIndex(List<string> ColumnsToIndex)
        {
            foreach (PropertyInfo pi in Properties.Values)
                if (ColumnsToIndex.Contains(pi.Name))
                    if (!pi.PropertyType.IsAssignableFrom(typeof(IComparable)))
                        m_IndexedColumns.Add(pi.Name);
                    else
                        throw new Exception(string.Format("Column {0} is either not a valid property or does not implement IComparable interface.", pi.Name));

            foreach (T dr in m_lstT)
            {
                AddTreeNode(ColumnsToIndex, dr, 0);
            }
        }

        public List<T> FindAll(List<RowFetcher> lstValues, ref List<T> m_SetReturn)
        {
            m_SetReturn.Clear();
            if (m_lstT.Count > 0)
                SetTreeNodeValues(lstValues, 0, ref m_SetReturn);
            return m_SetReturn;
        }

        private void SetTreeNodeValues(List<RowFetcher> lstValues, int startIndx, ref List<T> m_SetReturn)
        {
            List<ObjectBinaryTree<T>> indexers = GetTreeNodeValues(lstValues[startIndx]);
            if (string.CompareOrdinal(lstValues[startIndx].ColumnName, m_IndexedColumnName) != 0)
                throw new Exception(string.Format("Expected the property {0} and not {1} to fetch.", m_IndexedColumnName, lstValues[startIndx].ColumnName));

            foreach (ObjectBinaryTree<T> index in indexers)
            {
                if (startIndx < lstValues.Count - 1)
                    index.SetTreeNodeValues(lstValues, startIndx + 1, ref m_SetReturn);
                else
                    m_SetReturn.AddRange(index.m_NodeValues);
            }
            startIndx++;
        }

        private void AddTreeNode(List<string> propertiesToBranch, T obj, int startIndx)
        {
            string prop = propertiesToBranch[startIndx];
            if (!m_IndexedColumns.Contains(prop)) throw new Exception(string.Format("Property {0} is either not a valid property or does not implement IComparable interface.", prop));
            m_IndexedColumnName = prop;
            PropertyInfo pi = Properties[prop];
            IComparable indx = pi.GetValue(obj, null) as IComparable;
            if (!m_NodeTree.ContainsKey(indx))
                m_NodeTree.Add(indx, new ObjectBinaryTree<T>(m_IndexedColumns));
            if (startIndx < propertiesToBranch.Count - 1)
                m_NodeTree[indx].AddTreeNode(propertiesToBranch, obj, ++startIndx);
            else
                m_NodeTree[indx].m_NodeValues.Add(obj);
        }

        private List<ObjectBinaryTree<T>> GetTreeNodeValues(RowFetcher fetch)
        {
            Operator opr = fetch.Operation;
            IComparable compareTo = fetch.Val;
            List<ObjectBinaryTree<T>> rtn = new List<ObjectBinaryTree<T>>();
            int totCount = m_NodeTree.Count - 1;
            int indx = -1; bool isKeyFound = false;
            if (fetch.Operation != Operator.In && fetch.Operation != Operator.All)
            {
                if (compareTo.CompareTo(m_NodeTree.Keys[0]) == -1) indx = -1;
                else if (compareTo.CompareTo(m_NodeTree.Keys[m_NodeTree.Keys.Count - 1]) == 1) indx = m_NodeTree.Keys.Count - 1;
                else isKeyFound = IndexOfKey(m_NodeTree.Keys, compareTo, out indx);
            }
            switch (opr)
            {
                case Operator.Equal:
                    if (IndexOfKey(m_NodeTree.Keys, compareTo, out indx))
                        rtn.Add(m_NodeTree.Values[indx]);
                    break;
                case Operator.NotEqual:
                    rtn.AddRange(m_NodeTree.Values);
                    if (indx >= 0) rtn.RemoveAt(indx);
                    break;
                case Operator.LessThan:
                    rtn.AddRange(GetValuesInRange(m_NodeTree.Values, 0, isKeyFound ? --indx : indx));
                    break;
                case Operator.LessThanOrEqual:
                    rtn.AddRange(GetValuesInRange(m_NodeTree.Values, 0, indx));
                    break;
                case Operator.GreaterThan:
                    rtn.AddRange(GetValuesInRange(m_NodeTree.Values, ++indx, totCount));
                    break;
                case Operator.GreaterThanOrEqual:
                    rtn.AddRange(GetValuesInRange(m_NodeTree.Values, isKeyFound ? indx : ++indx, totCount));
                    break;
                case Operator.All:
                    rtn.AddRange(GetValuesInRange(m_NodeTree.Values, 0, totCount));
                    break;
                case Operator.In:
                    foreach (IComparable ic in fetch.Values)
                    {
                        indx = m_NodeTree.IndexOfKey(ic);
                        if (indx >= 0)
                            rtn.Add(m_NodeTree.Values[indx]);
                    }
                    break;
            }
            return rtn;
        }

        private List<ObjectBinaryTree<T>> GetValuesInRange(IList<ObjectBinaryTree<T>> lst, int start, int end)
        {
            List<ObjectBinaryTree<T>> rtn = new List<ObjectBinaryTree<T>>();
            while (start <= end && start < lst.Count) rtn.Add(lst[start++]);
            return rtn;
        }

        private bool IndexOfKey(IList<IComparable> lst, IComparable val, out int index)
        {
            if (lst.Count == 1)
            {
                index = val.CompareTo(lst[0]);
                return 0 == index;
            }
            index = -1;
            int first = 0;
            int last = lst.Count - 1;
            int middle = (first + last) / 2;

            while (first <= last)
            {
                if (lst[middle].CompareTo(val) < 0)
                    first = middle + 1;
                else if (lst[middle].CompareTo(val) == 0)
                {
                    index = middle;
                    return true;
                }
                else
                    last = middle - 1;
                middle = (first + last) / 2;
            }
            index = middle;
            return false;
        }
    }

    public enum Operator
    {
        All = 0,
        Equal,
        LessThan,
        GreaterThan,
        NotEqual,
        LessThanOrEqual,
        GreaterThanOrEqual,
        In
    };
    public sealed class RowFetcher
    {
        public Operator Operation { get; set; }
        public IComparable Val { get; set; }
        public string ColumnName { get; set; }
        public List<IComparable> Values { get; set; }
    }
}
