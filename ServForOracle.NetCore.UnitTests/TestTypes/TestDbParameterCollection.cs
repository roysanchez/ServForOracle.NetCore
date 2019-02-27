using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace ServForOracle.NetCore.UnitTests.TestTypes
{
    public class TestDbParameterCollection : DbParameterCollection
    {
        public List<DbParameter> _Parameters { get; set; } = new List<DbParameter>();
        public override int Count => _Parameters.Count;

        public virtual object _syncRoot { get; set; }
        public override object SyncRoot => _syncRoot;

        public override int Add(object value)
        {
            _Parameters.Add(value as DbParameter);
            return _Parameters.IndexOf(value as DbParameter);
        }

        public override void AddRange(Array values)
        {
            _Parameters.AddRange(values as DbParameter[]);
        }

        public override void Clear()
        {
            _Parameters.Clear();
        }

        public override bool Contains(object value)
        {
            return _Parameters.Any(p => p.Value == value);
        }

        public override bool Contains(string value)
        {
            return _Parameters.Any(p => p.ParameterName == value);
        }

        public override void CopyTo(Array array, int index)
        {
            array.CopyTo(_Parameters.ToArray(), index);
        }

        public override IEnumerator GetEnumerator()
        {
            return _Parameters.GetEnumerator();
        }

        public override int IndexOf(object value)
        {
            return _Parameters.FindIndex(c => c.Value == value);
        }

        public override int IndexOf(string parameterName)
        {
            return _Parameters.FindIndex(c => c.ParameterName == parameterName);
        }

        public override void Insert(int index, object value)
        {
            _Parameters.Insert(index, value as DbParameter);
        }

        public override void Remove(object value)
        {
            _Parameters.Remove(value as DbParameter);
        }

        public override void RemoveAt(int index)
        {
            _Parameters.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            _Parameters.RemoveAt(_Parameters.FindIndex(p => p.ParameterName == parameterName));
        }

        protected override DbParameter GetParameter(int index)
        {
            return _Parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _Parameters[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            RemoveAt(parameterName);
            Add(value);
        }
    }
}
